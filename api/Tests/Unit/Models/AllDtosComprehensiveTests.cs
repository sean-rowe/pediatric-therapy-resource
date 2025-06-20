using TherapyDocs.Api.Models.DTOs;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class AllDtosComprehensiveTests
{
    [Fact]
    public void RegisterRequest_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var request = new RegisterRequest();

        // Assert
        Assert.Equal(string.Empty, request.Email);
        Assert.Equal(string.Empty, request.Password);
        Assert.Equal(string.Empty, request.FirstName);
        Assert.Equal(string.Empty, request.LastName);
        Assert.Null(request.LicenseNumber);
        Assert.Null(request.LicenseState);
    }

    [Fact]
    public void RegisterRequest_AllProperties_ShouldBeSettableAndGettable()
    {
        // Act
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "LIC123456",
            LicenseState = "CA"
        };

        // Assert
        Assert.Equal("test@example.com", request.Email);
        Assert.Equal("TestPassword123!", request.Password);
        Assert.Equal("John", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.Equal("LIC123456", request.LicenseNumber);
        Assert.Equal("CA", request.LicenseState);
    }

    [Fact]
    public void RegisterResponse_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var response = new RegisterResponse();

        // Assert
        Assert.False(response.Success);
        Assert.Equal(string.Empty, response.Message);
        Assert.Null(response.UserId);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void RegisterResponse_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var response = new RegisterResponse
        {
            Success = true,
            Message = "Registration successful",
            UserId = userId,
            Errors = errors
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Registration successful", response.Message);
        Assert.Equal(userId, response.UserId);
        Assert.Equal(errors, response.Errors);
    }

    [Fact]
    public void LoginRequest_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var request = new LoginRequest();

        // Assert
        Assert.Equal(string.Empty, request.Email);
        Assert.Equal(string.Empty, request.Password);
    }

    [Fact]
    public void LoginRequest_Properties_ShouldBeSettableAndGettable()
    {
        // Act
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "SecretPassword123!"
        };

        // Assert
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("SecretPassword123!", request.Password);
    }

    [Fact]
    public void LoginResponse_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var response = new LoginResponse();

        // Assert
        Assert.False(response.Success);
        Assert.Equal(string.Empty, response.Message);
        Assert.Null(response.Token);
        Assert.Null(response.User);
    }

    [Fact]
    public void LoginResponse_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var userDto = new UserDto { FirstName = "Test", LastName = "User" };

        // Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            Token = "jwt-token-here",
            User = userDto
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal("jwt-token-here", response.Token);
        Assert.Equal(userDto, response.User);
    }

    [Fact]
    public void UserDto_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var dto = new UserDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Email);
        Assert.Equal(string.Empty, dto.FirstName);
        Assert.Equal(string.Empty, dto.LastName);
        Assert.False(dto.IsEmailVerified);
        Assert.Equal(DateTime.MinValue, dto.CreatedAt);
        Assert.Null(dto.LicenseNumber);
        Assert.Null(dto.LicenseState);
        Assert.Null(dto.LicenseVerifiedAt);
    }

    [Fact]
    public void UserDto_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var licenseVerifiedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var dto = new UserDto
        {
            Id = id,
            Email = "therapist@example.com",
            FirstName = "Dr. Jane",
            LastName = "Smith",
            IsEmailVerified = true,
            CreatedAt = createdAt,
            LicenseNumber = "TH789012",
            LicenseState = "NY",
            LicenseVerifiedAt = licenseVerifiedAt
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal("therapist@example.com", dto.Email);
        Assert.Equal("Dr. Jane", dto.FirstName);
        Assert.Equal("Smith", dto.LastName);
        Assert.True(dto.IsEmailVerified);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal("TH789012", dto.LicenseNumber);
        Assert.Equal("NY", dto.LicenseState);
        Assert.Equal(licenseVerifiedAt, dto.LicenseVerifiedAt);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("x@y.z")]
    [InlineData("")]
    public void RegisterRequest_Email_ShouldAcceptVariousFormats(string email)
    {
        // Act
        var request = new RegisterRequest { Email = email };

        // Assert
        Assert.Equal(email, request.Email);
    }

    [Theory]
    [InlineData("SimplePassword")]
    [InlineData("Complex123!@#")]
    [InlineData("VeryLongPasswordWithManyCharacters123!@#$%^&*()")]
    [InlineData("")]
    public void RegisterRequest_Password_ShouldAcceptVariousPasswords(string password)
    {
        // Act
        var request = new RegisterRequest { Password = password };

        // Assert
        Assert.Equal(password, request.Password);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Mary-Jane")]
    [InlineData("José")]
    [InlineData("")]
    [InlineData("VeryLongFirstNameThatExceedsNormalLength")]
    public void RegisterRequest_FirstName_ShouldAcceptVariousNames(string firstName)
    {
        // Act
        var request = new RegisterRequest { FirstName = firstName };

        // Assert
        Assert.Equal(firstName, request.FirstName);
    }

    [Theory]
    [InlineData("Smith")]
    [InlineData("O'Connor")]
    [InlineData("Van Der Berg")]
    [InlineData("")]
    [InlineData("VeryLongLastNameThatExceedsNormalLength")]
    public void RegisterRequest_LastName_ShouldAcceptVariousNames(string lastName)
    {
        // Act
        var request = new RegisterRequest { LastName = lastName };

        // Assert
        Assert.Equal(lastName, request.LastName);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterRequest_LicenseState_ShouldAcceptValidStates(string? licenseState)
    {
        // Act
        var request = new RegisterRequest { LicenseState = licenseState };

        // Assert
        Assert.Equal(licenseState, request.LicenseState);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("ABC123")]
    [InlineData("LIC-123-456")]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterRequest_LicenseNumber_ShouldAcceptValidNumbers(string? licenseNumber)
    {
        // Act
        var request = new RegisterRequest { LicenseNumber = licenseNumber };

        // Assert
        Assert.Equal(licenseNumber, request.LicenseNumber);
    }

    [Fact]
    public void RegisterResponse_Errors_ShouldBeModifiable()
    {
        // Arrange
        var response = new RegisterResponse();

        // Act
        response.Errors.Add("Password too weak");
        response.Errors.Add("Email already exists");

        // Assert
        Assert.Equal(2, response.Errors.Count);
        Assert.Contains("Password too weak", response.Errors);
        Assert.Contains("Email already exists", response.Errors);
    }

    [Fact]
    public void RegisterResponse_WithNullUserId_ShouldBeValid()
    {
        // Act
        var response = new RegisterResponse
        {
            Success = false,
            Message = "Registration failed",
            UserId = null
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Registration failed", response.Message);
        Assert.Null(response.UserId);
    }

    [Fact]
    public void LoginResponse_WithNullToken_ShouldBeValid()
    {
        // Act
        var response = new LoginResponse
        {
            Success = false,
            Message = "Invalid credentials",
            Token = null
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid credentials", response.Message);
        Assert.Null(response.Token);
    }

    [Fact]
    public void LoginResponse_WithNullUser_ShouldBeValid()
    {
        // Act
        var response = new LoginResponse
        {
            Success = false,
            Message = "User not found",
            User = null
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("User not found", response.Message);
        Assert.Null(response.User);
    }

    [Fact]
    public void UserDto_WithNullOptionalFields_ShouldBeValid()
    {
        // Act
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            LicenseNumber = null,
            LicenseState = null,
            LicenseVerifiedAt = null
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("test@example.com", dto.Email);
        Assert.Equal("Test", dto.FirstName);
        Assert.Equal("User", dto.LastName);
        Assert.True(dto.IsEmailVerified);
        Assert.Null(dto.LicenseNumber);
        Assert.Null(dto.LicenseState);
        Assert.Null(dto.LicenseVerifiedAt);
    }

    [Fact]
    public void AllDtos_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialChars = "Special chars: <>\"'&@#$%^*()[]{}";

        // Act
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = specialChars,
            FirstName = specialChars,
            LastName = specialChars,
            LicenseNumber = specialChars
        };

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = specialChars
        };

        var userDto = new UserDto
        {
            Email = "test@example.com",
            FirstName = specialChars,
            LastName = specialChars,
            LicenseNumber = specialChars
        };

        // Assert
        Assert.Equal(specialChars, registerRequest.Password);
        Assert.Equal(specialChars, registerRequest.FirstName);
        Assert.Equal(specialChars, registerRequest.LastName);
        Assert.Equal(specialChars, registerRequest.LicenseNumber);

        Assert.Equal(specialChars, loginRequest.Password);

        Assert.Equal(specialChars, userDto.FirstName);
        Assert.Equal(specialChars, userDto.LastName);
        Assert.Equal(specialChars, userDto.LicenseNumber);
    }

    [Fact]
    public void AllDtos_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var unicodeChars = "Unicode: 测试 🏥 ñáéíóú";

        // Act
        var registerRequest = new RegisterRequest
        {
            FirstName = unicodeChars,
            LastName = unicodeChars
        };

        var userDto = new UserDto
        {
            FirstName = unicodeChars,
            LastName = unicodeChars
        };

        // Assert
        Assert.Equal(unicodeChars, registerRequest.FirstName);
        Assert.Equal(unicodeChars, registerRequest.LastName);
        Assert.Equal(unicodeChars, userDto.FirstName);
        Assert.Equal(unicodeChars, userDto.LastName);
    }

    [Theory]
    [InlineData(true, "Success message")]
    [InlineData(false, "Failure message")]
    [InlineData(true, "")]
    [InlineData(false, "")]
    public void ResponseDtos_SuccessAndMessage_ShouldWorkTogether(bool success, string message)
    {
        // Act
        var registerResponse = new RegisterResponse
        {
            Success = success,
            Message = message
        };

        var loginResponse = new LoginResponse
        {
            Success = success,
            Message = message
        };

        // Assert
        Assert.Equal(success, registerResponse.Success);
        Assert.Equal(message, registerResponse.Message);
        Assert.Equal(success, loginResponse.Success);
        Assert.Equal(message, loginResponse.Message);
    }

    [Fact]
    public void UserDto_DateFields_ShouldHandleVariousValues()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;
        var currentDate = DateTime.UtcNow;

        // Act
        var dto1 = new UserDto { CreatedAt = minDate, LicenseVerifiedAt = minDate };
        var dto2 = new UserDto { CreatedAt = maxDate, LicenseVerifiedAt = maxDate };
        var dto3 = new UserDto { CreatedAt = currentDate, LicenseVerifiedAt = currentDate };

        // Assert
        Assert.Equal(minDate, dto1.CreatedAt);
        Assert.Equal(minDate, dto1.LicenseVerifiedAt);
        Assert.Equal(maxDate, dto2.CreatedAt);
        Assert.Equal(maxDate, dto2.LicenseVerifiedAt);
        Assert.Equal(currentDate, dto3.CreatedAt);
        Assert.Equal(currentDate, dto3.LicenseVerifiedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UserDto_IsEmailVerified_ShouldAcceptBothValues(bool isEmailVerified)
    {
        // Act
        var dto = new UserDto { IsEmailVerified = isEmailVerified };

        // Assert
        Assert.Equal(isEmailVerified, dto.IsEmailVerified);
    }

    [Fact]
    public void AllDtos_PropertyInitialization_ShouldBeConsistent()
    {
        // This test ensures that all DTOs have consistent property initialization behavior

        // Act
        var registerRequest = new RegisterRequest();
        var registerResponse = new RegisterResponse();
        var loginRequest = new LoginRequest();
        var loginResponse = new LoginResponse();
        var userDto = new UserDto();

        // Assert - String properties should be empty, not null
        Assert.Equal(string.Empty, registerRequest.Email);
        Assert.Equal(string.Empty, registerRequest.Password);
        Assert.Equal(string.Empty, registerRequest.FirstName);
        Assert.Equal(string.Empty, registerRequest.LastName);

        Assert.Equal(string.Empty, registerResponse.Message);
        Assert.NotNull(registerResponse.Errors);

        Assert.Equal(string.Empty, loginRequest.Email);
        Assert.Equal(string.Empty, loginRequest.Password);

        Assert.Equal(string.Empty, loginResponse.Message);

        Assert.Equal(string.Empty, userDto.Email);
        Assert.Equal(string.Empty, userDto.FirstName);
        Assert.Equal(string.Empty, userDto.LastName);
    }
}