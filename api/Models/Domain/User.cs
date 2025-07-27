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

    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Active;

    public DateTime? SubscriptionStartDate { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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