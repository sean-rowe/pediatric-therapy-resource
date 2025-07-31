using System.Linq.Expressions;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public interface IMarketplaceRepository : IRepository<MarketplaceTransaction>
{
    Task<IEnumerable<MarketplaceTransaction>> GetTransactionsAsync(
        Guid? buyerId = null,
        Guid? sellerId = null,
        Guid? resourceId = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20);
    Task<MarketplaceTransaction> CreateTransactionAsync(MarketplaceTransaction transaction);
    Task UpdateTransactionStatusAsync(
        Guid transactionId,
        PaymentStatus status,
        string? paymentIntentId = null,
        DateTime? processedAt = null);
    Task<SellerRevenueStatisticsDto> GetSellerRevenueStatisticsAsync(
        Guid sellerId,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<IEnumerable<BuyerPurchaseHistoryDto>> GetBuyerPurchaseHistoryAsync(
        Guid buyerId,
        int skip = 0,
        int take = 20);
    Task<MarketplaceAnalyticsDto> GetMarketplaceAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<SellerPayout> ProcessSellerPayoutAsync(
        Guid sellerId,
        decimal amount,
        string payoutMethod,
        string? payoutReference = null,
        List<Guid>? transactionIds = null);
    Task<IEnumerable<SellerPayout>> GetSellerPayoutHistoryAsync(
        Guid sellerId,
        int skip = 0,
        int take = 20);
}

public class SellerRevenueStatisticsDto
{
    public int TotalTransactions { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public int UniqueBuyers { get; set; }
    public List<ResourceRevenueDto> ResourceRevenue { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
}

public class ResourceRevenueDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal ResourceRevenue { get; set; }
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public int TransactionCount { get; set; }
    public decimal DailyGrossRevenue { get; set; }
    public decimal DailyNetRevenue { get; set; }
}

public class BuyerPurchaseHistoryDto
{
    public Guid TransactionId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Amount { get; set; }
    public List<Guid> ResourceIds { get; set; } = new();
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
}

public class MarketplaceAnalyticsDto
{
    public OverallStats Overall { get; set; } = new();
    public List<TopSellerDto> TopSellers { get; set; } = new();
    public List<TopResourceDto> TopResources { get; set; } = new();
    public List<RevenueTrendDto> RevenueTrend { get; set; } = new();
}

public class OverallStats
{
    public int TotalTransactions { get; set; }
    public int UniqueBuyers { get; set; }
    public int ActiveSellers { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageTransactionValue { get; set; }
}

public class TopSellerDto
{
    public Guid UserId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
}

public class TopResourceDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal ResourceRevenue { get; set; }
}

public class RevenueTrendDto
{
    public DateTime Date { get; set; }
    public int TransactionCount { get; set; }
    public decimal DailyRevenue { get; set; }
    public decimal DailyCommission { get; set; }
}