using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TherapyDocs.Api.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("role")]
    public string Role { get; set; } = "therapist";

    [StringLength(50)]
    [Column("license_number")]
    public string? LicenseNumber { get; set; }

    [StringLength(2)]
    [Column("license_state")]
    public string? LicenseState { get; set; }

    [Required]
    [StringLength(50)]
    [Column("service_type")]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(50)]
    [Column("subscription_tier")]
    public string SubscriptionTier { get; set; } = "basic";

    [Column("subscription_expires")]
    public DateTime? SubscriptionExpires { get; set; }

    [Column("monthly_content_generated")]
    public int MonthlyContentGenerated { get; set; } = 0;

    [Column("content_generation_limit")]
    public int ContentGenerationLimit { get; set; } = 50;

    [StringLength(50)]
    [Column("timezone")]
    public string Timezone { get; set; } = "America/New_York";

    [StringLength(50)]
    [Column("preferred_note_template")]
    public string PreferredNoteTemplate { get; set; } = "soap";

    [Column("auto_save_notes")]
    public bool AutoSaveNotes { get; set; } = true;

    [Column("offline_sync_enabled")]
    public bool OfflineSyncEnabled { get; set; } = true;

    [Column("push_notifications")]
    public bool PushNotifications { get; set; } = true;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("email_verified")]
    public bool EmailVerified { get; set; } = false;

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [StringLength(50)]
    [Column("status")]
    public string Status { get; set; } = "pending";
}