using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Therapy session entity for tracking sessions and resource usage
/// </summary>
public class Session
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TherapistId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public DateTime SessionDate { get; set; }

    [Required]
    [Range(1, 480)] // Max 8 hours
    public int DurationMinutes { get; set; }

    [Required]
    public SessionType SessionType { get; set; } = SessionType.Individual;

    public string? NotesEncrypted { get; set; } // Encrypted

    public string? DataPointsJson { get; set; } // JSONB
    
    // Alias for compatibility with stored procedures
    public string DataPoints 
    { 
        get => DataPointsJson ?? "{}"; 
        set => DataPointsJson = value; 
    }

    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

    public bool IsCompleted { get; set; } = false;

    public bool IsBillable { get; set; } = true;

    [MaxLength(50)]
    public string? CptCode { get; set; }
    
    // Alias for compatibility with stored procedures
    public string? BillingCode 
    { 
        get => CptCode; 
        set => CptCode = value; 
    }
    
    [MaxLength(200)]
    public string? Location { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User Therapist { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
    public virtual ICollection<SessionResource> SessionResources { get; set; } = new List<SessionResource>();
    public virtual ICollection<SessionDataPoint> DataPointEntries { get; set; } = new List<SessionDataPoint>();

    // Additional navigation properties for controllers
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public virtual ICollection<StudentGoal> Goals { get; set; } = new List<StudentGoal>();
    
    // Property for stored procedure compatibility
    private List<Guid>? _resourcesUsed;
    public List<Guid> ResourcesUsed 
    { 
        get => _resourcesUsed ?? new List<Guid>();
        set => _resourcesUsed = value;
    }
}

public enum SessionType
{
    Individual,
    Group,
    Teletherapy,
    Evaluation,
    Consultation,
    Supervision
}

/// <summary>
/// Junction table for resources used in sessions
/// </summary>
public class SessionResource
{
    [Key]
    public Guid SessionResourceId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid ResourceId { get; set; }

    public int? MinutesUsed { get; set; }

    public string? Notes { get; set; }

    [Required]
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Session Session { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
}