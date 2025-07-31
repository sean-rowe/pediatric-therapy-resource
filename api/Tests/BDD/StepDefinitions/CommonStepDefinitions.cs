using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;
using FluentAssertions;
using System.Text.Json;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class CommonStepDefinitions : BaseStepDefinitions
{
    public CommonStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    [Given(@"I am logged in as a ""(.*)""")]
    public async Task GivenIAmLoggedInAsStep(string role)
    {
        await GivenIAmLoggedInAs(role);
    }

    [Given(@"I am not authenticated")]
    public void GivenIAmNotAuthenticatedStep()
    {
        GivenIAmNotAuthenticated();
    }

    [When(@"I send a GET request to ""(.*)""")]
    public async Task WhenISendAGETRequestToStep(string endpoint)
    {
        Console.WriteLine($"Original endpoint: {endpoint}");
        
        // Replace test user IDs with actual GUIDs
        if (endpoint.Contains("user-123") && _scenarioContext.ContainsKey("TargetUserId"))
        {
            var actualUserId = _scenarioContext.Get<string>("TargetUserId");
            endpoint = endpoint.Replace("user-123", actualUserId);
            Console.WriteLine($"Replaced endpoint: {endpoint}");
        }
        else
        {
            Console.WriteLine($"No replacement needed. Has TargetUserId: {_scenarioContext.ContainsKey("TargetUserId")}");
        }
        
        await WhenISendAGETRequestTo(endpoint);
    }

    [When(@"I send a POST request to ""(.*)"" with:")]
    public async Task WhenISendAPOSTRequestToWithStep(string endpoint, Table table)
    {
        var data = TableToJson(table);
        await WhenISendAPOSTRequestToWithData(endpoint, data);
    }
    
    [When(@"I send a POST request to ""(.*)""")]
    public async Task WhenISendAPOSTRequestToStep(string endpoint)
    {
        // This handles POST requests without a table, using data from context
        var registrationData = ScenarioContext.ContainsKey("RegistrationData") 
            ? ScenarioContext.Get<Dictionary<string, object>>("RegistrationData")
            : new Dictionary<string, object>();
            
        await WhenISendAPOSTRequestToWithData(endpoint, registrationData);
    }
    [When(@"I send a PUT request to ""(.*)"" with:")]
    public async Task WhenISendAPUTRequestToWithStep(string endpoint, Table table)
    {
        Console.WriteLine($"Original PUT endpoint: {endpoint}");
        
        // Replace test user IDs with actual GUIDs
        if (endpoint.Contains("user-123") && _scenarioContext.ContainsKey("TargetUserId"))
        {
            var actualUserId = _scenarioContext.Get<string>("TargetUserId");
            endpoint = endpoint.Replace("user-123", actualUserId);
            Console.WriteLine($"Replaced PUT endpoint: {endpoint}");
        }
        else
        {
            Console.WriteLine($"No replacement needed. Has TargetUserId: {_scenarioContext.ContainsKey("TargetUserId")}");
        }
        
        var data = TableToJson(table);
        await WhenISendAPUTRequestToWithData(endpoint, data);
    }

    [When(@"I send a DELETE request to ""(.*)""")]
    public async Task WhenISendADELETERequestToStep(string endpoint)
    {
        await WhenISendADELETERequestTo(endpoint);
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBeStep(int statusCode)
    {
        // Get LastResponse from ScenarioContext
        if (!ScenarioContext.ContainsKey("LastResponse"))
        {
            throw new InvalidOperationException("No response found in ScenarioContext. Make sure the request was sent.");
        }
        
        SetLastResponse(ScenarioContext.Get<HttpResponseMessage>("LastResponse"));
        ThenTheResponseStatusShouldBe(statusCode);
    }

    [Then(@"the response should contain ""(.*)""")]
    public void ThenTheResponseShouldContainStep(string expectedContent)
    {
        ThenTheResponseShouldContain(expectedContent);
    }

    [Then(@"the response should contain:")]
    public void ThenTheResponseShouldContainFieldsStep(Table table)
    {
        var fields = table.Rows.Select(r => r["field"]).ToArray();
        ThenTheResponseShouldContainFields(fields);
    }

    [Then(@"the response should have a ""(.*)"" header with value ""(.*)""")]
    public void ThenTheResponseShouldHaveAHeaderWithValueStep(string headerName, string headerValue)
    {
        ThenTheResponseShouldHaveAHeaderWithValue(headerName, headerValue);
    }
    [Then(@"the response should contain array of:")]
    public async Task ThenTheResponseShouldContainArrayOfStep(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.ValueKind.Should().Be(JsonValueKind.Array);
        
        // Verify array structure matches expected fields
        foreach (var element in root.EnumerateArray())
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["field"];
                element.TryGetProperty(fieldName, out _).Should().BeTrue($"Array element should contain field '{fieldName}'");
            }
        }
    }
    
    [Then(@"report should include:")]
    public async Task ThenReportShouldIncludeStep(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        foreach (var row in table.Rows)
        {
            var sectionName = row.ContainsKey("Section") ? row["Section"] : row["section"];
            content.Should().Contain(sectionName, $"Report should include section '{sectionName}'");
        }
    }
    
    // Additional common step definitions that are widely used
    
    [Given(@"the API is available")]
    public void GivenTheAPIIsAvailable()
    {
        // The API is available through the test factory
        // This step is primarily for documentation and doesn't need to do anything
        ScenarioContext["APIAvailable"] = true;
    }

    [Given(@"I am a system administrator")]
    public void GivenIAmASystemAdministrator()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
}
