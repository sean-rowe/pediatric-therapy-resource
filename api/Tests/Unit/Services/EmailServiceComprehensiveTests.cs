using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class EmailServiceComprehensiveTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailService>> _mockLogger;

    public EmailServiceComprehensiveTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailService>>();
    }

    /**
     * Feature: Email Service Configuration
     *   As a system administrator
     *   I want email service to be properly configured
     *   So that emails can be sent to users
     * 
     * Scenario: Email service with valid configuration
     *   Given SendGrid API key is configured
     *   When EmailService is instantiated
     *   Then no exception is thrown
     */
    [Fact]
    public void Constructor_ValidConfiguration_CreatesInstance()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["SendGrid:ApiKey"]).Returns("test-api-key");

        // Act & Assert
        var act = () => new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        
        act.Should().NotThrow();
    }

    /**
     * Scenario: Missing SendGrid API key
     *   Given SendGrid API key is not configured
     *   When EmailService is instantiated
     *   Then InvalidOperationException is thrown
     */
    [Fact]
    public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["SendGrid:ApiKey"]).Returns((string?)null);

        // Act & Assert
        var act = () => new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("SendGrid API key not configured");
    }

    /**
     * Scenario: Empty SendGrid API key
     *   Given SendGrid API key is empty string
     *   When EmailService is instantiated
     *   Then InvalidOperationException is thrown
     */
    [Fact]
    public void Constructor_EmptyApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["SendGrid:ApiKey"]).Returns("");

        // Act & Assert
        var act = () => new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("SendGrid API key not configured");
    }

    /**
     * Scenario: Whitespace SendGrid API key
     *   Given SendGrid API key is whitespace
     *   When EmailService is instantiated
     *   Then InvalidOperationException is thrown
     */
    [Fact]
    public void Constructor_WhitespaceApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["SendGrid:ApiKey"]).Returns("   ");

        // Act & Assert
        var act = () => new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("SendGrid API key not configured");
    }

    /**
     * Note: Due to the design of EmailService creating its own SendGridClient instance,
     * we cannot easily unit test the actual email sending methods without integration tests.
     * The SendVerificationEmailAsync and SendWelcomeEmailAsync methods are tested via
     * integration tests that use actual SendGrid sandbox or test environments.
     * 
     * To improve testability, EmailService should accept ISendGridClient as a dependency
     * rather than creating its own instance.
     */
}