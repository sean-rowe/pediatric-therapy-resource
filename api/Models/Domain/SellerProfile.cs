using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Marketplace seller entity for therapists selling resources
/// </summary>
public class SellerProfile
{
    [Key]
    public Guid SellerId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string StoreUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Bio { get; set; } = string.Empty;

    public List<string> Specialties { get; set; } = new();

    [Range(0, 5)]
    public decimal Rating { get; set; } = 0;

    public int TotalSales { get; set; } = 0;

    public int TotalReviews { get; set; } = 0;

    [Range(0, 1)]
    public decimal CommissionRate { get; set; } = 0.30m;

    public decimal TotalEarnings { get; set; } = 0;

    public decimal AvailableBalance { get; set; } = 0;

    public bool IsVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSaleAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Seller settings
    public bool AcceptsCustomOrders { get; set; } = false;
    public int ProcessingTimeDays { get; set; } = 1;
    public bool VacationMode { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Resource> Products { get; set; } = new List<Resource>();
    public virtual ICollection<MarketplaceTransaction> Sales { get; set; } = new List<MarketplaceTransaction>();
    public virtual ICollection<SellerFollower> Followers { get; set; } = new List<SellerFollower>();
    public virtual ICollection<SellerPayout> Payouts { get; set; } = new List<SellerPayout>();
}