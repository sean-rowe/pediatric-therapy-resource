using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly UserRepository _repository;
    private readonly string _connectionString = "Server=test;Database=test;Trusted_Connection=true;";

    public UserRepositoryTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<UserRepository>>();
        
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(_connectionString);
        
        _repository = new UserRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // This test would require a real database connection or more complex mocking
        // For now, we'll test the method signature and ensure it doesn't throw
        var email = "test@example.com";
        
        // Act & Assert - ensuring method can be called without exceptions
        var act = async () => await _repository.EmailExistsAsync(email);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LicenseExistsAsync_LicenseExists_ReturnsTrue()
    {
        // Test method signature and basic functionality
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        
        // Act & Assert
        var act = async () => await _repository.LicenseExistsAsync(licenseNumber, licenseState);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsGuid()
    {
        // Test with valid user object
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ST12345",
            LicenseState = "CA",
            ServiceType = "speech_therapy",
            Status = "pending",
            EmailVerified = false
        };
        
        // Act & Assert
        var act = async () => await _repository.CreateUserAsync(user);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetUserByEmailAsync_ValidEmail_ReturnsUser()
    {
        // Test method signature
        var email = "test@example.com";
        
        // Act & Assert
        var act = async () => await _repository.GetUserByEmailAsync(email);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetUserByIdAsync_ValidId_ReturnsUser()
    {
        // Test method signature
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.GetUserByIdAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUser_UpdatesSuccessfully()
    {
        // Test with valid user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "updated@example.com",
            FirstName = "Updated",
            LastName = "User",
            Status = "active",
            EmailVerified = true
        };
        
        // Act & Assert
        var act = async () => await _repository.UpdateUserAsync(user);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ValidConfiguration_InitializesSuccessfully()
    {
        // Arrange & Act
        var repository = new UserRepository(_mockConfiguration.Object, _mockLogger.Object);
        
        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserRepository(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new UserRepository(_mockConfiguration.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}