using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Therapy material entity representing worksheets, games, videos, assessments, etc.
/// </summary>
public class Resource
{
    [Key]
    public Guid ResourceId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public ResourceType ResourceType { get; set; }

    [Required]
    public string SkillAreas { get; set; } = "{}"; // JSONB - stored as JSON string

    [Required]
    public string GradeLevels { get; set; } = "[]"; // Array of integers as JSON

    [Required]
    public GenerationMethod GenerationMethod { get; set; } = GenerationMethod.Manual;

    public string? AiGenerationMetadata { get; set; } // JSONB

    [Required]
    public ClinicalReviewStatus ClinicalReviewStatus { get; set; } = ClinicalReviewStatus.Pending;

    [Range(1, 5)]
    public int? EvidenceLevel { get; set; }

    [Required]
    public List<string> LanguagesAvailable { get; set; } = new() { "English" };

    public bool IsInteractive { get; set; } = false;

    public bool HasAudio { get; set; } = false;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public long? FileSizeBytes { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public bool IsPublished { get; set; } = false;

    public bool IsMarketplaceItem { get; set; } = false;

    public decimal? Price { get; set; }

    public Guid? SellerId { get; set; }
    
    // Alias for compatibility with stored procedures
    public Guid? SellerUserId 
    { 
        get => SellerId; 
        set => SellerId = value; 
    }

    public string? PreviewUrl { get; set; }
    
    public bool IsFree { get; set; } = true;
    
    public decimal CommissionRate { get; set; } = 0.30m;
    
    // Alias for compatibility with stored procedures
    public List<string> Languages 
    { 
        get => LanguagesAvailable; 
        set => LanguagesAvailable = value; 
    }

    public int ViewCount { get; set; } = 0;

    public int DownloadCount { get; set; } = 0;

    public decimal Rating { get; set; } = 0;
    
    // Alias for compatibility with stored procedures
    public decimal AverageRating 
    { 
        get => Rating; 
        set => Rating = value; 
    }

    public int ReviewCount { get; set; } = 0;

    public DateTime? RetiredAt { get; set; }

    public string? RetiredReason { get; set; }

    public Guid? RetiredBy { get; set; }

    public List<Guid> SuggestedAlternatives { get; set; } = new();

    public string? Version { get; set; }

    public Guid? PreviousVersionId { get; set; }

    public Guid? LatestVersionId { get; set; }

    public bool IsSuperseded { get; set; } = false;

    public DateTime? SupersededAt { get; set; }
    
    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual SellerProfile? Seller { get; set; }
    public virtual ICollection<ResourceCategory> Categories { get; set; } = new List<ResourceCategory>();
    public virtual ICollection<ResourceDownload> Downloads { get; set; } = new List<ResourceDownload>();
    public virtual ICollection<ResourceRating> Ratings { get; set; } = new List<ResourceRating>();
    public virtual ICollection<SessionResource> SessionUses { get; set; } = new List<SessionResource>();

    // Helper methods for JSON fields
    public Dictionary<string, object> GetSkillAreas() =>
        string.IsNullOrEmpty(SkillAreas) ? new() : JsonSerializer.Deserialize<Dictionary<string, object>>(SkillAreas) ?? new();

    public void SetSkillAreas(Dictionary<string, object> value) =>
        SkillAreas = JsonSerializer.Serialize(value);

    public List<int> GetGradeLevels() =>
        string.IsNullOrEmpty(GradeLevels) ? new() : JsonSerializer.Deserialize<List<int>>(GradeLevels) ?? new();

    public void SetGradeLevels(List<int> value) =>
        GradeLevels = JsonSerializer.Serialize(value);

    // Helper method for suggested alternatives (stored as JSON in the database if needed)
    public void SetSuggestedAlternatives(List<Guid> value) =>
        SuggestedAlternatives = value;
}

public enum ResourceType
{
    Worksheet,
    Game,
    Video,
    Assessment,
    DigitalActivity,
    Flashcard,
    VisualSupport,
    SocialStory,
    DataSheet,
    ParentHandout,
    ProfessionalResource
}

public enum GenerationMethod
{
    Manual,
    AiGenerated,
    AiAssisted
}

public enum ClinicalReviewStatus
{
    Pending,
    Approved,
    Rejected,
    NeedsRevision,
    Retired
}