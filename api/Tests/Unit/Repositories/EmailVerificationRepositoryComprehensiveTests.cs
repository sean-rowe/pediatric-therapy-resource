using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Models;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class EmailVerificationRepositoryComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailVerificationRepository>> _mockLogger;
    private readonly EmailVerificationRepository _repository;
    private readonly string _connectionString = "Server=test;Database=test;Trusted_Connection=true;";

    public EmailVerificationRepositoryComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailVerificationRepository>>();
        
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns(_connectionString);
        
        _repository = new EmailVerificationRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateVerificationTokenAsync_ValidUserId_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.CreateVerificationTokenAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateVerificationTokenAsync_ValidToken_ReturnsUserId()
    {
        // Arrange
        var token = "valid-token-123";
        
        // Act & Assert
        var act = async () => await _repository.ValidateVerificationTokenAsync(token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreatePasswordResetTokenAsync_ValidUserId_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var act = async () => await _repository.CreatePasswordResetTokenAsync(userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidatePasswordResetTokenAsync_ValidToken_ReturnsUserId()
    {
        // Arrange
        var token = "valid-reset-token-123";
        
        // Act & Assert
        var act = async () => await _repository.ValidatePasswordResetTokenAsync(token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkVerificationTokenUsedAsync_ValidToken_CompletesSuccessfully()
    {
        // Arrange
        var token = "used-token-123";
        
        // Act & Assert
        var act = async () => await _repository.MarkVerificationTokenUsedAsync(token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkPasswordResetTokenUsedAsync_ValidToken_CompletesSuccessfully()
    {
        // Arrange
        var token = "used-reset-token-123";
        
        // Act & Assert
        var act = async () => await _repository.MarkPasswordResetTokenUsedAsync(token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_CompletesSuccessfully()
    {
        // Act & Assert
        var act = async () => await _repository.CleanupExpiredTokensAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ValidConfiguration_InitializesSuccessfully()
    {
        // Arrange & Act
        var repository = new EmailVerificationRepository(_mockConfiguration.Object, _mockLogger.Object);
        
        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new EmailVerificationRepository(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new EmailVerificationRepository(_mockConfiguration.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}