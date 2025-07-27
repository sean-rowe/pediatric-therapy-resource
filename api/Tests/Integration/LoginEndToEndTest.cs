using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.BDD;
using UPTRMS.Api.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace UPTRMS.Api.Tests.Integration;

[Collection("Integration")]
public class LoginEndToEndTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public LoginEndToEndTest(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Login_Should_Accept_Lowercase_Json_Fields()
    {
        // Clear any existing state
        MockAuthenticationService.ClearState();
        
        // First register a user through the mock service
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            RegisterRequest registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User",
                LicenseNumber = "TEST123",
                Languages = new List<string> { "English" },
                Specialties = new List<string> { "OT" },
                AcceptedTerms = true
            };
            
            var (user, _, _) = await authService.RegisterAsync(registerRequest);
            
            // Verify the email
            if (authService is MockAuthenticationService mockService)
            {
                await mockService.VerifyEmailByEmailAsync(user.Email);
            }
        }
        
        // Now test login with lowercase JSON fields (as sent by BDD tests)
        string loginJson = """{"email":"test@example.com","password":"Test123!"}""";
        StringContent content = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        _output.WriteLine($"Sending login request with JSON: {loginJson}");
        
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response content: {responseContent}");
        
        // The test should pass with 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify response contains expected fields
        JsonDocument doc = JsonDocument.Parse(responseContent);
        Assert.True(doc.RootElement.TryGetProperty("success", out JsonElement successElement));
        Assert.True(successElement.GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("token", out _));
        Assert.True(doc.RootElement.TryGetProperty("refreshToken", out _));
        Assert.True(doc.RootElement.TryGetProperty("user", out _));
    }
}