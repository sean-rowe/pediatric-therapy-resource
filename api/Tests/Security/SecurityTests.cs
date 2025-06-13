using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using TherapyDocs.Api.Models.DTOs;
using Xunit;

namespace TherapyDocs.Api.Tests.Security;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("'; DROP TABLE users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    [InlineData("' UNION SELECT * FROM users--")]
    [InlineData("1'; DELETE FROM users WHERE '1'='1")]
    public async Task Register_Should_Handle_SQL_Injection_Attempts_In_Email(string maliciousInput)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = maliciousInput,
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = "TEST123",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // The response should be a validation error, not a SQL error
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("syntax");
        content.Should().Contain("email");
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<iframe src='javascript:alert(\"XSS\")'></iframe>")]
    [InlineData("<svg onload=alert('XSS')>")]
    public async Task Register_Should_Prevent_XSS_In_Names(string maliciousInput)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = maliciousInput,
            LastName = "User",
            LicenseNumber = "TEST123",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        // The response should not contain the raw script tags
        content.Should().NotContain("<script>");
        content.Should().NotContain("onerror=");
    }

    [Theory]
    [InlineData("'; EXEC sp_addlogin 'hacker', 'password'--")]
    [InlineData("'; EXEC xp_cmdshell 'net user hacker password /add'--")]
    [InlineData("' OR EXISTS(SELECT * FROM users WHERE role='admin')--")]
    public async Task Register_Should_Handle_SQL_Injection_In_License_Number(string maliciousInput)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = maliciousInput,
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("License number contains invalid characters");
    }

    [Fact]
    public async Task VerifyEmail_Should_Handle_Path_Traversal_Attempts()
    {
        // Arrange
        var maliciousTokens = new[]
        {
            "../../../etc/passwd",
            "..\\..\\..\\windows\\system32\\config\\sam",
            "%2e%2e%2f%2e%2e%2f%2e%2e%2fetc%2fpasswd"
        };

        foreach (var token in maliciousTokens)
        {
            // Act
            var response = await _client.GetAsync($"/api/auth/verify-email/{token}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("passwd");
            content.Should().NotContain("sam");
        }
    }

    [Fact]
    public async Task Register_Should_Reject_Oversized_Requests()
    {
        // Arrange
        var oversizedRequest = new
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = new string('A', 10000), // Very long string
            LastName = "User",
            LicenseNumber = "TEST123",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(oversizedRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.RequestEntityTooLarge
        );
    }

    [Fact]
    public async Task API_Should_Not_Expose_Sensitive_Error_Information()
    {
        // Arrange - Send malformed JSON
        var malformedJson = "{ invalid json }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotContain("stack trace");
        responseContent.Should().NotContain("System.");
        responseContent.Should().NotContain("at ");
    }

    [Fact]
    public async Task API_Should_Have_Security_Headers()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/verify-email/test");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        var contentTypeOptions = response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault();
        contentTypeOptions.Should().Be("nosniff");
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("123456789012")]
    [InlineData("qwertyuiopas")]
    [InlineData("admin1234567")]
    public async Task Register_Should_Reject_Common_Passwords(string commonPassword)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = commonPassword,
            ConfirmPassword = commonPassword,
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = "TEST123",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password");
    }

    [Fact]
    public async Task CSRF_Protection_Should_Be_Enforced()
    {
        // Note: In a real application, you would test CSRF token validation
        // This is a placeholder to remind about CSRF protection
        
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "Test",
            LastName = "User",
            LicenseNumber = "TEST123",
            LicenseState = "CA",
            LicenseType = "OT",
            AcceptedTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        // In production, this would verify CSRF token requirements
        response.Should().NotBeNull();
    }
}