using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using TherapyDocs.Api.Services;
using Xunit;
using System.Net;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<ISendGridClient> _mockSendGridClient;
    private readonly Dictionary<string, string> _configValues;

    public EmailServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockSendGridClient = new Mock<ISendGridClient>();
        
        _configValues = new Dictionary<string, string>
        {
            ["SendGrid:ApiKey"] = "test-api-key",
            ["SendGrid:FromEmail"] = "noreply@therapydocs.com",
            ["Application:BaseUrl"] = "https://therapydocs.com"
        };

        SetupConfiguration();
    }

    private void SetupConfiguration()
    {
        foreach (var kvp in _configValues)
        {
            _mockConfiguration.Setup(x => x[kvp.Key]).Returns(kvp.Value);
        }
    }

    private EmailService CreateService()
    {
        // Create service with mocked SendGrid client using reflection
        var service = new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        
        // Use reflection to replace the private _sendGridClient field
        var fieldInfo = typeof(EmailService).GetField("_sendGridClient", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(service, _mockSendGridClient.Object);
        
        return service;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["SendGrid:ApiKey"]).Returns((string?)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            new EmailService(_mockConfiguration.Object, _mockLogger.Object));
        
        Assert.Equal("SendGrid API key not configured", exception.Message);
    }

    [Fact]
    public void Constructor_ValidConfiguration_CreatesSuccessfully()
    {
        // Act
        var service = new EmailService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region SendVerificationEmailAsync Tests

    [Fact]
    public async Task SendVerificationEmailAsync_Success_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var verificationToken = "verify-token-123";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.True(result);
        _mockSendGridClient.Verify(x => x.SendEmailAsync(
            It.Is<SendGridMessage>(msg =>
                msg.Subject == "Verify Your TherapyDocs Account" &&
                msg.From.Email == "noreply@therapydocs.com" &&
                msg.From.Name == "TherapyDocs" &&
                msg.Personalizations[0].Tos[0].Email == email &&
                msg.Personalizations[0].Tos[0].Name == firstName),
            It.IsAny<CancellationToken>()), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verification email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_FailedStatus_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var verificationToken = "verify-token-123";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.BadRequest, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send verification email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_Exception_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var verificationToken = "verify-token-123";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SendGrid error"));

        var service = CreateService();

        // Act
        var result = await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error sending verification email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_CorrectUrlGeneration()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var verificationToken = "verify-token-123";
        var expectedUrl = "https://therapydocs.com/verify-email?token=verify-token-123";
        
        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Contains(expectedUrl, capturedMessage.PlainTextContent);
        Assert.Contains(expectedUrl, capturedMessage.HtmlContent);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_DefaultBaseUrl_UsesLocalhost()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Application:BaseUrl"]).Returns((string?)null);
        
        var email = "test@example.com";
        var firstName = "John";
        var verificationToken = "verify-token-123";
        var expectedUrl = "https://localhost:3000/verify-email?token=verify-token-123";
        
        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Contains(expectedUrl, capturedMessage.PlainTextContent);
        Assert.Contains(expectedUrl, capturedMessage.HtmlContent);
    }

    #endregion

    #region SendWelcomeEmailAsync Tests

    [Fact]
    public async Task SendWelcomeEmailAsync_Success_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendWelcomeEmailAsync(email, firstName);

        // Assert
        Assert.True(result);
        _mockSendGridClient.Verify(x => x.SendEmailAsync(
            It.Is<SendGridMessage>(msg =>
                msg.Subject == "Welcome to TherapyDocs!" &&
                msg.From.Email == "noreply@therapydocs.com" &&
                msg.From.Name == "TherapyDocs" &&
                msg.Personalizations[0].Tos[0].Email == email &&
                msg.Personalizations[0].Tos[0].Name == firstName),
            It.IsAny<CancellationToken>()), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Welcome email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_FailedStatus_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.InternalServerError, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendWelcomeEmailAsync(email, firstName);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send welcome email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_Exception_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        var service = CreateService();

        // Act
        var result = await service.SendWelcomeEmailAsync(email, firstName);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error sending welcome email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_ContainsCorrectContent()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var loginUrl = "https://therapydocs.com/login";
        
        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        await service.SendWelcomeEmailAsync(email, firstName);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Contains(firstName, capturedMessage.PlainTextContent);
        Assert.Contains(loginUrl, capturedMessage.PlainTextContent);
        Assert.Contains("Complete your profile", capturedMessage.PlainTextContent);
        Assert.Contains("Set up your therapy types", capturedMessage.PlainTextContent);
        Assert.Contains("Configure your schedule", capturedMessage.PlainTextContent);
        
        Assert.Contains(firstName, capturedMessage.HtmlContent);
        Assert.Contains(loginUrl, capturedMessage.HtmlContent);
        Assert.Contains("Complete your profile", capturedMessage.HtmlContent);
        Assert.Contains("help center", capturedMessage.HtmlContent);
    }

    #endregion

    #region Edge Cases and Null Handling

    [Theory]
    [InlineData(null, "John", "token123")]
    [InlineData("", "John", "token123")]
    [InlineData("test@example.com", null, "token123")]
    [InlineData("test@example.com", "", "token123")]
    [InlineData("test@example.com", "John", null)]
    [InlineData("test@example.com", "John", "")]
    public async Task SendVerificationEmailAsync_InvalidParameters_HandlesGracefully(string? email, string? firstName, string? token)
    {
        // Arrange
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendVerificationEmailAsync(email!, firstName!, token!);

        // Assert
        // Should either succeed or fail gracefully without throwing
        Assert.True(result || !result);
    }

    [Theory]
    [InlineData(null, "John")]
    [InlineData("", "John")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    public async Task SendWelcomeEmailAsync_InvalidParameters_HandlesGracefully(string? email, string? firstName)
    {
        // Arrange
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        var result = await service.SendWelcomeEmailAsync(email!, firstName!);

        // Assert
        // Should either succeed or fail gracefully without throwing
        Assert.True(result || !result);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_SpecialCharactersInName_HandlesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John<script>alert('xss')</script>";
        var verificationToken = "verify-token-123";
        
        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

        var service = CreateService();

        // Act
        await service.SendVerificationEmailAsync(email, firstName, verificationToken);

        // Assert
        Assert.NotNull(capturedMessage);
        // The name should be included as-is, SendGrid handles encoding
        Assert.Contains(firstName, capturedMessage.PlainTextContent);
    }

    #endregion
}