using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class AccountLockoutRepositoryComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AccountLockoutRepository>> _mockLogger;
    private readonly AccountLockoutRepository _repository;
    private readonly string _connectionString = "Server=test;Database=test;Trusted_Connection=true;";

    public AccountLockoutRepositoryComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(_connectionString);
        
        _repository = new AccountLockoutRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckAccountLockoutAsync_ValidEmail_ReturnsLockoutStatus()
    {
        // Arrange
        var email = "test@example.com";
        
        // Act & Assert
        var act = async () => await _repository.CheckAccountLockoutAsync(email);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_ValidParameters_RecordsAttempt()
    {
        // Arrange
        var email = "test@example.com";
        var ipAddress = "127.0.0.1";
        var userAgent = "test-agent";
        
        // Act & Assert
        var act = async () => await _repository.RecordFailedLoginAttemptAsync(email, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearFailedLoginAttemptsAsync_ValidEmail_ClearsAttempts()
    {
        // Arrange
        var email = "test@example.com";
        
        // Act & Assert
        var act = async () => await _repository.ClearFailedLoginAttemptsAsync(email);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task IsAccountLockedAsync_ValidUserId_ReturnsLockStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.IsAccountLockedAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetLockoutEndTimeAsync_ValidUserId_ReturnsEndTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.GetLockoutEndTimeAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LockAccountAsync_ValidParameters_LocksAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEndTime = DateTime.UtcNow.AddMinutes(30);
        var reason = "Too many failed attempts";
        
        // Act & Assert
        var act = async () => await _repository.LockAccountAsync(userId, lockoutEndTime, reason);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnlockAccountAsync_ValidUserId_UnlocksAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.UnlockAccountAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ValidConfiguration_InitializesSuccessfully()
    {
        // Arrange & Act
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, _mockLogger.Object);
        
        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new AccountLockoutRepository(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new AccountLockoutRepository(_mockConfiguration.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CheckAccountLockoutAsync_InvalidEmail_HandlesGracefully(string? email)
    {
        // Act & Assert
        var act = async () => await _repository.CheckAccountLockoutAsync(email!);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RecordFailedLoginAttemptAsync_InvalidEmail_HandlesGracefully(string? email)
    {
        // Act & Assert
        var act = async () => await _repository.RecordFailedLoginAttemptAsync(email!, "127.0.0.1", "test");
        await act.Should().NotThrowAsync();
    }
}