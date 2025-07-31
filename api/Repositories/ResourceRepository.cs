using Microsoft.EntityFrameworkCore;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Repositories;

public class ResourceRepository : Repository<Resource>, IResourceRepository
{
    public ResourceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Resource>> SearchResourcesAsync(
        string? searchTerm = null,
        List<string>? skillAreas = null,
        List<string>? gradeLevels = null,
        ResourceType? resourceType = null,
        List<string>? languages = null,
        int? minEvidenceLevel = null,
        bool? isInteractive = null,
        int skip = 0,
        int take = 20)
    {
        var query = _dbSet.AsQueryable();

        // Apply search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var loweredSearch = searchTerm.ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(loweredSearch) ||
                (r.Description != null && r.Description.ToLower().Contains(loweredSearch)));
        }

        // Apply skill area filter
        if (skillAreas != null && skillAreas.Any())
        {
            // Since SkillAreas is stored as JSON string, we need to check if any of the requested skills are in the JSON
            foreach (var skill in skillAreas)
            {
                query = query.Where(r => r.SkillAreas.Contains(skill));
            }
        }

        // Apply grade level filter
        if (gradeLevels != null && gradeLevels.Any())
        {
            // Since GradeLevels is stored as JSON string, we need to check if any of the requested levels are in the JSON
            foreach (var grade in gradeLevels)
            {
                query = query.Where(r => r.GradeLevels.Contains(grade));
            }
        }

        // Apply resource type filter
        if (resourceType.HasValue)
        {
            query = query.Where(r => r.ResourceType == resourceType.Value);
        }

        // Apply language filter
        if (languages != null && languages.Any())
        {
            query = query.Where(r => r.LanguagesAvailable.Any(l => languages.Contains(l)));
        }

        // Apply evidence level filter
        if (minEvidenceLevel.HasValue)
        {
            query = query.Where(r => r.EvidenceLevel >= minEvidenceLevel.Value);
        }

        // Apply interactive filter
        if (isInteractive.HasValue)
        {
            query = query.Where(r => r.IsInteractive == isInteractive.Value);
        }

        // Only return approved resources (excludes retired, pending, rejected, etc.)
        query = query.Where(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Approved);

        return await query
            .OrderByDescending(r => r.EvidenceLevel)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetBySellerAsync(Guid sellerId)
    {
        return await _dbSet
            .Where(r => r.SellerId == sellerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetPopularResourcesAsync(int take = 10)
    {
        return await _dbSet
            .Where(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Approved)
            .OrderByDescending(r => r.ViewCount)
            .ThenByDescending(r => r.DownloadCount)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetRecentResourcesAsync(int take = 10)
    {
        return await _dbSet
            .Where(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(r => r.Categories.Any(c => c.CategoryId == categoryId))
            .Where(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Approved)
            .OrderByDescending(r => r.EvidenceLevel)
            .ThenBy(r => r.Title)
            .ToListAsync();
    }

    public async Task<Resource?> GetResourceWithDetailsAsync(Guid resourceId)
    {
        return await _dbSet
            .Include(r => r.Categories)
            .Include(r => r.Ratings)
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId);
    }

    public async Task IncrementViewCountAsync(Guid resourceId)
    {
        var resource = await _dbSet.FindAsync(resourceId);
        if (resource != null)
        {
            resource.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<Guid, int>> GetDownloadCountsAsync(List<Guid> resourceIds)
    {
        return await _dbSet
            .Where(r => resourceIds.Contains(r.ResourceId))
            .ToDictionaryAsync(r => r.ResourceId, r => r.DownloadCount);
    }

    // Content management method implementations
    public async Task<IEnumerable<Resource>> GetByReviewStatusAsync(ClinicalReviewStatus status)
    {
        return await _dbSet
            .Where(r => r.ClinicalReviewStatus == status)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task AssignReviewerAsync(ReviewAssignment assignment)
    {
        var dbContext = _context as ApplicationDbContext;
        if (dbContext != null)
        {
            dbContext.ReviewAssignments.Add(assignment);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("ApplicationDbContext not available for ReviewAssignment operations");
        }
    }

    public async Task SubmitReviewEvaluationAsync(ReviewEvaluation evaluation)
    {
        var dbContext = _context as ApplicationDbContext;
        if (dbContext != null)
        {
            dbContext.ReviewEvaluations.Add(evaluation);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("ApplicationDbContext not available for ReviewEvaluation operations");
        }
    }

    public async Task<IEnumerable<ReviewEvaluation>> GetResourceReviewsAsync(Guid resourceId)
    {
        var dbContext = _context as ApplicationDbContext;
        if (dbContext != null)
        {
            return await dbContext.ReviewEvaluations
                .Where(re => re.ResourceId == resourceId)
                .OrderByDescending(re => re.ReviewedAt)
                .ToListAsync();
        }
        else
        {
            return new List<ReviewEvaluation>();
        }
    }

    public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
    {
        var dbContext = _context as ApplicationDbContext;

        // Calculate review statistics based on resources and assignments
        var pendingCount = await _dbSet.CountAsync(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Pending);
        var totalReviewed = await _dbSet.CountAsync(r => r.ClinicalReviewStatus != ClinicalReviewStatus.Pending);
        var approvedCount = await _dbSet.CountAsync(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Approved);

        var approvalRate = totalReviewed > 0 ? (double)approvedCount / totalReviewed * 100 : 0;

        // Calculate average review time from evaluations if available
        double averageReviewHours = 48.0; // Default fallback
        if (dbContext != null)
        {
            var completedReviews = await dbContext.ReviewEvaluations
                .Join(dbContext.ReviewAssignments,
                    eval => eval.ResourceId,
                    assign => assign.ResourceId,
                    (eval, assign) => new { eval.ReviewedAt, assign.AssignedAt })
                .ToListAsync();

            if (completedReviews.Any())
            {
                averageReviewHours = completedReviews
                    .Average(r => (r.ReviewedAt - r.AssignedAt).TotalHours);
            }
        }

        // Get reviewer workload if assignments available
        var reviewerWorkload = new Dictionary<string, int>();
        if (dbContext != null)
        {
            reviewerWorkload = await dbContext.ReviewAssignments
                .Where(ra => ra.Status == ReviewAssignmentStatus.Pending || ra.Status == ReviewAssignmentStatus.InProgress)
                .GroupBy(ra => ra.ReviewerId)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        return new ReviewStatisticsDto
        {
            Overall = new OverallStatistics
            {
                PendingReviews = pendingCount,
                AvgReviewTimeHours = averageReviewHours
            },
            Approval = new ApprovalStatistics
            {
                // These would need to be calculated from actual data
                ApprovedCount = (int)(pendingCount * (approvalRate / 100)),
                RejectedCount = (int)(pendingCount * ((100 - approvalRate) / 100))
            },
            ReviewerWorkload = reviewerWorkload
        };
    }

    // Version control method implementations
    public async Task<IEnumerable<Resource>> GetResourcesAsync(Func<Resource, bool> predicate)
    {
        var allResources = await _dbSet.ToListAsync();
        return allResources.Where(predicate);
    }
}