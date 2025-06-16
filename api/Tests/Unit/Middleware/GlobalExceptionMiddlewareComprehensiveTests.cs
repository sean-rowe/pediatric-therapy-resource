using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using TherapyDocs.Api.Middleware;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Middleware;

public class GlobalExceptionMiddlewareComprehensiveTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly GlobalExceptionMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionMiddlewareComprehensiveTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false); // Default to production
        
        _middleware = new GlobalExceptionMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
            
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    /**
     * Feature: Global Exception Handling
     *   As an API
     *   I want to handle all unhandled exceptions gracefully
     *   So that users receive consistent error responses
     * 
     * Scenario: Normal request flow without exceptions
     *   Given a request that doesn't throw an exception
     *   When the middleware processes the request
     *   Then the next middleware is called
     *   And no exception handling occurs
     */
    [Fact]
    public async Task InvokeAsync_NoException_CallsNextMiddleware()
    {
        // Arrange
        _mockNext.Setup(x => x(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(x => x(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200); // Default status
    }

    /**
     * Scenario: NotImplementedException handling
     *   Given a NotImplementedException is thrown
     *   When the middleware catches the exception
     *   Then a 501 Not Implemented response is returned
     *   And appropriate error details are included
     */
    [Fact]
    public async Task InvokeAsync_NotImplementedException_Returns501()
    {
        // Arrange
        var exception = new NotImplementedException("Feature not implemented");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotImplemented);
        _httpContext.Response.ContentType.Should().Be("application/json");
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("This feature is not yet implemented");
        errorResponse.ErrorCode.Should().Be("NOT_IMPLEMENTED");
        errorResponse.CorrelationId.Should().Be(_httpContext.TraceIdentifier);
        
        // Verify logging
        VerifyErrorLogged(exception);
    }

    /**
     * Scenario: UnauthorizedAccessException handling
     *   Given an UnauthorizedAccessException is thrown
     *   When the middleware catches the exception
     *   Then a 401 Unauthorized response is returned
     */
    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("You are not authorized to access this resource");
        errorResponse.ErrorCode.Should().Be("UNAUTHORIZED");
        
        VerifyErrorLogged(exception);
    }

    /**
     * Scenario: ArgumentNullException handling
     *   Given an ArgumentNullException is thrown
     *   When the middleware catches the exception
     *   Then a 400 Bad Request response is returned
     */
    [Fact]
    public async Task InvokeAsync_ArgumentNullException_Returns400()
    {
        // Arrange
        var exception = new ArgumentNullException("param", "Parameter cannot be null");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Invalid request parameters");
        errorResponse.ErrorCode.Should().Be("INVALID_REQUEST");
        
        VerifyErrorLogged(exception);
    }

    /**
     * Scenario: ArgumentException handling
     *   Given an ArgumentException is thrown
     *   When the middleware catches the exception
     *   Then a 400 Bad Request response is returned
     */
    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument value");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("Invalid request parameters");
        errorResponse.ErrorCode.Should().Be("INVALID_REQUEST");
        
        VerifyErrorLogged(exception);
    }

    /**
     * Scenario: TimeoutException handling
     *   Given a TimeoutException is thrown
     *   When the middleware catches the exception
     *   Then a 408 Request Timeout response is returned
     */
    [Fact]
    public async Task InvokeAsync_TimeoutException_Returns408()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.RequestTimeout);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("The request timed out");
        errorResponse.ErrorCode.Should().Be("REQUEST_TIMEOUT");
        
        VerifyErrorLogged(exception);
    }

    /**
     * Scenario: Generic exception handling
     *   Given a generic Exception is thrown
     *   When the middleware catches the exception
     *   Then a 500 Internal Server Error response is returned
     */
    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var exception = new Exception("Something went wrong");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Be("An error occurred while processing your request");
        errorResponse.ErrorCode.Should().Be("INTERNAL_ERROR");
        
        VerifyErrorLogged(exception);
    }

    /**
     * Feature: Development vs Production Environment Handling
     *   As a developer
     *   I want detailed error information in development
     *   But secure error responses in production
     * 
     * Scenario: Exception in development environment
     *   Given the environment is development
     *   When an exception is thrown
     *   Then detailed error information is included in the response
     */
    [Fact]
    public async Task InvokeAsync_DevelopmentEnvironment_IncludesDetails()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var exception = new Exception("Detailed error message");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Details.Should().Be(exception.Message);
        errorResponse.StackTrace.Should().NotBeNullOrEmpty();
    }

    /**
     * Scenario: Exception in production environment
     *   Given the environment is production
     *   When an exception is thrown
     *   Then detailed error information is excluded from the response
     */
    [Fact]
    public async Task InvokeAsync_ProductionEnvironment_ExcludesDetails()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);
        var exception = new Exception("Sensitive error details");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Details.Should().BeNull();
        errorResponse.StackTrace.Should().BeNull();
    }

    /**
     * Scenario: Correlation ID is always included
     *   Given any exception is thrown
     *   When the middleware handles the exception
     *   Then the correlation ID from the request is included in the response
     */
    [Fact]
    public async Task InvokeAsync_AnyException_IncludesCorrelationId()
    {
        // Arrange
        var customTraceId = "custom-trace-123";
        _httpContext.TraceIdentifier = customTraceId;
        var exception = new Exception("Test exception");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.CorrelationId.Should().Be(customTraceId);
    }

    /**
     * Feature: JSON Response Format
     *   As a client application
     *   I want consistent JSON error responses
     *   So that I can handle errors predictably
     * 
     * Scenario: Response is valid JSON
     *   Given an exception occurs
     *   When the middleware handles it
     *   Then the response is valid JSON with camelCase properties
     */
    [Fact]
    public async Task InvokeAsync_Exception_ReturnsValidJson()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockNext.Setup(x => x(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.ContentType.Should().Be("application/json");
        
        var responseBody = await GetResponseBodyAsync();
        responseBody.Should().NotBeNullOrEmpty();
        
        // Should deserialize without exception
        var act = () => JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        act.Should().NotThrow();
        
        // Check camelCase formatting
        responseBody.Should().Contain("errorCode");
        responseBody.Should().Contain("correlationId");
    }

    /**
     * Feature: Constructor Validation
     *   As a middleware
     *   I want to validate my dependencies
     *   So that I fail fast if misconfigured
     * 
     * Scenario: Constructor with valid parameters
     *   Given all required dependencies
     *   When creating the middleware
     *   Then no exception is thrown
     */
    [Fact]
    public void Constructor_ValidParameters_CreatesMiddleware()
    {
        // Act & Assert
        var act = () => new GlobalExceptionMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);
            
        act.Should().NotThrow();
    }

    /**
     * Scenario: Constructor with null next delegate
     *   Given a null next delegate
     *   When creating the middleware
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GlobalExceptionMiddleware(
            null!,
            _mockLogger.Object,
            _mockEnvironment.Object));
    }

    /**
     * Scenario: Constructor with null logger
     *   Given a null logger
     *   When creating the middleware
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GlobalExceptionMiddleware(
            _mockNext.Object,
            null!,
            _mockEnvironment.Object));
    }

    /**
     * Scenario: Constructor with null environment
     *   Given a null environment
     *   When creating the middleware
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public void Constructor_NullEnvironment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GlobalExceptionMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            null!));
    }

    /**
     * Feature: ErrorResponse Class
     *   As an error response
     *   I want to have proper default values
     *   So that serialization works correctly
     * 
     * Scenario: Default ErrorResponse properties
     *   Given a new ErrorResponse instance
     *   When checking default values
     *   Then all required properties have appropriate defaults
     */
    [Fact]
    public void ErrorResponse_DefaultConstructor_HasCorrectDefaults()
    {
        // Act
        var errorResponse = new ErrorResponse();

        // Assert
        errorResponse.Message.Should().Be(string.Empty);
        errorResponse.ErrorCode.Should().Be(string.Empty);
        errorResponse.CorrelationId.Should().Be(string.Empty);
        errorResponse.Details.Should().BeNull();
        errorResponse.StackTrace.Should().BeNull();
    }

    /**
     * Scenario: ErrorResponse property setters
     *   Given an ErrorResponse instance
     *   When setting properties
     *   Then values are stored correctly
     */
    [Fact]
    public void ErrorResponse_PropertySetters_WorkCorrectly()
    {
        // Arrange
        var errorResponse = new ErrorResponse();
        var message = "Test message";
        var errorCode = "TEST_ERROR";
        var correlationId = "test-123";
        var details = "Test details";
        var stackTrace = "Test stack trace";

        // Act
        errorResponse.Message = message;
        errorResponse.ErrorCode = errorCode;
        errorResponse.CorrelationId = correlationId;
        errorResponse.Details = details;
        errorResponse.StackTrace = stackTrace;

        // Assert
        errorResponse.Message.Should().Be(message);
        errorResponse.ErrorCode.Should().Be(errorCode);
        errorResponse.CorrelationId.Should().Be(correlationId);
        errorResponse.Details.Should().Be(details);
        errorResponse.StackTrace.Should().Be(stackTrace);
    }

    // Helper methods
    private async Task<string> GetResponseBodyAsync()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private void VerifyErrorLogged(Exception exception)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}