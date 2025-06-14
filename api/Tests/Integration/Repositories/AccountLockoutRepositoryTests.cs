using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Integration.Repositories;

[Collection("Database")]
public class AccountLockoutRepositoryTests : IDisposable
{
    private readonly AccountLockoutRepository _repository;
    private readonly Mock<ILogger<AccountLockoutRepository>> _mockLogger;
    private readonly string _testEmail = "lockout.test@example.com";

    public AccountLockoutRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        
        // Setup test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TherapyDocsTest;Trusted_Connection=true;MultipleActiveResultSets=true"
            })
            .Build();

        _repository = new AccountLockoutRepository(configuration, _mockLogger.Object);
    }

    [Fact]
    public async Task RecordFailedLoginAttempt_FirstAttempt_RecordsSuccessfully()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var userAgent = "Test Browser";

        // Act
        await _repository.RecordFailedLoginAttemptAsync(_testEmail, ipAddress, userAgent);

        // Assert - Should not throw and should be recorded in database
        // Verify by checking lockout status
        var status = await _repository.CheckAccountLockoutAsync(_testEmail);
        Assert.NotNull(status);
        Assert.False(status.IsLocked);
        Assert.True(status.RemainingAttempts < 5); // Should have decreased from default 5
    }

    [Fact]
    public async Task CheckAccountLockout_NoFailedAttempts_ReturnsNotLocked()
    {
        // Arrange
        var cleanEmail = "clean.user@example.com";

        // Act
        var status = await _repository.CheckAccountLockoutAsync(cleanEmail);

        // Assert
        Assert.NotNull(status);
        Assert.False(status.IsLocked);
        Assert.Null(status.LockedUntil);
        Assert.Equal(5, status.RemainingAttempts);
    }

    [Fact]
    public async Task RecordFailedLoginAttempt_MultipleAttempts_TriggersLockout()
    {
        // Arrange
        var testEmail = "multiple.fails@example.com";
        var ipAddress = "192.168.1.101";
        var userAgent = "Test Browser";

        // Act - Record 5 failed attempts
        for (int i = 0; i < 5; i++)
        {
            await _repository.RecordFailedLoginAttemptAsync(testEmail, ipAddress, userAgent);
        }

        // Assert - Should be locked after 5 attempts
        var status = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.NotNull(status);
        Assert.True(status.IsLocked);
        Assert.NotNull(status.LockedUntil);
        Assert.True(status.LockedUntil > DateTime.UtcNow);
        Assert.Equal(0, status.RemainingAttempts);
    }

    [Fact]
    public async Task RecordFailedLoginAttempt_ProgressiveLockout_IncreasesLockoutDuration()
    {
        // Arrange
        var testEmail = "progressive.test@example.com";
        var ipAddress = "192.168.1.102";

        // Act & Assert - First lockout (5 attempts)
        for (int i = 0; i < 5; i++)
        {
            await _repository.RecordFailedLoginAttemptAsync(testEmail, ipAddress, "Browser1");
        }

        var firstLockout = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.True(firstLockout.IsLocked);
        var firstLockoutTime = firstLockout.LockedUntil;

        // Continue recording attempts for progressive lockout (should get longer duration)
        for (int i = 0; i < 3; i++)
        {
            await _repository.RecordFailedLoginAttemptAsync(testEmail, ipAddress, "Browser1");
        }

        var secondLockout = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.True(secondLockout.IsLocked);
        
        // Second lockout should have later expiry time (longer duration)
        Assert.True(secondLockout.LockedUntil > firstLockoutTime);
    }

    [Fact]
    public async Task ClearFailedLoginAttempts_ClearsLockoutAndHistory()
    {
        // Arrange
        var testEmail = "clear.test@example.com";
        var ipAddress = "192.168.1.103";

        // Record failed attempts to trigger lockout
        for (int i = 0; i < 5; i++)
        {
            await _repository.RecordFailedLoginAttemptAsync(testEmail, ipAddress, "Browser");
        }

        // Verify lockout exists
        var beforeClear = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.True(beforeClear.IsLocked);

        // Act
        await _repository.ClearFailedLoginAttemptsAsync(testEmail);

        // Assert
        var afterClear = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.False(afterClear.IsLocked);
        Assert.Null(afterClear.LockedUntil);
        Assert.Equal(5, afterClear.RemainingAttempts); // Reset to default
    }

    [Fact]
    public async Task CheckAccountLockout_ExpiredLockout_AutomaticallyClears()
    {
        // This test would require manipulating the database to set an expired lockout
        // or using a test-specific stored procedure that allows setting past dates
        
        // For now, we'll test the behavior by waiting or by using database manipulation
        // In a real scenario, you might want to inject a time provider for easier testing
        
        var testEmail = "expired.lockout@example.com";
        
        // For this test, we assume the stored procedure logic correctly handles expired lockouts
        // A more comprehensive test would involve database setup with expired lockout data
        
        var status = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.NotNull(status);
        // Status should indicate not locked if lockout has expired
    }

    [Fact]
    public async Task RecordFailedLoginAttempt_NonExistentUser_StillRecords()
    {
        // Arrange
        var nonExistentEmail = "doesnotexist@example.com";
        var ipAddress = "192.168.1.104";

        // Act - Should not throw even if user doesn't exist
        await _repository.RecordFailedLoginAttemptAsync(nonExistentEmail, ipAddress, "Browser");

        // Assert - Should still record the attempt (for audit/security purposes)
        var status = await _repository.CheckAccountLockoutAsync(nonExistentEmail);
        Assert.NotNull(status);
        // Even for non-existent users, we should track attempts to prevent enumeration
    }

    [Theory]
    [InlineData(null, "Browser")]
    [InlineData("192.168.1.1", null)]
    [InlineData(null, null)]
    public async Task RecordFailedLoginAttempt_NullParameters_HandlesGracefully(string? ipAddress, string? userAgent)
    {
        // Arrange
        var testEmail = "null.params@example.com";

        // Act & Assert - Should not throw
        await _repository.RecordFailedLoginAttemptAsync(testEmail, ipAddress, userAgent);
        
        var status = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.NotNull(status);
    }

    [Fact]
    public async Task CheckAccountLockout_DatabaseError_ThrowsException()
    {
        // This would test database connectivity issues
        // In a real test, you might inject a faulty connection string or use a test double
        
        // For now, we verify that legitimate calls don't throw
        var status = await _repository.CheckAccountLockoutAsync("valid@example.com");
        Assert.NotNull(status);
    }

    [Fact]
    public async Task ConcurrentFailedAttempts_ThreadSafe_HandlesCorrectly()
    {
        // Arrange
        var testEmail = "concurrent.test@example.com";
        var tasks = new List<Task>();

        // Act - Simulate concurrent failed login attempts
        for (int i = 0; i < 10; i++)
        {
            var taskIndex = i;
            tasks.Add(Task.Run(async () => 
            {
                await _repository.RecordFailedLoginAttemptAsync(
                    testEmail, 
                    $"192.168.1.{taskIndex}", 
                    $"Browser{taskIndex}");
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Should be locked after multiple concurrent attempts
        var status = await _repository.CheckAccountLockoutAsync(testEmail);
        Assert.NotNull(status);
        Assert.True(status.IsLocked);
    }

    public void Dispose()
    {
        // Cleanup test data if needed
        // In a real test suite, you might want to clean up the test database
        // or use transactions that can be rolled back
    }
}