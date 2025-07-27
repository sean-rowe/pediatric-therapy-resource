using System;

namespace UPTRMS.Api.Models.Domain;

public class PasswordResetToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsUsed { get; set; }
    public string? IpAddress { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}