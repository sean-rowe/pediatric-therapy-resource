using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Individual learner entity with encrypted PII
/// </summary>
public class Student
{
    [Key]
    public Guid StudentId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TherapistId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstNameEncrypted { get; set; } = string.Empty; // Encrypted

    [Required]
    [MaxLength(100)]
    public string LastNameEncrypted { get; set; } = string.Empty; // Encrypted

    [Required]
    public string DateOfBirthEncrypted { get; set; } = string.Empty; // Encrypted

    [MaxLength(255)]
    public string? ParentEmailEncrypted { get; set; } // Encrypted

    public string? IepGoalsEncrypted { get; set; } // JSONB - Encrypted

    [Required]
    [MaxLength(10)]
    public string AccessCode { get; set; } = GenerateAccessCode();

    [MaxLength(50)]
    public string? ExternalId { get; set; } // From school SIS

    [MaxLength(20)]
    public string? GradeLevel { get; set; }

    [MaxLength(100)]
    public string? PrimaryDisability { get; set; }

    public bool IsActive { get; set; } = true;
    
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    
    public bool HasParentAccess { get; set; } = false;
    
    public DateTime? DischargedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityAt { get; set; }

    // Navigation properties
    public virtual User Therapist { get; set; } = null!;
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<StudentProgress> ProgressEntries { get; set; } = new List<StudentProgress>();
    public virtual ICollection<StudentAssignment> Assignments { get; set; } = new List<StudentAssignment>();
    public virtual ICollection<ParentAccess> ParentAccesses { get; set; } = new List<ParentAccess>();
    public virtual ICollection<StudentGoal> Goals { get; set; } = new List<StudentGoal>();

    private static string GenerateAccessCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}