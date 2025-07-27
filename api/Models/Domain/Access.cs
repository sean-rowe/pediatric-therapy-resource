using System;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Parent access to student information
/// </summary>
public class ParentAccess
{
    [Key]
    public Guid AccessId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ParentUserId { get; set; }

    [Required]
    [MaxLength(10)]
    public string AccessCode { get; set; } = string.Empty;

    [Required]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.ViewOnly;

    public DateTime? ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastAccessedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual User ParentUser { get; set; } = null!;
}

public enum AccessLevel
{
    ViewOnly,
    ViewAndDownload,
    ViewDownloadAndMessage,
    Full
}

/// <summary>
/// Student assignment tracking
/// </summary>
public class StudentAssignment
{
    [Key]
    public Guid AssignmentId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    public Guid AssignedByUserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Instructions { get; set; }

    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;

    public string? CompletionData { get; set; } // JSONB

    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual User AssignedByUser { get; set; } = null!;
}

public enum AssignmentStatus
{
    Assigned,
    InProgress,
    Completed,
    Late,
    Cancelled
}

/// <summary>
/// AI content generation record
/// </summary>
public class AIGeneration
{
    [Key]
    public Guid GenerationId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ModelUsed { get; set; } = string.Empty;

    [Required]
    public int TokensConsumed { get; set; }

    [Required]
    public int GenerationTimeMs { get; set; }

    public Guid? OutputResourceId { get; set; }

    [Required]
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

    [MaxLength(2000)]
    public string? ClinicalReviewNotes { get; set; }

    public string? GenerationParameters { get; set; } // JSONB

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Resource? OutputResource { get; set; }
}

public enum GenerationStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Rejected,
    RequiresReview
}