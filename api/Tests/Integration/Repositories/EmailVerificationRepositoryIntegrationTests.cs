using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using Xunit;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TherapyDocs.Api.Tests.Integration.Repositories;

public class EmailVerificationRepositoryIntegrationTests : IClassFixture<EmailVerificationDatabaseFixture>
{
    private readonly EmailVerificationDatabaseFixture _fixture;
    private readonly EmailVerificationRepository _repository;

    public EmailVerificationRepositoryIntegrationTests(EmailVerificationDatabaseFixture fixture)
    {
        _fixture = fixture;
        var mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        _repository = new EmailVerificationRepository(_fixture.Configuration, mockLogger.Object);
    }

    [Fact]
    public async Task CreateVerificationTokenAsync_CreatesUniqueToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);

        // Act
        var token1 = await _repository.CreateVerificationTokenAsync(userId);
        var token2 = await _repository.CreateVerificationTokenAsync(userId);

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2); // Each token should be unique
        
        // Verify tokens are URL-safe
        Assert.DoesNotContain("+", token1);
        Assert.DoesNotContain("/", token1);
        Assert.DoesNotContain("=", token1);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task GetTokenAsync_ValidToken_ReturnsTokenDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        var token = await _repository.CreateVerificationTokenAsync(userId);

        // Act
        var result = await _repository.GetTokenAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(token, result.Token);
        Assert.Null(result.UsedAt);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task GetTokenAsync_InvalidToken_ReturnsNull()
    {
        // Act
        var result = await _repository.GetTokenAsync("invalid-token-12345");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task MarkTokenUsedAsync_ValidUnusedToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        var token = await _repository.CreateVerificationTokenAsync(userId);

        // Act
        var result = await _repository.MarkTokenUsedAsync(token);

        // Assert
        Assert.True(result);

        // Verify token is marked as used
        var tokenDetails = await _repository.GetTokenAsync(token);
        Assert.NotNull(tokenDetails);
        Assert.NotNull(tokenDetails.UsedAt);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task MarkTokenUsedAsync_AlreadyUsedToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        var token = await _repository.CreateVerificationTokenAsync(userId);
        
        // Use the token once
        await _repository.MarkTokenUsedAsync(token);

        // Act - Try to use it again
        var result = await _repository.MarkTokenUsedAsync(token);

        // Assert
        Assert.False(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task MarkTokenUsedAsync_InvalidToken_ReturnsFalse()
    {
        // Act
        var result = await _repository.MarkTokenUsedAsync("invalid-token-12345");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasValidTokenAsync_UserWithValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        await _repository.CreateVerificationTokenAsync(userId);

        // Act
        var result = await _repository.HasValidTokenAsync(userId);

        // Assert
        Assert.True(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task HasValidTokenAsync_UserWithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        await _fixture.CreateExpiredToken(userId);

        // Act
        var result = await _repository.HasValidTokenAsync(userId);

        // Assert
        Assert.False(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task HasValidTokenAsync_UserWithUsedToken_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        var token = await _repository.CreateVerificationTokenAsync(userId);
        await _repository.MarkTokenUsedAsync(token);

        // Act
        var result = await _repository.HasValidTokenAsync(userId);

        // Assert
        Assert.False(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task HasValidTokenAsync_UserWithNoTokens_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);

        // Act
        var result = await _repository.HasValidTokenAsync(userId);

        // Assert
        Assert.False(result);

        // Cleanup
        await _fixture.CleanupUser(userId);
    }

    [Fact]
    public async Task CreateVerificationTokenAsync_ConcurrentRequests_CreatesMultipleTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _fixture.CreateTestUser(userId);
        var tasks = new List<Task<string>>();

        // Act - Create multiple tokens concurrently
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_repository.CreateVerificationTokenAsync(userId));
        }

        var tokens = await Task.WhenAll(tasks);

        // Assert - All tokens should be unique
        Assert.Equal(5, tokens.Distinct().Count());

        // Cleanup
        await _fixture.CleanupUser(userId);
    }
}

public class EmailVerificationDatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    private readonly string _connectionString;

    public EmailVerificationDatabaseFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json");
        
        Configuration = builder.Build();
        _connectionString = Configuration.GetConnectionString("DefaultConnection")!;
        
        // Initialize test database
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Create tables if they don't exist
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U')
            CREATE TABLE users (
                id UNIQUEIDENTIFIER PRIMARY KEY,
                email NVARCHAR(255) NOT NULL,
                first_name NVARCHAR(100),
                last_name NVARCHAR(100),
                password_hash NVARCHAR(255),
                email_verified BIT DEFAULT 0,
                status NVARCHAR(50),
                created_at DATETIME2 DEFAULT GETUTCDATE()
            )");

        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='email_verification_tokens' AND xtype='U')
            CREATE TABLE email_verification_tokens (
                id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                user_id UNIQUEIDENTIFIER NOT NULL,
                token NVARCHAR(255) NOT NULL UNIQUE,
                expires_at DATETIME2 NOT NULL,
                used_at DATETIME2 NULL,
                created_at DATETIME2 DEFAULT GETUTCDATE(),
                FOREIGN KEY (user_id) REFERENCES users(id)
            )");
    }

    public async Task CreateTestUser(Guid userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, email, first_name, last_name, password_hash, email_verified, status)
            VALUES (@id, @email, 'Test', 'User', 'hash', 0, 'pending')",
            new { id = userId, email = $"test_{userId}@example.com" });
    }

    public async Task CreateExpiredToken(Guid userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(@"
            INSERT INTO email_verification_tokens (user_id, token, expires_at)
            VALUES (@userId, @token, @expiresAt)",
            new 
            { 
                userId = userId,
                token = $"expired-token-{Guid.NewGuid()}",
                expiresAt = DateTime.UtcNow.AddDays(-1)
            });
    }

    public async Task CleanupUser(Guid userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM email_verification_tokens WHERE user_id = @userId", new { userId });
        await connection.ExecuteAsync("DELETE FROM users WHERE id = @userId", new { userId });
    }

    public void Dispose()
    {
        // Cleanup test database
        using var connection = new SqlConnection(_connectionString);
        connection.Execute("TRUNCATE TABLE email_verification_tokens");
        connection.Execute("DELETE FROM users WHERE email LIKE 'test_%'");
    }
}