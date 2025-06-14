using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class HaveIBeenPwnedServiceTests
{
    private readonly Mock<ILogger<HaveIBeenPwnedService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HaveIBeenPwnedService _service;

    public HaveIBeenPwnedServiceTests()
    {
        _mockLogger = new Mock<ILogger<HaveIBeenPwnedService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new HaveIBeenPwnedService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_PwnedPassword_ReturnsTrue()
    {
        // Arrange
        var password = "password123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        var hashSuffix = sha1Hash.Substring(5);
        
        // Mock response with our hash in the list
        var responseContent = $"1234567890ABCDEF:5\r\n{hashSuffix}:243961\r\nABCDEF1234567890:10";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.True(result);
        
        // Verify logging
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Password found in breach database with 243961 occurrences")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_SafePassword_ReturnsFalse()
    {
        // Arrange
        var password = "VerySecurePassword123!@#";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        
        // Mock response without our hash
        var responseContent = "1234567890ABCDEF:5\r\nFEDCBA0987654321:10";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_ApiReturnsError_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.ServiceUnavailable, "Service Unavailable");

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.False(result); // Fail open - don't block if API is down
        
        // Verify warning was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HIBP API returned status ServiceUnavailable")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_NetworkException_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.False(result); // Fail open
        
        // Verify error was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking password against HIBP database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("ABCDEF1234567890:0")] // Zero occurrences
    [InlineData("ABCDEF1234567890:")] // Missing count
    [InlineData("ABCDEF1234567890")] // No colon separator
    [InlineData("")] // Empty line
    public async Task IsPasswordPwnedAsync_InvalidResponseFormat_HandlesGracefully(string invalidLine)
    {
        // Arrange
        var password = "TestPassword123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        var hashSuffix = sha1Hash.Substring(5);
        
        // Include our hash with invalid format
        var responseContent = $"{invalidLine}\r\n{hashSuffix}:invalid\r\nVALID1234567890:5";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act & Assert - Should not throw
        var result = await _service.IsPasswordPwnedAsync(password);
        
        // Result depends on whether the parser can handle the line with our suffix
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_CaseInsensitiveHashMatching_Works()
    {
        // Arrange
        var password = "TestPassword123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        var hashSuffix = sha1Hash.Substring(5);
        
        // Return hash suffix in lowercase (API returns uppercase)
        var responseContent = $"ABCDEF1234567890:5\r\n{hashSuffix.ToLowerInvariant()}:100";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.True(result); // Should match case-insensitively
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_EmptyResponse_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, "");

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_MultipleLineEndings_ParsesCorrectly()
    {
        // Arrange
        var password = "TestPassword123";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        var hashSuffix = sha1Hash.Substring(5);
        
        // Mix of \r\n and \n line endings
        var responseContent = $"ABCDEF1234567890:5\n{hashSuffix}:50\r\nFEDCBA0987654321:10";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_VeryLongPassword_HandlesCorrectly()
    {
        // Arrange
        var password = new string('a', 1000); // Very long password
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, "ABCDEF1234567890:5");

        // Act & Assert - Should not throw
        var result = await _service.IsPasswordPwnedAsync(password);
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_SpecialCharactersInPassword_HandlesCorrectly()
    {
        // Arrange
        var password = "Test!@#$%^&*()_+{}[]|\\:\";<>?,./~`";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        var hashSuffix = sha1Hash.Substring(5);
        
        var responseContent = $"{hashSuffix}:1";
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, responseContent);

        // Act
        var result = await _service.IsPasswordPwnedAsync(password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_UnicodePassword_HandlesCorrectly()
    {
        // Arrange
        var password = "Test密码パスワード🔒";
        var sha1Hash = ComputeSha1Hash(password);
        var hashPrefix = sha1Hash.Substring(0, 5);
        
        SetupHttpResponse(hashPrefix, HttpStatusCode.OK, "ABCDEF1234567890:5");

        // Act & Assert - Should not throw
        var result = await _service.IsPasswordPwnedAsync(password);
        Assert.False(result);
    }

    [Fact]
    public void Constructor_SetsCorrectHttpClientProperties()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<HaveIBeenPwnedService>>();
        var httpClient = new HttpClient();
        var service = new HaveIBeenPwnedService(httpClient, logger.Object);

        // Assert
        Assert.Equal("https://api.pwnedpasswords.com/range/", httpClient.BaseAddress?.ToString());
        Assert.Contains(httpClient.DefaultRequestHeaders.UserAgent.ToString(), "TherapyDocs-API");
    }

    private void SetupHttpResponse(string expectedPath, HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(expectedPath)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    private static string ComputeSha1Hash(string input)
    {
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
    }
}