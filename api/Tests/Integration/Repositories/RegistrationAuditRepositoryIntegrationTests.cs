using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Repositories;
using Xunit;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TherapyDocs.Api.Tests.Integration.Repositories;

public class RegistrationAuditRepositoryIntegrationTests : IClassFixture<RegistrationAuditDatabaseFixture>
{
    private readonly RegistrationAuditDatabaseFixture _fixture;
    private readonly RegistrationAuditRepository _repository;
    private readonly Mock<ILogger<RegistrationAuditRepository>> _mockLogger;

    public RegistrationAuditRepositoryIntegrationTests(RegistrationAuditDatabaseFixture fixture)
    {
        _fixture = fixture;
        _mockLogger = new Mock<ILogger<RegistrationAuditRepository>>();
        _repository = new RegistrationAuditRepository(_fixture.Configuration, _mockLogger.Object);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_SuccessfulAttempt_LogsCorrectly()
    {
        // Arrange
        var email = $"success_{Guid.NewGuid()}@example.com";
        var licenseNumber = "LIC123456";
        var licenseState = "CA";
        var ipAddress = "192.168.1.100";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        // Act
        await _repository.LogRegistrationAttemptAsync(
            email, licenseNumber, licenseState, 
            success: true, failureReason: null, 
            ipAddress, userAgent);

        // Assert
        var logEntry = await _fixture.GetLatestAuditLog(email);
        Assert.NotNull(logEntry);
        Assert.Equal(email, logEntry.Email);
        Assert.Equal(licenseNumber, logEntry.LicenseNumber);
        Assert.Equal(licenseState, logEntry.LicenseState);
        Assert.True(logEntry.Success);
        Assert.Null(logEntry.FailureReason);
        Assert.Equal(ipAddress, logEntry.IpAddress);
        Assert.Equal(userAgent, logEntry.UserAgent);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_FailedAttempt_LogsWithReason()
    {
        // Arrange
        var email = $"failed_{Guid.NewGuid()}@example.com";
        var failureReason = "Invalid license number";

        // Act
        await _repository.LogRegistrationAttemptAsync(
            email, "INVALID", "CA",
            success: false, failureReason: failureReason,
            ipAddress: null, userAgent: null);

        // Assert
        var logEntry = await _fixture.GetLatestAuditLog(email);
        Assert.NotNull(logEntry);
        Assert.False(logEntry.Success);
        Assert.Equal(failureReason, logEntry.FailureReason);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_NullOptionalFields_HandlesCorrectly()
    {
        // Arrange
        var email = $"minimal_{Guid.NewGuid()}@example.com";

        // Act
        await _repository.LogRegistrationAttemptAsync(
            email, null, null,
            success: true, failureReason: null,
            ipAddress: null, userAgent: null);

        // Assert
        var logEntry = await _fixture.GetLatestAuditLog(email);
        Assert.NotNull(logEntry);
        Assert.Equal(email, logEntry.Email);
        Assert.Null(logEntry.LicenseNumber);
        Assert.Null(logEntry.LicenseState);
        Assert.Null(logEntry.IpAddress);
        Assert.Null(logEntry.UserAgent);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_VeryLongStrings_TruncatesAppropriately()
    {
        // Arrange
        var email = $"long_{Guid.NewGuid()}@example.com";
        var veryLongFailureReason = new string('X', 1000);
        var veryLongUserAgent = new string('A', 500);

        // Act
        await _repository.LogRegistrationAttemptAsync(
            email, "LIC123", "CA",
            success: false, failureReason: veryLongFailureReason,
            ipAddress: "192.168.1.1", userAgent: veryLongUserAgent);

        // Assert
        var logEntry = await _fixture.GetLatestAuditLog(email);
        Assert.NotNull(logEntry);
        // Verify strings were stored (possibly truncated based on DB schema)
        Assert.NotNull(logEntry.FailureReason);
        Assert.NotNull(logEntry.UserAgent);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_SqlInjectionAttempt_HandlesSecurely()
    {
        // Arrange
        var email = $"sqli_{Guid.NewGuid()}@example.com";
        var maliciousLicense = "'; DROP TABLE registration_audit_logs; --";
        var maliciousFailure = "Invalid'); DELETE FROM users; --";

        // Act
        await _repository.LogRegistrationAttemptAsync(
            email, maliciousLicense, "CA",
            success: false, failureReason: maliciousFailure,
            ipAddress: "192.168.1.1", userAgent: "Mozilla");

        // Assert
        var logEntry = await _fixture.GetLatestAuditLog(email);
        Assert.NotNull(logEntry);
        Assert.Equal(maliciousLicense, logEntry.LicenseNumber); // Should be stored as-is, not executed
        Assert.Equal(maliciousFailure, logEntry.FailureReason);
        
        // Verify table still exists
        Assert.True(await _fixture.TableExists("registration_audit_logs"));
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_DatabaseError_DoesNotThrow()
    {
        // Arrange
        var email = "test@example.com";
        
        // Temporarily break the connection string
        var brokenConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=invalid;Database=invalid;"
            })
            .Build();
        
        var brokenRepository = new RegistrationAuditRepository(brokenConfig, _mockLogger.Object);

        // Act & Assert - Should not throw
        await brokenRepository.LogRegistrationAttemptAsync(
            email, null, null, false, "Test", null, null);

        // Verify error was logged
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error logging registration attempt")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Once);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_MultipleAttemptsSameEmail_LogsAll()
    {
        // Arrange
        var email = $"multiple_{Guid.NewGuid()}@example.com";

        // Act - Log multiple attempts
        await _repository.LogRegistrationAttemptAsync(
            email, "LIC111", "CA", false, "Invalid license", "192.168.1.1", "Mozilla");
        
        await Task.Delay(100); // Small delay to ensure different timestamps
        
        await _repository.LogRegistrationAttemptAsync(
            email, "LIC222", "CA", false, "License expired", "192.168.1.2", "Chrome");
        
        await Task.Delay(100);
        
        await _repository.LogRegistrationAttemptAsync(
            email, "LIC333", "CA", true, null, "192.168.1.3", "Safari");

        // Assert
        var attempts = await _fixture.GetAllAuditLogs(email);
        Assert.Equal(3, attempts.Count);
        
        // Verify they're ordered by time (most recent first)
        Assert.True(attempts[0].Success); // Most recent - successful
        Assert.Equal("License expired", attempts[1].FailureReason);
        Assert.Equal("Invalid license", attempts[2].FailureReason);
    }

    [Fact]
    public async Task LogRegistrationAttemptAsync_ConcurrentLogs_HandlesCorrectly()
    {
        // Arrange
        var baseEmail = $"concurrent_{Guid.NewGuid()}";
        var tasks = new List<Task>();

        // Act - Log 10 attempts concurrently
        for (int i = 0; i < 10; i++)
        {
            var email = $"{baseEmail}_{i}@example.com";
            var task = _repository.LogRegistrationAttemptAsync(
                email, $"LIC{i:000}", "CA", true, null, "192.168.1.1", "Mozilla");
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert - All logs should be created
        for (int i = 0; i < 10; i++)
        {
            var email = $"{baseEmail}_{i}@example.com";
            var log = await _fixture.GetLatestAuditLog(email);
            Assert.NotNull(log);
            Assert.Equal($"LIC{i:000}", log.LicenseNumber);
        }
    }
}

public class RegistrationAuditLog
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string? LicenseNumber { get; set; }
    public string? LicenseState { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RegistrationAuditDatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    private readonly string _connectionString;

    public RegistrationAuditDatabaseFixture()
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
        
        // Create table if it doesn't exist
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='registration_audit_logs' AND xtype='U')
            CREATE TABLE registration_audit_logs (
                id INT IDENTITY(1,1) PRIMARY KEY,
                email NVARCHAR(255) NOT NULL,
                license_number NVARCHAR(100) NULL,
                license_state NVARCHAR(10) NULL,
                success BIT NOT NULL,
                failure_reason NVARCHAR(500) NULL,
                ip_address NVARCHAR(45) NULL,
                user_agent NVARCHAR(500) NULL,
                created_at DATETIME2 DEFAULT GETUTCDATE()
            )");

        // Create stored procedure
        connection.Execute(@"
            IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_registration_audit_log')
                DROP PROCEDURE sp_registration_audit_log;
            
            CREATE PROCEDURE sp_registration_audit_log
                @email NVARCHAR(255),
                @license_number NVARCHAR(100) = NULL,
                @license_state NVARCHAR(10) = NULL,
                @success BIT,
                @failure_reason NVARCHAR(500) = NULL,
                @ip_address NVARCHAR(45) = NULL,
                @user_agent NVARCHAR(500) = NULL
            AS
            BEGIN
                INSERT INTO registration_audit_logs 
                    (email, license_number, license_state, success, failure_reason, ip_address, user_agent)
                VALUES 
                    (@email, @license_number, @license_state, @success, @failure_reason, @ip_address, @user_agent)
            END");
    }

    public async Task<RegistrationAuditLog?> GetLatestAuditLog(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<RegistrationAuditLog>(@"
            SELECT TOP 1 * FROM registration_audit_logs 
            WHERE email = @email 
            ORDER BY created_at DESC", 
            new { email });
    }

    public async Task<List<RegistrationAuditLog>> GetAllAuditLogs(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        var logs = await connection.QueryAsync<RegistrationAuditLog>(@"
            SELECT * FROM registration_audit_logs 
            WHERE email = @email 
            ORDER BY created_at DESC", 
            new { email });
        return logs.ToList();
    }

    public async Task<bool> TableExists(string tableName)
    {
        using var connection = new SqlConnection(_connectionString);
        var exists = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM sysobjects 
            WHERE name = @tableName AND xtype = 'U'", 
            new { tableName });
        return exists > 0;
    }

    public void Dispose()
    {
        using var connection = new SqlConnection(_connectionString);
        // Clean up test data
        connection.Execute("DELETE FROM registration_audit_logs WHERE email LIKE '%_@example.com'");
    }
}