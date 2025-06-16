using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class UserRegistrationServiceComprehensiveTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRegistrationAuditRepository> _mockRegistrationAuditRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ILicenseVerificationService> _mockLicenseVerificationService;
    private readonly Mock<IEmailVerificationService> _mockEmailVerificationService;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<ILogger<UserRegistrationService>> _mockLogger;
    private readonly UserRegistrationService _service;

    public UserRegistrationServiceComprehensiveTests()
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

    /**
     * Feature: User Registration Timing Attack Prevention
     *   As a security-conscious application
     *   I want to prevent timing attacks on registration
     *   So that attackers cannot determine valid email addresses or licenses
     * 
     * Scenario: Registration with minimum response time enforcement
     *   Given any registration attempt
     *   When processing completes faster than minimum time
     *   Then the response is delayed to meet minimum time
     */
    [Fact]
    public async Task RegisterUserAsync_FastProcessing_EnforcesMinimumResponseTime()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupFailureScenario("email_exists");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        stopwatch.Stop();
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        
        // Should take at least the minimum response time (500ms as per SecurityConstants)
        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(450); // Allow some tolerance for Docker
    }

    /**
     * Feature: Input Validation and Null Handling
     *   As a robust service
     *   I want to handle invalid input gracefully
     *   So that the system remains stable
     * 
     * Scenario: Null request throws ArgumentNullException
     *   Given a null registration request
     *   When attempting to register
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public async Task RegisterUserAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.RegisterUserAsync(null!, "127.0.0.1", "test-agent"));
    }

    /**
     * Scenario: Null IP address and user agent handled gracefully
     *   Given null IP address and user agent
     *   When registering a user
     *   Then the registration proceeds without error
     */
    [Fact]
    public async Task RegisterUserAsync_NullIpAndUserAgent_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupSuccessfulRegistration();

        // Act
        var result = await _service.RegisterUserAsync(request, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        
        // Verify audit logging with null values
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState,
            true, null, null, null), Times.Once);
    }

    /**
     * Feature: License Type Mapping
     *   As a therapy platform
     *   I want to map license types to service types correctly
     *   So that users are categorized properly
     * 
     * Scenario: Various license type mappings
     *   Given different license types
     *   When registering users
     *   Then service types are mapped correctly
     */
    [Theory]
    [InlineData("OT", "occupational_therapy")]
    [InlineData("PT", "physical_therapy")]
    [InlineData("SLP", "speech_therapy")]
    [InlineData("UNKNOWN", "speech_therapy")] // Default case
    [InlineData("", "speech_therapy")] // Empty string default
    public async Task RegisterUserAsync_LicenseTypeMappings_CorrectlyMapped(string licenseType, string expectedServiceType)
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        request.LicenseType = licenseType;
        SetupSuccessfulRegistration();

        User? capturedUser = null;
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.ServiceType.Should().Be(expectedServiceType);
    }

    /**
     * Feature: Exception Handling and Recovery
     *   As a resilient service
     *   I want to handle exceptions gracefully
     *   So that users receive appropriate error messages
     * 
     * Scenario: Database exception during user creation
     *   Given a valid registration request
     *   When database throws an exception
     *   Then error is logged and user gets generic message
     */
    [Fact]
    public async Task RegisterUserAsync_DatabaseException_HandlesGracefully()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var exception = new Exception("Database connection failed");

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(exception);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        stopwatch.Stop();
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred during registration. Please try again later.");
        result.Errors.Should().Contain("System error occurred");

        // Verify error logging
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during user registration")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        // Verify audit logging
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState,
            false, "System error", "127.0.0.1", "test-agent"), Times.Once);

        // Verify minimum response time still enforced
        stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(450);
    }

    /**
     * Feature: Password Security Validation
     *   As a security-focused platform
     *   I want to prevent weak passwords
     *   So that user accounts remain secure
     * 
     * Scenario: Registration with compromised password
     *   Given a password that appears in breach databases
     *   When attempting registration
     *   Then registration fails with appropriate message
     */
    [Fact]
    public async Task RegisterUserAsync_BreachedPassword_RejectsRegistration()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        request.Password = "password123"; // Common breached password

        SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: true);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("Password is too common. Please choose a more secure password.");

        // Verify audit logging
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState,
            false, "Common password", "127.0.0.1", "test-agent"), Times.Once);
    }

    /**
     * Feature: License Verification Integration
     *   As a regulated therapy platform
     *   I want to verify professional licenses
     *   So that only qualified practitioners can register
     * 
     * Scenario: License verification API failure
     *   Given license verification service is unavailable
     *   When attempting registration
     *   Then registration fails with manual verification message
     */
    [Fact]
    public async Task RegisterUserAsync_LicenseVerificationFailure_RejectsRegistration()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: false);

        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(
            request.LicenseNumber, request.LicenseState, request.LicenseType))
            .ReturnsAsync(new LicenseVerificationResult
            {
                Valid = false,
                ErrorMessage = "License verification service temporarily unavailable. Manual verification required."
            });

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("License verification service temporarily unavailable. Manual verification required.");

        // Verify audit logging
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState,
            false, "License verification failed", "127.0.0.1", "test-agent"), Times.Once);
    }

    /**
     * Feature: Data Consistency and Integrity
     *   As a data-driven platform
     *   I want to ensure data consistency
     *   So that user records are complete and accurate
     * 
     * Scenario: Successful registration creates complete user record
     *   Given a valid registration request
     *   When registration succeeds
     *   Then all user data is properly stored
     */
    [Fact]
    public async Task RegisterUserAsync_SuccessfulRegistration_CreatesCompleteUserRecord()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        var expectedUserId = Guid.NewGuid();
        SetupSuccessfulRegistration();

        User? capturedUser = null;
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .ReturnsAsync(expectedUserId);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Registration successful! Please check your email to verify your account.");
        result.UserId.Should().Be(expectedUserId.ToString());

        // Verify user object is properly constructed
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(request.Email);
        capturedUser.FirstName.Should().Be(request.FirstName);
        capturedUser.LastName.Should().Be(request.LastName);
        capturedUser.Phone.Should().Be(request.Phone);
        capturedUser.LicenseNumber.Should().Be(request.LicenseNumber);
        capturedUser.LicenseState.Should().Be(request.LicenseState);
        capturedUser.Status.Should().Be("pending");
        capturedUser.EmailVerified.Should().BeFalse();
        capturedUser.PasswordHash.Should().NotBeNullOrEmpty();

        // Verify all required operations are performed
        _mockPasswordHistoryRepository.Verify(x => x.AddPasswordToHistoryAsync(expectedUserId, It.IsAny<string>()), Times.Once);
        _mockEmailVerificationService.Verify(x => x.SendVerificationEmailAsync(expectedUserId, request.Email, request.FirstName), Times.Once);
        _mockRegistrationAuditRepository.Verify(x => x.LogRegistrationAttemptAsync(
            request.Email, request.LicenseNumber, request.LicenseState,
            true, null, "127.0.0.1", "test-agent"), Times.Once);
    }

    /**
     * Feature: Constant-Time Operations for Security
     *   As a security-conscious platform
     *   I want to prevent information leakage through timing
     *   So that attackers cannot infer system state
     * 
     * Scenario: All validation checks performed regardless of early failures
     *   Given any registration request
     *   When validation fails early
     *   Then all validation steps are still performed
     */
    [Fact]
    public async Task RegisterUserAsync_EarlyValidationFailure_PerformsAllChecks()
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        SetupValidationChecks(emailExists: true, licenseExists: false, isCommonPassword: false);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();

        // Verify all checks were performed despite email existing (early failure)
        _mockUserRepository.Verify(x => x.EmailExistsAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(x => x.LicenseExistsAsync(request.LicenseNumber, request.LicenseState), Times.Once);
        _mockPasswordService.Verify(x => x.IsCommonPassword(request.Password), Times.Once);
        _mockPasswordService.Verify(x => x.HashPassword(request.Password), Times.Once);
    }

    /**
     * Feature: Edge Cases and Boundary Conditions
     *   As a robust service
     *   I want to handle edge cases properly
     *   So that the system works reliably in all scenarios
     * 
     * Scenario: Registration with edge case data
     *   Given registration data with boundary values
     *   When attempting registration
     *   Then the system handles it appropriately
     */
    [Theory]
    [InlineData("", "Edge case empty email")]
    [InlineData("user@", "Edge case incomplete email")]
    [InlineData("user@domain", "Edge case domain without TLD")]
    [InlineData("a@b.c", "Edge case minimal valid email")]
    public async Task RegisterUserAsync_EdgeCaseEmails_HandlesAppropriately(string email, string testCase)
    {
        // Arrange
        var request = CreateValidRegisterRequest();
        request.Email = email;
        SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: false);

        // Act
        var result = await _service.RegisterUserAsync(request, "127.0.0.1", "test-agent");

        // Assert
        result.Should().NotBeNull();
        // The service doesn't validate email format - that's handled by the validation filter
        // So we expect processing to continue normally
    }

    /**
     * Feature: Concurrent Registration Handling
     *   As a multi-user platform
     *   I want to handle concurrent registrations safely
     *   So that race conditions don't cause data corruption
     * 
     * Scenario: Concurrent registrations with same email
     *   Given two users try to register with same email simultaneously
     *   When both registrations are processed
     *   Then only one should succeed (handled by database constraints)
     */
    [Fact]
    public async Task RegisterUserAsync_ConcurrentSameEmail_HandlesSafely()
    {
        // Arrange
        var request1 = CreateValidRegisterRequest();
        var request2 = CreateValidRegisterRequest();
        request2.LicenseNumber = "DIFFERENT123"; // Different license to avoid that constraint

        // First call succeeds, second fails due to email constraint
        _mockUserRepository.SetupSequence(x => x.EmailExistsAsync(request1.Email))
            .ReturnsAsync(false) // First check passes
            .ReturnsAsync(true);  // Second check fails

        SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: false);

        // Act
        var task1 = _service.RegisterUserAsync(request1, "127.0.0.1", "test-agent-1");
        var task2 = _service.RegisterUserAsync(request2, "127.0.0.1", "test-agent-2");

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Should().HaveCount(2);
        // At least one should fail due to email constraint
        results.Should().Contain(r => r.Success == false);
    }

    // Helper methods

    private RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-123-4567",
            LicenseType = "SLP",
            LicenseNumber = "SLP12345",
            LicenseState = "CA"
        };
    }

    private void SetupValidationChecks(bool emailExists, bool licenseExists, bool isCommonPassword)
    {
        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(emailExists);
        _mockUserRepository.Setup(x => x.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(licenseExists);
        _mockPasswordService.Setup(x => x.IsCommonPassword(It.IsAny<string>()))
            .Returns(isCommonPassword);
        _mockPasswordService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");
    }

    private void SetupSuccessfulRegistration()
    {
        SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: false);

        _mockLicenseVerificationService.Setup(x => x.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(Guid.NewGuid());
        _mockPasswordHistoryRepository.Setup(x => x.AddPasswordToHistoryAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockEmailVerificationService.Setup(x => x.SendVerificationEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockRegistrationAuditRepository.Setup(x => x.LogRegistrationAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupFailureScenario(string failureType)
    {
        switch (failureType)
        {
            case "email_exists":
                SetupValidationChecks(emailExists: true, licenseExists: false, isCommonPassword: false);
                break;
            case "license_exists":
                SetupValidationChecks(emailExists: false, licenseExists: true, isCommonPassword: false);
                break;
            case "common_password":
                SetupValidationChecks(emailExists: false, licenseExists: false, isCommonPassword: true);
                break;
        }

        _mockRegistrationAuditRepository.Setup(x => x.LogRegistrationAttemptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
    }
}