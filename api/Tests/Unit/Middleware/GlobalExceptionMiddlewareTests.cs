using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Middleware;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly GlobalExceptionMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<RequestDelegate> _mockNext;

    public GlobalExceptionMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockNext = new Mock<RequestDelegate>();
        
        _middleware = new GlobalExceptionMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        _httpContext.TraceIdentifier = "test-trace-id";
    }

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        // Arrange
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(x => x(_httpContext), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ThrowsException_HandlesAndLogsError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Equal("application/json", _httpContext.Response.ContentType);
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
    }

    #endregion

    #region Exception Type Handling Tests

    [Fact]
    public async Task HandleExceptionAsync_NotImplementedException_Returns501()
    {
        // Arrange
        var exception = new NotImplementedException("Feature not implemented");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotImplemented, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("This feature is not yet implemented", response.Message);
        Assert.Equal("NOT_IMPLEMENTED", response.ErrorCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("You are not authorized to access this resource", response.Message);
        Assert.Equal("UNAUTHORIZED", response.ErrorCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_ArgumentException_Returns400()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("Invalid request parameters", response.Message);
        Assert.Equal("INVALID_REQUEST", response.ErrorCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_ArgumentNullException_Returns400()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("Invalid request parameters", response.Message);
        Assert.Equal("INVALID_REQUEST", response.ErrorCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_TimeoutException_Returns408()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.RequestTimeout, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("The request timed out", response.Message);
        Assert.Equal("REQUEST_TIMEOUT", response.ErrorCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_GenericException_Returns500()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        
        var response = await GetResponseBody();
        Assert.Equal("An error occurred while processing your request", response.Message);
        Assert.Equal("INTERNAL_ERROR", response.ErrorCode);
    }

    #endregion

    #region Environment-Specific Tests

    [Fact]
    public async Task HandleExceptionAsync_DevelopmentEnvironment_IncludesDetails()
    {
        // Arrange
        var exception = new Exception("Detailed error message");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var response = await GetResponseBody();
        Assert.NotNull(response.Details);
        Assert.Equal("Detailed error message", response.Details);
        Assert.NotNull(response.StackTrace);
    }

    [Fact]
    public async Task HandleExceptionAsync_ProductionEnvironment_ExcludesDetails()
    {
        // Arrange
        var exception = new Exception("Sensitive error message");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var response = await GetResponseBody();
        Assert.Null(response.Details);
        Assert.Null(response.StackTrace);
    }

    #endregion

    #region Correlation ID Tests

    [Fact]
    public async Task HandleExceptionAsync_IncludesCorrelationId()
    {
        // Arrange
        var exception = new Exception("Test error");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var response = await GetResponseBody();
        Assert.Equal("test-trace-id", response.CorrelationId);
    }

    [Fact]
    public async Task HandleExceptionAsync_NoTraceIdentifier_HandlesGracefully()
    {
        // Arrange
        _httpContext.TraceIdentifier = null!;
        var exception = new Exception("Test error");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var response = await GetResponseBody();
        Assert.Null(response.CorrelationId);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task HandleExceptionAsync_NestedExceptions_HandlesCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");
        var outerException = new Exception("Outer exception", innerException);
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(outerException);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var response = await GetResponseBody();
        Assert.Equal("Outer exception", response.Details);
        Assert.Contains("Inner exception", response.StackTrace!);
    }

    [Fact]
    public async Task HandleExceptionAsync_ResponseAlreadyStarted_HandlesGracefully()
    {
        // Arrange
        var exception = new Exception("Test error");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Callback(() =>
        {
            _httpContext.Response.StatusCode = 200;
            // _httpContext.Response.HasStarted = true; // This property is read-only
        }).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act & Assert
        // Should not throw, but won't be able to modify response
        await _middleware.InvokeAsync(_httpContext);
    }

    [Fact]
    public async Task HandleExceptionAsync_JsonSerializationCamelCase()
    {
        // Arrange
        var exception = new Exception("Test error");
        _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(exception);
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(_httpContext.Response.Body);
        var jsonResponse = await reader.ReadToEndAsync();
        
        // Verify camelCase property names
        Assert.Contains("\"message\":", jsonResponse);
        Assert.Contains("\"errorCode\":", jsonResponse);
        Assert.Contains("\"correlationId\":", jsonResponse);
        Assert.DoesNotContain("\"Message\":", jsonResponse);
        Assert.DoesNotContain("\"ErrorCode\":", jsonResponse);
        Assert.DoesNotContain("\"CorrelationId\":", jsonResponse);
    }

    #endregion

    #region ErrorResponse Model Tests

    [Fact]
    public void ErrorResponse_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var errorResponse = new ErrorResponse();

        // Assert
        Assert.Equal(string.Empty, errorResponse.Message);
        Assert.Equal(string.Empty, errorResponse.ErrorCode);
        Assert.Equal(string.Empty, errorResponse.CorrelationId);
        Assert.Null(errorResponse.Details);
        Assert.Null(errorResponse.StackTrace);
    }

    [Fact]
    public void ErrorResponse_AllPropertiesSet_SerializesCorrectly()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = "Test message",
            ErrorCode = "TEST_ERROR",
            CorrelationId = "12345",
            Details = "Detailed error",
            StackTrace = "Stack trace here"
        };

        // Act
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.Equal("Test message", deserialized!["message"].GetString());
        Assert.Equal("TEST_ERROR", deserialized["errorCode"].GetString());
        Assert.Equal("12345", deserialized["correlationId"].GetString());
        Assert.Equal("Detailed error", deserialized["details"].GetString());
        Assert.Equal("Stack trace here", deserialized["stackTrace"].GetString());
    }

    #endregion

    #region Helper Methods

    private async Task<ErrorResponse> GetResponseBody()
    {
        _httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(_httpContext.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })!;
    }

    #endregion
}