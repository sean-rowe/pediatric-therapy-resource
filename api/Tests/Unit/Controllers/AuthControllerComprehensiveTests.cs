using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using TherapyDocs.Api.Controllers;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using TherapyDocs.Api.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Xunit;
using Microsoft.Extensions.Primitives;

namespace TherapyDocs.Api.Tests.Unit.Controllers;

public class AuthControllerComprehensiveTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly Mock<IValidator<RegisterRequest>> _mockValidator;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<ConnectionInfo> _mockConnection;
    private readonly Mock<IHeaderDictionary> _mockHeaders;

    public AuthControllerComprehensiveTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLoginService = new Mock<ILoginService>();
        _mockValidator = new Mock<IValidator<RegisterRequest>>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        
        _controller = new AuthController(
            _mockAuthService.Object,
            _mockLoginService.Object,
            _mockValidator.Object,
            _mockLogger.Object);

        // Setup HttpContext
        _mockHttpContext = new Mock<HttpContext>();
        _mockConnection = new Mock<ConnectionInfo>();
        _mockHeaders = new Mock<IHeaderDictionary>();
        
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Headers).Returns(_mockHeaders.Object);
        
        _mockConnection.Setup(x => x.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));
        _mockHttpContext.Setup(x => x.Connection).Returns(_mockConnection.Object);
        _mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        _mockHeaders.Setup(x => x.UserAgent).Returns(new StringValues("test-agent"));
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    #region Register Tests

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var validationResult = new ValidationResult();
        var registerResponse = new RegisterResponse { Success = true, Message = "Success" };

        _mockValidator.Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);
        _mockAuthService.Setup(x => x.RegisterUserAsync(request, "127.0.0.1", "test-agent"))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(registerResponse);
        
        // Verify logger was called
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User registered successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var validationResult = new ValidationResult(new[] 
        { 
            new ValidationFailure("Email", "Invalid email"),
            new ValidationFailure("Password", "Password too weak")
        });

        _mockValidator.Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as RegisterResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().Contain("Invalid email");
        response.Errors.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task Register_ServiceFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var validationResult = new ValidationResult();
        var registerResponse = new RegisterResponse 
        { 
            Success = false, 
            Message = "Registration failed",
            Errors = { "Email already exists" }
        };

        _mockValidator.Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);
        _mockAuthService.Setup(x => x.RegisterUserAsync(request, "127.0.0.1", "test-agent"))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        badResult!.Value.Should().Be(registerResponse);
    }

    [Fact]
    public async Task Register_NullIpAddress_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var validationResult = new ValidationResult();
        var registerResponse = new RegisterResponse { Success = true };

        _mockConnection.Setup(x => x.RemoteIpAddress).Returns((IPAddress?)null);
        _mockValidator.Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);
        _mockAuthService.Setup(x => x.RegisterUserAsync(request, null, "test-agent"))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_EmptyUserAgent_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var validationResult = new ValidationResult();
        var registerResponse = new RegisterResponse { Success = true };

        _mockHeaders.Setup(x => x.UserAgent).Returns(StringValues.Empty);
        _mockValidator.Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);
        _mockAuthService.Setup(x => x.RegisterUserAsync(request, "127.0.0.1", ""))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region VerifyEmail Tests

    [Fact]
    public async Task VerifyEmail_ValidToken_ReturnsOk()
    {
        // Arrange
        var token = "valid-token-123";
        _mockAuthService.Setup(x => x.VerifyEmailAsync(token))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyEmail(token);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        response.Should().BeEquivalentTo(new { success = true, message = "Email verified successfully! You can now log in." });
        
        // Verify logger
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email verified successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var token = "invalid-token";
        _mockAuthService.Setup(x => x.VerifyEmailAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.VerifyEmail(token);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Invalid or expired verification token" });
    }

    [Fact]
    public async Task VerifyEmail_NullToken_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.VerifyEmail(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Invalid verification token" });
    }

    [Fact]
    public async Task VerifyEmail_EmptyToken_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.VerifyEmail(string.Empty);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Invalid verification token" });
    }

    [Fact]
    public async Task VerifyEmail_WhitespaceToken_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.VerifyEmail("   ");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Invalid verification token" });
    }

    #endregion

    #region ResendVerification Tests

    [Fact]
    public async Task ResendVerification_ValidEmail_ReturnsOk()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = "test@example.com" };
        _mockAuthService.Setup(x => x.ResendVerificationEmailAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResendVerification(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value;
        response.Should().BeEquivalentTo(new { success = true, message = "Verification email sent successfully" });
        
        // Verify logger
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification email resent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerification_ServiceFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = "test@example.com" };
        _mockAuthService.Setup(x => x.ResendVerificationEmailAsync(request.Email))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ResendVerification(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Unable to resend verification email. Please check your email address." });
    }

    [Fact]
    public async Task ResendVerification_NullEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = null! };

        // Act
        var result = await _controller.ResendVerification(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Email is required" });
    }

    [Fact]
    public async Task ResendVerification_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = string.Empty };

        // Act
        var result = await _controller.ResendVerification(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Email is required" });
    }

    [Fact]
    public async Task ResendVerification_WhitespaceEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = "   " };

        // Act
        var result = await _controller.ResendVerification(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value;
        response.Should().BeEquivalentTo(new { success = false, message = "Email is required" });
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };
        var loginResponse = new LoginResponse
        {
            Success = true,
            Token = "jwt-token",
            Message = "Login successful"
        };

        _mockLoginService.Setup(x => x.LoginAsync(request, "127.0.0.1", "test-agent"))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(loginResponse);
        
        // Verify logger
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logged in successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong"
        };
        var loginResponse = new LoginResponse
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mockLoginService.Setup(x => x.LoginAsync(request, "127.0.0.1", "test-agent"))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        badResult!.Value.Should().Be(loginResponse);
    }

    [Fact]
    public async Task Login_NullEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = null!,
            Password = "password"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = "password"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_WhitespaceEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "   ",
            Password = "password"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_NullPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = null!
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = string.Empty
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_WhitespacePassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "   "
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        var response = badResult!.Value as LoginResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("Email and password are required");
    }

    [Fact]
    public async Task Login_NullIpAddress_HandlesGracefully()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        };
        var loginResponse = new LoginResponse { Success = true };

        _mockConnection.Setup(x => x.RemoteIpAddress).Returns((IPAddress?)null);
        _mockLoginService.Setup(x => x.LoginAsync(request, null, "test-agent"))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_EmptyUserAgent_HandlesGracefully()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        };
        var loginResponse = new LoginResponse { Success = true };

        _mockHeaders.Setup(x => x.UserAgent).Returns(StringValues.Empty);
        _mockLoginService.Setup(x => x.LoginAsync(request, "127.0.0.1", ""))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Helper Methods

    private static RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ST12345",
            LicenseState = "CA",
            LicenseType = "speech_therapy",
            AcceptedTerms = true
        };
    }

    #endregion
}