using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Organization entity for schools, clinics, and therapy practices
/// </summary>
public class Organization
{
    [Key]
    public Guid OrganizationId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public OrganizationType Type { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [Required]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.SmallGroup;

    public int LicenseCount { get; set; } = 5;

    public int UsedLicenses { get; set; } = 0;

    public bool SsoEnabled { get; set; } = false;

    [MaxLength(100)]
    public string? SsoProvider { get; set; }

    [MaxLength(500)]
    public string? SsoConfiguration { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubscriptionExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

public enum OrganizationType
{
    School,
    SchoolDistrict,
    PrivatePractice,
    Clinic,
    Hospital,
    Other
}