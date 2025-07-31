using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Services;
using UPTRMS.Api.Repositories;
using UPTRMS.Api.Tests.BDD;
using Xunit;

namespace UPTRMS.Api.Tests;

public class DebugJwtTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DebugJwtTest(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Test_Authentication_Flow()
    {
        // Step 1: Get auth token
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        
        var loginRequest = new LoginRequest
        {
            Email = "admin@uptrms.com",
            Password = "TestPassword123!"
        };
        
        var (user, token, refreshToken) = await authService.LoginAsync(loginRequest);
        
        // Step 2: Use token to call protected endpoint
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // First check which user ID the authentication service gave us
        Console.WriteLine($"Authenticated user ID: {user.UserId}");
        Console.WriteLine($"Authenticated user email: {user.Email}");
        
        // Step 3: Call profile endpoint
        var response = await _client.GetAsync("/api/users/profile");
        
        // Output debugging info
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
        Console.WriteLine($"Content: {responseContent}");
        Console.WriteLine($"Token: {token}");
        
        // Check if it's authentication issue or user not found
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // Try to understand why
            using var scope2 = _factory.Services.CreateScope();
            var userRepo = scope2.ServiceProvider.GetRequiredService<IUserRepository>();
            var userInRepo = await userRepo.GetByIdAsync(user.UserId);
            Console.WriteLine($"User exists in repository: {userInRepo != null}");
            if (userInRepo != null)
            {
                Console.WriteLine($"Repo user email: {userInRepo.Email}");
            }
        }
        
        // Should be successful
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Health_Endpoint()
    {
        // Test if the app is running at all
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"Health Status: {response.StatusCode}");
        Console.WriteLine($"Health Content: {content}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_Routes_Available()
    {
        // Test various endpoints to see what's available
        var endpoints = new[] { "/", "/swagger", "/api", "/api/users", "/api/auth/login" };
        
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Console.WriteLine($"{endpoint}: {response.StatusCode}");
        }
    }
}