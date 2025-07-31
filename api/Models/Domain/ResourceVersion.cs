using System;

namespace UPTRMS.Api.Models.Domain;

public class ResourceVersion
{
    public Guid VersionId { get; set; }
    public Guid ResourceId { get; set; }
    public int VersionNumber { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string ChangeNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PreviousContent { get; set; } // JSON snapshot

    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User ChangedByUser { get; set; } = null!;
}