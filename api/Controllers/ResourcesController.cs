using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/resources")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(
        IResourceRepository resourceRepository,
        IUserRepository userRepository,
        ILogger<ResourcesController> logger)
    {
        _resourceRepository = resourceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ResourceSearchResponse>> SearchResources(
        [FromQuery] string? search,
        [FromQuery] List<string>? skillAreas,
        [FromQuery] List<string>? gradeLevels,
        [FromQuery] ResourceType? resourceType,
        [FromQuery] List<string>? languages,
        [FromQuery] int? minEvidenceLevel,
        [FromQuery] bool? isInteractive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        throw new NotImplementedException("SearchResources endpoint not yet implemented");
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ResourceDto>> GetResource(Guid id)
    {
        throw new NotImplementedException("GetResource endpoint not yet implemented");
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetPopularResources([FromQuery] int count = 10)
    {
        throw new NotImplementedException("GetPopularResources endpoint not yet implemented");
    }

    [HttpGet("recent")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetRecentResources([FromQuery] int count = 10)
    {
        throw new NotImplementedException("GetRecentResources endpoint not yet implemented");
    }

    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetSellerResources(Guid sellerId)
    {
        throw new NotImplementedException("GetSellerResources endpoint not yet implemented");
    }

    [HttpPost]
    [Authorize(Policy = "SellerOnly")]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceRequest request)
    {
        throw new NotImplementedException("CreateResource endpoint not yet implemented");
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<ActionResult<ResourceDto>> UpdateResource(Guid id, [FromBody] UpdateResourceRequest request)
    {
        throw new NotImplementedException("UpdateResource endpoint not yet implemented");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> DeleteResource(Guid id)
    {
        throw new NotImplementedException("DeleteResource endpoint not yet implemented");
    }

    [HttpGet("free")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetFreeResources([FromQuery] int count = 20)
    {
        throw new NotImplementedException("GetFreeResources endpoint not yet implemented");
    }

    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> FavoriteResource(Guid id)
    {
        throw new NotImplementedException("FavoriteResource endpoint not yet implemented");
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<List<ResourceDto>>> GetFavorites()
    {
        throw new NotImplementedException("GetFavorites endpoint not yet implemented");
    }

    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
    {
        throw new NotImplementedException("CreateFolder endpoint not yet implemented");
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadResource(Guid id)
    {
        throw new NotImplementedException("DownloadResource endpoint not yet implemented");
    }

    [HttpDelete("{id}/favorite")]
    public async Task<IActionResult> RemoveFromFavorites(Guid id)
    {
        throw new NotImplementedException("RemoveFromFavorites endpoint not yet implemented");
    }

    [HttpPost("{id}/rate")]
    public async Task<IActionResult> RateResource(Guid id, [FromBody] RateResourceRequest request)
    {
        throw new NotImplementedException("RateResource endpoint not yet implemented");
    }

    [HttpGet("{id}/ratings")]
    public async Task<IActionResult> GetResourceRatings(Guid id)
    {
        throw new NotImplementedException("GetResourceRatings endpoint not yet implemented");
    }

    [HttpPost("{id}/report")]
    public async Task<IActionResult> ReportResource(Guid id, [FromBody] ReportResourceRequest request)
    {
        throw new NotImplementedException("ReportResource endpoint not yet implemented");
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetResourceVersions(Guid id)
    {
        throw new NotImplementedException("GetResourceVersions endpoint not yet implemented");
    }

    [HttpPost("{id}/copy")]
    public async Task<IActionResult> CopyResource(Guid id, [FromBody] CopyResourceRequest request)
    {
        throw new NotImplementedException("CopyResource endpoint not yet implemented");
    }

    [HttpGet("{id}/related")]
    public async Task<IActionResult> GetRelatedResources(Guid id)
    {
        throw new NotImplementedException("GetRelatedResources endpoint not yet implemented");
    }

    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareResource(Guid id, [FromBody] ShareResourceRequest request)
    {
        throw new NotImplementedException("ShareResource endpoint not yet implemented");
    }

    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedWithMe()
    {
        throw new NotImplementedException("GetSharedWithMe endpoint not yet implemented");
    }

    [HttpPost("{id}/collections")]
    public async Task<IActionResult> AddToCollection(Guid id, [FromBody] AddToCollectionRequest request)
    {
        throw new NotImplementedException("AddToCollection endpoint not yet implemented");
    }

    [HttpGet("{id}/usage")]
    public async Task<IActionResult> GetResourceUsageStats(Guid id)
    {
        throw new NotImplementedException("GetResourceUsageStats endpoint not yet implemented");
    }

    [HttpPost("{id}/clinical-review")]
    public async Task<IActionResult> SubmitForClinicalReview(Guid id, [FromBody] ClinicalReviewRequest request)
    {
        throw new NotImplementedException("SubmitForClinicalReview endpoint not yet implemented");
    }

    private ResourceDto MapToDto(Resource resource)
    {
        return new ResourceDto
        {
            ResourceId = resource.ResourceId,
            Title = resource.Title,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            SkillAreas = resource.GetSkillAreas(),
            GradeLevels = resource.GetGradeLevels(),
            EvidenceLevel = resource.EvidenceLevel,
            LanguagesAvailable = resource.LanguagesAvailable,
            IsInteractive = resource.IsInteractive,
            HasAudio = resource.HasAudio,
            FileUrl = resource.FileUrl,
            PreviewUrl = resource.PreviewUrl,
            Price = resource.Price,
            ViewCount = resource.ViewCount,
            DownloadCount = resource.DownloadCount,
            Rating = resource.Rating,
            ReviewCount = resource.ReviewCount,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt
        };
    }
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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public List<string> SkillAreas { get; set; } = new();
    public List<string> GradeLevels { get; set; } = new();
    public List<string> LanguagesAvailable { get; set; } = new();
    public bool IsInteractive { get; set; }
    public bool HasAudio { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public decimal? Price { get; set; }
}

public class UpdateResourceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SkillAreas { get; set; } = new();
    public List<string> GradeLevels { get; set; } = new();
    public List<string> LanguagesAvailable { get; set; } = new();
    public decimal? Price { get; set; }
}

public class CreateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RateResourceRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReportResourceRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}

public class CopyResourceRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ShareResourceRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? ExpiresIn { get; set; }
}

public class AddToCollectionRequest
{
    public string CollectionId { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ClinicalReviewRequest
{
    public List<string> RequestedReviewers { get; set; } = new();
    public string? Notes { get; set; }
}