using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TherapyDocs.Api.Models;

[Table("students")]
public class Student
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    [Column("gender")]
    public string? Gender { get; set; }

    [Required]
    [Column("therapist_id")]
    public Guid TherapistId { get; set; }

    [StringLength(100)]
    [Column("student_id")]
    public string? StudentId { get; set; }

    [StringLength(200)]
    [Column("school_name")]
    public string? SchoolName { get; set; }

    [StringLength(50)]
    [Column("grade_level")]
    public string? GradeLevel { get; set; }

    [StringLength(200)]
    [Column("primary_diagnosis")]
    public string? PrimaryDiagnosis { get; set; }

    [StringLength(500)]
    [Column("secondary_diagnoses")]
    public string? SecondaryDiagnoses { get; set; }

    [StringLength(1000)]
    [Column("goals")]
    public string? Goals { get; set; }

    [StringLength(1000)]
    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TherapistId")]
    public virtual User? Therapist { get; set; }
}