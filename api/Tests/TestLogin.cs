using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.BDD;
using UPTRMS.Api.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace UPTRMS.Api.Tests;

public class TestLogin : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public TestLogin(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Debug_Login_Flow()
    {
        // Clear any existing users
        MockAuthenticationService.ClearState();
        
        // Create a user through the mock service
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            RegisterRequest registerRequest = new RegisterRequest
            {
                Email = "user@clinic.com",
                Password = "SecurePass123!",
                FirstName = "Test",
                LastName = "User",
                LicenseNumber = "TEST123",
                Languages = new List<string> { "English" },
                Specialties = new List<string> { "OT" },
                AcceptedTerms = true
            };
            
            _output.WriteLine("=== REGISTERING USER ===");
            (User user, string token, string refreshToken) = await authService.RegisterAsync(registerRequest);
            _output.WriteLine($"User registered: {user.Email}, ID: {user.UserId}, EmailVerified: {user.EmailVerified}");
            
            // Verify the email
            _output.WriteLine("\n=== VERIFYING EMAIL ===");
            bool verified = await authService.VerifyEmailAsync("test-token");
            _output.WriteLine($"Email verification result: {verified}");
            
            // Verify again using the helper if it's MockAuthenticationService
            if (authService is MockAuthenticationService mockService)
            {
                await mockService.VerifyEmailByEmailAsync(user.Email);
                _output.WriteLine("Email verified using helper method");
            }
        }
        
        // Now try to login with exact same JSON structure as BDD test
        _output.WriteLine("\n=== ATTEMPTING LOGIN ===");
        
        // Create the exact JSON structure from the BDD test
        string json = @"{""field"":""email"",""value"":""user@clinic.com""},{""field"":""password"",""value"":""SecurePass123!""}";
        
        // Wait, that's not valid JSON. Let me check what TableToJson actually produces
        // The BDD test sends a table with field/value pairs. Let's simulate that
        Dictionary<string, object> loginData = new Dictionary<string, object>
        {
            ["email"] = "user@clinic.com",
            ["password"] = "SecurePass123!"
        };
        
        string loginJson = JsonSerializer.Serialize(loginData);
        _output.WriteLine($"Login JSON: {loginJson}");
        
        StringContent content = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        // Add debugging headers
        content.Headers.Add("X-Test-Debug", "true");
        
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"\n=== RESPONSE ===");
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
        _output.WriteLine($"Content: {responseContent}");
        
        // Check if it's a model validation error
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _output.WriteLine("\n=== ANALYZING BAD REQUEST ===");
            try
            {
                JsonDocument doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("errors", out JsonElement errors))
                {
                    _output.WriteLine("Validation errors found:");
                    foreach (JsonProperty prop in errors.EnumerateObject())
                    {
                        _output.WriteLine($"  {prop.Name}: {prop.Value}");
                    }
                }
            }
            catch { }
        }
        
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}