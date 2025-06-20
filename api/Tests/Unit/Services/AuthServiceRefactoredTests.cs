using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class AuthServiceRefactoredTests
{
    private readonly Mock<IUserRegistrationService> _mockRegistrationService;
    private readonly Mock<IEmailVerificationService> _mockVerificationService;
    private readonly AuthServiceRefactored _authService;

    public AuthServiceRefactoredTests()
    {
        _mockRegistrationService = new Mock<IUserRegistrationService>();
        _mockVerificationService = new Mock<IEmailVerificationService>();
        _authService = new AuthServiceRefactored(_mockRegistrationService.Object, _mockVerificationService.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldDelegateToRegistrationService()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var expectedResponse = new RegisterResponse
        {
            Success = true,
            Message = "Registration successful"
        };

        _mockRegistrationService
            .Setup(x => x.RegisterUserAsync(request, ipAddress, userAgent))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _authService.RegisterUserAsync(request, ipAddress, userAgent);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockRegistrationService.Verify(x => x.RegisterUserAsync(request, ipAddress, userAgent), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WithNullParameters_ShouldDelegateToRegistrationService()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        var expectedResponse = new RegisterResponse
        {
            Success = true,
            Message = "Registration successful"
        };

        _mockRegistrationService
            .Setup(x => x.RegisterUserAsync(request, null, null))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockRegistrationService.Verify(x => x.RegisterUserAsync(request, null, null), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldDelegateToVerificationService()
    {
        // Arrange
        var token = "test-token";
        var expectedResult = true;

        _mockVerificationService
            .Setup(x => x.VerifyEmailAsync(token))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockVerificationService.Verify(x => x.VerifyEmailAsync(token), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var token = "invalid-token";
        var expectedResult = false;

        _mockVerificationService
            .Setup(x => x.VerifyEmailAsync(token))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockVerificationService.Verify(x => x.VerifyEmailAsync(token), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ShouldDelegateToVerificationService()
    {
        // Arrange
        var email = "test@example.com";
        var expectedResult = true;

        _mockVerificationService
            .Setup(x => x.ResendVerificationEmailAsync(email))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockVerificationService.Verify(x => x.ResendVerificationEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var expectedResult = false;

        _mockVerificationService
            .Setup(x => x.ResendVerificationEmailAsync(email))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        Assert.False(result);
        _mockVerificationService.Verify(x => x.ResendVerificationEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task VerifyEmailAsync_WithInvalidTokens_ShouldHandleGracefully(string? token)
    {
        // Arrange
        _mockVerificationService
            .Setup(x => x.VerifyEmailAsync(token!))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.VerifyEmailAsync(token!);

        // Assert
        Assert.False(result);
        _mockVerificationService.Verify(x => x.VerifyEmailAsync(token!), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ResendVerificationEmailAsync_WithInvalidEmails_ShouldHandleGracefully(string? email)
    {
        // Arrange
        _mockVerificationService
            .Setup(x => x.ResendVerificationEmailAsync(email!))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email!);

        // Assert
        Assert.False(result);
        _mockVerificationService.Verify(x => x.ResendVerificationEmailAsync(email!), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenRegistrationServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };
        var exception = new InvalidOperationException("Registration failed");

        _mockRegistrationService
            .Setup(x => x.RegisterUserAsync(request, null, null))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterUserAsync(request, null, null));
        
        Assert.Equal(exception.Message, thrownException.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenVerificationServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var token = "test-token";
        var exception = new InvalidOperationException("Verification failed");

        _mockVerificationService
            .Setup(x => x.VerifyEmailAsync(token))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.VerifyEmailAsync(token));
        
        Assert.Equal(exception.Message, thrownException.Message);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenVerificationServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var email = "test@example.com";
        var exception = new InvalidOperationException("Resend failed");

        _mockVerificationService
            .Setup(x => x.ResendVerificationEmailAsync(email))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.ResendVerificationEmailAsync(email));
        
        Assert.Equal(exception.Message, thrownException.Message);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new AuthServiceRefactored(_mockRegistrationService.Object, _mockVerificationService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullRegistrationService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AuthServiceRefactored(null!, _mockVerificationService.Object));
    }

    [Fact]
    public void Constructor_WithNullVerificationService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AuthServiceRefactored(_mockRegistrationService.Object, null!));
    }
}