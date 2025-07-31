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

    private string _resourceIdsJson = "[]";
    [Required]
    public string ResourceIdsJson 
    { 
        get => _resourceIdsJson;
        set => _resourceIdsJson = value;
    }
    
    // Property for stored procedure compatibility - as List<Guid>
    private List<Guid>? _resourceIds;
    public List<Guid> ResourceIds
    {
        get
        {
            if (_resourceIds == null)
            {
                _resourceIds = string.IsNullOrEmpty(ResourceIdsJson) 
                    ? new List<Guid>() 
                    : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(ResourceIdsJson) ?? new List<Guid>();
            }
            return _resourceIds;
        }
        set
        {
            _resourceIds = value;
            ResourceIdsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }

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
    
    // Alias for compatibility with stored procedures
    public string? PaymentIntentId 
    { 
        get => StripePaymentIntentId; 
        set => StripePaymentIntentId = value; 
    }
    
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
    
    // Alias for compatibility with stored procedures
    public DateTime? ProcessedAt 
    { 
        get => CompletedAt; 
        set => CompletedAt = value; 
    }

    // Navigation properties
    public virtual User Buyer { get; set; } = null!;
    public virtual SellerProfile Seller { get; set; } = null!;

    // Helper methods (now use the ResourceIds property directly)
    public List<Guid> GetResourceIds() => ResourceIds;

    public void SetResourceIds(List<Guid> ids) => ResourceIds = ids;
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

    private PayoutStatus _status = PayoutStatus.Pending;
    [Required]
    public PayoutStatus Status 
    { 
        get => _status;
        set => _status = value;
    }

    private PayoutMethod _method = PayoutMethod.DirectDeposit;
    [Required]
    public PayoutMethod Method 
    { 
        get => _method;
        set => _method = value;
    }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    // Alias for compatibility with stored procedures
    public string? PayoutReference 
    { 
        get => ReferenceNumber; 
        set => ReferenceNumber = value; 
    }
    
    // String version for stored procedure compatibility
    [MaxLength(50)]
    public string PayoutMethodString 
    { 
        get => Method.ToString(); 
        set => Method = Enum.TryParse<PayoutMethod>(value, out PayoutMethod method) ? method : PayoutMethod.DirectDeposit; 
    }
    
    // String version for stored procedure compatibility
    [MaxLength(50)]
    public string StatusString 
    { 
        get => Status.ToString(); 
        set => Status = Enum.TryParse<PayoutStatus>(value, out PayoutStatus status) ? status : PayoutStatus.Pending; 
    }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    // Alias for compatibility with stored procedures
    public DateTime CreatedAt 
    { 
        get => RequestedAt; 
        set => RequestedAt = value; 
    }

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Transaction IDs for this payout
    public List<Guid>? TransactionIds { get; set; }

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