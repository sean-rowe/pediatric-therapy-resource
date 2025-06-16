using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class LoginServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IAccountLockoutRepository> _mockLockoutRepository;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LoginService>> _mockLogger;
    private readonly LoginService _loginService;

    public LoginServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLockoutRepository = new Mock<IAccountLockoutRepository>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LoginService>>();

        // Setup JWT configuration
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-that-is-at-least-32-characters-long");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TherapyDocs");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TherapyDocs");

        _loginService = new LoginService(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockLockoutRepository.Object,
            _mockPasswordHistoryRepository.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 30 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockPasswordHistoryRepository.Setup(r => r.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal("Login successful", result.Message);
        
        // Verify interactions
        _mockLockoutRepository.Verify(r => r.ClearFailedLoginAttemptsAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailureAndRecordsAttempt()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "WrongPassword" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 4 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);
        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.Equal("Invalid email or password.", result.Message);
        Assert.Equal(4, result.RemainingAttempts);
        
        // Verify failed attempt was recorded
        _mockLockoutRepository.Verify(r => r.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "TestAgent"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_AccountLocked_ReturnsLockedMessage()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var lockoutStatus = new AccountLockoutStatus 
        { 
            IsLocked = true, 
            LockedUntil = DateTime.UtcNow.AddMinutes(15),
            RemainingAttempts = 0 
        };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.True(result.IsLocked);
        Assert.Equal(0, result.RemainingAttempts);
        Assert.Contains("Account is locked", result.Message);
        
        // Verify no password verification occurred
        _mockPasswordService.Verify(s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UnverifiedEmail_ReturnsEmailVerificationRequired()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        user.EmailVerified = false;
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.True(result.RequiresEmailVerification);
        Assert.Contains("verify your email", result.Message);
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsNotActiveMessage()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        user.Status = "suspended";
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not active", result.Message);
    }

    [Fact]
    public async Task LoginAsync_PasswordChangeRequired_ReturnsWarning()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = true, DaysUntilExpiry = -5 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockPasswordHistoryRepository.Setup(r => r.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.PasswordChangeRequired);
        Assert.Contains("password has expired", result.Message);
    }

    [Fact]
    public async Task LoginAsync_PasswordExpiryWarning_ReturnsWarning()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var user = CreateValidUser();
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };
        var passwordRequirement = new PasswordChangeRequirement { ChangeRequired = false, DaysUntilExpiry = 7 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _mockPasswordHistoryRepository.Setup(r => r.CheckPasswordChangeRequiredAsync(user.Id))
            .ReturnsAsync(passwordRequirement);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.PasswordExpiryWarning);
        Assert.Equal(7, result.DaysUntilPasswordExpiry);
        Assert.Contains("expire in 7 days", result.Message);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsFailureAndRecordsAttempt()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@test.com", Password = "SomePassword" };
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 4 };

        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        // Simulate constant-time password verification for non-existent user
        _mockPasswordService.Setup(s => s.VerifyPassword(request.Password, It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);
        
        // Verify failed attempt was recorded even for non-existent user
        _mockLockoutRepository.Verify(r => r.RecordFailedLoginAttemptAsync(request.Email, "127.0.0.1", "TestAgent"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ExceptionThrown_ReturnsGenericError()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        
        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("error occurred during login", result.Message);
        
        // Verify error was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during login")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(500)] // Should take at least 500ms
    [InlineData(750)] // Should take at least 500ms but not more than test allows
    public async Task LoginAsync_ConstantTimeResponse_TakesMinimumTime(int expectedMinimumMs)
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword123!" };
        var lockoutStatus = new AccountLockoutStatus { IsLocked = false, RemainingAttempts = 5 };
        
        _mockLockoutRepository.Setup(r => r.CheckAccountLockoutAsync(request.Email))
            .ReturnsAsync(lockoutStatus);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _loginService.LoginAsync(request, "127.0.0.1", "TestAgent");
        stopwatch.Stop();

        // Assert
        Assert.False(result.Success);
        Assert.True(stopwatch.ElapsedMilliseconds >= expectedMinimumMs - 50, // Allow 50ms tolerance
            $"Expected minimum {expectedMinimumMs}ms, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    private User CreateValidUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Test",
            LastName = "User",
            ServiceType = "speech_therapy",
            Status = "active",
            EmailVerified = true,
            LicenseNumber = "12345",
            LicenseState = "CA"
        };
    }
}