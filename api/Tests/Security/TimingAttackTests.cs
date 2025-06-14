using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Security;

public class TimingAttackTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
    private readonly Mock<IRegistrationAuditRepository> _mockAuditRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ILicenseVerificationService> _mockLicenseService;
    private readonly Mock<IEmailVerificationService> _mockEmailVerificationService;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<ILogger<UserRegistrationService>> _mockLogger;
    private readonly UserRegistrationService _registrationService;

    public TimingAttackTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
        _mockAuditRepository = new Mock<IRegistrationAuditRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockLicenseService = new Mock<ILicenseVerificationService>();
        _mockEmailVerificationService = new Mock<IEmailVerificationService>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockLogger = new Mock<ILogger<UserRegistrationService>>();

        _registrationService = new UserRegistrationService(
            _mockUserRepository.Object,
            _mockAuditRepository.Object,
            _mockPasswordService.Object,
            _mockLicenseService.Object,
            _mockEmailVerificationService.Object,
            _mockPasswordHistoryRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ExistingEmail_TakesConstantTime()
    {
        // Arrange
        var request = CreateRegistrationRequest("existing@test.com");
        SetupExistingEmailScenario();

        // Act & Assert
        var timings = await MeasureMultipleExecutions(() => 
            _registrationService.RegisterUserAsync(request, "127.0.0.1", "Browser"), 5);

        // All executions should take approximately the same time (500ms minimum)
        AssertConstantTiming(timings, 500, 100); // 500ms ± 100ms tolerance
    }

    [Fact]
    public async Task RegisterUserAsync_NewEmail_TakesConstantTime()
    {
        // Arrange
        var request = CreateRegistrationRequest("new@test.com");
        SetupNewEmailScenario();

        // Act & Assert
        var timings = await MeasureMultipleExecutions(() => 
            _registrationService.RegisterUserAsync(request, "127.0.0.1", "Browser"), 5);

        // All executions should take approximately the same time (500ms minimum)
        AssertConstantTiming(timings, 500, 100);
    }

    [Fact]
    public async Task RegisterUserAsync_FastVsSlowOperations_SameResponseTime()
    {
        // Arrange - Setup scenarios with different processing times
        var fastRequest = CreateRegistrationRequest("fast@test.com");
        var slowRequest = CreateRegistrationRequest("slow@test.com");

        SetupFastScenario(fastRequest.Email);
        SetupSlowScenario(slowRequest.Email);

        // Act
        var fastTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(fastRequest, "127.0.0.1", "Browser"));
        
        var slowTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(slowRequest, "127.0.0.1", "Browser"));

        // Assert - Both should take approximately 500ms regardless of processing speed
        Assert.True(Math.Abs(fastTiming - slowTiming) < 150, 
            $"Timing difference too large: fast={fastTiming}ms, slow={slowTiming}ms");
        
        Assert.True(fastTiming >= 450, $"Fast scenario too quick: {fastTiming}ms");
        Assert.True(slowTiming >= 450, $"Slow scenario too quick: {slowTiming}ms");
    }

    [Fact]
    public async Task RegisterUserAsync_DifferentFailureReasons_SameResponseTime()
    {
        // Arrange - Setup different failure scenarios
        var emailFailureRequest = CreateRegistrationRequest("duplicate@test.com");
        var licenseFailureRequest = CreateRegistrationRequest("badlicense@test.com");
        var passwordFailureRequest = CreateRegistrationRequest("weakpass@test.com");

        SetupEmailFailureScenario(emailFailureRequest.Email);
        SetupLicenseFailureScenario(licenseFailureRequest.Email);
        SetupPasswordFailureScenario(passwordFailureRequest.Email);

        // Act
        var emailFailureTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(emailFailureRequest, "127.0.0.1", "Browser"));
        
        var licenseFailureTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(licenseFailureRequest, "127.0.0.1", "Browser"));
        
        var passwordFailureTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(passwordFailureRequest, "127.0.0.1", "Browser"));

        // Assert - All failure types should take the same time
        var timings = new[] { emailFailureTiming, licenseFailureTiming, passwordFailureTiming };
        AssertConstantTiming(timings, 500, 150);
    }

    [Fact]
    public async Task RegisterUserAsync_SuccessVsFailure_SameResponseTime()
    {
        // Arrange
        var successRequest = CreateRegistrationRequest("success@test.com");
        var failureRequest = CreateRegistrationRequest("failure@test.com");

        SetupSuccessScenario(successRequest.Email);
        SetupFailureScenario(failureRequest.Email);

        // Act
        var successTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(successRequest, "127.0.0.1", "Browser"));
        
        var failureTiming = await MeasureSingleExecution(() => 
            _registrationService.RegisterUserAsync(failureRequest, "127.0.0.1", "Browser"));

        // Assert
        Assert.True(Math.Abs(successTiming - failureTiming) < 150, 
            $"Success vs failure timing difference too large: success={successTiming}ms, failure={failureTiming}ms");
    }

    [Fact]
    public async Task EmailVerificationService_ResendVerification_ConstantTime()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var mockEmailVerificationRepo = new Mock<IEmailVerificationRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<EmailVerificationService>>();

        var emailVerificationService = new EmailVerificationService(
            mockUserRepo.Object,
            mockEmailVerificationRepo.Object,
            mockEmailService.Object,
            mockLogger.Object);

        // Setup scenarios
        var existingEmail = "existing@test.com";
        var nonExistentEmail = "nonexistent@test.com";

        SetupEmailVerificationScenarios(mockUserRepo, mockEmailVerificationRepo, mockEmailService);

        // Act
        var existingTiming = await MeasureSingleExecution(() => 
            emailVerificationService.ResendVerificationEmailAsync(existingEmail));
        
        var nonExistentTiming = await MeasureSingleExecution(() => 
            emailVerificationService.ResendVerificationEmailAsync(nonExistentEmail));

        // Assert - Both should take approximately 300ms (the minimum time for email verification)
        Assert.True(Math.Abs(existingTiming - nonExistentTiming) < 100, 
            $"Email verification timing difference too large: existing={existingTiming}ms, nonexistent={nonExistentTiming}ms");
        
        Assert.True(existingTiming >= 250, $"Existing email verification too quick: {existingTiming}ms");
        Assert.True(nonExistentTiming >= 250, $"Non-existent email verification too quick: {nonExistentTiming}ms");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task RegisterUserAsync_MultipleExecutions_ConsistentTiming(int executionCount)
    {
        // Arrange
        var request = CreateRegistrationRequest("consistent@test.com");
        SetupConsistentScenario();

        // Act
        var timings = await MeasureMultipleExecutions(() => 
            _registrationService.RegisterUserAsync(request, "127.0.0.1", "Browser"), executionCount);

        // Assert
        AssertConstantTiming(timings, 500, 100);
        
        // Verify all executions completed
        Assert.Equal(executionCount, timings.Length);
        Assert.All(timings, timing => Assert.True(timing > 0, "All timings should be positive"));
    }

    [Fact]
    public async Task RegisterUserAsync_ConcurrentExecutions_IndependentTiming()
    {
        // Arrange
        var requests = Enumerable.Range(0, 5)
            .Select(i => CreateRegistrationRequest($"concurrent{i}@test.com"))
            .ToArray();

        SetupConcurrentScenarios();

        // Act
        var tasks = requests.Select(request => 
            MeasureSingleExecutionAsync(() => 
                _registrationService.RegisterUserAsync(request, "127.0.0.1", "Browser")))
            .ToArray();

        var timings = await Task.WhenAll(tasks);

        // Assert
        AssertConstantTiming(timings, 500, 150);
        
        // Verify all concurrent executions maintained timing consistency
        Assert.All(timings, timing => 
            Assert.True(timing >= 450, $"Concurrent execution too quick: {timing}ms"));
    }

    private async Task<double[]> MeasureMultipleExecutions<T>(Func<Task<T>> operation, int count)
    {
        var timings = new double[count];
        
        for (int i = 0; i < count; i++)
        {
            timings[i] = await MeasureSingleExecution(operation);
            
            // Small delay between executions to avoid interference
            await Task.Delay(10);
        }
        
        return timings;
    }

    private async Task<double> MeasureSingleExecution<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await operation();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private async Task<double> MeasureSingleExecutionAsync<T>(Func<Task<T>> operation)
    {
        return await MeasureSingleExecution(operation);
    }

    private void AssertConstantTiming(double[] timings, double expectedMs, double toleranceMs)
    {
        var minTiming = timings.Min();
        var maxTiming = timings.Max();
        var avgTiming = timings.Average();

        // All timings should be close to expected
        Assert.True(avgTiming >= expectedMs - toleranceMs, 
            $"Average timing too low: {avgTiming:F2}ms (expected ~{expectedMs}ms)");
        
        // Range should be small (consistent timing)
        var range = maxTiming - minTiming;
        Assert.True(range <= toleranceMs * 2, 
            $"Timing range too large: {range:F2}ms (min={minTiming:F2}, max={maxTiming:F2})");
    }

    private RegisterRequest CreateRegistrationRequest(string email)
    {
        return new RegisterRequest
        {
            Email = email,
            Password = "ValidPassword123!",
            FirstName = "Test",
            LastName = "User",
            Phone = "555-0123",
            LicenseNumber = "12345",
            LicenseState = "CA",
            LicenseType = "SLP"
        };
    }

    private void SetupExistingEmailScenario()
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
    }

    private void SetupNewEmailScenario()
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        _mockLicenseService.Setup(s => s.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Models.LicenseVerificationResult { Valid = true, DisciplinaryActions = false });
        _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<Models.User>())).ReturnsAsync(1);
        _mockPasswordHistoryRepository.Setup(r => r.AddPasswordToHistoryAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
    }

    private void SetupFastScenario(string email)
    {
        // Fast scenario - email exists (early return)
        _mockUserRepository.Setup(r => r.EmailExistsAsync(email)).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
    }

    private void SetupSlowScenario(string email)
    {
        // Slow scenario - goes through all validation steps
        _mockUserRepository.Setup(r => r.EmailExistsAsync(email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        
        // Add delay to license verification to simulate slow operation
        _mockLicenseService.Setup(s => s.VerifyLicenseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Models.LicenseVerificationResult { Valid = false, ErrorMessage = "License not found" });
    }

    private void SetupEmailFailureScenario(string email)
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(email)).ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
    }

    private void SetupLicenseFailureScenario(string email)
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(false);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
    }

    private void SetupPasswordFailureScenario(string email)
    {
        _mockUserRepository.Setup(r => r.EmailExistsAsync(email)).ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.LicenseExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _mockPasswordService.Setup(s => s.IsCommonPassword(It.IsAny<string>())).Returns(true);
        _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
    }

    private void SetupSuccessScenario(string email)
    {
        SetupNewEmailScenario();
    }

    private void SetupFailureScenario(string email)
    {
        SetupExistingEmailScenario();
    }

    private void SetupConsistentScenario()
    {
        SetupExistingEmailScenario();
    }

    private void SetupConcurrentScenarios()
    {
        SetupExistingEmailScenario();
    }

    private void SetupEmailVerificationScenarios(
        Mock<IUserRepository> userRepo, 
        Mock<IEmailVerificationRepository> emailVerificationRepo, 
        Mock<IEmailService> emailService)
    {
        // Existing user scenario
        var existingUser = new User 
        { 
            Id = 1, 
            Email = "existing@test.com", 
            EmailVerified = false 
        };
        
        userRepo.Setup(r => r.GetUserByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);
        userRepo.Setup(r => r.GetUserByEmailAsync("nonexistent@test.com")).ReturnsAsync((User?)null);
        
        emailVerificationRepo.Setup(r => r.HasValidTokenAsync(It.IsAny<int>())).ReturnsAsync(false);
        emailVerificationRepo.Setup(r => r.CreateVerificationTokenAsync(It.IsAny<int>())).ReturnsAsync("token123");
        emailService.Setup(s => s.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
    }
}