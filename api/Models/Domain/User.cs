using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Core user entity representing therapy professionals, parents, and other system users
/// </summary>
public class User
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    public Guid? OrganizationId { get; set; }

    public bool IsSellerApproved { get; set; } = false;

    [Required]
    public List<string> Languages { get; set; } = new() { "English" };

    public List<string> Specialties { get; set; } = new();

    [Required]
    public UserRole Role { get; set; } = UserRole.Therapist;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    public bool TwoFactorEnabled { get; set; } = false;

    public string PreferredLanguage { get; set; } = "en";

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Active;

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Soft delete fields
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(2)]
    public string LicenseState { get; set; } = "CA";

    [MaxLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    // License verification fields
    [MaxLength(10)]
    public string? LicenseType { get; set; }
    
    public bool LicenseVerified { get; set; } = false;
    
    public DateTime? LicenseVerifiedAt { get; set; }
    
    public DateTime? LicenseExpirationDate { get; set; }

    // User preferences
    [MaxLength(50)]
    public string? Timezone { get; set; }
    
    public bool EmailNotificationsEnabled { get; set; } = true;
    
    [MaxLength(20)]
    public string? Theme { get; set; }
    
    [MaxLength(50)]
    public string? DefaultView { get; set; }

    // Navigation properties
    public virtual Organization? Organization { get; set; }
    public virtual SellerProfile? SellerProfile { get; set; }
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<ResourceDownload> Downloads { get; set; } = new List<ResourceDownload>();
    public virtual ICollection<ResourceRating> Ratings { get; set; } = new List<ResourceRating>();
}

public enum SubscriptionTier
{
    Free,
    Basic,
    Pro,
    SmallGroup,
    LargeGroup,
    Enterprise
}

public enum UserRole
{
    Therapist,
    Parent,
    Student,
    Supervisor,
    Admin,
    SystemAdmin,
    OrganizationAdmin
}

public enum SubscriptionStatus
{
    Active,
    Inactive,
    Expired,
    Cancelled,
    PendingPayment,
    Trial
}