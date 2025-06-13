using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TherapyDocs.Api.Models;

[Table("registration_audit_log")]
public class RegistrationAuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("license_number")]
    public string? LicenseNumber { get; set; }

    [StringLength(2)]
    [Column("license_state")]
    public string? LicenseState { get; set; }

    [Column("success")]
    public bool Success { get; set; }

    [StringLength(500)]
    [Column("failure_reason")]
    public string? FailureReason { get; set; }

    [StringLength(45)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}