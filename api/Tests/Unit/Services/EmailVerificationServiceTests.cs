using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class EmailVerificationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<EmailVerificationService>> _mockLogger;
    private readonly EmailVerificationService _service;

    public EmailVerificationServiceTests()
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

    #region SendVerificationEmailAsync Tests

    [Fact]
    public async Task SendVerificationEmailAsync_Success_CreatesTokenAndSendsEmail()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var firstName = "John";
        var token = "verification-token-123";

        _mockEmailVerificationRepository
            .Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(token);

        _mockEmailService
            .Setup(x => x.SendVerificationEmailAsync(email, firstName, token))
            .ReturnsAsync(true);

        // Act
        await _service.SendVerificationEmailAsync(userId, email, firstName);

        // Assert
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(userId), Times.Once);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(email, firstName, token), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_EmailServiceFails_LogsWarning()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var firstName = "John";
        var token = "verification-token-123";

        _mockEmailVerificationRepository
            .Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(token);

        _mockEmailService
            .Setup(x => x.SendVerificationEmailAsync(email, firstName, token))
            .ReturnsAsync(false);

        // Act
        await _service.SendVerificationEmailAsync(userId, email, firstName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send verification email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_RepositoryThrows_PropagatesException()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var firstName = "John";

        _mockEmailVerificationRepository
            .Setup(x => x.CreateVerificationTokenAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _service.SendVerificationEmailAsync(userId, email, firstName));
    }

    #endregion

    #region VerifyEmailAsync Tests

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token";
        var userId = 123;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            EmailVerified = false,
            Status = "pending"
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        _mockEmailVerificationRepository
            .Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _mockEmailService
            .Setup(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName))
            .ReturnsAsync(true);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.True(result);
        Assert.True(user.EmailVerified);
        Assert.Equal("active", user.Status);
        
        _mockEmailVerificationRepository.Verify(x => x.MarkTokenUsedAsync(token), Times.Once);
        _mockUserRepository.Verify(x => x.VerifyEmailAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var token = "invalid-token";

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid verification token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyUsedToken_ReturnsFalse()
    {
        // Arrange
        var token = "used-token";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = 123,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = DateTime.UtcNow.AddMinutes(-10),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token already used")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var token = "expired-token";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = 123,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddHours(-25)
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token expired")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_MarkTokenUsedFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = 123,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        _mockEmailVerificationRepository
            .Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(x => x.VerifyEmailAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_VerifyEmailFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";
        var userId = 123;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        _mockEmailVerificationRepository
            .Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(x => x.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_UserNotFound_StillReturnsTrue()
    {
        // Arrange
        var token = "valid-token";
        var userId = 123;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        _mockEmailVerificationRepository
            .Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";

        _mockEmailVerificationRepository
            .Setup(x => x.GetTokenAsync(token))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.VerifyEmailAsync(token);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying email with token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ResendVerificationEmailAsync Tests

    [Fact]
    public async Task ResendVerificationEmailAsync_ValidUserWithoutToken_SendsEmail()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = 123,
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };
        var token = "new-token-123";

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        _mockEmailVerificationRepository
            .Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(false);

        _mockEmailVerificationRepository
            .Setup(x => x.CreateVerificationTokenAsync(user.Id))
            .ReturnsAsync(token);

        _mockEmailService
            .Setup(x => x.SendVerificationEmailAsync(user.Email, user.FirstName, token))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        Assert.True(result);
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(user.Id), Times.Once);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(user.Email, user.FirstName, token), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserAlreadyHasValidToken_DoesNotSendEmail()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = 123,
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        _mockEmailVerificationRepository
            .Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        Assert.False(result);
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<int>()), Times.Never);
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User already has valid verification token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        Assert.False(result);
        _mockEmailVerificationRepository.Verify(x => x.HasValidTokenAsync(It.IsAny<int>()), Times.Never);
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserAlreadyVerified_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = 123,
            Email = email,
            FirstName = "John",
            EmailVerified = true
        };

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        Assert.False(result);
        _mockEmailVerificationRepository.Verify(x => x.HasValidTokenAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_EmailServiceFails_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = 123,
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };
        var token = "new-token-123";

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        _mockEmailVerificationRepository
            .Setup(x => x.HasValidTokenAsync(user.Id))
            .ReturnsAsync(false);

        _mockEmailVerificationRepository
            .Setup(x => x.CreateVerificationTokenAsync(user.Id))
            .ReturnsAsync(token);

        _mockEmailService
            .Setup(x => x.SendVerificationEmailAsync(user.Email, user.FirstName, token))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ResendVerificationEmailAsync(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Exception_ReturnsFalseWithConstantTime()
    {
        // Arrange
        var email = "test@example.com";

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _service.ResendVerificationEmailAsync(email);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        Assert.False(result);
        Assert.True(elapsedTime.TotalMilliseconds >= 300, "Response time should be at least 300ms for timing attack prevention");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error resending verification email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_FastOperation_DelaysToConstantTime()
    {
        // Arrange
        var email = "test@example.com";

        _mockUserRepository
            .Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _service.ResendVerificationEmailAsync(email);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        Assert.False(result);
        Assert.True(elapsedTime.TotalMilliseconds >= 300, "Response time should be at least 300ms for timing attack prevention");
    }

    #endregion
}