using TherapyDocs.Api.Models;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class RegistrationAuditLogTests
{
    [Fact]
    public void RegistrationAuditLog_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var auditLog = new RegistrationAuditLog();

        // Assert
        Assert.Equal(Guid.Empty, auditLog.Id);
        Assert.Equal(string.Empty, auditLog.Email);
        Assert.False(auditLog.Success);
        Assert.Equal(string.Empty, auditLog.ErrorMessage);
        Assert.Null(auditLog.IpAddress);
        Assert.Null(auditLog.UserAgent);
        Assert.Equal(DateTime.MinValue, auditLog.Timestamp);
    }

    [Fact]
    public void RegistrationAuditLog_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var errorMessage = "Registration failed due to invalid data";
        var ipAddress = "192.168.1.100";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var timestamp = DateTime.UtcNow;

        // Act
        var auditLog = new RegistrationAuditLog
        {
            Id = id,
            Email = email,
            Success = true,
            ErrorMessage = errorMessage,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(id, auditLog.Id);
        Assert.Equal(email, auditLog.Email);
        Assert.True(auditLog.Success);
        Assert.Equal(errorMessage, auditLog.ErrorMessage);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(timestamp, auditLog.Timestamp);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("x@y.z")]
    [InlineData("very.long.email.address@very.long.domain.name.com")]
    [InlineData("")]
    public void RegistrationAuditLog_Email_ShouldAcceptVariousFormats(string email)
    {
        // Act
        var auditLog = new RegistrationAuditLog { Email = email };

        // Assert
        Assert.Equal(email, auditLog.Email);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegistrationAuditLog_Success_ShouldAcceptBothValues(bool success)
    {
        // Act
        var auditLog = new RegistrationAuditLog { Success = success };

        // Assert
        Assert.Equal(success, auditLog.Success);
    }

    [Theory]
    [InlineData("Password too weak")]
    [InlineData("Email already exists")]
    [InlineData("Invalid license number")]
    [InlineData("")]
    [InlineData("Very long error message that describes a complex validation failure with multiple issues and detailed information about what went wrong during the registration process")]
    public void RegistrationAuditLog_ErrorMessage_ShouldAcceptVariousMessages(string errorMessage)
    {
        // Act
        var auditLog = new RegistrationAuditLog { ErrorMessage = errorMessage };

        // Assert
        Assert.Equal(errorMessage, auditLog.ErrorMessage);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("")]
    [InlineData(null)]
    public void RegistrationAuditLog_IpAddress_ShouldAcceptVariousFormats(string? ipAddress)
    {
        // Act
        var auditLog = new RegistrationAuditLog { IpAddress = ipAddress };

        // Assert
        Assert.Equal(ipAddress, auditLog.IpAddress);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36")]
    [InlineData("Custom User Agent")]
    [InlineData("")]
    [InlineData(null)]
    public void RegistrationAuditLog_UserAgent_ShouldAcceptVariousFormats(string? userAgent)
    {
        // Act
        var auditLog = new RegistrationAuditLog { UserAgent = userAgent };

        // Assert
        Assert.Equal(userAgent, auditLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_Timestamp_ShouldAcceptVariousDates()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;

        // Act
        var log1 = new RegistrationAuditLog { Timestamp = pastDate };
        var log2 = new RegistrationAuditLog { Timestamp = futureDate };
        var log3 = new RegistrationAuditLog { Timestamp = minDate };
        var log4 = new RegistrationAuditLog { Timestamp = maxDate };

        // Assert
        Assert.Equal(pastDate, log1.Timestamp);
        Assert.Equal(futureDate, log2.Timestamp);
        Assert.Equal(minDate, log3.Timestamp);
        Assert.Equal(maxDate, log4.Timestamp);
    }

    [Fact]
    public void RegistrationAuditLog_Id_ShouldAcceptVariousGuids()
    {
        // Arrange
        var randomGuid = Guid.NewGuid();
        var emptyGuid = Guid.Empty;

        // Act
        var log1 = new RegistrationAuditLog { Id = randomGuid };
        var log2 = new RegistrationAuditLog { Id = emptyGuid };

        // Assert
        Assert.Equal(randomGuid, log1.Id);
        Assert.Equal(emptyGuid, log2.Id);
    }

    [Fact]
    public void RegistrationAuditLog_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialEmail = "test+tag@domain-name.com";
        var specialErrorMessage = "Error with special chars: <>\"'&@#$%^*()[]{}";
        var specialUserAgent = "Custom/1.0 (Special; Chars; <>\"'&)";

        // Act
        var auditLog = new RegistrationAuditLog
        {
            Email = specialEmail,
            ErrorMessage = specialErrorMessage,
            UserAgent = specialUserAgent
        };

        // Assert
        Assert.Equal(specialEmail, auditLog.Email);
        Assert.Equal(specialErrorMessage, auditLog.ErrorMessage);
        Assert.Equal(specialUserAgent, auditLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var unicodeEmail = "测试@例子.中国";
        var unicodeErrorMessage = "Erreur avec caractères: ñáéíóú 测试 🌍";
        var unicodeUserAgent = "Navegador/1.0 (Español; 中文; العربية)";

        // Act
        var auditLog = new RegistrationAuditLog
        {
            Email = unicodeEmail,
            ErrorMessage = unicodeErrorMessage,
            UserAgent = unicodeUserAgent
        };

        // Assert
        Assert.Equal(unicodeEmail, auditLog.Email);
        Assert.Equal(unicodeErrorMessage, auditLog.ErrorMessage);
        Assert.Equal(unicodeUserAgent, auditLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_SuccessScenario_ShouldBeValid()
    {
        // Act
        var successLog = new RegistrationAuditLog
        {
            Id = Guid.NewGuid(),
            Email = "newuser@example.com",
            Success = true,
            ErrorMessage = "", // No error for successful registration
            IpAddress = "192.168.1.50",
            UserAgent = "Mozilla/5.0 Chrome",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, successLog.Id);
        Assert.True(successLog.Success);
        Assert.Equal("", successLog.ErrorMessage);
        Assert.NotNull(successLog.IpAddress);
        Assert.NotNull(successLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_FailureScenario_ShouldBeValid()
    {
        // Act
        var failureLog = new RegistrationAuditLog
        {
            Id = Guid.NewGuid(),
            Email = "invalid@example.com",
            Success = false,
            ErrorMessage = "Email validation failed",
            IpAddress = "10.0.0.100",
            UserAgent = "Mozilla/5.0 Firefox",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, failureLog.Id);
        Assert.False(failureLog.Success);
        Assert.NotEqual("", failureLog.ErrorMessage);
        Assert.NotNull(failureLog.IpAddress);
        Assert.NotNull(failureLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_WithNullOptionalFields_ShouldBeValid()
    {
        // Act
        var auditLog = new RegistrationAuditLog
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Success = false,
            ErrorMessage = "Some error",
            IpAddress = null,
            UserAgent = null,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("test@example.com", auditLog.Email);
        Assert.False(auditLog.Success);
        Assert.Equal("Some error", auditLog.ErrorMessage);
        Assert.Null(auditLog.IpAddress);
        Assert.Null(auditLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_WithVeryLongStrings_ShouldAcceptLargeValues()
    {
        // Arrange
        var longEmail = $"very.long.email.{new string('a', 100)}@{new string('b', 100)}.com";
        var longErrorMessage = new string('E', 5000);
        var longUserAgent = new string('U', 2000);

        // Act
        var auditLog = new RegistrationAuditLog
        {
            Email = longEmail,
            ErrorMessage = longErrorMessage,
            UserAgent = longUserAgent
        };

        // Assert
        Assert.Equal(longEmail, auditLog.Email);
        Assert.Equal(longErrorMessage, auditLog.ErrorMessage);
        Assert.Equal(longUserAgent, auditLog.UserAgent);
    }

    [Fact]
    public void RegistrationAuditLog_PropertyTypes_ShouldBeCorrect()
    {
        // This test verifies that all properties have the expected types
        var auditLog = new RegistrationAuditLog();

        // Assert
        Assert.IsType<Guid>(auditLog.Id);
        Assert.IsType<string>(auditLog.Email);
        Assert.IsType<bool>(auditLog.Success);
        Assert.IsType<string>(auditLog.ErrorMessage);
        Assert.IsType<DateTime>(auditLog.Timestamp);
        // IpAddress and UserAgent are nullable strings
    }

    [Fact]
    public void RegistrationAuditLog_LogicalConsistency_ShouldBeFlexible()
    {
        // This test shows that the model doesn't enforce logical consistency
        // (e.g., Success = true but ErrorMessage is set)

        // Act
        var inconsistentLog = new RegistrationAuditLog
        {
            Success = true,
            ErrorMessage = "This should not happen if success is true"
        };

        // Assert - Model allows this, business logic should handle validation
        Assert.True(inconsistentLog.Success);
        Assert.NotEqual("", inconsistentLog.ErrorMessage);
    }

    [Fact]
    public void RegistrationAuditLog_AllPropertiesSet_ShouldMaintainValues()
    {
        // Arrange - Set all properties to specific test values
        var id = Guid.NewGuid();
        var email = "comprehensive.test@example.com";
        var errorMessage = "Comprehensive test error message";
        var ipAddress = "203.0.113.1";
        var userAgent = "Test-Agent/1.0 (Testing; Comprehensive)";
        var timestamp = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var auditLog = new RegistrationAuditLog
        {
            Id = id,
            Email = email,
            Success = true,
            ErrorMessage = errorMessage,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = timestamp
        };

        // Assert - Verify all properties maintain their exact values
        Assert.Equal(id, auditLog.Id);
        Assert.Equal(email, auditLog.Email);
        Assert.True(auditLog.Success);
        Assert.Equal(errorMessage, auditLog.ErrorMessage);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(timestamp, auditLog.Timestamp);
    }
}