using System;

namespace UPTRMS.Api.Models.Domain;

public class EmailVerificationToken
{
    public Guid TokenId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}