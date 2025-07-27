using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Marketplace transaction for resource purchases
/// </summary>
public class MarketplaceTransaction
{
    [Key]
    public Guid TransactionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BuyerId { get; set; }

    [Required]
    public Guid SellerId { get; set; }

    [Required]
    public string ResourceIds { get; set; } = "[]"; // JSON array of resource IDs

    [Required]
    [Range(0, 999999.99)]
    public decimal Amount { get; set; }

    [Required]
    [Range(0, 999999.99)]
    public decimal Commission { get; set; }

    [Required]
    [Range(0, 999999.99)]
    public decimal SellerEarnings { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? StripePaymentIntentId { get; set; }

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual User Buyer { get; set; } = null!;
    public virtual SellerProfile Seller { get; set; } = null!;

    // Helper methods
    public List<Guid> GetResourceIds() => 
        string.IsNullOrEmpty(ResourceIds) ? new() : 
        System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(ResourceIds) ?? new();

    public void SetResourceIds(List<Guid> ids) => 
        ResourceIds = System.Text.Json.JsonSerializer.Serialize(ids);
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    PartialRefund
}

/// <summary>
/// Seller follower relationship
/// </summary>
public class SellerFollower
{
    [Key]
    public Guid FollowerId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SellerId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    public bool NotificationsEnabled { get; set; } = true;

    // Navigation properties
    public virtual SellerProfile Seller { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Seller payout record
/// </summary>
public class SellerPayout
{
    [Key]
    public Guid PayoutId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SellerId { get; set; }

    [Required]
    [Range(0, 999999.99)]
    public decimal Amount { get; set; }

    [Required]
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    [Required]
    public PayoutMethod Method { get; set; } = PayoutMethod.DirectDeposit;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual SellerProfile Seller { get; set; } = null!;
}

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum PayoutMethod
{
    DirectDeposit,
    PayPal,
    Check
}