using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Filters;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Filters;

public class ValidationExceptionFilterTests
{
    private readonly Mock<ILogger<ValidationExceptionFilter>> _mockLogger;
    private readonly ValidationExceptionFilter _filter;

    public ValidationExceptionFilterTests()
    {
        _mockLogger = new Mock<ILogger<ValidationExceptionFilter>>();
        _filter = new ValidationExceptionFilter(_mockLogger.Object);
    }

    [Fact]
    public void OnException_WithValidationException_ShouldReturnBadRequestResult()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Password", "Password must be at least 8 characters")
        };
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.True(exceptionContext.ExceptionHandled);
        Assert.IsType<BadRequestObjectResult>(exceptionContext.Result);

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result;
        var response = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Validation failed", response.Message);
        Assert.Equal(2, response.Errors.Count);
        
        var emailError = response.Errors.First(e => e.Property == "Email");
        Assert.Equal("Email is required", emailError.Message);
        
        var passwordError = response.Errors.First(e => e.Property == "Password");
        Assert.Equal("Password must be at least 8 characters", passwordError.Message);
    }

    [Fact]
    public void OnException_WithValidationException_ShouldLogWarning()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "Email is required")
        };
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation error occurred: 1 errors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnException_WithNonValidationException_ShouldNotHandleException()
    {
        // Arrange
        var generalException = new InvalidOperationException("General error");

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = generalException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.False(exceptionContext.ExceptionHandled);
        Assert.Null(exceptionContext.Result);
    }

    [Fact]
    public void OnException_WithEmptyValidationException_ShouldReturnEmptyErrorsList()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>();
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.True(exceptionContext.ExceptionHandled);
        Assert.IsType<BadRequestObjectResult>(exceptionContext.Result);

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result;
        var response = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Validation failed", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void OnException_WithMultipleValidationFailuresForSameProperty_ShouldIncludeAll()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email format is invalid")
        };
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.True(exceptionContext.ExceptionHandled);
        Assert.IsType<BadRequestObjectResult>(exceptionContext.Result);

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result;
        var response = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.Equal("Validation failed", response.Message);
        Assert.Equal(2, response.Errors.Count);
        
        var emailErrors = response.Errors.Where(e => e.Property == "Email").ToList();
        Assert.Equal(2, emailErrors.Count);
        Assert.Contains(emailErrors, e => e.Message == "Email is required");
        Assert.Contains(emailErrors, e => e.Message == "Email format is invalid");
    }

    [Fact]
    public void OnException_WithValidationExceptionHavingSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Special.Property", "Error with special characters: <>&\"'")
        };
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.True(exceptionContext.ExceptionHandled);
        Assert.IsType<BadRequestObjectResult>(exceptionContext.Result);

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result;
        var response = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.Single(response.Errors);
        Assert.Equal("Special.Property", response.Errors[0].Property);
        Assert.Equal("Error with special characters: <>&\"'", response.Errors[0].Message);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Act
        var filter = new ValidationExceptionFilter(_mockLogger.Object);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ValidationExceptionFilter(null!));
    }

    [Fact]
    public void ValidationErrorResponse_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var response = new ValidationErrorResponse();

        // Assert
        Assert.Equal(string.Empty, response.Message);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void ValidationError_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var error = new ValidationError();

        // Assert
        Assert.Equal(string.Empty, error.Property);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void ValidationErrorResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var errors = new List<ValidationError>
        {
            new() { Property = "Test", Message = "Test message" }
        };

        // Act
        var response = new ValidationErrorResponse
        {
            Message = "Test validation failed",
            Errors = errors
        };

        // Assert
        Assert.Equal("Test validation failed", response.Message);
        Assert.Equal(errors, response.Errors);
    }

    [Fact]
    public void ValidationError_Properties_ShouldBeSettable()
    {
        // Act
        var error = new ValidationError
        {
            Property = "TestProperty",
            Message = "Test error message"
        };

        // Assert
        Assert.Equal("TestProperty", error.Property);
        Assert.Equal("Test error message", error.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void OnException_WithEmptyPropertyNames_ShouldHandleCorrectly(string? propertyName)
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new(propertyName, "Error message")
        };
        var validationException = new ValidationException(validationFailures);

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = validationException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        Assert.True(exceptionContext.ExceptionHandled);
        Assert.IsType<BadRequestObjectResult>(exceptionContext.Result);

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result;
        var response = Assert.IsType<ValidationErrorResponse>(badRequestResult.Value);
        
        Assert.Single(response.Errors);
        Assert.Equal(propertyName ?? string.Empty, response.Errors[0].Property);
        Assert.Equal("Error message", response.Errors[0].Message);
    }
}