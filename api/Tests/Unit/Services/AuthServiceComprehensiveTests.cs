using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class AuthServiceComprehensiveTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
    private readonly Mock<IRegistrationAuditRepository> _mockRegistrationAuditRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILicenseVerificationService> _mockLicenseVerificationService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceComprehensiveTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
        _mockRegistrationAuditRepository = new Mock<IRegistrationAuditRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLicenseVerificationService = new Mock<ILicenseVerificationService>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockEmailVerificationRepository.Object,
            _mockRegistrationAuditRepository.Object,
            _mockPasswordService.Object,
            _mockEmailService.Object,
            _mockLicenseVerificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var userId = Guid.NewGuid();
        var token = "verification-token";
        var hashedPassword = "hashed-password";

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(userId);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(token);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(request.Email, request.FirstName, token))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");
        result.UserId.Should().Be(userId.ToString());
        result.Errors.Should().BeEmpty();

        // Verify all services were called
        _mockUserRepository.Verify(x => x.CreateUserAsync(It.Is<User>(u => 
            u.Email == request.Email && 
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName &&
            u.PasswordHash == hashedPassword &&
            u.ServiceType == "speech_therapy" &&
            u.Status == "pending" &&
            !u.EmailVerified)), Times.Once);
        
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            true, null, "127.0.0.1", "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("Registration failed. Please check your information and try again.");

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "Email already exists", "127.0.0.1", "test-agent"), Times.Once);
        
        // Verify user was not created
        _mockUserRepository.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_LicenseAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(true);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("Registration failed. Please check your information and try again.");

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "License already exists", "127.0.0.1", "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_CommonPassword_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(true);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("Password is too common. Please choose a more secure password.");

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "Common password", "127.0.0.1", "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidLicense_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = false, ErrorMessage = "License not found" });

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("License not found");

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "License verification failed", "127.0.0.1", "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_LicenseWithDisciplinaryActions_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = true });

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("License has disciplinary actions. Please contact support for manual verification.");

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "License has disciplinary actions", "127.0.0.1", "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailServiceFails_StillReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var userId = Guid.NewGuid();
        var token = "verification-token";

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(userId);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(token);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(request.Email, request.FirstName, token))
            .ReturnsAsync(false); // Email fails

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue(); // Still succeeds
        result.Message.Should().Contain("Registration successful");
        
        // Verify warning was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send verification email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Exception_ReturnsErrorResponse()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var exception = new Exception("Database error");

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ThrowsAsync(exception);

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during registration. Please try again later.");
        result.Errors.Should().Contain("System error occurred");

        // Verify error was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during user registration")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        // Verify audit log
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "System error", "127.0.0.1", "test-agent"), Times.Once);
    }

    [Theory]
    [InlineData("OT", "occupational_therapy")]
    [InlineData("PT", "physical_therapy")]
    [InlineData("SLP", "speech_therapy")]
    [InlineData("ST", "speech_therapy")] // Default
    [InlineData("ot", "occupational_therapy")] // Case insensitive
    [InlineData("unknown", "speech_therapy")] // Default for unknown
    public async Task RegisterUserAsync_MapsLicenseTypeCorrectly(string licenseType, string expectedServiceType)
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        request.LicenseType = licenseType;
        var userId = Guid.NewGuid();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(userId);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync("token");
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(request.Email, request.FirstName, "token"))
            .ReturnsAsync(true);

        // Act
        await _authService.RegisterUserAsync(request, null, null);

        // Assert
        _mockUserRepository.Verify(x => x.CreateUserAsync(It.Is<User>(u => 
            u.ServiceType == expectedServiceType)), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
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
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue();
        
        // Verify user was updated
        _mockUserRepository.Verify(x => x.UpdateUserAsync(It.Is<User>(u => 
            u.EmailVerified == true && 
            u.Status == "active")), Times.Once);
        
        // Verify welcome email was sent
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var token = "invalid-token";
        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify warning was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid verification token attempted")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        
        // Verify no further operations
        _mockUserRepository.Verify(x => x.VerifyEmailAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_TokenAlreadyUsed_ReturnsFalse()
    {
        // Arrange
        var token = "used-token";
        var verificationToken = new EmailVerificationToken
        {
            UserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify warning was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token already used")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var token = "expired-token";
        var verificationToken = new EmailVerificationToken
        {
            UserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            UsedAt = null
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify warning was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification token expired")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_MarkTokenFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(false); // Fails

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify user email was not verified
        _mockUserRepository.Verify(x => x.VerifyEmailAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_VerifyEmailFails_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(false); // Fails

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify user was not retrieved
        _mockUserRepository.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_UserNotFound_StillReturnsTrue()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid();
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _mockEmailVerificationRepository.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null); // User not found

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue(); // Still returns true
        
        // Verify welcome email was not sent
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_Exception_ReturnsFalse()
    {
        // Arrange
        var token = "valid-token";
        var exception = new Exception("Database error");
        
        _mockEmailVerificationRepository.Setup(x => x.GetTokenAsync(token))
            .ThrowsAsync(exception);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
        
        // Verify error was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error verifying email with token")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ValidUser_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };
        var token = "new-token";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _mockEmailVerificationRepository.Setup(x => x.HasValidTokenAsync(userId))
            .ReturnsAsync(false);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(token);
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(email, user.FirstName, token))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeTrue();
        
        // Verify email was sent
        _mockEmailService.Verify(x => x.SendVerificationEmailAsync(email, user.FirstName, token), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        
        // Verify no token was created
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserAlreadyVerified_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            EmailVerified = true // Already verified
        };

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        
        // Verify no token was created
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_UserHasValidToken_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            EmailVerified = false
        };

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _mockEmailVerificationRepository.Setup(x => x.HasValidTokenAsync(userId))
            .ReturnsAsync(true); // Already has valid token

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        
        // Verify info was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User already has valid verification token")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        
        // Verify no new token was created
        _mockEmailVerificationRepository.Verify(x => x.CreateVerificationTokenAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Exception_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var exception = new Exception("Database error");
        
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ThrowsAsync(exception);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
        
        // Verify error was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error resending verification email")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_NullIpAddress_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var userId = Guid.NewGuid();

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed");
        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(userId);
        _mockEmailVerificationRepository.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync("token");
        _mockEmailService.Setup(x => x.SendVerificationEmailAsync(request.Email, request.FirstName, "token"))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null); // Null IP and user agent

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify audit log with null values
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            true, null, null, null), Times.Once);
    }

    private static RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-123-4567",
            LicenseNumber = "ST12345",
            LicenseState = "CA",
            LicenseType = "ST",
            AcceptedTerms = true
        };
    }
}