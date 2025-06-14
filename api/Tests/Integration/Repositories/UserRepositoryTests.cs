using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using Microsoft.Data.SqlClient;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Integration.Repositories;

[Collection("Database")]
public class UserRepositoryTests : IDisposable
{
    private readonly UserRepository _repository;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly string _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=TherapyDocsTest;Trusted_Connection=true;MultipleActiveResultSets=true";

    public UserRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
        
        // Setup test configuration
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _testConnectionString
            })
            .Build();

        _repository = new UserRepository(_configuration, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var testEmail = "test.user@example.com";
        var testUser = await CreateTestUserAsync();

        // Act
        var result = await _repository.GetUserByEmailAsync(testEmail);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testEmail, result.Email);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.NotNull(result.PasswordHash);
        Assert.Equal("speech_therapy", result.ServiceType);
    }

    [Fact]
    public async Task GetUserByEmailAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var nonExistentEmail = "doesnotexist@example.com";

        // Act
        var result = await _repository.GetUserByEmailAsync(nonExistentEmail);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = await CreateTestUserAsync();

        // Act
        var result = await _repository.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test.user@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetUserByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        await CreateTestUserAsync();

        // Act
        var result = await _repository.EmailExistsAsync("test.user@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        // Arrange
        var nonExistentEmail = "new.user@example.com";

        // Act
        var result = await _repository.EmailExistsAsync(nonExistentEmail);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LicenseExistsAsync_ExistingLicense_ReturnsTrue()
    {
        // Arrange
        await CreateTestUserAsync();

        // Act
        var result = await _repository.LicenseExistsAsync("TEST12345", "CA");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LicenseExistsAsync_NonExistentLicense_ReturnsFalse()
    {
        // Arrange
        var nonExistentLicense = "NONEXIST999";

        // Act
        var result = await _repository.LicenseExistsAsync(nonExistentLicense, "TX");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsUserId()
    {
        // Arrange
        var user = new User
        {
            Email = $"create.test.{Guid.NewGuid()}@example.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Create",
            LastName = "Test",
            Phone = "555-0123",
            LicenseNumber = $"CREATE{DateTime.Now.Ticks}",
            LicenseState = "NY",
            ServiceType = "occupational_therapy"
        };

        // Act
        var userId = await _repository.CreateUserAsync(user);

        // Assert
        Assert.NotEqual(Guid.Empty, userId);
        
        // Verify user was created
        var createdUser = await _repository.GetUserByIdAsync(userId);
        Assert.NotNull(createdUser);
        Assert.Equal(user.Email, createdUser.Email);
        Assert.Equal(user.FirstName, createdUser.FirstName);
        Assert.Equal(user.LastName, createdUser.LastName);
    }

    [Fact]
    public async Task CreateUserAsync_UserWithNullOptionalFields_CreatesSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Email = $"minimal.{Guid.NewGuid()}@example.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Minimal",
            LastName = "User",
            ServiceType = "physical_therapy",
            Phone = null,
            LicenseNumber = null,
            LicenseState = null
        };

        // Act
        var userId = await _repository.CreateUserAsync(user);

        // Assert
        Assert.NotEqual(Guid.Empty, userId);
        
        var createdUser = await _repository.GetUserByIdAsync(userId);
        Assert.NotNull(createdUser);
        Assert.Null(createdUser.Phone);
        Assert.Null(createdUser.LicenseNumber);
        Assert.Null(createdUser.LicenseState);
    }

    [Fact]
    public async Task UpdateUserAsync_ExistingUser_UpdatesSuccessfully()
    {
        // Arrange
        var userId = await CreateTestUserAsync();
        var user = await _repository.GetUserByIdAsync(userId);
        Assert.NotNull(user);
        
        user.FirstName = "Updated";
        user.LastName = "Name";
        user.Phone = "555-9999";
        user.EmailVerified = true;
        user.Status = "active";
        user.IsActive = true;

        // Act
        await _repository.UpdateUserAsync(user);

        // Assert
        var updatedUser = await _repository.GetUserByIdAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("Name", updatedUser.LastName);
        Assert.Equal("555-9999", updatedUser.Phone);
        Assert.True(updatedUser.EmailVerified);
        Assert.Equal("active", updatedUser.Status);
        Assert.True(updatedUser.IsActive);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExistingUser_ReturnsTrue()
    {
        // Arrange
        var userId = await CreateTestUserAsync();

        // Act
        var result = await _repository.VerifyEmailAsync(userId);

        // Assert
        Assert.True(result);
        
        // Verify email is now verified
        var user = await _repository.GetUserByIdAsync(userId);
        Assert.NotNull(user);
        Assert.True(user.EmailVerified);
    }

    [Fact]
    public async Task VerifyEmailAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.VerifyEmailAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_DatabaseError_LogsAndThrows()
    {
        // Arrange
        var badConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=badserver;Database=bad;User Id=bad;Password=bad;"
            })
            .Build();
        var badRepository = new UserRepository(badConfig, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(async () => 
            await badRepository.GetUserByEmailAsync("test@test.com"));
        
        // Verify error was logged
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting user by email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var email = $"duplicate.{Guid.NewGuid()}@example.com";
        var user1 = new User
        {
            Email = email,
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "First",
            LastName = "User",
            ServiceType = "speech_therapy"
        };
        
        var user2 = new User
        {
            Email = email, // Same email
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Second",
            LastName = "User",
            ServiceType = "speech_therapy"
        };

        // Act
        await _repository.CreateUserAsync(user1);
        
        // Assert
        await Assert.ThrowsAsync<SqlException>(async () => 
            await _repository.CreateUserAsync(user2));
    }

    [Fact]
    public void GetConnectionString_NoConnectionString_ThrowsException()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var repository = new UserRepository(emptyConfig, _mockLogger.Object);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await repository.GetUserByEmailAsync("test@test.com"));
    }

    [Fact]
    public async Task MapUserFromReader_AllFields_MapsCorrectly()
    {
        // Arrange - Create a user with all fields populated
        var user = new User
        {
            Email = $"fulluser.{Guid.NewGuid()}@example.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Full",
            LastName = "User",
            Phone = "555-1234",
            LicenseNumber = $"FULL{DateTime.Now.Ticks}",
            LicenseState = "FL",
            ServiceType = "speech_therapy"
        };
        
        var userId = await _repository.CreateUserAsync(user);

        // Act
        var retrievedUser = await _repository.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(userId, retrievedUser.Id);
        Assert.Equal(user.Email, retrievedUser.Email);
        Assert.Equal(user.PasswordHash, retrievedUser.PasswordHash);
        Assert.Equal(user.FirstName, retrievedUser.FirstName);
        Assert.Equal(user.LastName, retrievedUser.LastName);
        Assert.Equal(user.Phone, retrievedUser.Phone);
        Assert.Equal(user.LicenseNumber, retrievedUser.LicenseNumber);
        Assert.Equal(user.LicenseState, retrievedUser.LicenseState);
        Assert.Equal(user.ServiceType, retrievedUser.ServiceType);
        Assert.Equal("free", retrievedUser.SubscriptionTier); // Default value
        Assert.Equal("therapist", retrievedUser.Role); // Default value
        Assert.Equal("America/New_York", retrievedUser.Timezone); // Default value
        Assert.Equal("soap", retrievedUser.PreferredNoteTemplate); // Default value
        Assert.True(retrievedUser.AutoSaveNotes); // Default value
        Assert.True(retrievedUser.OfflineSyncEnabled); // Default value
        Assert.True(retrievedUser.PushNotifications); // Default value
        Assert.True(retrievedUser.IsActive); // Default value
        Assert.False(retrievedUser.EmailVerified); // Default value
        Assert.Equal("pending", retrievedUser.Status); // Default value
        Assert.NotEqual(DateTime.MinValue, retrievedUser.CreatedAt);
        Assert.NotEqual(DateTime.MinValue, retrievedUser.UpdatedAt);
    }

    private async Task<Guid> CreateTestUserAsync()
    {
        var user = new User
        {
            Email = "test.user@example.com",
            PasswordHash = "$2a$12$dummy.hash.for.testing",
            FirstName = "Test",
            LastName = "User",
            Phone = "555-0123",
            LicenseNumber = "TEST12345",
            LicenseState = "CA",
            ServiceType = "speech_therapy"
        };

        // Check if user already exists
        var existing = await _repository.GetUserByEmailAsync(user.Email);
        if (existing != null)
        {
            return existing.Id;
        }

        return await _repository.CreateUserAsync(user);
    }

    public void Dispose()
    {
        // Cleanup test data if needed
        // In a real test suite, you might want to clean up test users
    }
}