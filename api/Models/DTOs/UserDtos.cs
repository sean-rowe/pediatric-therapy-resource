using System.ComponentModel.DataAnnotations;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Models.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password too weak")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password too weak")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required")]
    public string? LicenseNumber { get; set; }

    public List<string> Specialties { get; set; } = new();

    public List<string> Languages { get; set; } = new() { "English" };

    public UserRole Role { get; set; } = UserRole.Therapist;
    
    // Additional fields expected by tests
    public string? ConfirmPassword { get; set; }
    public string? LicenseState { get; set; }
    public string? LicenseType { get; set; } 
    public string? Phone { get; set; }
    
    [Required(ErrorMessage = "Terms must be accepted")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Terms must be accepted")]
    public bool AcceptedTerms { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? TwoFactorCode { get; set; }
}

public class LoginResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public UserRole Role { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> Specialties { get; set; } = new();
    public bool IsSellerApproved { get; set; }
    public bool EmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public OrganizationDto? Organization { get; set; }
}

public class OrganizationDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public OrganizationType Type { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public int LicenseCount { get; set; }
    public int UsedLicenses { get; set; }
    public bool SsoEnabled { get; set; }
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? LicenseNumber { get; set; }
    public List<string>? Specialties { get; set; }
    public List<string>? Languages { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordConfirmRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}