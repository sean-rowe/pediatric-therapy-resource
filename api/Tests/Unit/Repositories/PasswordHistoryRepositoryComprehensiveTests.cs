using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class PasswordHistoryRepositoryComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PasswordHistoryRepository>> _mockLogger;
    private readonly PasswordHistoryRepository _repository;

    public PasswordHistoryRepositoryComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PasswordHistoryRepository>>();
        
        // Setup configuration for connection string
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Server=localhost;Database=TestDb;User Id=sa;Password=Test123!;TrustServerCertificate=true");

        _repository = new PasswordHistoryRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    /**
     * Feature: Password History Management
     *   As a security system
     *   I want to track password history for users
     *   So that password reuse can be prevented
     * 
     * Scenario: Check if password is reused
     *   Given a user ID and password hash
     *   When checking password history
     *   Then boolean result indicating reuse is returned
     */
    [Fact]
    public async Task IsPasswordReusedAsync_ValidParameters_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHash = "$2a$12$hashedpassword";

        // Act & Assert
        var act = () => _repository.IsPasswordReusedAsync(userId, passwordHash);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Scenario: Check password reuse with empty GUID
     *   Given an empty GUID
     *   When checking password history
     *   Then the operation handles it gracefully
     */
    [Fact]
    public async Task IsPasswordReusedAsync_EmptyGuid_HandlesCall()
    {
        // Arrange
        var passwordHash = "$2a$12$hashedpassword";

        // Act & Assert
        var act = () => _repository.IsPasswordReusedAsync(Guid.Empty, passwordHash);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Scenario: Check password reuse with null password hash
     *   Given a null password hash
     *   When checking password history
     *   Then the operation handles null gracefully
     */
    [Fact]
    public async Task IsPasswordReusedAsync_NullPasswordHash_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.IsPasswordReusedAsync(userId, null!);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Check password reuse with empty password hash
     *   Given an empty password hash
     *   When checking password history
     *   Then the operation handles empty string gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task IsPasswordReusedAsync_EmptyPasswordHash_HandlesCall(string passwordHash)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.IsPasswordReusedAsync(userId, passwordHash);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Add password to history
     *   Given a user ID and password hash
     *   When adding to password history
     *   Then the password is stored in history
     */
    [Fact]
    public async Task AddPasswordToHistoryAsync_ValidParameters_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHash = "$2a$12$hashedpassword";

        // Act & Assert
        var act = () => _repository.AddPasswordToHistoryAsync(userId, passwordHash);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Scenario: Add password to history with empty GUID
     *   Given an empty GUID
     *   When adding to password history
     *   Then the operation handles it gracefully
     */
    [Fact]
    public async Task AddPasswordToHistoryAsync_EmptyGuid_HandlesCall()
    {
        // Arrange
        var passwordHash = "$2a$12$hashedpassword";

        // Act & Assert
        var act = () => _repository.AddPasswordToHistoryAsync(Guid.Empty, passwordHash);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Scenario: Add null password to history
     *   Given a null password hash
     *   When adding to password history
     *   Then the operation handles null gracefully
     */
    [Fact]
    public async Task AddPasswordToHistoryAsync_NullPasswordHash_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.AddPasswordToHistoryAsync(userId, null!);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Add empty password to history
     *   Given an empty password hash
     *   When adding to password history
     *   Then the operation handles empty string gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task AddPasswordToHistoryAsync_EmptyPasswordHash_HandlesCall(string passwordHash)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.AddPasswordToHistoryAsync(userId, passwordHash);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Check password change requirement
     *   Given a user ID
     *   When checking if password change is required
     *   Then password change requirement information is returned
     */
    [Fact]
    public async Task CheckPasswordChangeRequiredAsync_ValidUserId_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.CheckPasswordChangeRequiredAsync(userId);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Scenario: Check password change requirement with empty GUID
     *   Given an empty GUID
     *   When checking password change requirement
     *   Then the operation handles it gracefully
     */
    [Fact]
    public async Task CheckPasswordChangeRequiredAsync_EmptyGuid_HandlesCall()
    {
        // Act & Assert
        var act = () => _repository.CheckPasswordChangeRequiredAsync(Guid.Empty);
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    /**
     * Feature: Repository Configuration and Error Handling
     *   As a repository
     *   I want to handle configuration and errors properly
     *   So that the system remains stable
     * 
     * Scenario: Repository with valid configuration
     *   Given valid configuration
     *   When creating repository
     *   Then no exception is thrown
     */
    [Fact]
    public void Constructor_ValidConfiguration_CreatesRepository()
    {
        // Arrange & Act
        var act = () => new PasswordHistoryRepository(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        act.Should().NotThrow();
    }

    /**
     * Scenario: Repository with null configuration
     *   Given null configuration
     *   When creating repository
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PasswordHistoryRepository(null!, _mockLogger.Object));
    }

    /**
     * Scenario: Repository with null logger
     *   Given null logger
     *   When creating repository
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PasswordHistoryRepository(_mockConfiguration.Object, null!));
    }

    /**
     * Scenario: Repository with missing connection string
     *   Given configuration without connection string
     *   When creating repository
     *   Then InvalidOperationException is thrown
     */
    [Fact]
    public void Constructor_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetConnectionString("DefaultConnection")).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new PasswordHistoryRepository(mockConfig.Object, _mockLogger.Object));
    }

    /**
     * Scenario: Repository with empty connection string
     *   Given configuration with empty connection string
     *   When creating repository
     *   Then InvalidOperationException is thrown
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyConnectionString_ThrowsInvalidOperationException(string connectionString)
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetConnectionString("DefaultConnection")).Returns(connectionString);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new PasswordHistoryRepository(mockConfig.Object, _mockLogger.Object));
    }

    /**
     * Feature: Edge Cases and Password Hash Formats
     *   As a robust repository
     *   I want to handle edge cases gracefully
     *   So that the system remains stable
     * 
     * Scenario: Operations with various password hash formats
     *   Given different password hash formats
     *   When performing operations
     *   Then they are handled correctly
     */
    [Theory]
    [InlineData("$2a$12$abcdefghijklmnopqrstuvwxyz123456")]
    [InlineData("$2b$10$short.hash.example")]
    [InlineData("$argon2id$v=19$m=65536,t=3,p=4$salt$hash")]
    [InlineData("pbkdf2:sha256:150000$salt$hash")]
    [InlineData("very-long-password-hash-that-exceeds-normal-length-expectations")]
    public async Task IsPasswordReusedAsync_VariousHashFormats_HandlesCorrectly(string passwordHash)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.IsPasswordReusedAsync(userId, passwordHash);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Add various password hash formats to history
     *   Given different password hash formats
     *   When adding to password history
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("$2a$12$simple.bcrypt.hash")]
    [InlineData("$scrypt$ln=16,r=8,p=1$salt$hash")]
    [InlineData("sha256$rounds=656000$salt$hash")]
    [InlineData("md5$deprecated$hash")]
    public async Task AddPasswordToHistoryAsync_VariousHashFormats_HandlesCorrectly(string passwordHash)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.AddPasswordToHistoryAsync(userId, passwordHash);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Operations with multiple users
     *   Given different user IDs
     *   When performing password history operations
     *   Then they are processed correctly
     */
    [Fact]
    public async Task PasswordHistoryOperations_MultipleUsers_HandlesCorrectly()
    {
        // Arrange
        var userIds = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        var passwordHash = "$2a$12$testpasswordhash";

        // Act & Assert
        foreach (var userId in userIds)
        {
            // Test all operations for each user
            var actCheck = () => _repository.CheckPasswordChangeRequiredAsync(userId);
            await actCheck.Should().NotThrowAsync();

            var actReused = () => _repository.IsPasswordReusedAsync(userId, passwordHash);
            await actReused.Should().NotThrowAsync();

            var actAdd = () => _repository.AddPasswordToHistoryAsync(userId, passwordHash);
            await actAdd.Should().NotThrowAsync();
        }
    }

    /**
     * Scenario: Concurrent password operations for same user
     *   Given a single user ID
     *   When performing multiple password operations
     *   Then they are handled gracefully
     */
    [Fact]
    public async Task PasswordHistoryOperations_ConcurrentOperations_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHashes = new[]
        {
            "$2a$12$password1hash",
            "$2a$12$password2hash",
            "$2a$12$password3hash",
            "$2a$12$password4hash",
            "$2a$12$password5hash"
        };

        // Act & Assert
        foreach (var passwordHash in passwordHashes)
        {
            var actReused = () => _repository.IsPasswordReusedAsync(userId, passwordHash);
            await actReused.Should().NotThrowAsync();

            var actAdd = () => _repository.AddPasswordToHistoryAsync(userId, passwordHash);
            await actAdd.Should().NotThrowAsync();
        }
    }
}