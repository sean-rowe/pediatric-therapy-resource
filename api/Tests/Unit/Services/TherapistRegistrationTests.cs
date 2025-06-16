using FluentAssertions;
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

public class TherapistRegistrationTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRegistrationAuditRepository> _mockAuditRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ILicenseVerificationService> _mockLicenseService;
    private readonly Mock<IEmailVerificationService> _mockEmailVerificationService;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<ILogger<UserRegistrationService>> _mockLogger;
    private readonly UserRegistrationService _service;

    public TherapistRegistrationTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuditRepository = new Mock<IRegistrationAuditRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLicenseService = new Mock<ILicenseVerificationService>();
        _mockEmailVerificationService = new Mock<IEmailVerificationService>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockLogger = new Mock<ILogger<UserRegistrationService>>();

        _service = new UserRegistrationService(
            _mockUserRepository.Object,
            _mockAuditRepository.Object,
            _mockPasswordService.Object,
            _mockLicenseService.Object,
            _mockEmailVerificationService.Object,
            _mockPasswordHistoryRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidTherapistData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password))
            .Returns("hashedpassword");
        _mockLicenseService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(Guid.NewGuid());
        _mockPasswordHistoryRepository.Setup(x => x.AddPasswordToHistoryAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockEmailVerificationService.Setup(x => x.SendVerificationEmailAsync(It.IsAny<Guid>(), request.Email, request.FirstName))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");

        // Verify all required calls were made
        _mockUserRepository.Verify(x => x.EmailExistsAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState), Times.Once);
        _mockPasswordService.Verify(x => x.IsCommonPassword(request.Password), Times.Once);
        _mockLicenseService.Verify(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType), Times.Once);
        _mockUserRepository.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
        _mockEmailVerificationService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<Guid>(), request.Email, request.FirstName), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");

        // Verify audit logging
        _mockAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email,
            request.LicenseNumber,
            request.LicenseState,
            false,
            "Email already exists",
            "127.0.0.1",
            "test-agent"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_LicenseAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
    }

    [Fact]
    public async Task RegisterUserAsync_CommonPassword_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "password", // Common password
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(true);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidLicense_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "INVALID123",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockLicenseService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult { Valid = false, ErrorMessage = "License not found" });

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
    }

    [Fact]
    public async Task RegisterUserAsync_LicenseWithDisciplinaryActions_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = "speech_therapy",
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        _mockUserRepository.Setup(x => x.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(request.Password))
            .Returns(false);
        _mockLicenseService.Setup(x => x.VerifyLicenseAsync(request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult 
            { 
                Valid = true, 
                DisciplinaryActions = true 
            });

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
    }

    [Theory]
    [InlineData("speech_therapy")]
    [InlineData("occupational_therapy")]
    [InlineData("physical_therapy")]
    [InlineData("behavioral_therapy")]
    public async Task RegisterUserAsync_ValidLicenseTypes_AcceptsRegistration(string serviceType)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseType = serviceType,
            LicenseNumber = "ST12345",
            LicenseState = "CA"
        };

        SetupSuccessfulRegistration();

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    private void SetupSuccessfulRegistration()
    {
        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsCommonPassword(It.IsAny<string>()))
            .Returns(false);
        _mockPasswordService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");
        _mockLicenseService.Setup(x => x.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(Guid.NewGuid());
        _mockPasswordHistoryRepository.Setup(x => x.AddPasswordToHistoryAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockEmailVerificationService.Setup(x => x.SendVerificationEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }
}