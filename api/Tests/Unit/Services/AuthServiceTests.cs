using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepositoryMock;
    private readonly Mock<IRegistrationAuditRepository> _registrationAuditRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILicenseVerificationService> _licenseVerificationServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailVerificationRepositoryMock = new Mock<IEmailVerificationRepository>();
        _registrationAuditRepositoryMock = new Mock<IRegistrationAuditRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _emailServiceMock = new Mock<IEmailService>();
        _licenseVerificationServiceMock = new Mock<ILicenseVerificationService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _emailVerificationRepositoryMock.Object,
            _registrationAuditRepositoryMock.Object,
            _passwordServiceMock.Object,
            _emailServiceMock.Object,
            _licenseVerificationServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Succeed_With_Valid_Request()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var userId = Guid.NewGuid();
        var hashedPassword = "hashed_password";
        var verificationToken = "verification_token";

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _passwordServiceMock.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _passwordServiceMock.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _licenseVerificationServiceMock.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true });
        _userRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(userId);
        _emailVerificationRepositoryMock.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(verificationToken);
        _emailServiceMock.Setup(x => x.SendVerificationEmailAsync(request.Email, request.FirstName, verificationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");
        result.UserId.Should().Be(userId.ToString());
        result.Errors.Should().BeEmpty();

        _registrationAuditRepositoryMock.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            true, null, "127.0.0.1", "TestAgent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Fail_When_Email_Already_Exists()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Email already registered"));

        _registrationAuditRepositoryMock.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "Email already exists", null, null), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Fail_When_License_Already_Exists()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("License number already associated"));

        _registrationAuditRepositoryMock.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, 
            false, "License already exists", null, null), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Fail_When_Password_Is_Common()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _passwordServiceMock.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(true);

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Password is too common"));
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Fail_When_License_Verification_Fails()
    {
        // Arrange
        var request = CreateValidRegisterRequest();

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _passwordServiceMock.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _licenseVerificationServiceMock.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = false, ErrorMessage = "License not found" });

        // Act
        var result = await _authService.RegisterUserAsync(request, null, null);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("License not found");
    }

    [Fact]
    public async Task VerifyEmailAsync_Should_Return_True_With_Valid_Token()
    {
        // Arrange
        var token = "valid_token";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            EmailVerified = false,
            Status = "pending"
        };
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _emailVerificationRepositoryMock.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);
        _emailVerificationRepositoryMock.Setup(x => x.MarkTokenUsedAsync(token))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.VerifyEmailAsync(userId))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);
        _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(user.Email, user.FirstName))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.UpdateUserAsync(It.Is<User>(u => 
            u.EmailVerified == true && u.Status == "active")), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_Should_Return_False_With_Invalid_Token()
    {
        // Arrange
        var token = "invalid_token";

        _emailVerificationRepositoryMock.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailAsync_Should_Return_False_With_Expired_Token()
    {
        // Arrange
        var token = "expired_token";
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            UsedAt = null
        };

        _emailVerificationRepositoryMock.Setup(x => x.GetTokenAsync(token))
            .ReturnsAsync(verificationToken);

        // Act
        var result = await _authService.VerifyEmailAsync(token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Should_Return_True_When_Successful()
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
        var verificationToken = "new_token";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
        _emailVerificationRepositoryMock.Setup(x => x.HasValidTokenAsync(userId))
            .ReturnsAsync(false);
        _emailVerificationRepositoryMock.Setup(x => x.CreateVerificationTokenAsync(userId))
            .ReturnsAsync(verificationToken);
        _emailServiceMock.Setup(x => x.SendVerificationEmailAsync(email, user.FirstName, verificationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Should_Return_False_When_User_Not_Found()
    {
        // Arrange
        var email = "notfound@example.com";

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_Should_Return_False_When_Already_Verified()
    {
        // Arrange
        var email = "verified@example.com";
        var user = new User
        {
            Email = email,
            EmailVerified = true
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ResendVerificationEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    private static RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ABC12345",
            LicenseState = "CA",
            LicenseType = "OT",
            Phone = "555-123-4567",
            AcceptedTerms = true
        };
    }
}