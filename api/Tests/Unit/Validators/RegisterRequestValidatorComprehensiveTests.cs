using FluentAssertions;
using TherapyDocs.Api.Models.DTOs;
using TherapyDocs.Api.Validators;
using Xunit;

namespace TherapyDocs.Api.Tests.Unit.Validators;

public class RegisterRequestValidatorComprehensiveTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorComprehensiveTests()
    {
        _validator = new RegisterRequestValidator();
    }

    /**
     * Feature: Email Validation
     *   As a registration system
     *   I want to validate email addresses
     *   So that only valid emails are accepted
     * 
     * Scenario: Valid email addresses
     *   Given a valid email format
     *   When validating the registration request
     *   Then no email validation errors occur
     */
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.gov")]
    public void Email_ValidFormats_PassesValidation(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    /**
     * Scenario: Invalid email addresses
     *   Given an invalid email format
     *   When validating the registration request
     *   Then email validation errors occur
     */
    [Theory]
    [InlineData("", "Email is required")]
    [InlineData("invalid-email", "Invalid email format")]
    [InlineData("@domain.com", "Invalid email format")]
    [InlineData("user@", "Invalid email format")]
    [InlineData("user..name@domain.com", "Invalid email format")]
    public void Email_InvalidFormats_FailsValidation(string email, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = email;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email) && e.ErrorMessage == expectedError);
    }

    /**
     * Scenario: Email too long
     *   Given an email exceeding 255 characters
     *   When validating the registration request
     *   Then validation fails with length error
     */
    [Fact]
    public void Email_ExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = new string('a', 250) + "@example.com"; // 261 characters

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email) && e.ErrorMessage == "Email must not exceed 255 characters");
    }

    /**
     * Feature: Password Validation
     *   As a security system
     *   I want to validate password strength
     *   So that user accounts are secure
     * 
     * Scenario: Valid passwords
     *   Given a password meeting all requirements
     *   When validating the registration request
     *   Then no password validation errors occur
     */
    [Theory]
    [InlineData("SecurePass123!")]
    [InlineData("MyPassword1@")]
    [InlineData("ComplexP@ssw0rd")]
    [InlineData("VeryLongPasswordWith123!")]
    public void Password_ValidFormats_PassesValidation(string password)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    /**
     * Scenario: Invalid passwords
     *   Given passwords not meeting requirements
     *   When validating the registration request
     *   Then password validation errors occur
     */
    [Theory]
    [InlineData("", "Password is required")]
    [InlineData("short", "Password must be at least 12 characters")]
    [InlineData("nouppercase123!", "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character")]
    [InlineData("NOLOWERCASE123!", "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character")]
    [InlineData("NoNumbers!", "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character")]
    [InlineData("NoSpecialChars123", "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character")]
    public void Password_InvalidFormats_FailsValidation(string password, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password) && e.ErrorMessage == expectedError);
    }

    /**
     * Feature: Password Confirmation Validation
     *   As a user registration system
     *   I want to ensure password confirmation matches
     *   So that users don't accidentally mistype their password
     * 
     * Scenario: Matching passwords
     *   Given password and confirmation match
     *   When validating the registration request
     *   Then no confirmation validation errors occur
     */
    [Fact]
    public void PasswordConfirmation_MatchesPassword_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = "SecurePass123!";
        request.ConfirmPassword = "SecurePass123!";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }

    /**
     * Scenario: Non-matching passwords
     *   Given password and confirmation don't match
     *   When validating the registration request
     *   Then confirmation validation errors occur
     */
    [Theory]
    [InlineData("", "Password confirmation is required")]
    [InlineData("DifferentPassword123!", "Passwords do not match")]
    public void PasswordConfirmation_DoesNotMatch_FailsValidation(string confirmPassword, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Password = "SecurePass123!";
        request.ConfirmPassword = confirmPassword;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.ConfirmPassword) && e.ErrorMessage == expectedError);
    }

    /**
     * Feature: Name Validation
     *   As a registration system
     *   I want to validate user names
     *   So that only valid names are accepted
     * 
     * Scenario: Valid first names
     *   Given valid first name formats
     *   When validating the registration request
     *   Then no first name validation errors occur
     */
    [Theory]
    [InlineData("John")]
    [InlineData("Mary-Jane")]
    [InlineData("José")]
    [InlineData("O'Connor")]
    [InlineData("Jean-Pierre")]
    [InlineData("Dr. Smith")]
    public void FirstName_ValidFormats_PassesValidation(string firstName)
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = firstName;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.FirstName));
    }

    /**
     * Scenario: Invalid first names
     *   Given invalid first name formats
     *   When validating the registration request
     *   Then first name validation errors occur
     */
    [Theory]
    [InlineData("", "First name is required")]
    [InlineData("John123", "First name contains invalid characters")]
    [InlineData("John@Smith", "First name contains invalid characters")]
    public void FirstName_InvalidFormats_FailsValidation(string firstName, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = firstName;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.FirstName) && e.ErrorMessage == expectedError);
    }

    /**
     * Scenario: First name too long
     *   Given a first name exceeding 100 characters
     *   When validating the registration request
     *   Then validation fails with length error
     */
    [Fact]
    public void FirstName_ExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.FirstName = new string('A', 101);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.FirstName) && e.ErrorMessage == "First name must not exceed 100 characters");
    }

    /**
     * Scenario: Valid last names
     *   Given valid last name formats
     *   When validating the registration request
     *   Then no last name validation errors occur
     */
    [Theory]
    [InlineData("Smith")]
    [InlineData("O'Brien")]
    [InlineData("Van Der Berg")]
    [InlineData("Martinez-Lopez")]
    [InlineData("Dr. Johnson")]
    public void LastName_ValidFormats_PassesValidation(string lastName)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LastName = lastName;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.LastName));
    }

    /**
     * Feature: License Validation
     *   As a professional verification system
     *   I want to validate license information
     *   So that only licensed professionals can register
     * 
     * Scenario: Valid license numbers
     *   Given valid license number formats
     *   When validating the registration request
     *   Then no license number validation errors occur
     */
    [Theory]
    [InlineData("ST12345")]
    [InlineData("ABC-123")]
    [InlineData("LIC123456")]
    [InlineData("12345")]
    public void LicenseNumber_ValidFormats_PassesValidation(string licenseNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseNumber = licenseNumber;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.LicenseNumber));
    }

    /**
     * Scenario: Invalid license numbers
     *   Given invalid license number formats
     *   When validating the registration request
     *   Then license number validation errors occur
     */
    [Theory]
    [InlineData("", "License number is required")]
    [InlineData("LIC.123.456", "License number contains invalid characters")]
    [InlineData("LIC 123", "License number contains invalid characters")]
    public void LicenseNumber_InvalidFormats_FailsValidation(string licenseNumber, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseNumber = licenseNumber;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.LicenseNumber) && e.ErrorMessage == expectedError);
    }

    /**
     * Scenario: License number too long
     *   Given a license number exceeding 50 characters
     *   When validating the registration request
     *   Then validation fails with length error
     */
    [Fact]
    public void LicenseNumber_ExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseNumber = new string('A', 51);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.LicenseNumber) && e.ErrorMessage == "License number must not exceed 50 characters");
    }

    /**
     * Scenario: Valid license states
     *   Given valid state codes
     *   When validating the registration request
     *   Then no license state validation errors occur
     */
    [Theory]
    [InlineData("CA")]
    [InlineData("NY")]
    [InlineData("TX")]
    [InlineData("FL")]
    [InlineData("DC")]
    [InlineData("ca")] // Should accept lowercase
    public void LicenseState_ValidStates_PassesValidation(string licenseState)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseState = licenseState;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.LicenseState));
    }

    /**
     * Scenario: Invalid license states
     *   Given invalid state codes
     *   When validating the registration request
     *   Then license state validation errors occur
     */
    [Theory]
    [InlineData("", "License state is required")]
    [InlineData("X", "License state must be 2 characters")]
    [InlineData("ABC", "License state must be 2 characters")]
    [InlineData("ZZ", "Invalid license state")]
    public void LicenseState_InvalidStates_FailsValidation(string licenseState, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseState = licenseState;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.LicenseState) && e.ErrorMessage == expectedError);
    }

    /**
     * Scenario: Valid license types
     *   Given valid license types
     *   When validating the registration request
     *   Then no license type validation errors occur
     */
    [Theory]
    [InlineData("SLP")]
    [InlineData("OT")]
    [InlineData("PT")]
    [InlineData("Psychologist")]
    [InlineData("LCSW")]
    [InlineData("LPC")]
    [InlineData("LMFT")]
    [InlineData("BCBA")]
    public void LicenseType_ValidTypes_PassesValidation(string licenseType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseType = licenseType;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.LicenseType));
    }

    /**
     * Scenario: Invalid license types
     *   Given invalid license types
     *   When validating the registration request
     *   Then license type validation errors occur
     */
    [Theory]
    [InlineData("", "License type is required")]
    [InlineData("INVALID", "Invalid license type")]
    [InlineData("MD", "Invalid license type")]
    public void LicenseType_InvalidTypes_FailsValidation(string licenseType, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.LicenseType = licenseType;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.LicenseType) && e.ErrorMessage == expectedError);
    }

    /**
     * Feature: Phone Number Validation
     *   As a contact system
     *   I want to validate phone numbers
     *   So that valid contact information is stored
     * 
     * Scenario: Valid phone numbers
     *   Given valid phone number formats
     *   When validating the registration request
     *   Then no phone validation errors occur
     */
    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    [InlineData("555 123 4567")]
    [InlineData("+1-555-123-4567")]
    [InlineData("15551234567")]
    [InlineData(null)] // Phone is optional
    [InlineData("")] // Phone is optional
    public void Phone_ValidFormats_PassesValidation(string? phone)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Phone = phone;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.Phone));
    }

    /**
     * Scenario: Invalid phone numbers
     *   Given invalid phone number formats
     *   When validating the registration request
     *   Then phone validation errors occur
     */
    [Theory]
    [InlineData("123", "Invalid phone number format")]
    [InlineData("555-12-4567", "Invalid phone number format")]
    [InlineData("555-123-456", "Invalid phone number format")]
    [InlineData("abc-def-ghij", "Invalid phone number format")]
    public void Phone_InvalidFormats_FailsValidation(string phone, string expectedError)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Phone = phone;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Phone) && e.ErrorMessage == expectedError);
    }

    /**
     * Feature: Terms Acceptance Validation
     *   As a legal compliance system
     *   I want to ensure users accept terms
     *   So that registration is legally valid
     * 
     * Scenario: Terms accepted
     *   Given the user accepts terms
     *   When validating the registration request
     *   Then no terms validation errors occur
     */
    [Fact]
    public void AcceptedTerms_True_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AcceptedTerms = true;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RegisterRequest.AcceptedTerms));
    }

    /**
     * Scenario: Terms not accepted
     *   Given the user doesn't accept terms
     *   When validating the registration request
     *   Then terms validation errors occur
     */
    [Fact]
    public void AcceptedTerms_False_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AcceptedTerms = false;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.AcceptedTerms) && e.ErrorMessage == "You must accept the terms of service");
    }

    /**
     * Feature: Complete Registration Validation
     *   As a registration system
     *   I want to validate complete registration requests
     *   So that all requirements are met
     * 
     * Scenario: Completely valid registration
     *   Given a registration request with all valid data
     *   When validating the registration request
     *   Then no validation errors occur
     */
    [Fact]
    public void CompleteRegistration_AllValidData_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /**
     * Scenario: Multiple validation errors
     *   Given a registration request with multiple invalid fields
     *   When validating the registration request
     *   Then multiple validation errors are returned
     */
    [Fact]
    public void CompleteRegistration_MultipleInvalidFields_ReturnsMultipleErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "weak",
            ConfirmPassword = "different",
            FirstName = "",
            LastName = "",
            LicenseNumber = "",
            LicenseState = "ZZ",
            LicenseType = "INVALID",
            Phone = "123",
            AcceptedTerms = false
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(5);
    }

    // Helper method to create a valid registration request
    private static RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest
        {
            Email = "therapist@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "ST12345",
            LicenseState = "CA",
            LicenseType = "SLP",
            Phone = "555-123-4567",
            AcceptedTerms = true
        };
    }
}