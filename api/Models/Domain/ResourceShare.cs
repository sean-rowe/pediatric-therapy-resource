using System;

namespace UPTRMS.Api.Models.Domain;

public class ResourceShare
{
    public Guid ShareId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid SharedByUserId { get; set; }
    public string ShareToken { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AccessedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User SharedByUser { get; set; } = null!;
}