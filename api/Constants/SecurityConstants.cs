namespace TherapyDocs.Api.Constants;

/// <summary>
/// Security-related constants for the application.
/// </summary>
public static class SecurityConstants
{
    /// <summary>
    /// Minimum response time to prevent timing attacks during registration and authentication.
    /// </summary>
    public static readonly TimeSpan MinimumRegistrationResponseTime = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Minimum response time for email verification operations.
    /// </summary>
    public static readonly TimeSpan MinimumEmailVerificationResponseTime = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Minimum response time for authentication operations.
    /// </summary>
    public static readonly TimeSpan MinimumAuthenticationResponseTime = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Maximum number of failed login attempts before account lockout.
    /// </summary>
    public const int MaxFailedLoginAttempts = 5;

    /// <summary>
    /// Account lockout duration in minutes.
    /// </summary>
    public const int AccountLockoutDurationMinutes = 15;

    /// <summary>
    /// JWT token expiration time in hours.
    /// </summary>
    public const int JwtTokenExpirationHours = 8;

    /// <summary>
    /// Email verification token expiration time in hours.
    /// </summary>
    public const int EmailVerificationTokenExpirationHours = 24;

    /// <summary>
    /// Password reset token expiration time in hours.
    /// </summary>
    public const int PasswordResetTokenExpirationHours = 1;

    /// <summary>
    /// Minimum password length required.
    /// </summary>
    public const int MinimumPasswordLength = 8;

    /// <summary>
    /// Maximum password length allowed.
    /// </summary>
    public const int MaximumPasswordLength = 128;

    /// <summary>
    /// Number of previous passwords to track for history.
    /// </summary>
    public const int PasswordHistoryCount = 5;
}