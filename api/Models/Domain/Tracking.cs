using System;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Resource download tracking
/// </summary>
public class ResourceDownload
{
    [Key]
    public Guid DownloadId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Resource rating and review
/// </summary>
public class ResourceRating
{
    [Key]
    public Guid RatingId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }

    public bool IsVerifiedPurchase { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Student progress tracking
/// </summary>
public class StudentProgress
{
    [Key]
    public Guid ProgressId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string GoalArea { get; set; } = string.Empty;

    [Required]
    public DateTime MeasurementDate { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal PercentageComplete { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public string? DataPoints { get; set; } // JSONB

    [Required]
    public Guid RecordedByUserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual User RecordedByUser { get; set; } = null!;
}

/// <summary>
/// Session data point for detailed tracking
/// </summary>
public class SessionDataPoint
{
    [Key]
    public Guid DataPointId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Metric { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Session Session { get; set; } = null!;
}