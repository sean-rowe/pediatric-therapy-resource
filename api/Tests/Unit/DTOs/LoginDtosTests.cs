using FluentAssertions;
using TherapyDocs.Api.Models.DTOs;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.DTOs;

public class LoginDtosTests
{
    [Fact]
    public void LoginRequest_DefaultConstructor_SetsEmptyStrings()
    {
        // Act
        var request = new LoginRequest();

        // Assert
        request.Email.Should().BeEmpty();
        request.Password.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequest_WithData_SetsPropertiesCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";

        // Act
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginResponse_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var response = new LoginResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().BeEmpty();
        response.Token.Should().BeNull();
        response.User.Should().BeNull();
        response.RemainingAttempts.Should().BeNull();
        response.IsLocked.Should().BeFalse();
        response.RequiresEmailVerification.Should().BeFalse();
        response.PasswordChangeRequired.Should().BeFalse();
        response.PasswordExpiryWarning.Should().BeFalse();
        response.DaysUntilPasswordExpiry.Should().BeNull();
    }

    [Fact]
    public void LoginResponse_SuccessfulLogin_SetsPropertiesCorrectly()
    {
        // Arrange
        var token = "jwt-token-here";
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            ServiceType = "speech_therapy"
        };

        // Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = user
        };

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Login successful");
        response.Token.Should().Be(token);
        response.User.Should().Be(user);
    }

    [Fact]
    public void UserDto_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var userDto = new UserDto();

        // Assert
        userDto.Id.Should().Be(Guid.Empty);
        userDto.Email.Should().BeEmpty();
        userDto.FirstName.Should().BeEmpty();
        userDto.LastName.Should().BeEmpty();
        userDto.ServiceType.Should().BeEmpty();
    }

    [Fact]
    public void AccountLockoutStatus_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var status = new AccountLockoutStatus();

        // Assert
        status.IsLocked.Should().BeFalse();
        status.LockedUntil.Should().BeNull();
        status.RemainingAttempts.Should().Be(0);
    }

    [Fact]
    public void PasswordChangeRequirement_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var requirement = new PasswordChangeRequirement();

        // Assert
        requirement.ChangeRequired.Should().BeFalse();
        requirement.DaysUntilExpiry.Should().Be(0);
    }
}