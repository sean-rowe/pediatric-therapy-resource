using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using Newtonsoft.Json;

namespace UPTRMS.Api.Repositories;

public class MarketplaceRepositoryStoredProc : IMarketplaceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public MarketplaceRepositoryStoredProc(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<MarketplaceTransaction?> GetByIdAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetMarketplaceTransactionById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TransactionId", id);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapTransactionFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<MarketplaceTransaction>> GetAllAsync()
    {
        return await GetAllAsync(0, int.MaxValue);
    }

    public async Task<IEnumerable<MarketplaceTransaction>> GetAllAsync(int skip, int take)
    {
        List<MarketplaceTransaction> transactions = new List<MarketplaceTransaction>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetMarketplaceTransactions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransactionFromReader(reader));
        }
        
        return transactions;
    }

    public async Task<MarketplaceTransaction> AddAsync(MarketplaceTransaction entity)
    {
        return await CreateTransactionAsync(entity);
    }

    public async Task UpdateAsync(MarketplaceTransaction entity)
    {
        await UpdateTransactionStatusAsync(
            entity.TransactionId,
            entity.PaymentStatus,
            entity.PaymentIntentId,
            entity.ProcessedAt);
    }

    public async Task DeleteAsync(Guid id)
    {
        // Marketplace transactions are typically not deleted
        throw new NotSupportedException("Marketplace transactions cannot be deleted");
    }

    public async Task<IEnumerable<MarketplaceTransaction>> GetTransactionsAsync(
        Guid? buyerId = null,
        Guid? sellerId = null,
        Guid? resourceId = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20)
    {
        List<MarketplaceTransaction> transactions = new List<MarketplaceTransaction>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetMarketplaceTransactions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BuyerId", (object?)buyerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SellerId", (object?)sellerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ResourceId", (object?)resourceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentStatus", (object?)paymentStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransactionFromReader(reader));
        }
        
        return transactions;
    }

    public async Task<MarketplaceTransaction> CreateTransactionAsync(MarketplaceTransaction transaction)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateMarketplaceTransaction", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter transactionIdParam = new SqlParameter("@TransactionId", SqlDbType.UniqueIdentifier);
        transactionIdParam.Direction = ParameterDirection.Output;
        transactionIdParam.Value = transaction.TransactionId == Guid.Empty ? DBNull.Value : transaction.TransactionId;
        command.Parameters.Add(transactionIdParam);
        
        command.Parameters.AddWithValue("@BuyerId", transaction.BuyerId);
        command.Parameters.AddWithValue("@SellerId", transaction.SellerId);
        command.Parameters.AddWithValue("@ResourceIds", JsonConvert.SerializeObject(transaction.ResourceIds));
        command.Parameters.AddWithValue("@Amount", transaction.Amount);
        command.Parameters.AddWithValue("@Commission", transaction.Commission);
        command.Parameters.AddWithValue("@PaymentStatus", (int)transaction.PaymentStatus);
        command.Parameters.AddWithValue("@PaymentIntentId", (object?)transaction.PaymentIntentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentMethod", (object?)transaction.PaymentMethod ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        transaction.TransactionId = (Guid)transactionIdParam.Value;
        transaction.CreatedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        return transaction;
    }

    public async Task UpdateTransactionStatusAsync(
        Guid transactionId,
        PaymentStatus status,
        string? paymentIntentId = null,
        DateTime? processedAt = null)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateMarketplaceTransactionStatus", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@TransactionId", transactionId);
        command.Parameters.AddWithValue("@PaymentStatus", (int)status);
        command.Parameters.AddWithValue("@PaymentIntentId", (object?)paymentIntentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ProcessedAt", (object?)processedAt ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<SellerRevenueStatisticsDto> GetSellerRevenueStatisticsAsync(
        Guid sellerId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        SellerRevenueStatisticsDto stats = new SellerRevenueStatisticsDto();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSellerRevenueStatistics", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SellerId", sellerId);
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        // First result set: Summary statistics
        if (await reader.ReadAsync())
        {
            stats.TotalTransactions = reader.GetInt32(reader.GetOrdinal("TotalTransactions"));
            stats.GrossRevenue = reader.GetDecimal(reader.GetOrdinal("GrossRevenue"));
            stats.NetRevenue = reader.GetDecimal(reader.GetOrdinal("NetRevenue"));
            stats.TotalCommission = reader.GetDecimal(reader.GetOrdinal("TotalCommission"));
            stats.AverageTransactionValue = reader.GetDecimal(reader.GetOrdinal("AverageTransactionValue"));
            stats.UniqueBuyers = reader.GetInt32(reader.GetOrdinal("UniqueBuyers"));
        }
        
        // Second result set: Revenue by resource
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            stats.ResourceRevenue.Add(new ResourceRevenueDto
            {
                ResourceId = reader.GetGuid(reader.GetOrdinal("ResourceId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                SalesCount = reader.GetInt32(reader.GetOrdinal("SalesCount")),
                ResourceRevenue = reader.GetDecimal(reader.GetOrdinal("ResourceRevenue"))
            });
        }
        
        // Third result set: Daily revenue trend
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            stats.DailyRevenue.Add(new DailyRevenueDto
            {
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount")),
                DailyGrossRevenue = reader.GetDecimal(reader.GetOrdinal("DailyGrossRevenue")),
                DailyNetRevenue = reader.GetDecimal(reader.GetOrdinal("DailyNetRevenue"))
            });
        }
        
        return stats;
    }

    public async Task<IEnumerable<BuyerPurchaseHistoryDto>> GetBuyerPurchaseHistoryAsync(
        Guid buyerId,
        int skip = 0,
        int take = 20)
    {
        List<BuyerPurchaseHistoryDto> history = new List<BuyerPurchaseHistoryDto>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetBuyerPurchaseHistory", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@BuyerId", buyerId);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            BuyerPurchaseHistoryDto purchase = new BuyerPurchaseHistoryDto
            {
                TransactionId = reader.GetGuid(reader.GetOrdinal("TransactionId")),
                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                SellerId = reader.GetGuid(reader.GetOrdinal("SellerId")),
                SellerName = reader.GetString(reader.GetOrdinal("SellerName")),
                SellerEmail = reader.GetString(reader.GetOrdinal("SellerEmail"))
            };
            
            string resourceIdsJson = reader.GetString(reader.GetOrdinal("ResourceIds"));
            purchase.ResourceIds = JsonConvert.DeserializeObject<List<Guid>>(resourceIdsJson) ?? new List<Guid>();
            
            history.Add(purchase);
        }
        
        return history;
    }

    public async Task<MarketplaceAnalyticsDto> GetMarketplaceAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        MarketplaceAnalyticsDto analytics = new MarketplaceAnalyticsDto();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetMarketplaceAnalytics", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        // First result set: Overall statistics
        if (await reader.ReadAsync())
        {
            analytics.Overall = new OverallStats
            {
                TotalTransactions = reader.GetInt32(reader.GetOrdinal("TotalTransactions")),
                UniqueBuyers = reader.GetInt32(reader.GetOrdinal("UniqueBuyers")),
                ActiveSellers = reader.GetInt32(reader.GetOrdinal("ActiveSellers")),
                TotalGrossRevenue = reader.GetDecimal(reader.GetOrdinal("TotalGrossRevenue")),
                TotalCommission = reader.GetDecimal(reader.GetOrdinal("TotalCommission")),
                AverageTransactionValue = reader.GetDecimal(reader.GetOrdinal("AverageTransactionValue"))
            };
        }
        
        // Second result set: Top sellers
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            analytics.TopSellers.Add(new TopSellerDto
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                SellerName = reader.GetString(reader.GetOrdinal("SellerName")),
                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount")),
                GrossRevenue = reader.GetDecimal(reader.GetOrdinal("GrossRevenue")),
                NetRevenue = reader.GetDecimal(reader.GetOrdinal("NetRevenue"))
            });
        }
        
        // Third result set: Top resources
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            analytics.TopResources.Add(new TopResourceDto
            {
                ResourceId = reader.GetGuid(reader.GetOrdinal("ResourceId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                SalesCount = reader.GetInt32(reader.GetOrdinal("SalesCount")),
                ResourceRevenue = reader.GetDecimal(reader.GetOrdinal("ResourceRevenue"))
            });
        }
        
        // Fourth result set: Revenue trend
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            analytics.RevenueTrend.Add(new RevenueTrendDto
            {
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount")),
                DailyRevenue = reader.GetDecimal(reader.GetOrdinal("DailyRevenue")),
                DailyCommission = reader.GetDecimal(reader.GetOrdinal("DailyCommission"))
            });
        }
        
        return analytics;
    }

    public async Task<SellerPayout> ProcessSellerPayoutAsync(
        Guid sellerId,
        decimal amount,
        string payoutMethod,
        string? payoutReference = null,
        List<Guid>? transactionIds = null)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_ProcessSellerPayout", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter payoutIdParam = new SqlParameter("@PayoutId", SqlDbType.UniqueIdentifier);
        payoutIdParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(payoutIdParam);
        
        command.Parameters.AddWithValue("@SellerId", sellerId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@PayoutMethod", payoutMethod);
        command.Parameters.AddWithValue("@PayoutReference", (object?)payoutReference ?? DBNull.Value);
        command.Parameters.AddWithValue("@TransactionIds", 
            transactionIds != null ? JsonConvert.SerializeObject(transactionIds) : DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        return new SellerPayout
        {
            PayoutId = (Guid)payoutIdParam.Value,
            SellerId = sellerId,
            Amount = amount,
            PayoutMethodString = payoutMethod,
            PayoutReference = payoutReference,
            StatusString = "Processed",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<SellerPayout>> GetSellerPayoutHistoryAsync(
        Guid sellerId,
        int skip = 0,
        int take = 20)
    {
        List<SellerPayout> payouts = new List<SellerPayout>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSellerPayoutHistory", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SellerId", sellerId);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            payouts.Add(MapPayoutFromReader(reader));
        }
        
        return payouts;
    }

    private MarketplaceTransaction MapTransactionFromReader(SqlDataReader reader)
    {
        MarketplaceTransaction transaction = new MarketplaceTransaction
        {
            TransactionId = reader.GetGuid(reader.GetOrdinal("TransactionId")),
            BuyerId = reader.GetGuid(reader.GetOrdinal("BuyerId")),
            SellerId = reader.GetGuid(reader.GetOrdinal("SellerId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Commission = reader.GetDecimal(reader.GetOrdinal("Commission")),
            PaymentStatus = (PaymentStatus)reader.GetInt32(reader.GetOrdinal("PaymentStatus")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

        // Parse ResourceIds JSON
        string resourceIdsJson = reader.GetString(reader.GetOrdinal("ResourceIds"));
        transaction.ResourceIds = JsonConvert.DeserializeObject<List<Guid>>(resourceIdsJson) ?? new List<Guid>();

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("PaymentIntentId");
        if (!reader.IsDBNull(ordinal))
            transaction.PaymentIntentId = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("PaymentMethod");
        if (!reader.IsDBNull(ordinal))
            transaction.PaymentMethod = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("ProcessedAt");
        if (!reader.IsDBNull(ordinal))
            transaction.ProcessedAt = reader.GetDateTime(ordinal);

        return transaction;
    }

    private SellerPayout MapPayoutFromReader(SqlDataReader reader)
    {
        SellerPayout payout = new SellerPayout
        {
            PayoutId = reader.GetGuid(reader.GetOrdinal("PayoutId")),
            SellerId = reader.GetGuid(reader.GetOrdinal("SellerId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            PayoutMethodString = reader.GetString(reader.GetOrdinal("PayoutMethod")),
            StatusString = reader.GetString(reader.GetOrdinal("Status")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("PayoutReference");
        if (!reader.IsDBNull(ordinal))
            payout.PayoutReference = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("ProcessedAt");
        if (!reader.IsDBNull(ordinal))
            payout.ProcessedAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("TransactionIds");
        if (!reader.IsDBNull(ordinal))
        {
            string transactionIdsJson = reader.GetString(ordinal);
            payout.TransactionIds = JsonConvert.DeserializeObject<List<Guid>>(transactionIdsJson);
        }

        return payout;
    }

    // Additional IRepository<MarketplaceTransaction> interface methods
    public async Task<IEnumerable<MarketplaceTransaction>> FindAsync(Expression<Func<MarketplaceTransaction, bool>> predicate)
    {
        List<MarketplaceTransaction> allTransactions = (await GetAllAsync()).ToList();
        return allTransactions.AsQueryable().Where(predicate).ToList();
    }

    public async Task DeleteAsync(MarketplaceTransaction entity)
    {
        await DeleteAsync(entity.TransactionId);
    }

    public async Task<bool> ExistsAsync(Expression<Func<MarketplaceTransaction, bool>> predicate)
    {
        List<MarketplaceTransaction> allTransactions = (await GetAllAsync()).ToList();
        return allTransactions.AsQueryable().Any(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<MarketplaceTransaction, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return (await GetAllAsync()).Count();
        }
        else
        {
            List<MarketplaceTransaction> allTransactions = (await GetAllAsync()).ToList();
            return allTransactions.AsQueryable().Count(predicate);
        }
    }

    public IQueryable<MarketplaceTransaction> Query()
    {
        throw new NotSupportedException("Query() is not supported with stored procedures. Use specific methods like GetByIdAsync, GetTransactionsAsync, etc.");
    }
}