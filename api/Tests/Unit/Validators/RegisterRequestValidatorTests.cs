using FluentAssertions;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Validators;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Should_Pass_With_Valid_Request()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ABC12345",
            LicenseState = "CA",
            LicenseType = "OT",
            Phone = "555-123-4567",
            AcceptedTerms = true
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Should_Fail_With_Invalid_Email(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("NoSpecialChar123")]
    [InlineData("no-uppercase-123!")]
    [InlineData("NO-LOWERCASE-123!")]
    [InlineData("NoNumbers!")]
    public void Should_Fail_With_Invalid_Password(string password)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Fail_When_Passwords_Dont_Match()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = "P@ssw0rd123!";
        request.ConfirmPassword = "DifferentP@ss123!";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("Name!@#")]
    public void Should_Fail_With_Invalid_Name(string name)
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = name;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("ZZ")]
    [InlineData("12")]
    public void Should_Fail_With_Invalid_State(string state)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseState = state;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LicenseState");
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("InvalidType")]
    public void Should_Fail_With_Invalid_License_Type(string licenseType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseType = licenseType;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LicenseType");
    }

    [Fact]
    public void Should_Fail_When_Terms_Not_Accepted()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AcceptedTerms = false;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AcceptedTerms");
    }

    private static RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest
        {
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123!",
            ConfirmPassword = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ABC12345",
            LicenseState = "CA",
            LicenseType = "OT",
            Phone = "555-123-4567",
            AcceptedTerms = true
        };
    }
}