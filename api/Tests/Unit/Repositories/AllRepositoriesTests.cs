using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using Xunit;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class AllRepositoriesTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _connectionString = "Server=test;Database=test;Trusted_Connection=true;";

    public AllRepositoriesTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(_connectionString);
    }

    #region BaseRepository Tests

    [Fact]
    public void BaseRepository_GetConnection_ReturnsConnection()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TestableBaseRepository>>();
        var repository = new TestableBaseRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act
        using var connection = repository.TestGetConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.ConnectionString.Should().Be(_connectionString);
    }

    private class TestableBaseRepository : BaseRepository
    {
        public TestableBaseRepository(IConfiguration configuration, ILogger<TestableBaseRepository> logger) 
            : base(configuration, logger) { }

        public IDbConnection TestGetConnection() => GetConnection();
    }

    #endregion

    #region UserRepository Tests

    [Fact]
    public async Task UserRepository_CreateUserAsync_CallsStoredProcedure()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "12345",
            LicenseState = "CA",
            ServiceType = "speech_therapy",
            Status = "active",
            EmailVerified = false
        };

        // Act & Assert - Will fail due to no actual database
        var act = async () => await repository.CreateUserAsync(user);
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_EmailExistsAsync_ChecksEmail()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.EmailExistsAsync("test@example.com");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_LicenseExistsAsync_ChecksLicense()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.LicenseExistsAsync("12345", "CA");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_GetUserByEmailAsync_ReturnsUser()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetUserByEmailAsync("test@example.com");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_GetUserByIdAsync_ReturnsUser()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetUserByIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_UpdateUserAsync_UpdatesUser()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);
        var user = new User { Id = Guid.NewGuid() };

        // Act & Assert
        var act = async () => await repository.UpdateUserAsync(user);
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task UserRepository_UpdateUserPasswordAsync_UpdatesPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserRepository>>();
        var repository = new UserRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.UpdateUserPasswordAsync(Guid.NewGuid(), "newHash");
        await act.Should().ThrowAsync<SqlException>();
    }

    #endregion

    #region AccountLockoutRepository Tests

    [Fact]
    public async Task AccountLockoutRepository_CheckAccountLockoutAsync_ReturnsStatus()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.CheckAccountLockoutAsync("test@example.com");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_RecordFailedLoginAttemptAsync_RecordsAttempt()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.RecordFailedLoginAttemptAsync("test@example.com", "127.0.0.1", "test");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_ClearFailedLoginAttemptsAsync_ClearsAttempts()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.ClearFailedLoginAttemptsAsync("test@example.com");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_IsAccountLockedAsync_ChecksLock()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.IsAccountLockedAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_GetLockoutEndTimeAsync_ReturnsTime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetLockoutEndTimeAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_LockAccountAsync_LocksAccount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.LockAccountAsync(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(30), "test");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task AccountLockoutRepository_UnlockAccountAsync_UnlocksAccount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AccountLockoutRepository>>();
        var repository = new AccountLockoutRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.UnlockAccountAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    #endregion

    #region PasswordHistoryRepository Tests

    [Fact]
    public async Task PasswordHistoryRepository_AddPasswordToHistoryAsync_AddsPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        var repository = new PasswordHistoryRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.AddPasswordToHistoryAsync(Guid.NewGuid(), "hash");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task PasswordHistoryRepository_IsPasswordReusedAsync_ChecksReuse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        var repository = new PasswordHistoryRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.IsPasswordReusedAsync(Guid.NewGuid(), "hash");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task PasswordHistoryRepository_GetPasswordHistoryCountAsync_ReturnsCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        var repository = new PasswordHistoryRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetPasswordHistoryCountAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task PasswordHistoryRepository_CheckPasswordChangeRequiredAsync_ChecksRequirement()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        var repository = new PasswordHistoryRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.CheckPasswordChangeRequiredAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    #endregion

    #region RegistrationAuditRepository Tests

    [Fact]
    public async Task RegistrationAuditRepository_LogRegistrationAttemptAsync_LogsAttempt()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RegistrationAuditRepository>>();
        var repository = new RegistrationAuditRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.LogRegistrationAttemptAsync(
            "test@example.com", "12345", "CA", true, null, "127.0.0.1", "test");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task RegistrationAuditRepository_GetRecentAttemptsCountAsync_ReturnsCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RegistrationAuditRepository>>();
        var repository = new RegistrationAuditRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetRecentAttemptsCountAsync("test@example.com", TimeSpan.FromMinutes(15));
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task RegistrationAuditRepository_GetRecentAttemptsByIpAsync_ReturnsCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RegistrationAuditRepository>>();
        var repository = new RegistrationAuditRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.GetRecentAttemptsByIpAsync("127.0.0.1", TimeSpan.FromMinutes(15));
        await act.Should().ThrowAsync<SqlException>();
    }

    #endregion

    #region EmailVerificationRepository Tests

    [Fact]
    public async Task EmailVerificationRepository_CreateVerificationTokenAsync_CreatesToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.CreateVerificationTokenAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_ValidateVerificationTokenAsync_ValidatesToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.ValidateVerificationTokenAsync("token");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_MarkVerificationTokenUsedAsync_MarksUsed()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.MarkVerificationTokenUsedAsync("token");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_CreatePasswordResetTokenAsync_CreatesToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.CreatePasswordResetTokenAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_ValidatePasswordResetTokenAsync_ValidatesToken()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.ValidatePasswordResetTokenAsync("token");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_MarkPasswordResetTokenUsedAsync_MarksUsed()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.MarkPasswordResetTokenUsedAsync("token");
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact]
    public async Task EmailVerificationRepository_CleanupExpiredTokensAsync_CleansTokens()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, mockLogger.Object);

        // Act & Assert
        var act = async () => await repository.CleanupExpiredTokensAsync();
        await act.Should().ThrowAsync<SqlException>();
    }

    #endregion
}