using System.ComponentModel.DataAnnotations;
using TherapyDocs.Api.Models;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class EmailVerificationTokenTests
{
    [Fact]
    public void EmailVerificationToken_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var token = new EmailVerificationToken();

        // Assert
        Assert.Equal(Guid.Empty, token.Id);
        Assert.Equal(Guid.Empty, token.UserId);
        Assert.Equal(string.Empty, token.Token);
        Assert.Equal(DateTime.MinValue, token.ExpirationDate);
        Assert.False(token.IsUsed);
        Assert.Equal(DateTime.MinValue, token.CreatedAt);
        Assert.Null(token.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = "verification-token-123";
        var expirationDate = DateTime.UtcNow.AddHours(24);
        var createdAt = DateTime.UtcNow;
        var usedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var emailToken = new EmailVerificationToken
        {
            Id = id,
            UserId = userId,
            Token = token,
            ExpirationDate = expirationDate,
            IsUsed = true,
            CreatedAt = createdAt,
            UsedAt = usedAt
        };

        // Assert
        Assert.Equal(id, emailToken.Id);
        Assert.Equal(userId, emailToken.UserId);
        Assert.Equal(token, emailToken.Token);
        Assert.Equal(expirationDate, emailToken.ExpirationDate);
        Assert.True(emailToken.IsUsed);
        Assert.Equal(createdAt, emailToken.CreatedAt);
        Assert.Equal(usedAt, emailToken.UsedAt);
    }

    [Theory]
    [InlineData("simple-token")]
    [InlineData("token-with-special-chars-!@#$%")]
    [InlineData("very-long-token-that-exceeds-normal-length-expectations-and-continues-with-more-characters")]
    [InlineData("")]
    [InlineData("token\nwith\nnewlines")]
    [InlineData("token\twith\ttabs")]
    public void EmailVerificationToken_Token_ShouldAcceptVariousFormats(string tokenValue)
    {
        // Act
        var emailToken = new EmailVerificationToken { Token = tokenValue };

        // Assert
        Assert.Equal(tokenValue, emailToken.Token);
    }

    [Fact]
    public void EmailVerificationToken_WithUnicodeToken_ShouldHandleCorrectly()
    {
        // Arrange
        var unicodeToken = "토큰-验证-токен-🔐";

        // Act
        var emailToken = new EmailVerificationToken { Token = unicodeToken };

        // Assert
        Assert.Equal(unicodeToken, emailToken.Token);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmailVerificationToken_IsUsed_ShouldAcceptBothValues(bool isUsed)
    {
        // Act
        var emailToken = new EmailVerificationToken { IsUsed = isUsed };

        // Assert
        Assert.Equal(isUsed, emailToken.IsUsed);
    }

    [Fact]
    public void EmailVerificationToken_UsedAt_ShouldAcceptNullValue()
    {
        // Act
        var emailToken = new EmailVerificationToken { UsedAt = null };

        // Assert
        Assert.Null(emailToken.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_UsedAt_ShouldAcceptDateTimeValue()
    {
        // Arrange
        var usedAt = DateTime.UtcNow;

        // Act
        var emailToken = new EmailVerificationToken { UsedAt = usedAt };

        // Assert
        Assert.Equal(usedAt, emailToken.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_ExpirationDate_ShouldAcceptVariousDates()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;

        // Act
        var token1 = new EmailVerificationToken { ExpirationDate = pastDate };
        var token2 = new EmailVerificationToken { ExpirationDate = futureDate };
        var token3 = new EmailVerificationToken { ExpirationDate = minDate };
        var token4 = new EmailVerificationToken { ExpirationDate = maxDate };

        // Assert
        Assert.Equal(pastDate, token1.ExpirationDate);
        Assert.Equal(futureDate, token2.ExpirationDate);
        Assert.Equal(minDate, token3.ExpirationDate);
        Assert.Equal(maxDate, token4.ExpirationDate);
    }

    [Fact]
    public void EmailVerificationToken_CreatedAt_ShouldAcceptVariousDates()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-2);

        // Act
        var emailToken = new EmailVerificationToken { CreatedAt = createdAt };

        // Assert
        Assert.Equal(createdAt, emailToken.CreatedAt);
    }

    [Fact]
    public void EmailVerificationToken_GuidsProperties_ShouldAcceptVariousGuids()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var emptyGuid = Guid.Empty;

        // Act
        var token1 = new EmailVerificationToken { Id = id, UserId = userId };
        var token2 = new EmailVerificationToken { Id = emptyGuid, UserId = emptyGuid };

        // Assert
        Assert.Equal(id, token1.Id);
        Assert.Equal(userId, token1.UserId);
        Assert.Equal(emptyGuid, token2.Id);
        Assert.Equal(emptyGuid, token2.UserId);
    }

    [Fact]
    public void EmailVerificationToken_LogicalStateConsistency_ShouldBeFlexible()
    {
        // This test shows that the model doesn't enforce logical state consistency
        // (e.g., IsUsed = true should have UsedAt set), which might be intentional

        // Act
        var inconsistentToken = new EmailVerificationToken
        {
            IsUsed = true,
            UsedAt = null // Inconsistent: marked as used but no usage date
        };

        // Assert - Model allows this, business logic should handle validation
        Assert.True(inconsistentToken.IsUsed);
        Assert.Null(inconsistentToken.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_DateOrderValidation_ShouldBeFlexible()
    {
        // This test shows that the model doesn't enforce logical date ordering
        // (CreatedAt before ExpirationDate, UsedAt after CreatedAt)

        // Arrange
        var createdAt = DateTime.UtcNow;
        var expirationDate = createdAt.AddDays(-1); // Expires before creation
        var usedAt = createdAt.AddDays(-2); // Used before creation

        // Act
        var illogicalToken = new EmailVerificationToken
        {
            CreatedAt = createdAt,
            ExpirationDate = expirationDate,
            UsedAt = usedAt,
            IsUsed = true
        };

        // Assert - Model allows this, business logic should handle validation
        Assert.Equal(createdAt, illogicalToken.CreatedAt);
        Assert.Equal(expirationDate, illogicalToken.ExpirationDate);
        Assert.Equal(usedAt, illogicalToken.UsedAt);
        Assert.True(illogicalToken.CreatedAt > illogicalToken.ExpirationDate);
        Assert.True(illogicalToken.CreatedAt > illogicalToken.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_WithMaxLengthToken_ShouldAcceptLargeValues()
    {
        // Arrange - Create a very long token
        var longToken = new string('A', 10000);

        // Act
        var emailToken = new EmailVerificationToken { Token = longToken };

        // Assert
        Assert.Equal(longToken, emailToken.Token);
        Assert.Equal(10000, emailToken.Token.Length);
    }

    [Fact]
    public void EmailVerificationToken_PropertyTypes_ShouldBeCorrect()
    {
        // This test verifies that all properties have the expected types
        var token = new EmailVerificationToken();

        // Assert
        Assert.IsType<Guid>(token.Id);
        Assert.IsType<Guid>(token.UserId);
        Assert.IsType<string>(token.Token);
        Assert.IsType<DateTime>(token.ExpirationDate);
        Assert.IsType<bool>(token.IsUsed);
        Assert.IsType<DateTime>(token.CreatedAt);
        // UsedAt is nullable DateTime, so it can be null
    }

    [Fact]
    public void EmailVerificationToken_DefaultBooleanValue_ShouldBeFalse()
    {
        // Act
        var token = new EmailVerificationToken();

        // Assert
        Assert.False(token.IsUsed); // Default bool value should be false
    }

    [Fact]
    public void EmailVerificationToken_AllPropertiesSet_ShouldMaintainValues()
    {
        // Arrange - Set all properties to specific values
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tokenValue = "comprehensive-test-token-123";
        var expirationDate = new DateTime(2025, 12, 31, 23, 59, 59);
        var createdAt = new DateTime(2024, 1, 1, 0, 0, 0);
        var usedAt = new DateTime(2024, 6, 15, 12, 30, 45);

        // Act
        var emailToken = new EmailVerificationToken
        {
            Id = id,
            UserId = userId,
            Token = tokenValue,
            ExpirationDate = expirationDate,
            IsUsed = true,
            CreatedAt = createdAt,
            UsedAt = usedAt
        };

        // Assert - Verify all properties maintain their exact values
        Assert.Equal(id, emailToken.Id);
        Assert.Equal(userId, emailToken.UserId);
        Assert.Equal(tokenValue, emailToken.Token);
        Assert.Equal(expirationDate, emailToken.ExpirationDate);
        Assert.True(emailToken.IsUsed);
        Assert.Equal(createdAt, emailToken.CreatedAt);
        Assert.Equal(usedAt, emailToken.UsedAt);
    }

    [Fact]
    public void EmailVerificationToken_TokenWithSpecialCharacters_ShouldPreserveExactValue()
    {
        // Arrange
        var specialToken = "token!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var emailToken = new EmailVerificationToken { Token = specialToken };

        // Assert
        Assert.Equal(specialToken, emailToken.Token);
        Assert.Equal(specialToken.Length, emailToken.Token.Length);
    }
}