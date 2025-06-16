using FluentAssertions;
using TherapyDocs.Api.Models;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class UserModelTests
{
    [Fact]
    public void User_DefaultConstructor_SetsExpectedDefaults()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(Guid.Empty); // Guid properties default to Empty
        user.Email.Should().BeEmpty();
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.ServiceType.Should().BeEmpty();
        user.Status.Should().Be("pending"); // Check actual default from model
        user.EmailVerified.Should().BeFalse();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_WithValidData_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var serviceType = "speech_therapy";
        var status = "active";

        // Act
        var user = new User
        {
            Id = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ServiceType = serviceType,
            Status = status,
            EmailVerified = true
        };

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.ServiceType.Should().Be(serviceType);
        user.Status.Should().Be(status);
        user.EmailVerified.Should().BeTrue();
    }

    [Theory]
    [InlineData("speech_therapy")]
    [InlineData("occupational_therapy")]
    [InlineData("physical_therapy")]
    [InlineData("behavioral_therapy")]
    public void User_ServiceType_AcceptsValidValues(string serviceType)
    {
        // Act
        var user = new User { ServiceType = serviceType };

        // Assert
        user.ServiceType.Should().Be(serviceType);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("inactive")]
    [InlineData("pending")]
    [InlineData("suspended")]
    public void User_Status_AcceptsValidValues(string status)
    {
        // Act
        var user = new User { Status = status };

        // Assert
        user.Status.Should().Be(status);
    }
}