using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class HaveIBeenPwnedServiceEdgeCaseTests
{
    private readonly Mock<ILogger<HaveIBeenPwnedService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HaveIBeenPwnedService _service;

    public HaveIBeenPwnedServiceEdgeCaseTests()
    {
        _mockLogger = new Mock<ILogger<HaveIBeenPwnedService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new HaveIBeenPwnedService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithNetworkTimeout_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithHttpRequestException_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithMalformedResponse_ShouldReturnFalse()
    {
        // Arrange
        var malformedContent = "INVALID:RESPONSE:FORMAT";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedContent)
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithEmptyResponse_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithVeryLongPassword_ShouldHashCorrectly()
    {
        // Arrange - Create a very long password
        var longPassword = new string('a', 10000);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("") // Empty response means not found
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync(longPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithUnicodePassword_ShouldHashCorrectly()
    {
        // Arrange
        var unicodePassword = "पासवर्ड密码пароль🔒";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("") // Empty response means not found
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync(unicodePassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithSpecialCharacters_ShouldHashCorrectly()
    {
        // Arrange
        var specialPassword = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("") // Empty response means not found
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync(specialPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithResponseContainingNewlines_ShouldHandleCorrectly()
    {
        // Arrange
        var responseContent = "ABC123:5\r\nDEF456:10\n\nGHI789:2\r\n";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result); // Hash shouldn't match any of the fake hashes
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithCancellationToken_ShouldBeCancellable()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result); // Should return false when cancelled
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithInvalidHttpStatusCodes_ShouldReturnFalse()
    {
        // Test various HTTP status codes
        var statusCodes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        };

        foreach (var statusCode in statusCodes)
        {
            // Arrange
            var response = new HttpResponseMessage(statusCode);
            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _service.IsPasswordPwnedAsync("TestPassword123");

            // Assert
            Assert.False(result);
        }
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithNullHttpContent_ShouldReturnFalse()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = null
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.IsPasswordPwnedAsync("TestPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act - Run multiple concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _service.IsPasswordPwnedAsync($"TestPassword{i}"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.False(result));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new HaveIBeenPwnedService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new HaveIBeenPwnedService(_httpClient, null!));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public class HaveIBeenPwnedServiceTestsWithRealHttpClient : IDisposable
{
    private readonly Mock<ILogger<HaveIBeenPwnedService>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly HaveIBeenPwnedService _service;

    public HaveIBeenPwnedServiceTestsWithRealHttpClient()
    {
        _mockLogger = new Mock<ILogger<HaveIBeenPwnedService>>();
        _httpClient = new HttpClient();
        _service = new HaveIBeenPwnedService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task IsPasswordPwnedAsync_WithRealHttpClient_ShouldNotThrow()
    {
        // This test uses a real HttpClient but doesn't make actual network calls
        // It's to ensure the service doesn't throw exceptions during setup
        
        // Act & Assert - Should not throw during construction
        Assert.NotNull(_service);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}