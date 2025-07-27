using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.Mocks;
using Xunit;

namespace UPTRMS.Api.Tests.Integration;

public class LoginTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LoginTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove real service if registered
                var authService = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationService));
                if (authService != null)
                {
                    services.Remove(authService);
                }
                
                // Add mock service
                services.AddSingleton<IAuthenticationService, MockAuthenticationService>();
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        // First register a user
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = "TEST123",
            Languages = new List<string> { "English" },
            Specialties = new List<string> { "OT" }
        };
        
        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Get the auth service to verify email
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        await authService.VerifyEmailAsync("test-token");
        
        // Act - Login
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await loginResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Login Response: {responseContent}");
        
        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        
        root.TryGetProperty("success", out var successElement).Should().BeTrue();
        successElement.GetBoolean().Should().BeTrue();
        
        root.TryGetProperty("token", out var tokenElement).Should().BeTrue();
        tokenElement.GetString().Should().NotBeNullOrEmpty();
        
        root.TryGetProperty("refreshToken", out var refreshTokenElement).Should().BeTrue();
        refreshTokenElement.GetString().Should().NotBeNullOrEmpty();
        
        root.TryGetProperty("user", out var userElement).Should().BeTrue();
        userElement.ValueKind.Should().Be(JsonValueKind.Object);
    }
    
    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        // First register a user
        var registerRequest = new RegisterRequest
        {
            Email = "test2@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = "TEST123",
            Languages = new List<string> { "English" },
            Specialties = new List<string> { "OT" }
        };
        
        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        await _client.PostAsync("/api/auth/register", registerContent);
        
        // Get the auth service to verify email
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        await authService.VerifyEmailAsync("test-token");
        
        // Act - Login with wrong password
        var loginRequest = new LoginRequest
        {
            Email = "test2@example.com",
            Password = "WrongPassword!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await loginResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Error Response: {responseContent}");
        
        responseContent.Should().Contain("Invalid credentials");
    }
}