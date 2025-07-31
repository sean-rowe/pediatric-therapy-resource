using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechTalk.SpecFlow;
using FluentAssertions;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

public abstract class BaseStepDefinitions
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly ScenarioContext _scenarioContext;
    protected HttpClient _client;
    protected string? _authToken;
    protected HttpResponseMessage? _response;
    protected string? _responseContent;
    
    // Provide uppercase properties for compatibility with step definitions
    protected HttpClient Client => _client;
    protected HttpResponseMessage? Response => _response;
    protected HttpResponseMessage? LastResponse => _response;
    protected string? LastResponseContent => _responseContent;
    protected ScenarioContext ScenarioContext => _scenarioContext;

    protected BaseStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
    {
        _factory = factory;
        _scenarioContext = scenarioContext;
        
        // Share the HTTP client across all step definitions through ScenarioContext
        if (_scenarioContext.ContainsKey("HttpClient"))
        {
            _client = _scenarioContext.Get<HttpClient>("HttpClient");
        }
        else
        {
            _client = _factory.CreateClient();
            _scenarioContext["HttpClient"] = _client;
        }
    }

    protected async Task GivenIAmLoggedInAs(string role)
    {
        // Create a test user and get auth token
        var loginRequest = new LoginRequest
        {
            Email = $"test.{role.ToLower()}@uptrms.com",
            Password = "TestPassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _authToken = loginResponse?.Token;
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authToken);
        }
        else
        {
            // For now, use a mock token for testing
            _authToken = "mock-test-token";
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authToken);
        }

        _scenarioContext["AuthToken"] = _authToken;
        _scenarioContext["UserRole"] = role;
    }

    protected void GivenIAmNotAuthenticated()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _authToken = null;
        _scenarioContext.Remove("AuthToken");
        _scenarioContext.Remove("UserRole");
    }

    protected async Task<HttpResponseMessage> SendGetRequest(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
        _responseContent = await _response.Content.ReadAsStringAsync();
        _scenarioContext["LastResponse"] = _response;
        _scenarioContext["LastResponseContent"] = _responseContent;
        
        // Debug output for failed requests
        if (!_response.IsSuccessStatusCode)
        {
            Console.WriteLine($"GET {endpoint} returned {_response.StatusCode}: {_responseContent}");
        }
        
        return _response;
    }

    protected async Task<HttpResponseMessage> SendPostRequest(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data != null)
        {
            content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");
        }

        _response = await _client.PostAsync(endpoint, content);
        _responseContent = await _response.Content.ReadAsStringAsync();
        _scenarioContext["LastResponse"] = _response;
        _scenarioContext["LastResponseContent"] = _responseContent;
        return _response;
    }

    protected async Task<HttpResponseMessage> SendPutRequest(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data != null)
        {
            content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");
        }

        _response = await _client.PutAsync(endpoint, content);
        _responseContent = await _response.Content.ReadAsStringAsync();
        _scenarioContext["LastResponse"] = _response;
        _scenarioContext["LastResponseContent"] = _responseContent;
        
        // Debug output for failed requests
        if (!_response.IsSuccessStatusCode)
        {
            Console.WriteLine($"PUT {endpoint} returned {_response.StatusCode}: {_responseContent}");
        }
        
        return _response;
    }

    protected async Task<HttpResponseMessage> SendDeleteRequest(string endpoint)
    {
        _response = await _client.DeleteAsync(endpoint);
        _responseContent = await _response.Content.ReadAsStringAsync();
        _scenarioContext["LastResponse"] = _response;
        _scenarioContext["LastResponseContent"] = _responseContent;
        return _response;
    }

    protected HttpResponseMessage GetLastResponse()
    {
        return _scenarioContext.Get<HttpResponseMessage>("LastResponse");
    }

    protected string GetLastResponseContent()
    {
        return _scenarioContext.Get<string>("LastResponseContent");
    }

    // Aliases for common methods used in step definitions
    protected string GetResponseContent()
    {
        return GetLastResponseContent();
    }

    protected T? GetResponseContent<T>()
    {
        return DeserializeResponse<T>();
    }

    protected T GetFromContext<T>(string key)
    {
        return _scenarioContext.Get<T>(key);
    }

    protected void SetLastResponse(HttpResponseMessage response)
    {
        _response = response;
        _responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        _scenarioContext["LastResponse"] = _response;
        _scenarioContext["LastResponseContent"] = _responseContent;
    }

    protected T? DeserializeResponse<T>()
    {
        var content = GetLastResponseContent();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    // Common When methods used by step definitions
    protected async Task WhenISendAGETRequestTo(string endpoint)
    {
        await SendGetRequest(endpoint);
    }

    protected async Task WhenISendAPOSTRequestTo(string endpoint)
    {
        await SendPostRequest(endpoint);
    }

    protected async Task WhenISendAPOSTRequestToWithData(string endpoint, object data)
    {
        await SendPostRequest(endpoint, data);
    }

    protected async Task WhenISendAPUTRequestTo(string endpoint)
    {
        await SendPutRequest(endpoint);
    }

    protected async Task WhenISendAPUTRequestToWithData(string endpoint, object data)
    {
        await SendPutRequest(endpoint, data);
    }

    protected async Task WhenISendADELETERequestTo(string endpoint)
    {
        await SendDeleteRequest(endpoint);
    }

    // Common Then methods used by step definitions
    protected void ThenTheResponseStatusShouldBe(HttpStatusCode expectedStatus)
    {
        if (_response == null)
        {
            throw new InvalidOperationException("No response received. Make sure to send a request first.");
        }
        _response.StatusCode.Should().Be(expectedStatus);
    }

    protected void ThenTheResponseStatusShouldBe(int expectedStatusCode)
    {
        ThenTheResponseStatusShouldBe((HttpStatusCode)expectedStatusCode);
    }

    protected void ThenTheResponseShouldContain(string expectedContent)
    {
        if (_responseContent == null)
        {
            throw new InvalidOperationException("No response content received.");
        }
        _responseContent.Should().Contain(expectedContent);
    }

    protected void ThenTheResponseShouldContainFields(params string[] fields)
    {
        if (_responseContent == null)
        {
            throw new InvalidOperationException("No response content received.");
        }
        
        var jsonDoc = JsonDocument.Parse(_responseContent);
        foreach (var field in fields)
        {
            jsonDoc.RootElement.TryGetProperty(field, out _).Should().BeTrue($"Response should contain field '{field}'");
        }
    }

    protected void ThenTheResponseShouldHaveAHeaderWithValue(string headerName, string expectedValue)
    {
        if (_response == null)
        {
            throw new InvalidOperationException("No response received.");
        }
        
        _response.Headers.Should().ContainKey(headerName);
        _response.Headers.GetValues(headerName).Should().Contain(expectedValue);
    }

    protected void StoreInContext(string key, object value)
    {
        _scenarioContext[key] = value;
    }

    protected string TableToJson(Table table)
    {
        var dict = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            dict[row["field"]] = row["value"];
        }
        return JsonSerializer.Serialize(dict);
    }
}