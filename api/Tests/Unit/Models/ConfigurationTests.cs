using TherapyDocs.Api.Models.Configuration;
using TherapyDocs.Api.Models.DTOs;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Models;

public class ConfigurationTests
{
    [Fact]
    public void LicenseVerificationConfig_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var config = new LicenseVerificationConfig();

        // Assert - All properties should have their default values
        Assert.NotNull(config);
    }

    [Fact]
    public void LicenseVerificationResult_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var result = new LicenseVerificationResult();

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(string.Empty, result.State);
        Assert.Equal(string.Empty, result.LicenseNumber);
        Assert.Equal(string.Empty, result.Message);
        Assert.Null(result.ExpirationDate);
        Assert.Null(result.IssueDate);
        Assert.Equal(string.Empty, result.LicenseType);
        Assert.Equal(string.Empty, result.Status);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void LicenseVerificationResult_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var issueDate = DateTime.Now.AddYears(-2);
        var expirationDate = DateTime.Now.AddYears(1);
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = new LicenseVerificationResult
        {
            IsValid = true,
            State = "CA",
            LicenseNumber = "12345",
            Message = "License is valid",
            ExpirationDate = expirationDate,
            IssueDate = issueDate,
            LicenseType = "Professional",
            Status = "Active",
            Errors = errors
        };

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("CA", result.State);
        Assert.Equal("12345", result.LicenseNumber);
        Assert.Equal("License is valid", result.Message);
        Assert.Equal(expirationDate, result.ExpirationDate);
        Assert.Equal(issueDate, result.IssueDate);
        Assert.Equal("Professional", result.LicenseType);
        Assert.Equal("Active", result.Status);
        Assert.Equal(errors, result.Errors);
    }

    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    [InlineData("")]
    public void LicenseVerificationResult_State_ShouldAcceptValidValues(string state)
    {
        // Act
        var result = new LicenseVerificationResult { State = state };

        // Assert
        Assert.Equal(state, result.State);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("ABC123")]
    [InlineData("")]
    [InlineData("123456789012345")]
    public void LicenseVerificationResult_LicenseNumber_ShouldAcceptValidValues(string licenseNumber)
    {
        // Act
        var result = new LicenseVerificationResult { LicenseNumber = licenseNumber };

        // Assert
        Assert.Equal(licenseNumber, result.LicenseNumber);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Suspended")]
    [InlineData("Expired")]
    [InlineData("Pending")]
    [InlineData("")]
    public void LicenseVerificationResult_Status_ShouldAcceptValidValues(string status)
    {
        // Act
        var result = new LicenseVerificationResult { Status = status };

        // Assert
        Assert.Equal(status, result.Status);
    }

    [Theory]
    [InlineData("Professional")]
    [InlineData("Temporary")]
    [InlineData("Student")]
    [InlineData("Provisional")]
    [InlineData("")]
    public void LicenseVerificationResult_LicenseType_ShouldAcceptValidValues(string licenseType)
    {
        // Act
        var result = new LicenseVerificationResult { LicenseType = licenseType };

        // Assert
        Assert.Equal(licenseType, result.LicenseType);
    }

    [Fact]
    public void LicenseVerificationResult_Dates_ShouldAcceptNullValues()
    {
        // Act
        var result = new LicenseVerificationResult
        {
            IssueDate = null,
            ExpirationDate = null
        };

        // Assert
        Assert.Null(result.IssueDate);
        Assert.Null(result.ExpirationDate);
    }

    [Fact]
    public void LicenseVerificationResult_Dates_ShouldAcceptValidDates()
    {
        // Arrange
        var issueDate = new DateTime(2020, 1, 1);
        var expirationDate = new DateTime(2025, 1, 1);

        // Act
        var result = new LicenseVerificationResult
        {
            IssueDate = issueDate,
            ExpirationDate = expirationDate
        };

        // Assert
        Assert.Equal(issueDate, result.IssueDate);
        Assert.Equal(expirationDate, result.ExpirationDate);
    }

    [Fact]
    public void LicenseVerificationResult_Errors_ShouldBeModifiable()
    {
        // Arrange
        var result = new LicenseVerificationResult();

        // Act
        result.Errors.Add("Error 1");
        result.Errors.Add("Error 2");

        // Assert
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Error 1", result.Errors);
        Assert.Contains("Error 2", result.Errors);
    }

    [Fact]
    public void LicenseVerificationResult_Errors_ShouldAcceptEmptyList()
    {
        // Act
        var result = new LicenseVerificationResult
        {
            Errors = new List<string>()
        };

        // Assert
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void LicenseVerificationResult_Errors_ShouldAcceptNullList()
    {
        // Act
        var result = new LicenseVerificationResult
        {
            Errors = null!
        };

        // Assert
        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData(true, "Valid license")]
    [InlineData(false, "Invalid license")]
    [InlineData(true, "")]
    [InlineData(false, "")]
    public void LicenseVerificationResult_IsValidAndMessage_ShouldWorkTogether(bool isValid, string message)
    {
        // Act
        var result = new LicenseVerificationResult
        {
            IsValid = isValid,
            Message = message
        };

        // Assert
        Assert.Equal(isValid, result.IsValid);
        Assert.Equal(message, result.Message);
    }

    [Fact]
    public void LicenseVerificationResult_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var specialMessage = "License contains special chars: <>\"'&@#$%";
        var specialLicenseNumber = "LIC-123/ABC*456";

        // Act
        var result = new LicenseVerificationResult
        {
            Message = specialMessage,
            LicenseNumber = specialLicenseNumber
        };

        // Assert
        Assert.Equal(specialMessage, result.Message);
        Assert.Equal(specialLicenseNumber, result.LicenseNumber);
    }

    [Fact]
    public void LicenseVerificationResult_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var unicodeMessage = "License message with unicode: 测试 🏥 ñáéíóú";
        var unicodeLicenseNumber = "LIC-测试-123";

        // Act
        var result = new LicenseVerificationResult
        {
            Message = unicodeMessage,
            LicenseNumber = unicodeLicenseNumber
        };

        // Assert
        Assert.Equal(unicodeMessage, result.Message);
        Assert.Equal(unicodeLicenseNumber, result.LicenseNumber);
    }

    [Fact]
    public void LicenseVerificationResult_ErrorsList_ShouldSupportComplexScenarios()
    {
        // Arrange
        var result = new LicenseVerificationResult();
        var errors = new List<string>
        {
            "Error with special chars: <>\"'&",
            "Error with unicode: 测试错误",
            "",
            "Very long error message that exceeds normal length expectations and continues with more text to test boundary conditions",
            "Error\nwith\nnewlines",
            "Error\twith\ttabs"
        };

        // Act
        result.Errors = errors;

        // Assert
        Assert.Equal(errors.Count, result.Errors.Count);
        foreach (var error in errors)
        {
            Assert.Contains(error, result.Errors);
        }
    }

    [Fact]
    public void LicenseVerificationResult_DateValidation_ShouldHandleEdgeCases()
    {
        // Act
        var result = new LicenseVerificationResult
        {
            IssueDate = DateTime.MinValue,
            ExpirationDate = DateTime.MaxValue
        };

        // Assert
        Assert.Equal(DateTime.MinValue, result.IssueDate);
        Assert.Equal(DateTime.MaxValue, result.ExpirationDate);
    }

    [Fact]
    public void LicenseVerificationResult_LogicalDateOrder_ShouldBeFlexible()
    {
        // This test shows that the model doesn't enforce logical date ordering
        // (issue date before expiration date), which might be intentional for flexibility

        // Arrange
        var issueDate = DateTime.Now;
        var expirationDate = DateTime.Now.AddDays(-30); // Expiration before issue

        // Act
        var result = new LicenseVerificationResult
        {
            IssueDate = issueDate,
            ExpirationDate = expirationDate
        };

        // Assert - Model allows this, business logic should handle validation
        Assert.Equal(issueDate, result.IssueDate);
        Assert.Equal(expirationDate, result.ExpirationDate);
        Assert.True(result.IssueDate > result.ExpirationDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void LicenseVerificationResult_ErrorsCount_ShouldSupportVariousSizes(int errorCount)
    {
        // Arrange
        var errors = new List<string>();
        for (int i = 0; i < errorCount; i++)
        {
            errors.Add($"Error {i + 1}");
        }

        // Act
        var result = new LicenseVerificationResult { Errors = errors };

        // Assert
        Assert.Equal(errorCount, result.Errors.Count);
    }

    [Fact]
    public void LicenseVerificationResult_AllPropertiesSet_ShouldMaintainValues()
    {
        // Arrange
        var issueDate = new DateTime(2020, 1, 1);
        var expirationDate = new DateTime(2025, 12, 31);
        var errors = new List<string> { "Warning: License expires soon" };

        // Act
        var result = new LicenseVerificationResult
        {
            IsValid = true,
            State = "CA",
            LicenseNumber = "CA123456",
            Message = "License verified successfully",
            ExpirationDate = expirationDate,
            IssueDate = issueDate,
            LicenseType = "Professional Therapist",
            Status = "Active",
            Errors = errors
        };

        // Assert - Verify all properties maintain their values
        Assert.True(result.IsValid);
        Assert.Equal("CA", result.State);
        Assert.Equal("CA123456", result.LicenseNumber);
        Assert.Equal("License verified successfully", result.Message);
        Assert.Equal(expirationDate, result.ExpirationDate);
        Assert.Equal(issueDate, result.IssueDate);
        Assert.Equal("Professional Therapist", result.LicenseType);
        Assert.Equal("Active", result.Status);
        Assert.Single(result.Errors);
        Assert.Equal("Warning: License expires soon", result.Errors[0]);
    }
}