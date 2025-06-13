using FluentAssertions;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hash_Than_Original()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hashes_For_Same_Password()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    [InlineData("123456789")]
    [InlineData("qwerty")]
    [InlineData("abc123")]
    [InlineData("password123")]
    [InlineData("admin")]
    [InlineData("letmein")]
    [InlineData("welcome")]
    [InlineData("monkey")]
    public void IsCommonPassword_Should_Return_True_For_Common_Passwords(string password)
    {
        // Act
        var result = _passwordService.IsCommonPassword(password);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("UncommonP@ssw0rd!")]
    [InlineData("MyS3cur3P@ssw0rd")]
    [InlineData("Th3r@pyD0cs2024!")]
    public void IsCommonPassword_Should_Return_False_For_Uncommon_Passwords(string password)
    {
        // Act
        var result = _passwordService.IsCommonPassword(password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCommonPassword_Should_Be_Case_Insensitive()
    {
        // Act
        var result1 = _passwordService.IsCommonPassword("PASSWORD");
        var result2 = _passwordService.IsCommonPassword("Password");
        var result3 = _passwordService.IsCommonPassword("password");

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Should_Handle_Unicode_Characters()
    {
        // Arrange
        var password = "Test🔒Password123!";

        // Act
        var hash = _passwordService.HashPassword(password);
        var verifyResult = _passwordService.VerifyPassword(password, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        verifyResult.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Should_Handle_Long_Passwords()
    {
        // Arrange
        var password = new string('a', 100) + "Test123!";

        // Act
        var hash = _passwordService.HashPassword(password);
        var verifyResult = _passwordService.VerifyPassword(password, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        verifyResult.Should().BeTrue();
    }
}