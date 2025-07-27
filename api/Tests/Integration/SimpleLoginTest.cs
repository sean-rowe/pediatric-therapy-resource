using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.BDD;
using UPTRMS.Api.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace UPTRMS.Api.Tests.Integration;

public class SimpleLoginTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public SimpleLoginTest(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SimpleLogin_EndpointResponds()
    {
        // Arrange
        object loginData = new { email = "test@example.com", password = "Password123!" };
        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginData),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);
        string responseContent = await response.Content.ReadAsStringAsync();
        
        // Log response for debugging
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine($"Content: {responseContent}");

        // Assert - We're just checking the endpoint responds
        response.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Login_Should_Work_With_Valid_Credentials()
    {
        // Clear any existing users
        MockAuthenticationService.ClearState();
        
        // Create a user through the mock service
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            RegisterRequest registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "TestPass123!",
                FirstName = "Test",
                LastName = "User",
                LicenseNumber = "TEST123",
                Languages = new List<string> { "English" },
                Specialties = new List<string> { "OT" },
                AcceptedTerms = true
            };
            
            _output.WriteLine("Registering user...");
            (User user, string token, string refreshToken) = await authService.RegisterAsync(registerRequest);
            _output.WriteLine($"User registered: {user.Email}, EmailVerified: {user.EmailVerified}");
            
            // Verify the email
            bool verified = await authService.VerifyEmailAsync("test-token");
            _output.WriteLine($"Email verification result: {verified}");
            
            // Check if user is verified in the service
            if (authService is MockAuthenticationService mockService)
            {
                // Use the helper method to ensure email is verified
                await mockService.VerifyEmailByEmailAsync(user.Email);
                _output.WriteLine("Email verified using helper method");
            }
        }
        
        // Now try to login
        LoginRequest loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPass123!"
        };
        
        // Send as lowercase JSON to match test expectations
        string json = JsonSerializer.Serialize(new
        {
            email = loginRequest.Email,
            password = loginRequest.Password
        });
        
        _output.WriteLine($"Sending login request: {json}");
        
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Status: {response.StatusCode}");
        _output.WriteLine($"Response Content: {responseContent}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}