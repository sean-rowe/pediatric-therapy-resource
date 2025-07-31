using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceRepository _marketplaceRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MarketplaceController> _logger;

    public MarketplaceController(
        IMarketplaceRepository marketplaceRepository,
        IResourceRepository resourceRepository,
        IUserRepository userRepository,
        ILogger<MarketplaceController> logger)
    {
        _marketplaceRepository = marketplaceRepository;
        _resourceRepository = resourceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets marketplace products with filtering and pagination
    /// </summary>
    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<ActionResult<MarketplaceProductsResponse>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? category,
        [FromQuery] string? sortBy = "recent",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Get all resources and filter for marketplace items
            var allResources = await _resourceRepository.SearchResourcesAsync(
                searchTerm: search,
                skillAreas: !string.IsNullOrEmpty(category) ? new List<string> { category } : null,
                skip: 0,
                take: int.MaxValue);

            // Filter for published marketplace items
            var marketplaceResources = allResources
                .Where(r => r.IsPublished && r.Price > 0 && r.ClinicalReviewStatus == ClinicalReviewStatus.Approved);

            // Apply price filters
            if (minPrice.HasValue)
            {
                marketplaceResources = marketplaceResources.Where(r => r.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                marketplaceResources = marketplaceResources.Where(r => r.Price <= maxPrice.Value);
            }

            // Apply sorting
            marketplaceResources = sortBy?.ToLower() switch
            {
                "price-low" => marketplaceResources.OrderBy(r => r.Price),
                "price-high" => marketplaceResources.OrderByDescending(r => r.Price),
                "popular" => marketplaceResources.OrderByDescending(r => r.DownloadCount),
                "rating" => marketplaceResources.OrderByDescending(r => r.Rating),
                _ => marketplaceResources.OrderByDescending(r => r.CreatedAt)
            };

            var resourcesList = marketplaceResources.ToList();
            int totalCount = resourcesList.Count;

            // Apply pagination
            List<Resource> products = resourcesList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Load seller profiles for the products
            foreach (var product in products)
            {
                if (product.CreatedByUserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(product.CreatedByUserId.Value);
                    product.CreatedByUser = user;
                }
            }

            MarketplaceProductsResponse response = new MarketplaceProductsResponse
            {
                Products = products.Select(MapToMarketplaceProductDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marketplace products");
            return StatusCode(500, new { message = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Gets a specific marketplace product by ID
    /// </summary>
    [HttpGet("products/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MarketplaceProductDetailDto>> GetProduct(Guid id)
    {
        try
        {
            Resource? product = await _resourceRepository.GetResourceWithDetailsAsync(id);
            
            if (product == null || !product.IsPublished || product.Price <= 0)
            {
                product = null;
            }
            
            // Load seller profile if product exists
            if (product != null && product.CreatedByUserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(product.CreatedByUserId.Value);
                product.CreatedByUser = user;
            }

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Increment view count
            await _resourceRepository.IncrementViewCountAsync(product.ResourceId);

            // Get related products
            var sellerResources = product.CreatedByUserId.HasValue ? 
                await _resourceRepository.GetBySellerAsync(product.CreatedByUserId.Value) : 
                new List<Resource>();
                
            List<Resource> relatedProducts = sellerResources
                .Where(r => r.ResourceId != id && r.IsPublished && r.Price > 0)
                .Take(5)
                .ToList();

            MarketplaceProductDetailDto dto = new MarketplaceProductDetailDto
            {
                Product = MapToMarketplaceProductDto(product),
                SellerInfo = MapToSellerInfoDto(product.CreatedByUser!.SellerProfile!),
                RelatedProducts = relatedProducts.Select(MapToMarketplaceProductDto).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Purchases a marketplace product
    /// </summary>
    [HttpPost("purchase")]
    [Authorize]
    public async Task<ActionResult<PurchaseResponse>> PurchaseProduct([FromBody] PurchaseRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Validate products exist and are available
            List<Resource> products = new List<Resource>();
            foreach (var productId in request.ProductIds)
            {
                var resource = await _resourceRepository.GetResourceWithDetailsAsync(productId);
                if (resource != null && resource.IsPublished && resource.Price > 0)
                {
                    // Load seller profile
                    if (resource.CreatedByUserId.HasValue)
                    {
                        var user = await _userRepository.GetByIdAsync(resource.CreatedByUserId.Value);
                        resource.CreatedByUser = user;
                    }
                    products.Add(resource);
                }
            }

            if (products.Count != request.ProductIds.Count)
            {
                return BadRequest(new { message = "One or more products are not available" });
            }

            // Calculate total
            decimal subtotal = products.Sum(p => p.Price ?? 0);
            decimal discountAmount = 0;

            // Apply coupon if provided
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                // In production, validate coupon code
                if (request.CouponCode == "SAVE20")
                {
                    discountAmount = subtotal * 0.20m;
                }
            }

            decimal total = subtotal - discountAmount;

            // Create transaction for each seller
            Dictionary<Guid, List<Resource>> productsBySeller = products
                .Where(p => p.CreatedByUserId.HasValue)
                .GroupBy(p => p.CreatedByUserId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            List<MarketplaceTransaction> transactions = new List<MarketplaceTransaction>();

            foreach (KeyValuePair<Guid, List<Resource>> sellerProducts in productsBySeller)
            {
                decimal sellerSubtotal = sellerProducts.Value.Sum(p => p.Price ?? 0);
                decimal sellerDiscount = sellerSubtotal / subtotal * discountAmount;
                decimal sellerTotal = sellerSubtotal - sellerDiscount;
                decimal commission = sellerTotal * 0.30m; // 30% platform fee

                MarketplaceTransaction transaction = new MarketplaceTransaction
                {
                    BuyerId = userId,
                    SellerId = sellerProducts.Key,
                    Amount = sellerTotal,
                    Commission = commission,
                    SellerEarnings = sellerTotal - commission,
                    PaymentStatus = PaymentStatus.Pending,
                    CouponCode = request.CouponCode,
                    DiscountAmount = sellerDiscount > 0 ? sellerDiscount : null
                };
                transaction.SetResourceIds(sellerProducts.Value.Select(p => p.ResourceId).ToList());

                transactions.Add(transaction);
                // Transaction will be added via repository
            }

            // In production, integrate with payment processor (Stripe)
            // For now, simulate successful payment
            foreach (MarketplaceTransaction transaction in transactions)
            {
                transaction.PaymentStatus = PaymentStatus.Completed;
                transaction.CompletedAt = DateTime.UtcNow;
                transaction.StripePaymentIntentId = "pi_simulated_" + Guid.NewGuid().ToString("N");
            }

            // Save all transactions
            foreach (var transaction in transactions)
            {
                await _marketplaceRepository.CreateTransactionAsync(transaction);
            }

            // Grant access to purchased resources
            // In production, this would create UserResource entries

            PurchaseResponse response = new PurchaseResponse
            {
                Success = true,
                TransactionIds = transactions.Select(t => t.TransactionId).ToList(),
                TotalAmount = total,
                DownloadLinks = products.Select(p => new DownloadLink
                {
                    ResourceId = p.ResourceId,
                    Title = p.Title,
                    DownloadUrl = $"/api/resources/{p.ResourceId}/download"
                }).ToList()
            };

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing purchase");
            return StatusCode(500, new { message = "An error occurred while processing your purchase" });
        }
    }

    /// <summary>
    /// Gets seller dashboard with analytics
    /// </summary>
    [HttpGet("seller/dashboard")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<ActionResult<SellerDashboardDto>> GetSellerDashboard()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get user and their seller profile
            User? user = await _userRepository.GetByIdAsync(userId);
            SellerProfile? seller = user?.SellerProfile;

            if (seller == null)
            {
                return NotFound(new { message = "Seller profile not found" });
            }

            // Get seller metrics
            List<Resource> products = (await _resourceRepository.GetBySellerAsync(userId)).ToList();

            var allTransactions = await _marketplaceRepository.GetTransactionsAsync(
                sellerId: userId,
                paymentStatus: PaymentStatus.Completed,
                skip: 0,
                take: int.MaxValue);
            List<MarketplaceTransaction> transactions = allTransactions.ToList();

            // TODO: Get followers count from SellerFollower table
            int followersCount = 0; // Placeholder until SellerFollower repository is implemented

            // Calculate metrics
            decimal totalRevenue = transactions.Sum(t => t.SellerEarnings);
            decimal monthlyRevenue = transactions
                .Where(t => t.CompletedAt > DateTime.UtcNow.AddDays(-30))
                .Sum(t => t.SellerEarnings);

            SellerDashboardDto dashboard = new SellerDashboardDto
            {
                SellerInfo = MapToSellerInfoDto(seller),
                Metrics = new SellerMetricsDto
                {
                    TotalProducts = products.Count,
                    PublishedProducts = products.Count(p => p.IsPublished),
                    TotalSales = transactions.Count,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    AverageRating = seller.Rating,
                    TotalReviews = seller.TotalReviews,
                    FollowersCount = followersCount
                },
                RecentSales = transactions.OrderByDescending(t => t.CompletedAt)
                    .Take(10)
                    .Select(t => new RecentSaleDto
                    {
                        TransactionId = t.TransactionId,
                        Amount = t.SellerEarnings,
                        Date = t.CompletedAt ?? t.CreatedAt,
                        ProductCount = t.GetResourceIds().Count
                    }).ToList(),
                TopProducts = products.Where(p => p.IsPublished)
                    .OrderByDescending(p => p.DownloadCount)
                    .Take(5)
                    .Select(MapToMarketplaceProductDto)
                    .ToList()
            };

            return Ok(dashboard);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seller dashboard");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard" });
        }
    }

    /// <summary>
    /// Applies to become a marketplace seller
    /// </summary>
    [HttpPost("seller/apply")]
    [Authorize]
    public async Task<ActionResult<SellerApplicationResponse>> ApplyToBeSeller([FromBody] SellerApplicationRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Check if already a seller
            User? existingUser = await _userRepository.GetByIdAsync(userId);
            bool existingSeller = existingUser?.SellerProfile != null;
            if (existingSeller)
            {
                return BadRequest(new { message = "You are already registered as a seller" });
            }

            // Create seller profile
            SellerProfile sellerProfile = new SellerProfile
            {
                UserId = userId,
                StoreName = request.StoreName,
                StoreUrl = await GenerateStoreUrlAsync(request.StoreName),
                Bio = request.Bio,
                Specialties = request.Specialties,
                IsVerified = false, // Will be verified after review
                CommissionRate = 0.30m // 30% platform fee
            };

            // Seller profile will be created when updating user

            // Update user role to include seller
            User? user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.SellerProfile = sellerProfile;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            // In production, notify admin for review
            _logger.LogInformation("New seller application from user {UserId}", userId);

            SellerApplicationResponse response = new SellerApplicationResponse
            {
                Success = true,
                SellerId = sellerProfile.SellerId,
                Status = "PendingReview",
                Message = "Your seller application has been submitted for review. You will be notified within 48 hours."
            };

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing seller application");
            return StatusCode(500, new { message = "An error occurred while processing your application" });
        }
    }

    private MarketplaceProductDto MapToMarketplaceProductDto(Resource resource)
    {
        return new MarketplaceProductDto
        {
            ResourceId = resource.ResourceId,
            Title = resource.Title,
            Description = resource.Description ?? string.Empty,
            Price = resource.Price ?? 0,
            PreviewUrl = resource.PreviewUrl,
            Rating = resource.Rating,
            ReviewCount = resource.ReviewCount,
            DownloadCount = resource.DownloadCount,
            SellerName = resource.CreatedByUser?.SellerProfile?.StoreName ?? "Unknown Seller",
            SellerId = resource.CreatedByUserId ?? Guid.Empty
        };
    }

    private SellerInfoDto MapToSellerInfoDto(SellerProfile seller)
    {
        return new SellerInfoDto
        {
            SellerId = seller.SellerId,
            StoreName = seller?.StoreName ?? string.Empty,
            StoreUrl = seller?.StoreUrl ?? string.Empty,
            Rating = seller?.Rating ?? 0,
            TotalSales = seller?.TotalSales ?? 0,
            IsVerified = seller?.IsVerified ?? false
        };
    }

    private async Task<string> GenerateStoreUrlAsync(string storeName)
    {
        // Generate URL-friendly store URL
        string baseUrl = storeName.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace(".", "")
            .Replace(",", "");

        // Ensure uniqueness - get all sellers to check
        var sellers = await _userRepository.GetSellersAsync();
        var existingUrls = sellers
            .Where(u => u.SellerProfile != null)
            .Select(u => u.SellerProfile!.StoreUrl)
            .ToHashSet();
        
        string url = baseUrl;
        int counter = 1;
        while (existingUrls.Contains(url))
        {
            url = $"{baseUrl}-{counter}";
            counter++;
        }

        return url;
    }
}

// DTOs
public class MarketplaceProductsResponse
{
    public List<MarketplaceProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class MarketplaceProductDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PreviewUrl { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int DownloadCount { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
}

public class MarketplaceProductDetailDto
{
    public MarketplaceProductDto Product { get; set; } = new();
    public SellerInfoDto SellerInfo { get; set; } = new();
    public List<MarketplaceProductDto> RelatedProducts { get; set; } = new();
}

public class PurchaseRequest
{
    [Required]
    public List<Guid> ProductIds { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class PurchaseResponse
{
    public bool Success { get; set; }
    public List<Guid> TransactionIds { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public List<DownloadLink> DownloadLinks { get; set; } = new();
}

public class DownloadLink
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class SellerDashboardDto
{
    public SellerInfoDto SellerInfo { get; set; } = new();
    public SellerMetricsDto Metrics { get; set; } = new();
    public List<RecentSaleDto> RecentSales { get; set; } = new();
    public List<MarketplaceProductDto> TopProducts { get; set; } = new();
}

public class SellerMetricsDto
{
    public int TotalProducts { get; set; }
    public int PublishedProducts { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FollowersCount { get; set; }
}

public class RecentSaleDto
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int ProductCount { get; set; }
}

public class SellerApplicationRequest
{
    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [Required]
    public List<string> Specialties { get; set; } = new();

    [Required]
    public string ProfessionalLicense { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }
}

public class SellerApplicationResponse
{
    public bool Success { get; set; }
    public Guid SellerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}