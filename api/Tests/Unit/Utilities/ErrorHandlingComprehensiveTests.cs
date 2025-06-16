using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Utilities;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Utilities;

public class ErrorHandlingComprehensiveTests
{
    private readonly Mock<ILogger> _mockLogger;

    public ErrorHandlingComprehensiveTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    /**
     * Feature: Generic Error Handling
     *   As a utility service
     *   I want to handle exceptions consistently
     *   So that error management is standardized
     * 
     * Scenario: Successful operation execution
     *   Given a successful operation
     *   When executing with error handling
     *   Then the operation result is returned
     */
    [Fact]
    public async Task HandleAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedResult = "success";
        var operation = new Func<Task<string>>(() => Task.FromResult(expectedResult));
        var fallbackResult = "fallback";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, "TestOperation", fallbackResult);

        // Assert
        result.Should().Be(expectedResult);
    }

    /**
     * Scenario: Operation throws ArgumentException
     *   Given an operation that throws ArgumentException
     *   When executing with error handling
     *   Then the fallback result is returned and warning is logged
     */
    [Fact]
    public async Task HandleAsync_ArgumentException_ReturnsFallbackAndLogsWarning()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var operation = new Func<Task<string>>(() => throw exception);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Warning, operationName, exception);
    }

    /**
     * Scenario: Operation throws UnauthorizedAccessException
     *   Given an operation that throws UnauthorizedAccessException
     *   When executing with error handling
     *   Then the fallback result is returned and warning is logged
     */
    [Fact]
    public async Task HandleAsync_UnauthorizedAccessException_ReturnsFallbackAndLogsWarning()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        var operation = new Func<Task<string>>(() => throw exception);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Warning, operationName, exception);
    }

    /**
     * Scenario: Operation throws InvalidOperationException
     *   Given an operation that throws InvalidOperationException
     *   When executing with error handling
     *   Then the fallback result is returned and error is logged
     */
    [Fact]
    public async Task HandleAsync_InvalidOperationException_ReturnsFallbackAndLogsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");
        var operation = new Func<Task<string>>(() => throw exception);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Error, operationName, exception);
    }

    /**
     * Scenario: Operation throws generic Exception
     *   Given an operation that throws a generic Exception
     *   When executing with error handling
     *   Then the fallback result is returned and error is logged
     */
    [Fact]
    public async Task HandleAsync_GenericException_ReturnsFallbackAndLogsError()
    {
        // Arrange
        var exception = new Exception("Generic error");
        var operation = new Func<Task<string>>(() => throw exception);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Error, operationName, exception);
    }

    /**
     * Scenario: Operation with log parameters
     *   Given an operation with additional log parameters
     *   When executing with error handling
     *   Then the parameters are included in logging
     */
    [Fact]
    public async Task HandleAsync_WithLogParameters_IncludesParametersInLog()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var operation = new Func<Task<string>>(() => throw exception);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";
        var logParameters = new { UserId = 123, Action = "TestAction" };

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult, logParameters);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Warning, operationName, exception);
    }

    /**
     * Feature: Boolean Error Handling
     *   As a utility service
     *   I want to handle boolean operations consistently
     *   So that success/failure operations are standardized
     * 
     * Scenario: Successful boolean operation
     *   Given a successful boolean operation
     *   When executing with boolean error handling
     *   Then the operation result is returned
     */
    [Fact]
    public async Task HandleBooleanAsync_SuccessfulOperation_ReturnsTrue()
    {
        // Arrange
        var operation = new Func<Task<bool>>(() => Task.FromResult(true));
        var operationName = "TestBooleanOperation";

        // Act
        var result = await ErrorHandling.HandleBooleanAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeTrue();
    }

    /**
     * Scenario: Failed boolean operation
     *   Given a boolean operation that returns false
     *   When executing with boolean error handling
     *   Then false is returned
     */
    [Fact]
    public async Task HandleBooleanAsync_FailedOperation_ReturnsFalse()
    {
        // Arrange
        var operation = new Func<Task<bool>>(() => Task.FromResult(false));
        var operationName = "TestBooleanOperation";

        // Act
        var result = await ErrorHandling.HandleBooleanAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
    }

    /**
     * Scenario: Boolean operation throws exception
     *   Given a boolean operation that throws an exception
     *   When executing with boolean error handling
     *   Then false is returned and exception is logged
     */
    [Fact]
    public async Task HandleBooleanAsync_ExceptionThrown_ReturnsFalseAndLogs()
    {
        // Arrange
        var exception = new InvalidOperationException("Operation failed");
        var operation = new Func<Task<bool>>(() => throw exception);
        var operationName = "TestBooleanOperation";

        // Act
        var result = await ErrorHandling.HandleBooleanAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
        VerifyLogCalled(LogLevel.Error, operationName, exception);
    }

    /**
     * Feature: Void Operation Error Handling
     *   As a utility service
     *   I want to handle void operations consistently
     *   So that operations without return values are standardized
     * 
     * Scenario: Successful void operation
     *   Given a successful void operation
     *   When executing with void error handling
     *   Then true is returned
     */
    [Fact]
    public async Task HandleVoidAsync_SuccessfulOperation_ReturnsTrue()
    {
        // Arrange
        var operation = new Func<Task>(() => Task.CompletedTask);
        var operationName = "TestVoidOperation";

        // Act
        var result = await ErrorHandling.HandleVoidAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeTrue();
    }

    /**
     * Scenario: Void operation throws ArgumentException
     *   Given a void operation that throws ArgumentException
     *   When executing with void error handling
     *   Then false is returned and warning is logged
     */
    [Fact]
    public async Task HandleVoidAsync_ArgumentException_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var operation = new Func<Task>(() => throw exception);
        var operationName = "TestVoidOperation";

        // Act
        var result = await ErrorHandling.HandleVoidAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
        VerifyLogCalled(LogLevel.Warning, operationName, exception);
    }

    /**
     * Scenario: Void operation throws UnauthorizedAccessException
     *   Given a void operation that throws UnauthorizedAccessException
     *   When executing with void error handling
     *   Then false is returned and warning is logged
     */
    [Fact]
    public async Task HandleVoidAsync_UnauthorizedAccessException_ReturnsFalseAndLogsWarning()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        var operation = new Func<Task>(() => throw exception);
        var operationName = "TestVoidOperation";

        // Act
        var result = await ErrorHandling.HandleVoidAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
        VerifyLogCalled(LogLevel.Warning, operationName, exception);
    }

    /**
     * Scenario: Void operation throws InvalidOperationException
     *   Given a void operation that throws InvalidOperationException
     *   When executing with void error handling
     *   Then false is returned and error is logged
     */
    [Fact]
    public async Task HandleVoidAsync_InvalidOperationException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");
        var operation = new Func<Task>(() => throw exception);
        var operationName = "TestVoidOperation";

        // Act
        var result = await ErrorHandling.HandleVoidAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
        VerifyLogCalled(LogLevel.Error, operationName, exception);
    }

    /**
     * Scenario: Void operation throws generic Exception
     *   Given a void operation that throws a generic Exception
     *   When executing with void error handling
     *   Then false is returned and error is logged
     */
    [Fact]
    public async Task HandleVoidAsync_GenericException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var exception = new Exception("Generic error");
        var operation = new Func<Task>(() => throw exception);
        var operationName = "TestVoidOperation";

        // Act
        var result = await ErrorHandling.HandleVoidAsync(_mockLogger.Object, operation, operationName);

        // Assert
        result.Should().BeFalse();
        VerifyLogCalled(LogLevel.Error, operationName, exception);
    }

    /**
     * Feature: String Validation
     *   As a validation utility
     *   I want to validate required string parameters
     *   So that null/empty values are caught early
     * 
     * Scenario: Valid string parameter
     *   Given a valid non-empty string
     *   When validating the required string
     *   Then no exception is thrown
     */
    [Theory]
    [InlineData("valid-string")]
    [InlineData("a")]
    [InlineData("  valid  ")]
    public void ValidateRequiredString_ValidString_DoesNotThrow(string value)
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateRequiredString(value, "testParam");
        act.Should().NotThrow();
    }

    /**
     * Scenario: Invalid string parameters
     *   Given null, empty, or whitespace strings
     *   When validating the required string
     *   Then ArgumentException is thrown
     */
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   ")]
    public void ValidateRequiredString_InvalidString_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateRequiredString(value, "testParam");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Parameter 'testParam' cannot be null or empty. (Parameter 'testParam')");
    }

    /**
     * Feature: Email Validation
     *   As a validation utility
     *   I want to validate email addresses
     *   So that only valid emails are accepted
     * 
     * Scenario: Valid email addresses
     *   Given valid email formats
     *   When validating the email
     *   Then no exception is thrown
     */
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.gov")]
    public void ValidateEmail_ValidEmail_DoesNotThrow(string email)
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateEmail(email);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Invalid email addresses
     *   Given invalid email formats
     *   When validating the email
     *   Then ArgumentException is thrown
     */
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user..name@domain.com")]
    public void ValidateEmail_InvalidEmail_ThrowsArgumentException(string? email)
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateEmail(email);
        act.Should().Throw<ArgumentException>();
    }

    /**
     * Scenario: Email validation with custom parameter name
     *   Given an invalid email with custom parameter name
     *   When validating the email
     *   Then ArgumentException includes the custom parameter name
     */
    [Fact]
    public void ValidateEmail_InvalidEmailWithCustomParam_ThrowsWithCustomParamName()
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateEmail("invalid", "customEmail");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Parameter 'customEmail' must be a valid email address. (Parameter 'customEmail')");
    }

    /**
     * Feature: GUID Validation
     *   As a validation utility
     *   I want to validate GUID parameters
     *   So that empty GUIDs are caught early
     * 
     * Scenario: Valid GUID
     *   Given a valid non-empty GUID
     *   When validating the GUID
     *   Then no exception is thrown
     */
    [Fact]
    public void ValidateGuid_ValidGuid_DoesNotThrow()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act & Assert
        var act = () => ErrorHandling.ValidateGuid(validGuid, "testGuid");
        act.Should().NotThrow();
    }

    /**
     * Scenario: Empty GUID
     *   Given an empty GUID
     *   When validating the GUID
     *   Then ArgumentException is thrown
     */
    [Fact]
    public void ValidateGuid_EmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ErrorHandling.ValidateGuid(Guid.Empty, "testGuid");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Parameter 'testGuid' cannot be empty. (Parameter 'testGuid')");
    }

    /**
     * Feature: Edge Cases and Complex Scenarios
     *   As a robust utility
     *   I want to handle edge cases gracefully
     *   So that the system remains stable
     * 
     * Scenario: Nested exceptions
     *   Given an operation that throws nested exceptions
     *   When executing with error handling
     *   Then the outer exception is properly handled
     */
    [Fact]
    public async Task HandleAsync_NestedException_HandlesOuterException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner exception");
        var outerException = new InvalidOperationException("Outer exception", innerException);
        var operation = new Func<Task<string>>(() => throw outerException);
        var fallbackResult = "fallback";
        var operationName = "TestOperation";

        // Act
        var result = await ErrorHandling.HandleAsync(_mockLogger.Object, operation, operationName, fallbackResult);

        // Assert
        result.Should().Be(fallbackResult);
        VerifyLogCalled(LogLevel.Error, operationName, outerException);
    }

    /**
     * Scenario: Operation with different return types
     *   Given operations returning different types
     *   When executing with error handling
     *   Then type-specific fallback values are returned
     */
    [Fact]
    public async Task HandleAsync_DifferentReturnTypes_HandlesCorrectly()
    {
        // Arrange
        var intOperation = new Func<Task<int>>(() => throw new Exception());
        var stringOperation = new Func<Task<string>>(() => throw new Exception());
        var boolOperation = new Func<Task<bool>>(() => throw new Exception());

        // Act
        var intResult = await ErrorHandling.HandleAsync(_mockLogger.Object, intOperation, "IntOp", -1);
        var stringResult = await ErrorHandling.HandleAsync(_mockLogger.Object, stringOperation, "StringOp", "default");
        var boolResult = await ErrorHandling.HandleAsync(_mockLogger.Object, boolOperation, "BoolOp", true);

        // Assert
        intResult.Should().Be(-1);
        stringResult.Should().Be("default");
        boolResult.Should().BeTrue();
    }

    // Helper method to verify logging was called
    private void VerifyLogCalled(LogLevel level, string operationName, Exception exception)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(operationName)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}