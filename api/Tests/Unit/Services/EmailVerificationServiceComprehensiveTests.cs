using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class EmailVerificationServiceComprehensiveTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<EmailVerificationService>> _mockLogger;
    private readonly EmailVerificationService _service;

    public EmailVerificationServiceComprehensiveTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<EmailVerificationService>>();

        _service = new EmailVerificationService(
            _mockUserRepository.Object,
            _mockEmailVerificationRepository.Object,
            _mockEmailService.Object,
            _mockLogger.Object);
    }

    /**
     * Feature: Send Email Verification
     *   As a user registration system
     *   I want to send verification emails to new users
     *   So that email addresses can be verified
     * 
     * Scenario: Successful verification email sent
     *   Given a user has just registered
     *   When I send verification email
     *   Then a verification token is created
     *   And the email is sent successfully
     */
    [Fact]
    public async Task SendVerificationEmailAsync_ValidUser_CreatesTokenAndSendsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var firstName = "John";
        var verificationToken = "test-token-123";

        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(verificationToken);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(email, firstName, verificationToken))
            .ReturnsAsync(true);

        // Act
        await _service.SendVerificationEmailAsync(userId, email, firstName);

        // Assert
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(userId), Times.Once);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(email, firstName, verificationToken), Times.Once);
    }

    /**
     * Scenario: Email service fails to send email
     *   Given a user needs verification email
     *   When the email service fails
     *   Then a warning is logged
     */
    [Fact]
    public async Task SendVerificationEmailAsync_EmailServiceFails_LogsWarning()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var firstName = "John";
        var verificationToken = "test-token-123";

        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(verificationToken);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(email, firstName, verificationToken))
            .ReturnsAsync(false);

        // Act
        await _service.SendVerificationEmailAsync(userId, email, firstName);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send verification email") && v.ToString()!.Contains(userId.ToString())),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Feature: Email Verification
     *   As a user registration system
     *   I want to verify email addresses using tokens
     *   So that only valid email addresses are activated
     * 
     * Scenario: Valid token verification
     *   Given a valid, unexpired verification token
     *   When I verify the email
     *   Then the token is marked as used
     *   And the user email is verified
     *   And the user status is updated to active
     *   And a welcome email is sent
     */
    [Fact]
    public async Task VerifyEmailAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token-123";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            FirstName = "John",
            EmailVerified = false,
            Status = "pending"
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockEmailService.Setup(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue();
        _mockEmailVerificationRepository.Verify(x => x.MarkTokenUsedAsync(token), Times.Once);
        _mockUserRepository.Verify(x => x.VerifyEmailAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.Is<User>(u => 
            u.EmailVerified == true && u.Status == "active")), Times.Once);
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName), Times.Once);
    }

    /**
     * Scenario: Invalid token
     *   Given an invalid verification token
     *   When I attempt to verify the email
     *   Then false is returned
     *   And a warning is logged
     */
    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var token = "invalid-token";

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid verification token attempted")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Already used token
     *   Given a verification token that has been used
     *   When I attempt to verify the email
     *   Then false is returned
     *   And a warning is logged
     */
    [Fact]
    public async Task VerifyEmailAsync_AlreadyUsedToken_ReturnsFalse()
    {
        // Arrange
        var token = "used-token-123";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = Guid.NewGuid(),
            UsedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token already used")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Expired token
     *   Given a verification token that has expired
     *   When I attempt to verify the email
     *   Then false is returned
     *   And a warning is logged
     */
    [Fact]
    public async Task VerifyEmailAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var token = "expired-token-123";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = Guid.NewGuid(),
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired 1 hour ago
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token expired")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Token marking fails
     *   Given a valid token
     *   When marking the token as used fails
     *   Then false is returned
     */
    [Fact]
    public async Task VerifyEmailAsync_TokenMarkingFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token-123";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = Guid.NewGuid(),
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    /**
     * Scenario: User email verification fails
     *   Given a valid token
     *   When user email verification fails
     *   Then false is returned
     */
    [Fact]
    public async Task VerifyEmailAsync_UserEmailVerificationFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token-123";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    /**
     * Scenario: User not found after verification
     *   Given a valid token
     *   When the user cannot be found after verification
     *   Then true is still returned (verification succeeded)
     *   But welcome email is not sent
     */
    [Fact]
    public async Task VerifyEmailAsync_UserNotFoundAfterVerification_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token-123";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue();
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: Exception during verification
     *   Given a verification token
     *   When an exception occurs during verification
     *   Then false is returned
     *   And the error is logged
     */
    [Fact]
    public async Task VerifyEmailAsync_ExceptionOccurs_ReturnsFalseAndLogsError()
    {
        // Arrange
        var token = "valid-token-123";
        var exception = new Exception("Database error");

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying email with token")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Feature: Resend Verification Email
     *   As a user registration system
     *   I want to allow users to resend verification emails
     *   So that users can complete verification if they didn't receive the initial email
     * 
     * Scenario: Successful resend for unverified user
     *   Given a user exists and is not verified
     *   And the user has no valid token
     *   When I resend verification email
     *   Then a new token is created
     *   And the email is sent
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_UnverifiedUserNoValidToken_CreatesTokenAndSendsEmail()
    {
        // Arrange
        var email = "user@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };
        var verificationToken = "new-token-123";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _mockEmailVerificationRepository.Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(false);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(user.Id))
            .ReturnsAsync(verificationToken);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(email, user.FirstName, verificationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeTrue();
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(user.Id), Times.Once);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(email, user.FirstName, verificationToken), Times.Once);
    }

    /**
     * Scenario: User already has valid token
     *   Given a user exists and is not verified
     *   And the user has a valid token
     *   When I resend verification email
     *   Then no new token is created
     *   And no email is sent
     *   And information is logged
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_UserHasValidToken_DoesNotCreateNewToken()
    {
        // Arrange
        var email = "user@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _mockEmailVerificationRepository.Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(user.Id), Times.Never);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User already has valid verification token")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: User not found
     *   Given an email address that doesn't exist
     *   When I attempt to resend verification email
     *   Then false is returned
     *   And no operations are performed
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<Guid>()), Times.Never);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: User already verified
     *   Given a user that has already verified their email
     *   When I attempt to resend verification email
     *   Then false is returned
     *   And no operations are performed
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_UserAlreadyVerified_ReturnsFalse()
    {
        // Arrange
        var email = "verified@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            EmailVerified = true
        };

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<Guid>()), Times.Never);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: Email service fails during resend
     *   Given a user needs verification email resent
     *   When the email service fails
     *   Then false is returned
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_EmailServiceFails_ReturnsFalse()
    {
        // Arrange
        var email = "user@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };
        var verificationToken = "new-token-123";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _mockEmailVerificationRepository.Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(false);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(user.Id))
            .ReturnsAsync(verificationToken);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(email, user.FirstName, verificationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    /**
     * Scenario: Exception during resend
     *   Given a resend request
     *   When an exception occurs
     *   Then false is returned
     *   And the error is logged
     *   And constant time is maintained
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_ExceptionOccurs_ReturnsFalseAndLogsError()
    {
        // Arrange
        var email = "user@example.com";
        var exception = new Exception("Database error");

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ThrowsAsync(exception);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        stopwatch.Stop();
        result.Should().BeFalse();
        
        // Verify minimum response time (should be at least 250ms, allowing for Docker overhead)
        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(250);
        
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error resending verification email")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Timing attack prevention
     *   Given any resend request
     *   When the operation completes quickly
     *   Then the response is delayed to maintain constant time
     */
    [Fact]
    public async Task ResendVerificationEmailAsync_FastOperation_MaintainsConstantTime()
    {
        // Arrange
        var email = "user@example.com";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null); // Fast operation

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        stopwatch.Stop();
        result.Should().BeFalse();
        
        // Verify minimum response time (should be at least 250ms, allowing for Docker overhead)
        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(250);
    }

    /**
     * Scenario: Edge case - null and empty parameters
     *   Given invalid parameters
     *   When methods are called
     *   Then they handle gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task ResendVerificationEmailAsync_InvalidEmail_HandlesGracefully(string email)
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task VerifyEmailAsync_InvalidToken_HandlesGracefully(string token)
    {
        // Arrange
        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
    }
}