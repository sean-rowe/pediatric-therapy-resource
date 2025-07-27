using System.ComponentModel.DataAnnotations;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Models.DTOs;

public class ResourceDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType ResourceType { get; set; }
    public Dictionary<string, object> SkillAreas { get; set; } = new();
    public List<int> GradeLevels { get; set; } = new();
    public List<string> LanguagesAvailable { get; set; } = new();
    public bool IsInteractive { get; set; }
    public bool HasAudio { get; set; }
    public string? FileUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? EvidenceLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsMarketplaceItem { get; set; }
    public decimal? Price { get; set; }
    public string? PreviewUrl { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public SellerInfoDto? Seller { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public ResourceStatsDto? Stats { get; set; }
}

public class ResourceSearchRequest
{
    public string? Query { get; set; }
    public List<ResourceType>? ResourceTypes { get; set; }
    public Dictionary<string, List<string>>? SkillAreas { get; set; }
    public List<int>? GradeLevels { get; set; }
    public List<string>? Languages { get; set; }
    public bool? IsInteractive { get; set; }
    public bool? HasAudio { get; set; }
    public int? MinEvidenceLevel { get; set; }
    public bool? FreeOnly { get; set; }
    public decimal? MaxPrice { get; set; }
    public ResourceSortBy SortBy { get; set; } = ResourceSortBy.Relevance;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum ResourceSortBy
{
    Relevance,
    Newest,
    MostDownloaded,
    BestRated,
    PriceLowToHigh,
    PriceHighToLow
}

public class ResourceSearchResponse
{
    public List<ResourceDto> Resources { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CreateResourceRequest
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public ResourceType ResourceType { get; set; }

    [Required]
    public Dictionary<string, object> SkillAreas { get; set; } = new();

    [Required]
    public List<int> GradeLevels { get; set; } = new();

    public List<string> LanguagesAvailable { get; set; } = new() { "English" };

    public bool IsInteractive { get; set; }

    public List<Guid> CategoryIds { get; set; } = new();
}

public class UpdateResourceRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? SkillAreas { get; set; }
    public List<int>? GradeLevels { get; set; }
    public List<string>? LanguagesAvailable { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

public class PublishResourceRequest
{
    public bool IsMarketplaceItem { get; set; }
    public decimal? Price { get; set; }
}

public class SellerInfoDto
{
    public Guid SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int TotalSales { get; set; }
    public bool IsVerified { get; set; }
}

public class ResourceStatsDto
{
    public int DownloadCount { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int SessionUseCount { get; set; }
}

public class ResourceRatingDto
{
    public Guid RatingId { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastInitial { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class CreateRatingRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }
}