namespace TherapyDocs.Api.Models.DTOs;

public class AccountLockoutStatus
{
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEndTime { get; set; }
    public int FailedAttempts { get; set; }
    public int RemainingAttempts { get; set; }
}