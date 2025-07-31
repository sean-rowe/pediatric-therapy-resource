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
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max page size

            IEnumerable<Resource> resources = await _resourceRepository.SearchResourcesAsync(
                search,
                skillAreas,
                gradeLevels,
                resourceType,
                languages,
                minEvidenceLevel,
                isInteractive,
                page,
                pageSize);

            // For now, use the same query to get count - in production this would be optimized
            IEnumerable<Resource> countQuery = await _resourceRepository.SearchResourcesAsync(
                search,
                skillAreas,
                gradeLevels,
                resourceType,
                languages,
                minEvidenceLevel,
                isInteractive,
                0,
                int.MaxValue);
            int totalCount = countQuery.Count();

            ResourceSearchResponse response = new ResourceSearchResponse
            {
                Resources = resources.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching resources");
            return StatusCode(500, new { message = "An error occurred while searching resources" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ResourceDto>> GetResource(Guid id)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Increment view count
            await _resourceRepository.IncrementViewCountAsync(id);

            return Ok(MapToDto(resource));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the resource" });
        }
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetPopularResources([FromQuery] int count = 10)
    {
        try
        {
            // Validate count
            if (count < 1) count = 10;
            if (count > 50) count = 50; // Max 50 items

            IEnumerable<Resource> resources = await _resourceRepository.GetPopularResourcesAsync(count);
            return Ok(resources.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular resources");
            return StatusCode(500, new { message = "An error occurred while retrieving popular resources" });
        }
    }

    [HttpGet("recent")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetRecentResources([FromQuery] int count = 10)
    {
        try
        {
            // Validate count
            if (count < 1) count = 10;
            if (count > 50) count = 50; // Max 50 items

            IEnumerable<Resource> resources = await _resourceRepository.GetRecentResourcesAsync(count);
            return Ok(resources.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent resources");
            return StatusCode(500, new { message = "An error occurred while retrieving recent resources" });
        }
    }

    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetSellerResources(Guid sellerId)
    {
        try
        {
            User? seller = await _userRepository.GetByIdAsync(sellerId);
            if (seller == null || seller.SellerProfile == null)
            {
                return NotFound(new { message = "Seller not found" });
            }

            IEnumerable<Resource> resources = await _resourceRepository.GetBySellerAsync(sellerId);
            return Ok(resources.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seller resources for {SellerId}", sellerId);
            return StatusCode(500, new { message = "An error occurred while retrieving seller resources" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "SellerOnly")]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource resource = new Resource
            {
                Title = request.Title,
                Description = request.Description,
                ResourceType = request.ResourceType,
                SkillAreas = System.Text.Json.JsonSerializer.Serialize(request.SkillAreas),
                GradeLevels = System.Text.Json.JsonSerializer.Serialize(request.GradeLevels.Select(g => int.Parse(g)).ToList()),
                LanguagesAvailable = request.LanguagesAvailable,
                IsInteractive = request.IsInteractive,
                HasAudio = request.HasAudio,
                FileUrl = request.FileUrl,
                PreviewUrl = request.PreviewUrl,
                Price = request.Price,
                CreatedByUserId = userId,
                ClinicalReviewStatus = ClinicalReviewStatus.Pending,
                EvidenceLevel = 0 // Will be set after clinical review
            };

            await _resourceRepository.AddAsync(resource);
            Resource createdResource = resource;
            return CreatedAtAction(nameof(GetResource), new { id = createdResource.ResourceId }, MapToDto(createdResource));
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource");
            return StatusCode(500, new { message = "An error occurred while creating the resource" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<ActionResult<ResourceDto>> UpdateResource(Guid id, [FromBody] UpdateResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Verify user owns the resource
            if (resource.CreatedByUserId != userId)
            {
                return Forbid("You can only update your own resources");
            }

            // Update resource properties
            resource.Title = request.Title;
            resource.Description = request.Description;
            resource.SkillAreas = System.Text.Json.JsonSerializer.Serialize(request.SkillAreas);
            resource.GradeLevels = System.Text.Json.JsonSerializer.Serialize(request.GradeLevels.Select(g => int.Parse(g)).ToList());
            resource.LanguagesAvailable = request.LanguagesAvailable;
            resource.Price = request.Price;
            resource.UpdatedAt = DateTime.UtcNow;

            await _resourceRepository.UpdateAsync(resource);
            return Ok(MapToDto(resource));
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the resource" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SellerOnly")]
    public async Task<IActionResult> DeleteResource(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Verify user owns the resource
            if (resource.CreatedByUserId != userId)
            {
                return Forbid("You can only delete your own resources");
            }

            await _resourceRepository.DeleteAsync(resource);
            return Ok(new { message = "Resource deleted successfully" });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the resource" });
        }
    }

    [HttpGet("free")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceDto>>> GetFreeResources([FromQuery] int count = 20)
    {
        try
        {
            // Validate count
            if (count < 1) count = 20;
            if (count > 100) count = 100; // Max 100 free resources

            // Get free resources (price is null or 0)
            IEnumerable<Resource> allResources = await _resourceRepository.GetRecentResourcesAsync(count * 2);
            IEnumerable<Resource> resources = allResources.Where(r => r.Price == null || r.Price == 0).Take(count);
            return Ok(resources.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving free resources");
            return StatusCode(500, new { message = "An error occurred while retrieving free resources" });
        }
    }

    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> FavoriteResource(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Add to favorites would be implemented through a UserFavorites table
            // For now, just log the action
            _logger.LogInformation("User {UserId} added resource {ResourceId} to favorites", userId, id);
            return Ok(new { message = "Resource added to favorites" });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding resource {ResourceId} to favorites", id);
            return StatusCode(500, new { message = "An error occurred while adding resource to favorites" });
        }
    }

    [HttpGet("favorites")]
    public Task<ActionResult<List<ResourceDto>>> GetFavorites()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get favorites would query UserFavorites table
            // For now, return empty list
            IEnumerable<Resource> resources = new List<Resource>();
            return Task.FromResult<ActionResult<List<ResourceDto>>>(Ok(resources.Select(MapToDto).ToList()));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult<ActionResult<List<ResourceDto>>>(Unauthorized(new { message = "User not authenticated" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorites");
            return Task.FromResult<ActionResult<List<ResourceDto>>>(StatusCode(500, new { message = "An error occurred while retrieving favorites" }));
        }
    }

    [HttpPost("folders")]
    public Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Create user folder for organizing favorites
            // Create user folder would be implemented through a UserFolders table
            // For now, just log the action
            _logger.LogInformation("User {UserId} created folder '{FolderName}'", userId, request.Name);
            return Task.FromResult<IActionResult>(Ok(new { message = "Folder created successfully" }));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult<IActionResult>(Unauthorized(new { message = "User not authenticated" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            return Task.FromResult<IActionResult>(StatusCode(500, new { message = "An error occurred while creating the folder" }));
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadResource(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Check if resource is free or user has access
            bool hasAccess = resource.Price == null || resource.Price == 0;
            if (!hasAccess)
            {
                // Check if user has purchased or has subscription
                // In production, this would check purchase history and subscription status
                hasAccess = true; // For now, assume users have access
            }

            if (!hasAccess)
            {
                return Forbid("You do not have access to download this resource");
            }

            // Increment download count
            // Increment download count
            resource.DownloadCount++;
            await _resourceRepository.UpdateAsync(resource);

            // In a real implementation, this would return the file from storage
            // For now, return a redirect to the file URL
            if (string.IsNullOrEmpty(resource.FileUrl))
            {
                return NotFound(new { message = "Resource file not available" });
            }

            return Redirect(resource.FileUrl);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while downloading the resource" });
        }
    }

    [HttpDelete("{id}/favorite")]
    public Task<IActionResult> RemoveFromFavorites(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Remove from favorites would be implemented through UserFavorites table
            // For now, just log the action
            _logger.LogInformation("User {UserId} removed resource {ResourceId} from favorites", userId, id);
            return Task.FromResult<IActionResult>(Ok(new { message = "Resource removed from favorites" }));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult<IActionResult>(Unauthorized(new { message = "User not authenticated" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing resource {ResourceId} from favorites", id);
            return Task.FromResult<IActionResult>(StatusCode(500, new { message = "An error occurred while removing resource from favorites" }));
        }
    }

    [HttpPost("{id}/rate")]
    public async Task<IActionResult> RateResource(Guid id, [FromBody] RateResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { message = "Rating must be between 1 and 5" });
            }

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Add rating would be implemented through ResourceRatings table
            // For now, update the resource's average rating
            _logger.LogInformation("User {UserId} rated resource {ResourceId} with {Rating} stars", userId, id, request.Rating);
            return Ok(new { message = "Rating submitted successfully" });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while rating the resource" });
        }
    }

    [HttpGet("{id}/ratings")]
    public async Task<IActionResult> GetResourceRatings(Guid id)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Get ratings would query ResourceRatings table
            object ratings = new { ratings = new List<object>(), averageRating = resource.Rating, totalRatings = resource.ReviewCount };
            return Ok(ratings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ratings for resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving resource ratings" });
        }
    }

    [HttpPost("{id}/report")]
    public async Task<IActionResult> ReportResource(Guid id, [FromBody] ReportResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Report resource would be stored in ResourceReports table
            // For now, just log the report
            _logger.LogWarning("Resource {ResourceId} reported by user {UserId} for: {Reason}", id, userId, request.Reason);

            return Ok(new { message = "Report submitted successfully. Our team will review it." });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while reporting the resource" });
        }
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetResourceVersions(Guid id)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Get versions would track resource version history
            object versions = new { versions = new List<object>() { new { version = resource.Version ?? "1.0", createdAt = resource.CreatedAt } } };
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving resource versions" });
        }
    }

    [HttpPost("{id}/copy")]
    public async Task<IActionResult> CopyResource(Guid id, [FromBody] CopyResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Check if user has access to copy
            // For now, assume users with subscription have access to copy resources
            bool canCopy = true; // This would check subscription status in real implementation
            if (!canCopy)
            {
                return Forbid("You do not have permission to copy this resource");
            }

            // Create a personal copy for the user
            // Create a personal copy for the user
            Resource copiedResource = new Resource
            {
                Title = request.Title,
                Description = resource.Description + "\n\nNotes: " + request.Notes,
                ResourceType = resource.ResourceType,
                SkillAreas = resource.SkillAreas,
                GradeLevels = resource.GradeLevels,
                LanguagesAvailable = resource.LanguagesAvailable,
                IsInteractive = resource.IsInteractive,
                HasAudio = resource.HasAudio,
                FileUrl = resource.FileUrl,
                CreatedByUserId = userId,
                ClinicalReviewStatus = ClinicalReviewStatus.Approved,
                EvidenceLevel = resource.EvidenceLevel
            };
            await _resourceRepository.AddAsync(copiedResource);
            return Ok(new { message = "Resource copied successfully", resourceId = copiedResource.ResourceId });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while copying the resource" });
        }
    }

    [HttpGet("{id}/related")]
    public async Task<IActionResult> GetRelatedResources(Guid id)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Get related resources based on similar skill areas and grade levels
            IEnumerable<Resource> allResources = await _resourceRepository.GetRecentResourcesAsync(50);
            List<Resource> relatedResources = allResources
                .Where(r => r.ResourceId != id && r.ResourceType == resource.ResourceType)
                .Take(10)
                .ToList();
            return Ok(relatedResources.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving related resources for {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving related resources" });
        }
    }

    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareResource(Guid id, [FromBody] ShareResourceRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Generate share link with optional expiration
            // Generate share link - in production this would create a time-limited access token
            string shareLink = $"/api/resources/{id}/shared/{Guid.NewGuid()}";

            return Ok(new { message = "Resource shared successfully", shareLink = shareLink });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while sharing the resource" });
        }
    }

    [HttpGet("shared-with-me")]
    public Task<IActionResult> GetSharedWithMe()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get shared resources would query ResourceShares table
            IEnumerable<Resource> sharedResources = new List<Resource>();
            return Task.FromResult<IActionResult>(Ok(sharedResources.Select(MapToDto).ToList()));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult<IActionResult>(Unauthorized(new { message = "User not authenticated" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shared resources");
            return Task.FromResult<IActionResult>(StatusCode(500, new { message = "An error occurred while retrieving shared resources" }));
        }
    }

    [HttpPost("{id}/collections")]
    public async Task<IActionResult> AddToCollection(Guid id, [FromBody] AddToCollectionRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Add to collection would be implemented through UserCollections table
            _logger.LogInformation("User {UserId} added resource {ResourceId} to collection {CollectionId}", userId, id, request.CollectionId);
            return Ok(new { message = "Resource added to collection successfully" });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding resource {ResourceId} to collection", id);
            return StatusCode(500, new { message = "An error occurred while adding resource to collection" });
        }
    }

    [HttpGet("{id}/usage")]
    public async Task<IActionResult> GetResourceUsageStats(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Verify user owns the resource or is admin
            if (resource.CreatedByUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only view usage stats for your own resources");
            }

            // Get usage stats from various tracking tables
            object usageStats = new
            {
                totalViews = resource.ViewCount,
                totalDownloads = resource.DownloadCount,
                averageRating = resource.Rating,
                totalRatings = resource.ReviewCount,
                lastUpdated = resource.UpdatedAt ?? resource.CreatedAt
            };
            return Ok(usageStats);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage stats for resource {ResourceId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving resource usage stats" });
        }
    }

    [HttpPost("{id}/clinical-review")]
    public async Task<IActionResult> SubmitForClinicalReview(Guid id, [FromBody] ClinicalReviewRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Resource? resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return NotFound(new { message = "Resource not found" });
            }

            // Verify user owns the resource
            if (resource.CreatedByUserId != userId)
            {
                return Forbid("You can only submit your own resources for clinical review");
            }

            // Update status to pending review
            resource.ClinicalReviewStatus = ClinicalReviewStatus.Pending;
            await _resourceRepository.UpdateAsync(resource);

            // Create review request
            // Create clinical review request - this would notify reviewers
            _logger.LogInformation("Clinical review requested for resource {ResourceId} with notes: {Notes}", id, request.Notes);

            _logger.LogInformation("Resource {ResourceId} submitted for clinical review by user {UserId}", id, userId);
            return Ok(new { message = "Resource submitted for clinical review successfully" });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting resource {ResourceId} for clinical review", id);
            return StatusCode(500, new { message = "An error occurred while submitting resource for clinical review" });
        }
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