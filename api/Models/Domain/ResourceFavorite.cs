using System;

namespace UPTRMS.Api.Models.Domain;

public class ResourceFavorite
{
    public Guid FavoriteId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FolderName { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}