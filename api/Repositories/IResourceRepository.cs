using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Controllers;

namespace UPTRMS.Api.Repositories;

public interface IResourceRepository : IRepository<Resource>
{
    Task<IEnumerable<Resource>> SearchResourcesAsync(
        string? searchTerm = null,
        List<string>? skillAreas = null,
        List<string>? gradelevels = null,
        ResourceType? resourceType = null,
        List<string>? languages = null,
        int? minEvidenceLevel = null,
        bool? isInteractive = null,
        int skip = 0,
        int take = 20);
    
    Task<IEnumerable<Resource>> GetBySellerAsync(Guid sellerId);
    Task<IEnumerable<Resource>> GetPopularResourcesAsync(int take = 10);
    Task<IEnumerable<Resource>> GetRecentResourcesAsync(int take = 10);
    Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId);
    Task<Resource?> GetResourceWithDetailsAsync(Guid resourceId);
    Task IncrementViewCountAsync(Guid resourceId);
    Task<Dictionary<Guid, int>> GetDownloadCountsAsync(List<Guid> resourceIds);
    
    // Content management methods
    Task<IEnumerable<Resource>> GetByReviewStatusAsync(ClinicalReviewStatus status);
    Task AssignReviewerAsync(ReviewAssignment assignment);
    Task SubmitReviewEvaluationAsync(ReviewEvaluation evaluation);
    Task<IEnumerable<ReviewEvaluation>> GetResourceReviewsAsync(Guid resourceId);
    Task<ReviewStatisticsDto> GetReviewStatisticsAsync();
    
    // Version control methods
    Task<IEnumerable<Resource>> GetResourcesAsync(Func<Resource, bool> predicate);
}