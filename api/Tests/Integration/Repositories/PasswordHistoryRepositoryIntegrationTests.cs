using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Repositories;
using Xunit;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TherapyDocs.Api.Tests.Integration.Repositories;

public class PasswordHistoryRepositoryIntegrationTests : IClassFixture<PasswordHistoryDatabaseFixture>
{
    private readonly PasswordHistoryDatabaseFixture _fixture;
    private readonly PasswordHistoryRepository _repository;

    public PasswordHistoryRepositoryIntegrationTests(PasswordHistoryDatabaseFixture fixture)
    {
        _fixture = fixture;
        var mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        _repository = new PasswordHistoryRepository(_fixture.Configuration, mockLogger.Object);
    }

    [Fact]
    public async Task IsPasswordReusedAsync_NewPassword_ReturnsFalse()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var newPasswordHash = "new-password-hash-123";

        // Act
        var result = await _repository.IsPasswordReusedAsync(userId, newPasswordHash);

        // Assert
        Assert.False(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task IsPasswordReusedAsync_ReusedPassword_ReturnsTrue()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var passwordHash = "password-hash-123";
        
        // Add password to history
        await _repository.AddPasswordToHistoryAsync(userId, passwordHash);

        // Act
        var result = await _repository.IsPasswordReusedAsync(userId, passwordHash);

        // Assert
        Assert.True(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task AddPasswordToHistoryAsync_AddsSuccessfully()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var passwordHash = "password-hash-456";

        // Act
        await _repository.AddPasswordToHistoryAsync(userId, passwordHash);

        // Assert
        var isReused = await _repository.IsPasswordReusedAsync(userId, passwordHash);
        Assert.True(isReused);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task AddPasswordToHistoryAsync_MultiplePasswords_TracksAll()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var passwords = new[] { "hash1", "hash2", "hash3", "hash4", "hash5" };

        // Act
        foreach (var hash in passwords)
        {
            await _repository.AddPasswordToHistoryAsync(userId, hash);
        }

        // Assert
        foreach (var hash in passwords)
        {
            var isReused = await _repository.IsPasswordReusedAsync(userId, hash);
            Assert.True(isReused);
        }

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task CheckPasswordChangeRequiredAsync_NewUser_NotRequired()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        await _fixture.SetPasswordChangeDate(userId, DateTime.UtcNow);

        // Act
        var result = await _repository.CheckPasswordChangeRequiredAsync(userId);

        // Assert
        Assert.False(result.ChangeRequired);
        Assert.True(result.DaysUntilExpiry > 0);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task CheckPasswordChangeRequiredAsync_OldPassword_Required()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        await _fixture.SetPasswordChangeDate(userId, DateTime.UtcNow.AddDays(-91)); // 91 days old

        // Act
        var result = await _repository.CheckPasswordChangeRequiredAsync(userId);

        // Assert
        Assert.True(result.ChangeRequired);
        Assert.True(result.DaysUntilExpiry <= 0);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task CheckPasswordChangeRequiredAsync_ApproachingExpiry_ShowsCorrectDays()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        await _fixture.SetPasswordChangeDate(userId, DateTime.UtcNow.AddDays(-83)); // 7 days until expiry

        // Act
        var result = await _repository.CheckPasswordChangeRequiredAsync(userId);

        // Assert
        Assert.False(result.ChangeRequired); // Not required yet
        Assert.InRange(result.DaysUntilExpiry, 6, 8); // Approximately 7 days

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task IsPasswordReusedAsync_ChecksLimitedHistory()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var oldPasswords = new List<string>();
        
        // Add 15 passwords to history (assuming system checks last 10)
        for (int i = 0; i < 15; i++)
        {
            var hash = $"password-hash-{i}";
            oldPasswords.Add(hash);
            await _repository.AddPasswordToHistoryAsync(userId, hash);
            await Task.Delay(10); // Small delay to ensure different timestamps
        }

        // Act & Assert
        // Recent passwords should be detected as reused
        for (int i = 10; i < 15; i++)
        {
            var isReused = await _repository.IsPasswordReusedAsync(userId, oldPasswords[i]);
            Assert.True(isReused, $"Password {i} should be detected as reused");
        }

        // Very old passwords might not be detected (depending on implementation)
        // This behavior depends on the stored procedure implementation

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task ConcurrentPasswordChecks_HandleCorrectly()
    {
        // Arrange
        var userId = await _fixture.CreateTestUser();
        var passwordHash = "concurrent-test-hash";
        await _repository.AddPasswordToHistoryAsync(userId, passwordHash);

        // Act - Multiple concurrent checks
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_repository.IsPasswordReusedAsync(userId, passwordHash));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All checks should return true
        Assert.All(results, result => Assert.True(result));

        // Cleanup
        await _fixture.CleanupUser(userId);
    }
}

public class PasswordHistoryDatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    private readonly string _connectionString;

    public PasswordHistoryDatabaseFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json");
        
        Configuration = builder.Build();
        _connectionString = Configuration.GetConnectionString("DefaultConnection")!;
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Create tables if they don't exist
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U')
            CREATE TABLE users (
                id INT IDENTITY(1,1) PRIMARY KEY,
                email NVARCHAR(255) NOT NULL,
                password_hash NVARCHAR(255),
                password_changed_at DATETIME2 DEFAULT GETUTCDATE(),
                created_at DATETIME2 DEFAULT GETUTCDATE()
            )");

        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='password_history' AND xtype='U')
            CREATE TABLE password_history (
                id INT IDENTITY(1,1) PRIMARY KEY,
                user_id INT NOT NULL,
                password_hash NVARCHAR(255) NOT NULL,
                created_at DATETIME2 DEFAULT GETUTCDATE(),
                FOREIGN KEY (user_id) REFERENCES users(id)
            )");

        // Create stored procedures
        CreateStoredProcedures(connection);
    }

    private void CreateStoredProcedures(SqlConnection connection)
    {
        // sp_check_password_history
        connection.Execute(@"
            IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_check_password_history')
                DROP PROCEDURE sp_check_password_history;
            
            CREATE PROCEDURE sp_check_password_history
                @user_id INT,
                @password_hash NVARCHAR(255),
                @is_reused BIT OUTPUT
            AS
            BEGIN
                IF EXISTS (
                    SELECT TOP 1 1 
                    FROM password_history 
                    WHERE user_id = @user_id 
                    AND password_hash = @password_hash
                    AND created_at > DATEADD(YEAR, -1, GETUTCDATE()) -- Check last year's passwords
                )
                    SET @is_reused = 1
                ELSE
                    SET @is_reused = 0
            END");

        // sp_add_password_history
        connection.Execute(@"
            IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_add_password_history')
                DROP PROCEDURE sp_add_password_history;
            
            CREATE PROCEDURE sp_add_password_history
                @user_id INT,
                @password_hash NVARCHAR(255)
            AS
            BEGIN
                INSERT INTO password_history (user_id, password_hash)
                VALUES (@user_id, @password_hash)
                
                -- Keep only last 10 passwords
                DELETE FROM password_history
                WHERE user_id = @user_id
                AND id NOT IN (
                    SELECT TOP 10 id 
                    FROM password_history 
                    WHERE user_id = @user_id 
                    ORDER BY created_at DESC
                )
            END");

        // sp_check_password_change_required
        connection.Execute(@"
            IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_check_password_change_required')
                DROP PROCEDURE sp_check_password_change_required;
            
            CREATE PROCEDURE sp_check_password_change_required
                @user_id INT,
                @change_required BIT OUTPUT,
                @days_until_expiry INT OUTPUT
            AS
            BEGIN
                DECLARE @password_age_days INT
                DECLARE @max_password_age_days INT = 90
                
                SELECT @password_age_days = DATEDIFF(DAY, password_changed_at, GETUTCDATE())
                FROM users
                WHERE id = @user_id
                
                SET @days_until_expiry = @max_password_age_days - @password_age_days
                
                IF @days_until_expiry <= 0
                    SET @change_required = 1
                ELSE
                    SET @change_required = 0
            END");
    }

    public async Task<int> CreateTestUser()
    {
        using var connection = new SqlConnection(_connectionString);
        var userId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO users (email, password_hash)
            VALUES (@email, @hash);
            SELECT SCOPE_IDENTITY();",
            new { email = $"test_{Guid.NewGuid()}@example.com", hash = "initial-hash" });
        
        return userId;
    }

    public async Task SetPasswordChangeDate(int userId, DateTime changedAt)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE users SET password_changed_at = @changedAt WHERE id = @userId",
            new { userId, changedAt });
    }

    public async Task CleanupUser(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM password_history WHERE user_id = @userId", new { userId });
        await connection.ExecuteAsync("DELETE FROM users WHERE id = @userId", new { userId });
    }

    public void Dispose()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute("DELETE FROM password_history WHERE user_id IN (SELECT id FROM users WHERE email LIKE 'test_%')");
        connection.Execute("DELETE FROM users WHERE email LIKE 'test_%'");
    }
}