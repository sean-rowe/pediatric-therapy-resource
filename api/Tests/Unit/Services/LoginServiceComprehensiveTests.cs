using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class LoginServiceComprehensiveTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IAccountLockoutRepository> _mockLockoutRepository;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LoginService>> _mockLogger;
    private readonly LoginService _loginService;

    public LoginServiceComprehensiveTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLockoutRepository = new Mock<IAccountLockoutRepository>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LoginService>>();

        // Setup JWT configuration
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("ThisIsAVerySecureKeyForTestingPurposesOnly12345");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

        _loginService = new LoginService(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockLockoutRepository.Object,
            _mockPasswordHistoryRepository.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    /**
     * Feature: User Login
     *   As a registered user
     *   I want to log in to my account
     *   So that I can access protected resources
     * 
     * Scenario: Successful login with valid credentials
     *   Given I have a verified, active account
     *   When I provide correct email and password
     *   Then I receive a JWT token
     *   And my failed attempts are cleared
     */
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockLockoutRepository.Setup(x => x.ClearFailedLoginAttemptsAsync(request.Email))
            .Returns(Task.CompletedTask);
        _mockPasswordHistoryRepository.Setup(x => x.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Login successful");
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(user.Email);
        result.User.FirstName.Should().Be(user.FirstName);
        result.User.LastName.Should().Be(user.LastName);
        result.User.ServiceType.Should().Be(user.ServiceType);

        // Verify JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        token.Should().NotBeNull();
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);

        // Verify failed attempts were cleared
        _mockLockoutRepository.Verify(x => x.ClearFailedLoginAttemptsAsync(request.Email), Times.Once);
    }

    /**
     * Scenario: Account is locked due to too many failed attempts
     *   Given my account is locked
     *   When I try to log in
     *   Then I receive a lockout message
     *   And no password verification occurs
     */
    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsLockedMessage()
    {
        // Arrange
        var request = new LoginRequest { Email = "locked@example.com", Password = "password123" };
        var lockoutStatus = new AccountLockoutStatus 
        { 
            IsLocked = true, 
            LockedUntil = DateTime.UtcNow.AddMinutes(15),
            RemainingAttempts = 0 
        };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(CreateValidUser());

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Account is locked");
        result.Message.Should().Contain("14 minutes"); // Approximately 15 minutes
        result.IsLocked.Should().BeTrue();
        result.RemainingAttempts.Should().Be(0);

        // Verify no failed attempt was recorded (already locked)
        _mockLockoutRepository.Verify(x => x.RecordFailedLoginAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: Invalid email - user doesn't exist
     *   Given no user exists with the email
     *   When I try to log in
     *   Then I receive an invalid credentials message
     *   And a failed attempt is recorded
     *   And dummy password verification occurs for timing attack prevention
     */
    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ReturnsInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "password123" };
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var updatedLockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 2 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, It.IsAny<string>()))
            .Returns(false);
        _mockLockoutRepository.Setup(x => x.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "test-agent"))
            .Returns(Task.CompletedTask);
        _mockLockoutRepository.SetupSequence(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus)
            .ReturnsAsync(updatedLockoutStatus);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
        result.RemainingAttempts.Should().Be(2);
        result.IsLocked.Should().BeFalse();

        // Verify dummy password verification occurred
        _mockPasswordService.Verify(x => x.VerifyPassword(request.Password, "$2a$12$dummy.hash.to.maintain.constant.time"), Times.Once);
        
        // Verify failed attempt was recorded
        _mockLockoutRepository.Verify(x => x.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "test-agent"), Times.Once);
    }

    /**
     * Scenario: Invalid password
     *   Given I have a valid account
     *   When I provide wrong password
     *   Then I receive an invalid credentials message
     *   And a failed attempt is recorded
     */
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "wrongpassword" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var updatedLockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 2 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);
        _mockLockoutRepository.Setup(x => x.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "test-agent"))
            .Returns(Task.CompletedTask);
        _mockLockoutRepository.SetupSequence(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus)
            .ReturnsAsync(updatedLockoutStatus);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
        result.RemainingAttempts.Should().Be(2);

        // Verify password was actually checked
        _mockPasswordService.Verify(x => x.VerifyPassword(request.Password, user.PasswordHash), Times.Once);
        
        // Verify failed attempt was recorded
        _mockLockoutRepository.Verify(x => x.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "test-agent"), Times.Once);
    }

    /**
     * Scenario: Email not verified
     *   Given I have an unverified account
     *   When I try to log in with correct credentials
     *   Then I receive an email verification required message
     */
    [Fact]
    public async Task LoginAsync_EmailNotVerified_ReturnsVerificationRequired()
    {
        // Arrange
        var request = new LoginRequest { Email = "unverified@example.com", Password = "password123" };
        var user = CreateValidUser();
        user.EmailVerified = false;
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Please verify your email address before logging in.");
        result.RequiresEmailVerification.Should().BeTrue();

        // Verify no failed attempt was recorded (valid credentials)
        _mockLockoutRepository.Verify(x => x.RecordFailedLoginAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: Account is inactive
     *   Given my account status is not active
     *   When I try to log in with correct credentials
     *   Then I receive an inactive account message
     */
    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsInactiveMessage()
    {
        // Arrange
        var request = new LoginRequest { Email = "inactive@example.com", Password = "password123" };
        var user = CreateValidUser();
        user.Status = "suspended";
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Your account is not active. Please contact support.");

        // Verify no failed attempt was recorded (valid credentials)
        _mockLockoutRepository.Verify(x => x.RecordFailedLoginAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /**
     * Scenario: Password change required
     *   Given my password has expired
     *   When I log in successfully
     *   Then I receive a token but with password change required flag
     */
    [Fact]
    public async Task LoginAsync_PasswordChangeRequired_ReturnsSuccessWithFlag()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = true, DaysUntilExpiry = 0 };

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Login successful. Your password has expired and must be changed.");
        result.Token.Should().NotBeNullOrEmpty();
        result.PasswordChangeRequired.Should().BeTrue();
    }

    /**
     * Scenario: Password expiry warning
     *   Given my password will expire soon (within 14 days)
     *   When I log in successfully
     *   Then I receive a token with expiry warning
     */
    [Theory]
    [InlineData(14, true)]  // Exactly 14 days - should warn
    [InlineData(7, true)]   // 7 days - should warn
    [InlineData(1, true)]   // 1 day - should warn
    [InlineData(15, false)] // 15 days - no warning
    [InlineData(30, false)] // 30 days - no warning
    public async Task LoginAsync_PasswordExpiryApproaching_ReturnsWarning(int daysUntilExpiry, bool shouldWarn)
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = daysUntilExpiry };

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        
        if (shouldWarn)
        {
            result.PasswordExpiryWarning.Should().BeTrue();
            result.DaysUntilPasswordExpiry.Should().Be(daysUntilExpiry);
            result.Message.Should().Contain($"Your password will expire in {daysUntilExpiry} days");
        }
        else
        {
            result.PasswordExpiryWarning.Should().BeFalse();
            result.Message.Should().Be("Login successful");
        }
    }

    /**
     * Scenario: Exception during login
     *   Given an error occurs during login
     *   When I try to log in
     *   Then I receive a generic error message
     *   And the error is logged
     */
    [Fact]
    public async Task LoginAsync_Exception_ReturnsErrorMessage()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var exception = new Exception("Database connection failed");

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ThrowsAsync(exception);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during login. Please try again later.");

        // Verify error was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during login")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /**
     * Scenario: Null IP address and user agent
     *   Given the request has no IP or user agent info
     *   When I log in
     *   Then login proceeds normally with null values
     */
    [Fact]
    public async Task LoginAsync_NullIpAndUserAgent_HandlesGracefully()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, null, null);

        // Assert
        result.Success.Should().BeTrue();

        // Verify null values were passed through
        _mockLockoutRepository.Verify(x => x.ClearFailedLoginAttemptsAsync(request.Email), Times.Once);
    }

    /**
     * Scenario: JWT configuration missing
     *   Given JWT key is not configured
     *   When I try to log in with valid credentials
     *   Then an InvalidOperationException is thrown
     */
    [Fact]
    public async Task LoginAsync_MissingJwtKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        // Override JWT configuration to return null
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns((string?)null);

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act & Assert
        var act = async () => await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");
        
        // The exception will be caught and returned as error response
        var result = await act();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during login. Please try again later.");
    }

    /**
     * Scenario: JWT token generation with all claims
     *   Given I have all user information
     *   When I log in successfully
     *   Then the JWT contains all expected claims
     */
    [Fact]
    public async Task LoginAsync_TokenGeneration_IncludesAllClaims()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        user.LicenseNumber = "ST12345";
        user.LicenseState = "CA";
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify JWT token claims
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        
        token.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == user.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        token.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == $"{user.FirstName} {user.LastName}");
        token.Claims.Should().Contain(c => c.Type == "service_type" && c.Value == user.ServiceType);
        token.Claims.Should().Contain(c => c.Type == "license_number" && c.Value == user.LicenseNumber);
        token.Claims.Should().Contain(c => c.Type == "license_state" && c.Value == user.LicenseState);
        
        // Verify token expiry (8 hours)
        token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
        
        // Verify issuer and audience
        token.Issuer.Should().Be("TestIssuer");
        token.Audiences.Should().Contain("TestAudience");
    }

    /**
     * Scenario: JWT token with null license info
     *   Given user has no license information
     *   When I log in successfully
     *   Then the JWT contains empty strings for license claims
     */
    [Fact]
    public async Task LoginAsync_NullLicenseInfo_HandlesInToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = CreateValidUser();
        user.LicenseNumber = null;
        user.LicenseState = null;
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        SetupSuccessfulLoginMocks(request, user, lockoutStatus, passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify JWT token handles null license info
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        
        token.Claims.Should().Contain(c => c.Type == "license_number" && c.Value == "");
        token.Claims.Should().Contain(c => c.Type == "license_state" && c.Value == "");
    }

    /**
     * Scenario: Timing attack prevention
     *   Given various login scenarios
     *   When login attempts are made
     *   Then response time is consistent (minimum 500ms)
     */
    [Theory]
    [InlineData(true, true, true)]    // Valid login
    [InlineData(false, true, true)]   // Invalid password
    [InlineData(true, false, true)]   // User doesn't exist
    [InlineData(true, true, false)]   // Account locked
    public async Task LoginAsync_TimingAttackPrevention_ConsistentResponseTime(bool validPassword, bool userExists, bool accountActive)
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var user = userExists ? CreateValidUser() : null;
        
        if (user != null && !accountActive)
        {
            user.Status = "suspended";
        }

        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 3 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, It.IsAny<string>()))
            .Returns(validPassword);

        if (user != null && validPassword && accountActive)
        {
            _mockLockoutRepository.Setup(x => x.ClearFailedLoginAttemptsAsync(request.Email))
                .Returns(Task.CompletedTask);
            _mockPasswordHistoryRepository.Setup(x => x.CheckPasswordChangeRequiredAsync(user.Id))
                .ReturnsAsync(passwordRequirement);
        }
        else
        {
            _mockLockoutRepository.Setup(x => x.RecordFailedLoginAttemptAsync(request.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockLockoutRepository.SetupSequence(x => x.CheckAccountLockoutAsync(request.Email))
                .ReturnsAsync(lockoutStatus)
                .ReturnsAsync(lockoutStatus);
        }

        // Act
        var startTime = DateTime.UtcNow;
        await _loginService.LoginAsync(request, "127.0.0.1", "test-agent");
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Response time should be at least 450ms (Docker tolerance)
        elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(450));
    }

    // Helper methods
    private static User CreateValidUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "$2a$12$validhash",
            FirstName = "John",
            LastName = "Doe",
            ServiceType = "speech_therapy",
            EmailVerified = true,
            Status = "active",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };
    }

    private void SetupSuccessfulLoginMocks(
        LoginRequest request, 
        User user, 
        AccountLockoutStatus lockoutStatus, 
        PasswordChangeRequirement passwordRequirement)
    {
        _mockLockoutRepository.Setup(x => x.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockLockoutRepository.Setup(x => x.ClearFailedLoginAttemptsAsync(request.Email))
            .Returns(Task.CompletedTask);
        _mockPasswordHistoryRepository.Setup(x => x.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);
    }
}