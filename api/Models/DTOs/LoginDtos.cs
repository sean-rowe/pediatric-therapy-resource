namespace TherapyDocs.Api.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public int? RemainingAttempts { get; set; }
    public bool IsLocked { get; set; }
    public bool RequiresEmailVerification { get; set; }
    public bool PasswordChangeRequired { get; set; }
    public bool PasswordExpiryWarning { get; set; }
    public int? DaysUntilPasswordExpiry { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
}

public class AccountLockoutStatus
{
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public int RemainingAttempts { get; set; }
}

public class PasswordChangeRequirement
{
    public bool ChangeRequired { get; set; }
    public int DaysUntilExpiry { get; set; }
}