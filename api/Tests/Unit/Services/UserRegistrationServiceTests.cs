using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class UserRegistrationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRegistrationAuditRepository> _mockRegistrationAuditRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ILicenseVerificationService> _mockLicenseVerificationService;
    private readonly Mock<IEmailVerificationService> _mockEmailVerificationService;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<ILogger<UserRegistrationService>> _mockLogger;
    private readonly UserRegistrationService _service;

    public UserRegistrationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRegistrationAuditRepository = new Mock<IRegistrationAuditRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLicenseVerificationService = new Mock<ILicenseVerificationService>();
        _mockEmailVerificationService = new Mock<IEmailVerificationService>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockLogger = new Mock<ILogger<UserRegistrationService>>();

        _service = new UserRegistrationService(
            _mockUserRepository.Object,
            _mockRegistrationAuditRepository.Object,
            _mockPasswordService.Object,
            _mockLicenseVerificationService.Object,
            _mockEmailVerificationService.Object,
            _mockPasswordHistoryRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_SuccessfulRegistration_ReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupSuccessfulRegistration();

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful! Please check your email to verify your account.", result.Message);
        Assert.NotNull(result.UserId);
        Assert.Empty(result.Errors);

        // Verify all services were called
        _mockUserRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Once);
        _mockPasswordHistoryRepository.Verify(r => r.AddPasswordToHistoryAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        _mockEmailVerificationService.Verify(s => s.SendVerificationEmailAsync(It.IsAny<int>(), request.Email, request.FirstName), Times.Once);
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, true, null, "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Registration failed", result.Message);
        Assert.Contains("Registration failed. Please check your information and try again.", result.Errors);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "Email already exists", "192.168.1.1", "TestBrowser"), Times.Once);
        
        // Verify user was not created
        _mockUserRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateLicense_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(request.LicenseNumber, request.LicenseState)).ReturnsAsync(true);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Registration failed", result.Message);
        Assert.Contains("Registration failed. Please check your information and try again.", result.Errors);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "License already exists", "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_CommonPassword_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(request.Password)).Returns(true);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Registration failed", result.Message);
        Assert.Contains("Password is too common. Please choose a more secure password.", result.Errors);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "Common password", "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidLicense_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        _mockLicenseVerificationService.Setup(s => s.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LicenseVerificationResult { Valid = false, ErrorMessage = "License not found" });

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Registration failed", result.Message);
        Assert.Contains("License not found", result.Errors);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "License verification failed", "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_LicenseWithDisciplinaryActions_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        _mockLicenseVerificationService.Setup(s => s.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = true });

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Registration failed", result.Message);
        Assert.Contains("License has disciplinary actions. Please contact support for manual verification.", result.Errors);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "License has disciplinary actions", "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExceptionThrown_ReturnsGenericError()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An error occurred during registration. Please try again later.", result.Message);
        Assert.Contains("System error occurred", result.Errors);
        
        // Verify error was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during user registration")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        // Verify audit log
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, false, "System error", "192.168.1.1", "TestBrowser"), Times.Once);
    }

    [Theory]
    [InlineData("OT", "occupational_therapy")]
    [InlineData("PT", "physical_therapy")]
    [InlineData("SLP", "speech_therapy")]
    [InlineData("UNKNOWN", "speech_therapy")] // Default case
    [InlineData("ot", "occupational_therapy")] // Case insensitive
    public async Task RegisterUserAsync_DifferentLicenseTypes_MapsCorrectly(string licenseType, string expectedServiceType)
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        request.LicenseType = licenseType;
        SetupSuccessfulRegistration();

        User? capturedUser = null;
        _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync(1);

        // Act
        await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal(expectedServiceType, capturedUser.ServiceType);
    }

    [Fact]
    public async Task RegisterUserAsync_ConstantTimeResponse_TakesMinimumTime()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true); // Fast failure

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");
        stopwatch.Stop();

        // Assert - Should take at least 500ms
        Assert.True(stopwatch.ElapsedMilliseconds >= 450, // Allow small tolerance
            $"Expected at least 450ms, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task RegisterUserAsync_NullIpAndUserAgent_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupSuccessfulRegistration();

        // Act
        var result = await _service.RegisterUserAsync(request, null, null);

        // Assert
        Assert.True(result.Success);
        
        // Verify audit log was called with null values
        _mockRegistrationAuditRepository.Verify(r => r.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState, true, null, null, null), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_CreatedUser_HasCorrectProperties()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupSuccessfulRegistration();

        User? capturedUser = null;
        _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync(1);

        // Act
        await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal(request.Email, capturedUser.Email);
        Assert.Equal(request.FirstName, capturedUser.FirstName);
        Assert.Equal(request.LastName, capturedUser.LastName);
        Assert.Equal(request.Phone, capturedUser.Phone);
        Assert.Equal(request.LicenseNumber, capturedUser.LicenseNumber);
        Assert.Equal(request.LicenseState, capturedUser.LicenseState);
        Assert.Equal("pending", capturedUser.Status);
        Assert.False(capturedUser.EmailVerified);
        Assert.Equal("hashedpassword", capturedUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterUserAsync_AllValidationsPerformed_EvenOnEarlyFailure()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true); // Early failure
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");

        // Act
        await _service.RegisterUserAsync(request, "192.168.1.1", "TestBrowser");

        // Assert - Verify all checks were performed (constant time)
        _mockUserRepository.Verify(r => r.EmailExistsAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(r => r.LicenseExistsAsync(request.LicenseNumber, request.LicenseState), Times.Once);
        _mockPasswordService.Verify(s => s.IsCommonPassword(request.Password), Times.Once);
        _mockPasswordService.Verify(s => s.HashPassword(request.Password), Times.Once);
    }

    private RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            FirstName = "Test",
            LastName = "User",
            Phone = "555-0123",
            LicenseNumber = "12345",
            LicenseState = "CA",
            LicenseType = "SLP"
        };
    }

    private void SetupSuccessfulRegistration()
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        _mockLicenseVerificationService.Setup(s => s.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(1);
        _mockPasswordHistoryRepository.Setup(r => r.AddPasswordToHistoryAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _mockEmailVerificationService.Setup(s => s.SendVerificationEmailAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _mockRegistrationAuditRepository.Setup(r => r.LogRegistrationAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
    }
}