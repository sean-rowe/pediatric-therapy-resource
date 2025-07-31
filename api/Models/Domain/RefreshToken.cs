using System;

namespace UPTRMS.Api.Models.Domain;

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}