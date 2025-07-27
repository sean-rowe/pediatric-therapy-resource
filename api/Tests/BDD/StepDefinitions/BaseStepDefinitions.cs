using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

public abstract class BaseStepDefinitions
{
    protected readonly HttpClient Client;
    protected readonly ScenarioContext ScenarioContext;
    protected HttpResponseMessage? LastResponse;
    protected string? AuthToken;
    
    protected BaseStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
    {
        ScenarioContext = scenarioContext;
        
        // Get the client from ScenarioContext if it exists, otherwise create a new one
        if (scenarioContext.ContainsKey("HttpClient"))
        {
            Client = scenarioContext.Get<HttpClient>("HttpClient");
        }
        else
        {
            Client = factory.CreateClient();
            scenarioContext["HttpClient"] = Client;
        }
    }

    // Common Given steps - these are protected methods to be called by derived classes
    protected async Task GivenIAmLoggedInAs(string role)
    {
        // Mock authentication for testing
        AuthToken = GenerateMockToken(role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
        ScenarioContext["CurrentRole"] = role;
    }

    protected void GivenIAmNotAuthenticated()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        AuthToken = null;
    }

    // Common When steps - these are protected methods to be called by derived classes
    protected async Task WhenISendAGETRequestTo(string endpoint)
    {
        LastResponse = await Client.GetAsync(endpoint);
        ScenarioContext["LastResponse"] = LastResponse;
    }

    protected async Task WhenISendAPOSTRequestToWith(string endpoint, Table table)
    {
        string json = TableToJson(table);
        Console.WriteLine($"[BaseStepDefinitions] Sending POST to {endpoint} with body: {json}");
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Add headers to show what's being sent
        Console.WriteLine($"[BaseStepDefinitions] Content-Type: {content.Headers.ContentType}");
        
        LastResponse = await Client.PostAsync(endpoint, content);
        
        // Add debug information
        Console.WriteLine($"[BaseStepDefinitions] Response Status: {LastResponse?.StatusCode}");
        if (LastResponse != null)
        {
            string responseContent = await LastResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[BaseStepDefinitions] Response Content: {responseContent}");
            
            // If it's a 400, let's parse the error details
            if (LastResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Console.WriteLine($"[BaseStepDefinitions] Bad Request - checking validation errors");
                try
                {
                    JsonDocument doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("error", out JsonElement error))
                    {
                        Console.WriteLine($"[BaseStepDefinitions] Error message: {error.GetString()}");
                    }
                    if (doc.RootElement.TryGetProperty("errors", out JsonElement errors))
                    {
                        Console.WriteLine($"[BaseStepDefinitions] Validation errors:");
                        foreach (JsonProperty prop in errors.EnumerateObject())
                        {
                            Console.WriteLine($"  {prop.Name}: {prop.Value}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BaseStepDefinitions] Failed to parse error response: {ex.Message}");
                }
            }
        }
        
        ScenarioContext["LastResponse"] = LastResponse;
    }

    protected async Task WhenISendAPUTRequestToWith(string endpoint, Table table)
    {
        var json = TableToJson(table);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        LastResponse = await Client.PutAsync(endpoint, content);
        ScenarioContext["LastResponse"] = LastResponse;
    }

    protected async Task WhenISendADELETERequestTo(string endpoint)
    {
        LastResponse = await Client.DeleteAsync(endpoint);
        ScenarioContext["LastResponse"] = LastResponse;
    }

    // Common Then steps - these are protected methods to be called by derived classes
    protected void ThenTheResponseStatusShouldBe(int statusCode)
    {
        LastResponse.Should().NotBeNull();
        
        // If the status code doesn't match, output the response body for debugging
        if ((int)LastResponse!.StatusCode != statusCode)
        {
            string content = LastResponse.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"Expected status {statusCode} but got {(int)LastResponse.StatusCode}");
            Console.WriteLine($"Response content: {content}");
        }
        
        ((int)LastResponse!.StatusCode).Should().Be(statusCode);
    }

    protected async Task ThenTheResponseShouldContain(string expectedContent)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        content.Should().Contain(expectedContent);
    }

    protected async Task ThenTheResponseShouldContainFields(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);
        
        foreach (var row in table.Rows)
        {
            var fieldName = row.ContainsKey("field") ? row["field"] : row["Field"];
            var fieldType = row.ContainsKey("type") ? row["type"] : row["Type"];
            
            // Check if field exists
            jsonDocument.RootElement.TryGetProperty(fieldName, out var property).Should().BeTrue($"Field '{fieldName}' should exist in response");
            
            // Basic type validation
            switch (fieldType.ToLower())
            {
                case "string":
                    property.ValueKind.Should().Be(JsonValueKind.String);
                    break;
                case "number":
                    property.ValueKind.Should().BeOneOf(JsonValueKind.Number);
                    break;
                case "boolean":
                    property.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
                    break;
                case "array":
                    property.ValueKind.Should().Be(JsonValueKind.Array);
                    break;
                case "object":
                    property.ValueKind.Should().Be(JsonValueKind.Object);
                    break;
            }
        }
    }

    protected void ThenTheResponseShouldHaveAHeaderWithValue(string headerName, string headerValue)
    {
        LastResponse.Should().NotBeNull();
        LastResponse!.Headers.Should().ContainKey(headerName);
        LastResponse.Headers.GetValues(headerName).Should().Contain(headerValue);
    }

    // Helper methods
    protected string TableToJson(Table table)
    {
        Console.WriteLine($"[TableToJson] Processing table with {table.Rows.Count} rows");
        Dictionary<string, object> dict = new Dictionary<string, object>();
        foreach (TableRow row in table.Rows)
        {
            Console.WriteLine($"[TableToJson] Row keys: {string.Join(", ", row.Keys)}");
            
            // Handle both lowercase and uppercase field names
            string? fieldKey = null;
            string? valueKey = null;
            
            if (row.ContainsKey("field")) fieldKey = "field";
            else if (row.ContainsKey("Field")) fieldKey = "Field";
            
            if (row.ContainsKey("value")) valueKey = "value";
            else if (row.ContainsKey("Value")) valueKey = "Value";
            
            if (fieldKey != null && valueKey != null)
            {
                // Use the field name as-is from the BDD test (lowercase)
                string fieldName = row[fieldKey];
                object fieldValue = ParseValue(row[valueKey]);
                dict[fieldName] = fieldValue;
                Console.WriteLine($"[TableToJson] Added {fieldName} = {fieldValue} (type: {fieldValue?.GetType().Name})");
            }
            else
            {
                Console.WriteLine($"[TableToJson] Could not find field/value keys in row");
            }
        }
        
        // Use JsonSerializerOptions to ensure property naming policy
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string json = JsonSerializer.Serialize(dict, options);
        Console.WriteLine($"[TableToJson] Final JSON: {json}");
        return json;
    }

    protected object ParseValue(string value)
    {
        if (bool.TryParse(value, out var boolValue)) return boolValue;
        if (int.TryParse(value, out var intValue)) return intValue;
        if (decimal.TryParse(value, out var decimalValue)) return decimalValue;
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            return value.Trim('[', ']').Split(',').Select(s => s.Trim()).ToArray();
        }
        return value;
    }

    protected string GenerateMockToken(string role)
    {
        // In real implementation, this would generate a proper JWT
        // For testing, we'll use a simple mock token
        return $"mock_token_{role}_{Guid.NewGuid()}";
    }

    protected T? GetResponseContent<T>()
    {
        if (LastResponse == null) return default;
        var json = LastResponse.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
                });
    }

    protected void StoreInContext(string key, object value)
    {
        ScenarioContext[key] = value;
    }

    protected T GetFromContext<T>(string key)
    {
        return ScenarioContext.Get<T>(key);
    }
    
    protected async Task WhenISendAPOSTRequestToWithData(string endpoint, Dictionary<string, object> data)
    {
        var json = JsonSerializer.Serialize(data);
        Console.WriteLine($"[DEBUG] Sending POST to {endpoint} with data: {json}");
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            LastResponse = await Client.PostAsync(endpoint, content);
            Console.WriteLine($"[DEBUG] Response Status: {LastResponse?.StatusCode}");
            
            if (LastResponse != null)
            {
                var responseContent = await LastResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Response Content: {responseContent}");
            }
            else
            {
                Console.WriteLine("[DEBUG] LastResponse is null!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Exception during POST: {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
            throw;
        }
        
        ScenarioContext["LastResponse"] = LastResponse;
    }
    
    protected async Task WhenISendAPUTRequestToWithData(string endpoint, Dictionary<string, object> data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        LastResponse = await Client.PutAsync(endpoint, content);
        ScenarioContext["LastResponse"] = LastResponse;
    }
    
    // Overloads for methods that receive Table from SpecFlow
    protected async Task WhenISendAPOSTRequestToWithData(string endpoint, Table table)
    {
        var json = TableToJson(table);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        LastResponse = await Client.PostAsync(endpoint, content);
        ScenarioContext["LastResponse"] = LastResponse;
    }
    
    protected async Task WhenISendAPUTRequestToWithData(string endpoint, Table table)
    {
        var json = TableToJson(table);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        LastResponse = await Client.PutAsync(endpoint, content);
        ScenarioContext["LastResponse"] = LastResponse;
    }
}
