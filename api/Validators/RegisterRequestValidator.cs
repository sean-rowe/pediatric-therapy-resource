using FluentValidation;
using TherapyDocs.Api.Models.DTOs;
using System.Text.RegularExpressions;

namespace TherapyDocs.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private static readonly string[] ValidLicenseTypes = new[]
    {
        "SLP", "OT", "PT", "Psychologist", "LCSW", "LPC", "LMFT", "BCBA"
    };

    private static readonly string[] ValidStates = new[]
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"
    };

    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters")
            .Must(BeValidPassword).WithMessage("Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required")
            .MaximumLength(50).WithMessage("License number must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("License number contains invalid characters");

        RuleFor(x => x.LicenseState)
            .NotEmpty().WithMessage("License state is required")
            .Length(2).WithMessage("License state must be 2 characters")
            .Must(state => ValidStates.Contains(state.ToUpper())).WithMessage("Invalid license state");

        RuleFor(x => x.LicenseType)
            .NotEmpty().WithMessage("License type is required")
            .Must(type => ValidLicenseTypes.Contains(type)).WithMessage("Invalid license type");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?1?[-.\s]?\(?[0-9]{3}\)?[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$")
            .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.AcceptedTerms)
            .Equal(true).WithMessage("You must accept the terms of service");
    }

    private static bool BeValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasNumber = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        return hasUpperCase && hasLowerCase && hasNumber && hasSpecialChar;
    }
}