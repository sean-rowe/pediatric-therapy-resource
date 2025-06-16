using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class PasswordServiceSimpleTests
{
    private readonly PasswordService _passwordService;
    private readonly Mock<IHaveIBeenPwnedService> _mockHibpService;
    private readonly Mock<ILogger<PasswordService>> _mockLogger;

    public PasswordServiceSimpleTests()
    {
        _mockHibpService = new Mock<IHaveIBeenPwnedService>();
        _mockLogger = new Mock<ILogger<PasswordService>>();
        _passwordService = new PasswordService(_mockHibpService.Object, _mockLogger.Object);
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordService.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Should().StartWith("$2a$");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    [InlineData("qwerty")]
    public void IsCommonPassword_CommonPasswords_ReturnsTrue(string commonPassword)
    {
        // Act
        var result = _passwordService.IsCommonPassword(commonPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCommonPassword_UncommonPassword_ReturnsFalse()
    {
        // Arrange
        var uncommonPassword = "MyVeryUniquePassword123!@#";

        // Act
        var result = _passwordService.IsCommonPassword(uncommonPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCommonPassword_ChecksHaveIBeenPwnedService()
    {
        // Arrange
        var password = "TestPassword123!";
        _mockHibpService.Setup(x => x.IsPasswordPwnedAsync(password))
            .ReturnsAsync(false);

        // Act
        var result = _passwordService.IsCommonPassword(password);

        // Assert
        result.Should().BeFalse();
        _mockHibpService.Verify(x => x.IsPasswordPwnedAsync(password), Times.Once);
    }
}