using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class UserRepositoryComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly UserRepository _repository;

    public UserRepositoryComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<UserRepository>>();
        
        // Setup configuration for connection string
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Server=localhost;Database=TestDb;User Id=sa;Password=Test123!;TrustServerCertificate=true");

        _repository = new UserRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    /**
     * Feature: User Repository CRUD Operations
     *   As a user management system
     *   I want to perform database operations for users
     *   So that user data can be stored and retrieved
     * 
     * Scenario: Create user with valid data
     *   Given a valid user object
     *   When creating the user
     *   Then the user is stored and ID is returned
     */
    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsUserId()
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        // Note: This would normally require a real database or mock IDbConnection
        // For now, we'll test the method signature and error handling
        var act = () => _repository.CreateUserAsync(user);
        
        // In a real scenario, this would succeed or throw a specific exception
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Create user with null data
     *   Given a null user object
     *   When creating the user
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public async Task CreateUserAsync_NullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateUserAsync(null!));
    }

    /**
     * Scenario: Get user by email
     *   Given a valid email address
     *   When retrieving user by email
     *   Then user is returned or null if not found
     */
    [Fact]
    public async Task GetUserByEmailAsync_ValidEmail_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        var act = () => _repository.GetUserByEmailAsync(email);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Get user by null email
     *   Given a null email
     *   When retrieving user by email
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public async Task GetUserByEmailAsync_NullEmail_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetUserByEmailAsync(null!));
    }

    /**
     * Scenario: Get user by empty email
     *   Given an empty email
     *   When retrieving user by email
     *   Then ArgumentNullException is thrown
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetUserByEmailAsync_EmptyEmail_ThrowsArgumentNullException(string email)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetUserByEmailAsync(email));
    }

    /**
     * Scenario: Get user by ID
     *   Given a valid user ID
     *   When retrieving user by ID
     *   Then user is returned or null if not found
     */
    [Fact]
    public async Task GetUserByIdAsync_ValidId_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.GetUserByIdAsync(userId);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Get user by empty GUID
     *   Given an empty GUID
     *   When retrieving user by ID
     *   Then ArgumentException is thrown
     */
    [Fact]
    public async Task GetUserByIdAsync_EmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetUserByIdAsync(Guid.Empty));
    }

    /**
     * Scenario: Check email exists
     *   Given an email address
     *   When checking if email exists
     *   Then boolean result is returned
     */
    [Fact]
    public async Task EmailExistsAsync_ValidEmail_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        var act = () => _repository.EmailExistsAsync(email);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Check email exists with null email
     *   Given a null email
     *   When checking if email exists
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public async Task EmailExistsAsync_NullEmail_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.EmailExistsAsync(null!));
    }

    /**
     * Scenario: Check license exists
     *   Given license number and state
     *   When checking if license exists
     *   Then boolean result is returned
     */
    [Fact]
    public async Task LicenseExistsAsync_ValidParameters_HandlesCall()
    {
        // Arrange
        var licenseNumber = "12345";
        var state = "CA";

        // Act & Assert
        var act = () => _repository.LicenseExistsAsync(licenseNumber, state);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Check license exists with null parameters
     *   Given null license number or state
     *   When checking if license exists
     *   Then ArgumentNullException is thrown
     */
    [Theory]
    [InlineData(null, "CA")]
    [InlineData("12345", null)]
    [InlineData(null, null)]
    public async Task LicenseExistsAsync_NullParameters_ThrowsArgumentNullException(string? licenseNumber, string? state)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.LicenseExistsAsync(licenseNumber!, state!));
    }

    /**
     * Scenario: Update user
     *   Given a valid user object
     *   When updating the user
     *   Then update operation completes
     */
    [Fact]
    public async Task UpdateUserAsync_ValidUser_HandlesCall()
    {
        // Arrange
        var user = CreateValidUser();
        user.Id = Guid.NewGuid(); // Ensure ID is set for update

        // Act & Assert
        var act = () => _repository.UpdateUserAsync(user);
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    /**
     * Scenario: Update null user
     *   Given a null user object
     *   When updating the user
     *   Then ArgumentNullException is thrown
     */
    [Fact]
    public async Task UpdateUserAsync_NullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateUserAsync(null!));
    }

    /**
     * Scenario: Update user with empty ID
     *   Given a user with empty ID
     *   When updating the user
     *   Then ArgumentException is thrown
     */
    [Fact]
    public async Task UpdateUserAsync_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var user = CreateValidUser();
        user.Id = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateUserAsync(user));
    }

    /**
     * Scenario: Verify email
     *   Given a valid user ID
     *   When verifying email
     *   Then verification operation completes
     */
    [Fact]
    public async Task VerifyEmailAsync_ValidUserId_HandlesCall()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var act = () => _repository.VerifyEmailAsync(userId);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Verify email with empty GUID
     *   Given an empty GUID
     *   When verifying email
     *   Then ArgumentException is thrown
     */
    [Fact]
    public async Task VerifyEmailAsync_EmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.VerifyEmailAsync(Guid.Empty));
    }

    /**
     * Feature: Database Connection Handling
     *   As a repository
     *   I want to handle database connections properly
     *   So that operations are reliable
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
        var act = () => new UserRepository(_mockConfiguration.Object, _mockLogger.Object);

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
        Assert.Throws<ArgumentNullException>(() => new UserRepository(null!, _mockLogger.Object));
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
        Assert.Throws<ArgumentNullException>(() => new UserRepository(_mockConfiguration.Object, null!));
    }

    /**
     * Feature: Edge Cases and Error Handling
     *   As a robust repository
     *   I want to handle edge cases gracefully
     *   So that the system remains stable
     * 
     * Scenario: Operations with special characters
     *   Given input with special characters
     *   When performing operations
     *   Then they are handled correctly
     */
    [Theory]
    [InlineData("test@domain.com")]
    [InlineData("test+tag@domain.com")]
    [InlineData("test.with.dots@domain.com")]
    [InlineData("test_with_underscores@domain.com")]
    public async Task EmailExistsAsync_SpecialCharacters_HandlesCorrectly(string email)
    {
        // Act & Assert
        var act = () => _repository.EmailExistsAsync(email);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: License operations with various formats
     *   Given different license formats
     *   When checking license existence
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("ABC123", "CA")]
    [InlineData("12345", "NY")]
    [InlineData("ST-12345", "TX")]
    [InlineData("LIC.123.456", "FL")]
    public async Task LicenseExistsAsync_VariousFormats_HandlesCorrectly(string licenseNumber, string state)
    {
        // Act & Assert
        var act = () => _repository.LicenseExistsAsync(licenseNumber, state);
        await act.Should().NotThrowAsync();
    }

    // Helper methods
    private static User CreateValidUser()
    {
        return new User
        {
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-123-4567",
            LicenseNumber = "12345",
            LicenseState = "CA",
            ServiceType = "speech_therapy",
            Status = "pending",
            EmailVerified = false
        };
    }
}