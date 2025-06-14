using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Interfaces;

public interface IAccountLockoutRepository
{
    Task<AccountLockoutStatus> CheckAccountLockoutAsync(string email);
    Task RecordFailedLoginAttemptAsync(string email, string? ipAddress, string? userAgent);
    Task ClearFailedLoginAttemptsAsync(string email);
}