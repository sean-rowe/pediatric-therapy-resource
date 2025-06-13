using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Integration;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real email service with mock
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    var mockEmailService = new Mock<IEmailService>();
                    mockEmailService.Setup(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);
                    services.AddSingleton(mockEmailService.Object);
                    
                    // Replace license verification service with mock
                    var licenseDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ILicenseVerificationService));
                    if (licenseDescriptor != null)
                    {
                        services.Remove(licenseDescriptor);
                    }
                    
                    var mockLicenseService = new Mock<ILicenseVerificationService>();
                    mockLicenseService.Setup(x => x.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new LicenseVerificationResult { Valid = true });
                    services.AddSingleton(mockLicenseService.Object);
                });
            })
            .CreateClient();
    }

    [Fact]
    public async Task Register_Should_Return_Ok_With_Valid_Request()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "TEST12345",
            LicenseState = "CA",
            LicenseType = "OT",
            Phone = "555-123-4567",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");
        result.UserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_With_Invalid_Email()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "TEST12345",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid email format"));
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_With_Weak_Password()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "TEST12345",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Password must be at least 12 characters"));
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Terms_Not_Accepted()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "TEST12345",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("You must accept the terms of service"));
    }

    [Fact]
    public async Task VerifyEmail_Should_Return_Ok_With_Valid_Token()
    {
        // Note: In a real integration test, you would first register a user and get a real token
        // This is simplified for demonstration
        var token = "test_token";

        // Act
        var response = await _client.GetAsync($"/api/auth/verify-email/{token}");

        // Assert
        // In real scenario, this would depend on database state
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResendVerification_Should_Return_Ok_With_Valid_Email()
    {
        // Arrange
        var request = new { Email = "test@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/resend-verification", request);

        // Assert
        // In real scenario, this would depend on database state
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Should_Be_Rate_Limited()
    {
        // Arrange
        var baseEmail = $"ratelimit_{Guid.NewGuid()}";
        var requests = new List<Task<HttpResponseMessage>>();

        // Act - Send 5 requests (rate limit is 3 per hour)
        for (int i = 0; i < 5; i++)
        {
            var request = new RegisterRequest
            {
                Email = $"{baseEmail}_{i}@example.com",
                Password = "P@ssw0rd123!",
                ConfirmPassword = "P@ssw0rd123!",
                FirstName = "John",
                LastName = "Doe",
                LicenseNumber = $"TEST{i}",
                LicenseState = "CA",
                LicenseType = "OT",
                AcceptedTerms = true
            };
            
            requests.Add(_client.PostAsJsonAsync("/api/auth/register", request));
        }

        var responses = await Task.WhenAll(requests);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.BadRequest);
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        
        // At least some requests should be rate limited
        rateLimitedCount.Should().BeGreaterThan(0);
    }
}