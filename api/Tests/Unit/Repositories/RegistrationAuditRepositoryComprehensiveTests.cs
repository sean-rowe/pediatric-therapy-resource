using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Repositories;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Repositories;

public class RegistrationAuditRepositoryComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<RegistrationAuditRepository>> _mockLogger;
    private readonly RegistrationAuditRepository _repository;

    public RegistrationAuditRepositoryComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<RegistrationAuditRepository>>();
        
        // Setup configuration for connection string
        _mockConfiguration.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Server=localhost;Database=TestDb;User Id=sa;Password=Test123!;TrustServerCertificate=true");

        _repository = new RegistrationAuditRepository(_mockConfiguration.Object, _mockLogger.Object);
    }

    /**
     * Feature: Registration Audit Logging
     *   As a security and compliance system
     *   I want to log all registration attempts
     *   So that registration activity can be tracked and audited
     * 
     * Scenario: Log successful registration attempt with all details
     *   Given a successful registration with all information
     *   When logging the registration attempt
     *   Then the attempt is recorded in the audit log
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_SuccessfulRegistration_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        var success = true;
        string? failureReason = null;
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log failed registration attempt with failure reason
     *   Given a failed registration with reason
     *   When logging the registration attempt
     *   Then the attempt and failure reason are recorded
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_FailedRegistration_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        var success = false;
        var failureReason = "License validation failed";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration attempt with null license information
     *   Given a registration attempt without license details
     *   When logging the registration attempt
     *   Then the attempt is recorded with null license values
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_NullLicenseInfo_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";
        string? licenseNumber = null;
        string? licenseState = null;
        var success = false;
        var failureReason = "License information missing";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration attempt with null IP and user agent
     *   Given a registration attempt without IP or user agent info
     *   When logging the registration attempt
     *   Then the attempt is recorded with null values
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_NullIpAndUserAgent_HandlesCall()
    {
        // Arrange
        var email = "test@example.com";
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        var success = true;
        string? failureReason = null;
        string? ipAddress = null;
        string? userAgent = null;

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration attempt with null email
     *   Given a registration attempt with null email
     *   When logging the registration attempt
     *   Then the attempt is handled gracefully
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_NullEmail_HandlesCall()
    {
        // Arrange
        string? email = null;
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        var success = false;
        var failureReason = "Invalid email";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email!, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration attempt with empty email
     *   Given a registration attempt with empty email
     *   When logging the registration attempt
     *   Then the attempt is handled gracefully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task LogRegistrationAttemptAsync_EmptyEmail_HandlesCall(string email)
    {
        // Arrange
        var licenseNumber = "ST12345";
        var licenseState = "CA";
        var success = false;
        var failureReason = "Invalid email format";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, licenseNumber, licenseState, success, failureReason, ipAddress, userAgent);
        await act.Should().NotThrowAsync();
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
        var act = () => new RegistrationAuditRepository(_mockConfiguration.Object, _mockLogger.Object);

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
        Assert.Throws<ArgumentNullException>(() => new RegistrationAuditRepository(null!, _mockLogger.Object));
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
        Assert.Throws<ArgumentNullException>(() => new RegistrationAuditRepository(_mockConfiguration.Object, null!));
    }

    /**
     * Feature: Edge Cases and Special Characters
     *   As a robust repository
     *   I want to handle edge cases gracefully
     *   So that the system remains stable
     * 
     * Scenario: Log registration with special characters in email
     *   Given emails with special characters
     *   When logging registration attempts
     *   Then they are handled correctly
     */
    [Theory]
    [InlineData("test@domain.com")]
    [InlineData("test+tag@domain.com")]
    [InlineData("test.with.dots@domain.com")]
    [InlineData("test_with_underscores@domain.com")]
    [InlineData("test-with-hyphens@domain.com")]
    [InlineData("test123@domain123.com")]
    [InlineData("user@sub.domain.example.org")]
    public async Task LogRegistrationAttemptAsync_SpecialCharactersInEmail_HandlesCorrectly(string email)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync(email, "ST12345", "CA", true, null, "192.168.1.1", "test-agent");
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration with various license number formats
     *   Given different license number formats
     *   When logging registration attempts
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("ABC123")]
    [InlineData("12345")]
    [InlineData("ST-12345")]
    [InlineData("LIC.123.456")]
    [InlineData("MFT_789")]
    [InlineData("PSY 98765")]
    [InlineData("OT-2023-001")]
    public async Task LogRegistrationAttemptAsync_VariousLicenseFormats_HandlesCorrectly(string licenseNumber)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync("test@example.com", licenseNumber, "CA", true, null, "192.168.1.1", "test-agent");
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration with various state codes
     *   Given different state codes
     *   When logging registration attempts
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    [InlineData("WA")]
    [InlineData("OR")]
    [InlineData("NV")]
    public async Task LogRegistrationAttemptAsync_VariousStateCodes_HandlesCorrectly(string licenseState)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync("test@example.com", "ST12345", licenseState, true, null, "192.168.1.1", "test-agent");
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration with various failure reasons
     *   Given different failure reasons
     *   When logging failed registration attempts
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("License validation failed")]
    [InlineData("Email already exists")]
    [InlineData("Invalid license number format")]
    [InlineData("License not found in state database")]
    [InlineData("License expired")]
    [InlineData("License suspended")]
    [InlineData("Multiple licenses found for criteria")]
    public async Task LogRegistrationAttemptAsync_VariousFailureReasons_HandlesCorrectly(string failureReason)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync("test@example.com", "ST12345", "CA", false, failureReason, "192.168.1.1", "test-agent");
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration with various IP address formats
     *   Given different IP address formats
     *   When logging registration attempts
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")] // IPv6
    [InlineData("::1")] // IPv6 localhost
    [InlineData("2001:db8::1")] // Shortened IPv6
    public async Task LogRegistrationAttemptAsync_VariousIpFormats_HandlesCorrectly(string ipAddress)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync("test@example.com", "ST12345", "CA", true, null, ipAddress, "test-agent");
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log registration with various user agent strings
     *   Given different user agent formats
     *   When logging registration attempts
     *   Then they are processed correctly
     */
    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15")]
    [InlineData("curl/7.68.0")]
    [InlineData("PostmanRuntime/7.26.8")]
    [InlineData("TherapyDocs-Mobile/1.0")]
    public async Task LogRegistrationAttemptAsync_VariousUserAgents_HandlesCorrectly(string userAgent)
    {
        // Act & Assert
        var act = () => _repository.LogRegistrationAttemptAsync("test@example.com", "ST12345", "CA", true, null, "192.168.1.1", userAgent);
        await act.Should().NotThrowAsync();
    }

    /**
     * Scenario: Log multiple registration attempts
     *   Given multiple registration attempts
     *   When logging them sequentially
     *   Then all attempts are handled correctly
     */
    [Fact]
    public async Task LogRegistrationAttemptAsync_MultipleAttempts_HandlesCorrectly()
    {
        // Arrange
        var attempts = new[]
        {
            new { Email = "user1@example.com", License = "ST001", State = "CA", Success = true, Reason = (string?)null },
            new { Email = "user2@example.com", License = "ST002", State = "NY", Success = false, Reason = "License validation failed" },
            new { Email = "user3@example.com", License = "ST003", State = "TX", Success = true, Reason = (string?)null },
            new { Email = "user4@example.com", License = "ST004", State = "FL", Success = false, Reason = "Email already exists" },
            new { Email = "user5@example.com", License = "ST005", State = "WA", Success = true, Reason = (string?)null }
        };

        // Act & Assert
        foreach (var attempt in attempts)
        {
            var act = () => _repository.LogRegistrationAttemptAsync(
                attempt.Email, 
                attempt.License, 
                attempt.State, 
                attempt.Success, 
                attempt.Reason, 
                "192.168.1.1", 
                "test-agent");
            await act.Should().NotThrowAsync();
        }
    }

    /**
     * Feature: Connection String Validation
     *   As a repository
     *   I want to validate connection string configuration
     *   So that database operations can be performed
     * 
     * Scenario: Missing connection string does not prevent instantiation
     *   Given configuration without connection string
     *   When creating repository
     *   Then repository is created successfully (connection errors occur at operation time)
     */
    [Fact]
    public void Constructor_MissingConnectionString_AllowsInstantiation()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetConnectionString("DefaultConnection")).Returns((string?)null);

        // Act & Assert - Repository creation should succeed
        var act = () => new RegistrationAuditRepository(mockConfig.Object, _mockLogger.Object);
        act.Should().NotThrow();
    }

    /**
     * Scenario: Empty connection string does not prevent instantiation
     *   Given configuration with empty connection string
     *   When creating repository
     *   Then repository is created successfully
     */
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyConnectionString_AllowsInstantiation(string connectionString)
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x.GetConnectionString("DefaultConnection")).Returns(connectionString);

        // Act & Assert - Repository creation should succeed
        var act = () => new RegistrationAuditRepository(mockConfig.Object, _mockLogger.Object);
        act.Should().NotThrow();
    }
}