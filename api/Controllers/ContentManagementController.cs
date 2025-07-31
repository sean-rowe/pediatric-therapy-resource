using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;
using UPTRMS.Api.Services;
using System.Security.Claims;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/content-management")]
[Authorize(Policy = "AdminOnly")]
public class ContentManagementController : ControllerBase
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ContentManagementController> _logger;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ContentManagementController(
        IResourceRepository resourceRepository,
        IUserRepository userRepository,
        ILogger<ContentManagementController> logger,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _resourceRepository = resourceRepository;
        _userRepository = userRepository;
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Content upload portal for verified content creators
    /// </summary>
    [HttpPost("upload-resource")]
    public async Task<ActionResult<ResourceUploadResponse>> UploadResource([FromBody] ContentUploadRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Create new resource with metadata provided by content creator
            Resource resource = new Resource
            {
                Title = request.Title,
                Description = request.Description,
                ResourceType = request.ResourceType,
                LanguagesAvailable = request.Languages,
                FileUrl = request.FileUrl,
                CreatedByUserId = userId,
                GenerationMethod = GenerationMethod.Manual,
                ClinicalReviewStatus = ClinicalReviewStatus.Pending,
                EvidenceLevel = request.EvidenceLevel,
                IsPublished = false // Will be published after review approval
            };

            // Set JSON fields using helper methods
            Dictionary<string, object> skillAreasDict = new Dictionary<string, object> { ["areas"] = request.SkillAreas };
            resource.SetSkillAreas(skillAreasDict);

            List<int> gradeLevels = request.GradeLevels.Select(ParseGradeLevel).Where(g => g.HasValue).Select(g => g!.Value).ToList();
            resource.SetGradeLevels(gradeLevels);

            // Save the resource
            Resource createdResource = await _resourceRepository.AddAsync(resource);

            _logger.LogInformation("Content creator {UserId} uploaded resource {ResourceId} for review",
                userId, createdResource.ResourceId);

            return Ok(new ResourceUploadResponse
            {
                ResourceId = createdResource.ResourceId,
                Status = "Pending Review",
                Message = "Resource uploaded successfully and submitted for clinical review",
                EstimatedReviewTime = "2-3 business days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading resource for content creator");
            return StatusCode(500, new { error = "An error occurred while uploading the resource" });
        }
    }

    /// <summary>
    /// Gets resources pending review for clinical accuracy
    /// </summary>
    [HttpGet("pending-review")]
    public async Task<ActionResult<List<ResourceReviewDto>>> GetPendingReviews()
    {
        try
        {
            List<Resource> resources = (await _resourceRepository.GetByReviewStatusAsync(ClinicalReviewStatus.Pending)).ToList();
            List<ResourceReviewDto> reviewDtos = resources.Select(r => MapToReviewDto(r)).ToList();

            _logger.LogInformation("Retrieved {Count} resources pending review", reviewDtos.Count);
            return Ok(reviewDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending reviews");
            return StatusCode(500, new { error = "An error occurred while retrieving pending reviews" });
        }
    }

    /// <summary>
    /// Assigns a resource to a clinical reviewer for peer review
    /// </summary>
    [HttpPost("assign-reviewer")]
    public async Task<ActionResult> AssignReviewer([FromBody] AssignReviewerRequest request)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
            {
                return NotFound(new { error = "Resource not found" });
            }

            User? reviewer = await _userRepository.GetByIdAsync(request.ReviewerId);
            if (reviewer == null)
            {
                return NotFound(new { error = "Reviewer not found" });
            }

            // Create review assignment record
            ReviewAssignment assignment = new ReviewAssignment
            {
                ResourceId = request.ResourceId,
                ReviewerId = request.ReviewerId,
                AssignedAt = DateTime.UtcNow,
                AssignedByUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
                Status = ReviewAssignmentStatus.Pending
            };

            // Store assignment (would need to implement this in repository)
            await _resourceRepository.AssignReviewerAsync(assignment);

            _logger.LogInformation("Assigned resource {ResourceId} to reviewer {ReviewerId}",
                request.ResourceId, request.ReviewerId);

            return Ok(new { message = "Reviewer assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning reviewer for resource {ResourceId}", request.ResourceId);
            return StatusCode(500, new { error = "An error occurred while assigning reviewer" });
        }
    }

    /// <summary>
    /// Submits peer review evaluation for clinical accuracy
    /// </summary>
    [HttpPost("submit-review")]
    public async Task<ActionResult> SubmitReview([FromBody] SubmitReviewRequest request)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
            {
                return NotFound(new { error = "Resource not found" });
            }

            var reviewerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Create review evaluation record
            var evaluation = new ReviewEvaluation
            {
                ReviewAssignmentId = Guid.NewGuid(), // In a real implementation, this would come from an existing assignment
                ResourceId = request.ResourceId,
                ReviewerId = reviewerId,
                ClinicalAccuracy = request.ClinicalAccuracy,
                AgeAppropriateness = request.AgeAppropriateness,
                EvidenceLevel = (request.ClinicalAccuracy + request.AgeAppropriateness + 
                               request.SafetyCompliance + request.TherapeuticValue) / 4, // Average of all scores
                ApprovalStatus = request.OverallApproval ? ReviewApprovalStatus.Approved : ReviewApprovalStatus.Rejected,
                Comments = request.Comments ?? string.Empty,
                ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Store evaluation
            await _resourceRepository.SubmitReviewEvaluationAsync(evaluation);

            // Update resource status based on review outcome
            if (request.OverallApproval)
            {
                resource.ClinicalReviewStatus = ClinicalReviewStatus.Approved;
                resource.EvidenceLevel = DetermineEvidenceLevel(evaluation);

                _logger.LogInformation("Resource {ResourceId} approved by reviewer {ReviewerId}",
                    request.ResourceId, reviewerId);
            }
            else
            {
                resource.ClinicalReviewStatus = ClinicalReviewStatus.NeedsRevision;

                _logger.LogInformation("Resource {ResourceId} needs revision per reviewer {ReviewerId}",
                    request.ResourceId, reviewerId);
            }

            resource.UpdatedAt = DateTime.UtcNow;
            await _resourceRepository.UpdateAsync(resource);

            // Send notification to resource creator about review outcome
            await NotifyResourceCreator(resource, evaluation);

            return Ok(new
            {
                message = "Review submitted successfully",
                status = resource.ClinicalReviewStatus.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for resource {ResourceId}", request.ResourceId);
            return StatusCode(500, new { error = "An error occurred while submitting review" });
        }
    }

    /// <summary>
    /// Gets review history for a specific resource
    /// </summary>
    [HttpGet("resource/{resourceId}/reviews")]
    public async Task<ActionResult<List<ReviewEvaluationDto>>> GetResourceReviews(Guid resourceId)
    {
        try
        {
            var reviews = await _resourceRepository.GetResourceReviewsAsync(resourceId);
            var reviewDtos = reviews.Select(r => MapToEvaluationDto(r)).ToList();

            return Ok(reviewDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for resource {ResourceId}", resourceId);
            return StatusCode(500, new { error = "An error occurred while retrieving reviews" });
        }
    }

    /// <summary>
    /// Initiates content retirement workflow for outdated resources
    /// </summary>
    [HttpPost("retire-resource")]
    public async Task<ActionResult> RetireResource([FromBody] RetireResourceRequest request)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
            {
                return NotFound(new { error = "Resource not found" });
            }

            // Only allow retirement of published/approved resources
            if (resource.ClinicalReviewStatus != ClinicalReviewStatus.Approved)
            {
                return BadRequest(new { error = "Only approved resources can be retired" });
            }

            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Update resource status to retired
            resource.ClinicalReviewStatus = ClinicalReviewStatus.Retired;
            resource.RetiredAt = DateTime.UtcNow;
            resource.RetiredBy = userId;
            resource.RetiredReason = request.Reason;
            resource.UpdatedAt = DateTime.UtcNow;

            // Set suggested alternatives if provided
            if (request.SuggestedAlternatives != null && request.SuggestedAlternatives.Any())
            {
                resource.SetSuggestedAlternatives(request.SuggestedAlternatives);
            }

            await _resourceRepository.UpdateAsync(resource);

            // Notify affected users about resource retirement
            await NotifyUsersOfRetirement(resource);

            _logger.LogInformation("Resource {ResourceId} retired by admin {UserId} with reason: {Reason}",
                request.ResourceId, userId, request.Reason);

            return Ok(new
            {
                message = "Resource retired successfully",
                retiredAt = resource.RetiredAt,
                status = "Retired",
                suggestedAlternatives = resource.SuggestedAlternatives?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retiring resource {ResourceId}", request.ResourceId);
            return StatusCode(500, new { error = "An error occurred while retiring the resource" });
        }
    }

    /// <summary>
    /// Initiates automated copyright verification for uploaded content
    /// </summary>
    [HttpPost("verify-copyright")]
    public async Task<ActionResult<CopyrightVerificationResponse>> VerifyCopyright([FromBody] CopyrightVerificationRequest request)
    {
        try
        {
            Resource? resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
            {
                return NotFound(new { error = "Resource not found" });
            }

            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Perform copyright checks
            var imageResults = await CheckImageCopyright(request.ImageUrls);
            var textResults = await CheckTextCopyright(request.TextContent);
            var rightsVerification = await VerifyContentRights(request.LicenseInfo);

            var hasViolations = imageResults.HasViolations || textResults.HasViolations || !rightsVerification.IsVerified;

            if (hasViolations)
            {
                // Flag resource for manual review
                resource.ClinicalReviewStatus = ClinicalReviewStatus.NeedsRevision;
                resource.UpdatedAt = DateTime.UtcNow;
                await _resourceRepository.UpdateAsync(resource);

                // Create copyright violation record
                var violation = new CopyrightViolation
                {
                    ResourceId = request.ResourceId,
                    DetectedAt = DateTime.UtcNow,
                    ImageViolations = imageResults.Violations,
                    TextViolations = textResults.Violations,
                    Status = "Pending Review"
                };

                await StoreCopyrightViolation(violation);
                await NotifyUserOfCopyrightConcern(userId, resource, violation);

                _logger.LogWarning("Copyright violations detected for resource {ResourceId}", request.ResourceId);
            }

            return Ok(new CopyrightVerificationResponse
            {
                ResourceId = request.ResourceId,
                HasViolations = hasViolations,
                ImageResults = imageResults,
                TextResults = textResults,
                RightsVerification = rightsVerification,
                NextSteps = hasViolations ? "Manual review required" : "Copyright verification passed",
                SuggestedAlternatives = hasViolations ? await GetAlternativeContent(request.ResourceId) : new List<string>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during copyright verification for resource {ResourceId}", request.ResourceId);
            return StatusCode(500, new { error = "An error occurred during copyright verification" });
        }
    }

    /// <summary>
    /// Initiates bulk upload process for content partners
    /// </summary>
    [HttpPost("bulk-upload")]
    public async Task<ActionResult<BulkUploadResponse>> BulkUpload([FromForm] BulkUploadRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Validate CSV metadata file
            var csvValidationResult = await ValidateCsvMetadata(request.MetadataCsv);
            if (!csvValidationResult.IsValid)
            {
                return BadRequest(new { error = "Invalid CSV format", details = csvValidationResult.Errors });
            }

            // Validate ZIP file
            var zipValidationResult = ValidateZipFile(request.ResourcesZip);
            if (!zipValidationResult.IsValid)
            {
                return BadRequest(new { error = "Invalid ZIP file", details = zipValidationResult.Errors });
            }

            // Create bulk upload job
            var bulkUploadJob = new BulkUploadJob
            {
                JobId = Guid.NewGuid(),
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = "Processing",
                TotalFiles = csvValidationResult.RecordCount,
                ProcessedFiles = 0,
                SuccessCount = 0,
                ErrorCount = 0
            };

            await StartBulkUploadProcessing(bulkUploadJob, csvValidationResult.Metadata, zipValidationResult.Files);

            _logger.LogInformation("Bulk upload initiated by user {UserId} with job {JobId}", userId, bulkUploadJob.JobId);

            return Ok(new BulkUploadResponse
            {
                JobId = bulkUploadJob.JobId,
                Status = "Processing",
                Message = "Bulk upload processing started",
                EstimatedTime = $"{csvValidationResult.RecordCount * 2} seconds",
                TrackingUrl = $"/api/content-management/bulk-upload/{bulkUploadJob.JobId}/status"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk upload initiation");
            return StatusCode(500, new { error = "An error occurred during bulk upload" });
        }
    }

    /// <summary>
    /// Gets bulk upload job status and progress
    /// </summary>
    [HttpGet("bulk-upload/{jobId}/status")]
    public async Task<ActionResult<BulkUploadStatusResponse>> GetBulkUploadStatus(Guid jobId)
    {
        try
        {
            var job = await GetBulkUploadJob(jobId);
            if (job == null)
            {
                return NotFound(new { error = "Bulk upload job not found" });
            }

            var progress = job.TotalFiles > 0 ? (double)job.ProcessedFiles / job.TotalFiles * 100 : 0;

            return Ok(new BulkUploadStatusResponse
            {
                JobId = jobId,
                Status = job.Status,
                Progress = progress,
                ProcessedFiles = job.ProcessedFiles,
                TotalFiles = job.TotalFiles,
                SuccessCount = job.SuccessCount,
                ErrorCount = job.ErrorCount,
                Errors = await GetJobErrors(jobId),
                CompletedAt = job.CompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bulk upload status for job {JobId}", jobId);
            return StatusCode(500, new { error = "An error occurred while retrieving upload status" });
        }
    }

    /// <summary>
    /// Gets retired resources with their alternatives
    /// </summary>
    [HttpGet("retired-resources")]
    public async Task<ActionResult<List<RetiredResourceDto>>> GetRetiredResources()
    {
        try
        {
            var retiredResources = await _resourceRepository.GetByReviewStatusAsync(ClinicalReviewStatus.Retired);
            var retiredDtos = retiredResources.Select(r => MapToRetiredDto(r)).ToList();

            return Ok(retiredDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retired resources");
            return StatusCode(500, new { error = "An error occurred while retrieving retired resources" });
        }
    }

    /// <summary>
    /// Submits updated version of existing resource
    /// </summary>
    [HttpPost("submit-version-update")]
    public async Task<ActionResult<VersionUpdateResponse>> SubmitVersionUpdate([FromBody] VersionUpdateRequest request)
    {
        try
        {
            var originalResource = await _resourceRepository.GetByIdAsync(request.OriginalResourceId);
            if (originalResource == null)
            {
                return NotFound(new { error = "Original resource not found" });
            }

            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Verify user owns the original resource or has admin rights
            if (originalResource.CreatedByUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only update resources you created");
            }

            // Create new version as pending resource
            var newVersion = new Resource
            {
                Title = request.Title ?? originalResource.Title,
                Description = request.Description ?? originalResource.Description,
                ResourceType = originalResource.ResourceType,
                LanguagesAvailable = request.Languages ?? originalResource.LanguagesAvailable,
                FileUrl = request.FileUrl,
                CreatedByUserId = userId,
                GenerationMethod = GenerationMethod.Manual,
                ClinicalReviewStatus = ClinicalReviewStatus.Pending,
                Version = request.Version,
                PreviousVersionId = request.OriginalResourceId,
                IsPublished = false
            };

            // Copy skill areas and grade levels from original if not provided
            newVersion.SetSkillAreas(originalResource.GetSkillAreas());
            newVersion.SetGradeLevels(originalResource.GetGradeLevels());

            var createdVersion = await _resourceRepository.AddAsync(newVersion);

            _logger.LogInformation("Version update submitted for resource {OriginalResourceId} by user {UserId}",
                request.OriginalResourceId, userId);

            return Ok(new VersionUpdateResponse
            {
                NewVersionId = createdVersion.ResourceId,
                Status = "Pending Review",
                Message = "Version update submitted for review",
                EstimatedReviewTime = "2-3 business days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting version update for resource {ResourceId}", request.OriginalResourceId);
            return StatusCode(500, new { error = "An error occurred while submitting version update" });
        }
    }

    /// <summary>
    /// Approves version update and manages version history
    /// </summary>
    [HttpPost("approve-version-update")]
    public async Task<ActionResult> ApproveVersionUpdate([FromBody] VersionApprovalRequest request)
    {
        try
        {
            var newVersion = await _resourceRepository.GetByIdAsync(request.NewVersionId);
            if (newVersion == null)
            {
                return NotFound(new { error = "New version not found" });
            }

            if (newVersion.PreviousVersionId == null)
            {
                return BadRequest(new { error = "Resource is not a version update" });
            }

            var originalVersion = await _resourceRepository.GetByIdAsync(newVersion.PreviousVersionId.Value);
            if (originalVersion == null)
            {
                return NotFound(new { error = "Original version not found" });
            }

            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Approve the new version
            newVersion.ClinicalReviewStatus = ClinicalReviewStatus.Approved;
            newVersion.IsPublished = true;
            newVersion.UpdatedAt = DateTime.UtcNow;

            // Mark original version as superseded
            originalVersion.IsSuperseded = true;
            originalVersion.SupersededAt = DateTime.UtcNow;
            originalVersion.LatestVersionId = newVersion.ResourceId;
            originalVersion.UpdatedAt = DateTime.UtcNow;

            // Update both resources
            await _resourceRepository.UpdateAsync(newVersion);
            await _resourceRepository.UpdateAsync(originalVersion);

            // Notify users who downloaded the original version
            await NotifyUsersOfVersionUpdate(originalVersion.ResourceId, newVersion.ResourceId);

            _logger.LogInformation("Version update approved: {NewVersionId} supersedes {OriginalVersionId}",
                newVersion.ResourceId, originalVersion.ResourceId);

            return Ok(new
            {
                message = "Version update approved successfully",
                newVersionId = newVersion.ResourceId,
                originalVersionId = originalVersion.ResourceId,
                status = "Published",
                notificationsSent = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving version update for {NewVersionId}", request.NewVersionId);
            return StatusCode(500, new { error = "An error occurred while approving version update" });
        }
    }

    /// <summary>
    /// Gets version history for a resource
    /// </summary>
    [HttpGet("resource/{resourceId}/version-history")]
    public async Task<ActionResult<List<ResourceVersionDto>>> GetVersionHistory(Guid resourceId)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
            {
                return NotFound(new { error = "Resource not found" });
            }

            var versions = await GetVersionsForResource(resourceId);
            var versionDtos = versions.Select(v => CreateVersionDto(v)).OrderBy(v => v.CreatedAt).ToList();

            return Ok(versionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving version history for resource {ResourceId}", resourceId);
            return StatusCode(500, new { error = "An error occurred while retrieving version history" });
        }
    }

    /// <summary>
    /// Gets quality review analytics and metrics for the content management dashboard
    /// </summary>
    [HttpGet("quality-metrics")]
    public async Task<ActionResult<QualityMetricsResponse>> GetQualityMetrics()
    {
        try
        {
            var stats = await _resourceRepository.GetReviewStatisticsAsync();

            // Calculate additional metrics for quality dashboard
            var metrics = new QualityMetricsResponse
            {
                AverageReviewTimeDays = Math.Round(stats.AverageReviewTimeHours / 24, 1),
                ApprovalRatePercent = Math.Round(stats.ApprovalRate, 1),
                ResourcesInQueue = stats.ResourcesInQueue,
                AverageReviewerWorkload = stats.ReviewerWorkload.Any() ? Math.Round(stats.ReviewerWorkload.Values.Average(), 1) : 0,
                ReviewerWorkloadDistribution = stats.ReviewerWorkload,

                // Quality trends (would be calculated from historical data in real implementation)
                QualityTrends = new QualityTrendsDto
                {
                    LastWeekApprovalRate = 84.5,
                    LastMonthApprovalRate = 86.2,
                    ReviewTimeImprovement = -0.3, // Negative = improvement
                    QueueSizeChange = 5 // Positive = increase
                },

                // Bottleneck analysis
                Bottlenecks = await IdentifyReviewBottlenecks(stats),

                // Alerts for dashboard
                Alerts = await GenerateQualityAlerts(stats),

                // Target vs actual comparison
                Targets = new QualityTargetsDto
                {
                    TargetReviewTimeDays = 3.0,
                    TargetApprovalRate = 80.0,
                    TargetMaxQueueSize = 50,
                    TargetMaxReviewerWorkload = 10
                }
            };

            _logger.LogInformation("Retrieved quality metrics: {QueueSize} resources, {ApprovalRate}% approval rate",
                metrics.ResourcesInQueue, metrics.ApprovalRatePercent);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quality metrics");
            return StatusCode(500, new { error = "An error occurred while retrieving quality metrics" });
        }
    }

    private async Task<List<BottleneckDto>> IdentifyReviewBottlenecks(ReviewStatisticsDto stats)
    {
        var bottlenecks = new List<BottleneckDto>();

        // Identify bottlenecks based on various metrics
        if (stats.ResourcesInQueue > 50)
        {
            bottlenecks.Add(new BottleneckDto
            {
                Type = "High Queue Volume",
                Severity = "High",
                Description = $"Review queue has {stats.ResourcesInQueue} resources (target: <50)",
                Recommendation = "Consider adding more reviewers or adjusting review processes"
            });
        }

        if (stats.AverageReviewTimeHours > 72) // > 3 days
        {
            bottlenecks.Add(new BottleneckDto
            {
                Type = "Slow Review Process",
                Severity = "Medium",
                Description = $"Average review time is {Math.Round(stats.AverageReviewTimeHours / 24, 1)} days (target: <3 days)",
                Recommendation = "Review workflow efficiency and reviewer training"
            });
        }

        // Check for uneven workload distribution
        if (stats.ReviewerWorkload.Any())
        {
            var workloads = stats.ReviewerWorkload.Values.ToList();
            var maxWorkload = workloads.Max();
            var minWorkload = workloads.Min();

            if (maxWorkload - minWorkload > 5)
            {
                bottlenecks.Add(new BottleneckDto
                {
                    Type = "Uneven Workload Distribution",
                    Severity = "Medium",
                    Description = $"Workload imbalance: {minWorkload}-{maxWorkload} reviews per reviewer",
                    Recommendation = "Implement automatic workload balancing"
                });
            }
        }

        await Task.CompletedTask; // Placeholder for async operations
        return bottlenecks;
    }

    private async Task<List<QualityAlertDto>> GenerateQualityAlerts(ReviewStatisticsDto stats)
    {
        var alerts = new List<QualityAlertDto>();

        // Generate alerts based on metrics exceeding thresholds
        if (stats.ResourcesInQueue > 60)
        {
            alerts.Add(new QualityAlertDto
            {
                Type = "Critical",
                Message = $"Review queue is critically high: {stats.ResourcesInQueue} resources",
                ActionRequired = "Immediate attention needed to clear backlog",
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (stats.ResourcesInQueue > 50)
        {
            alerts.Add(new QualityAlertDto
            {
                Type = "Warning",
                Message = $"Review queue approaching capacity: {stats.ResourcesInQueue} resources",
                ActionRequired = "Monitor queue and consider additional resources",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (stats.ApprovalRate < 70)
        {
            alerts.Add(new QualityAlertDto
            {
                Type = "Warning",
                Message = $"Approval rate is low: {Math.Round(stats.ApprovalRate, 1)}%",
                ActionRequired = "Review content quality guidelines and creator training",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check for reviewer overload
        var overloadedReviewers = stats.ReviewerWorkload.Where(r => int.Parse(r.Value.ToString()) > 12).ToList();
        if (overloadedReviewers.Any())
        {
            alerts.Add(new QualityAlertDto
            {
                Type = "Warning",
                Message = $"{overloadedReviewers.Count} reviewer(s) have excessive workload",
                ActionRequired = "Redistribute workload or add additional reviewers",
                CreatedAt = DateTime.UtcNow
            });
        }

        await Task.CompletedTask; // Placeholder for async operations
        return alerts;
    }

    /// <summary>
    /// Runs intelligent reviewer assignment system for pending resources
    /// </summary>
    [HttpPost("auto-assign-reviewers")]
    public async Task<ActionResult<ReviewerAssignmentResponse>> AutoAssignReviewers([FromBody] AutoAssignmentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            // Get pending resources
            var pendingResources = await _resourceRepository.GetByReviewStatusAsync(ClinicalReviewStatus.Pending);
            var resourcesList = pendingResources.Take(request.MaxResourcesPerRun ?? 20).ToList();

            if (!resourcesList.Any())
            {
                return Ok(new ReviewerAssignmentResponse
                {
                    AssignmentsCreated = 0,
                    Message = "No pending resources found for assignment",
                    AssignmentDetails = new List<AssignmentDetailDto>()
                });
            }

            // Get available reviewers with their current workload
            var availableReviewers = await GetAvailableReviewers();

            // Run intelligent assignment algorithm
            var assignments = await RunIntelligentAssignment(resourcesList, availableReviewers, request);

            // Create assignment records
            var assignmentDetails = new List<AssignmentDetailDto>();
            foreach (var assignment in assignments)
            {
                var reviewAssignment = new ReviewAssignment
                {
                    ReviewAssignmentId = Guid.NewGuid(),
                    ResourceId = assignment.ResourceId,
                    ReviewerUserId = assignment.ReviewerId,
                    AssignedByUserId = userId,
                    AssignedAt = DateTime.UtcNow,
                    Status = ReviewAssignmentStatus.Pending,
                    DueDate = DateTime.UtcNow.AddDays(3), // Default 3-day estimate
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _resourceRepository.AssignReviewerAsync(reviewAssignment);

                assignmentDetails.Add(new AssignmentDetailDto
                {
                    ResourceId = assignment.ResourceId,
                    ResourceTitle = assignment.ResourceTitle,
                    ReviewerId = assignment.ReviewerId,
                    ReviewerName = assignment.ReviewerName,
                    ReviewerSpecialty = assignment.ReviewerSpecialty,
                    Priority = assignment.Priority,
                    AssignmentReason = assignment.AssignmentReason,
                    EstimatedCompletionDate = reviewAssignment.DueDate ?? DateTime.UtcNow.AddDays(3)
                });
            }

            _logger.LogInformation("Auto-assignment completed: {Count} resources assigned to reviewers",
                assignments.Count);

            return Ok(new ReviewerAssignmentResponse
            {
                AssignmentsCreated = assignments.Count,
                Message = $"Successfully assigned {assignments.Count} resources to reviewers",
                AssignmentDetails = assignmentDetails,
                WorkloadSummary = await GetWorkloadSummary(availableReviewers)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-assignment of reviewers");
            return StatusCode(500, new { error = "An error occurred during reviewer auto-assignment" });
        }
    }

    private async Task<List<ReviewerProfileDto>> GetAvailableReviewers()
    {
        // In a real implementation, this would query the User/Reviewer tables
        // For now, returning sample reviewer data
        await Task.CompletedTask;

        return new List<ReviewerProfileDto>
        {
            new() { ReviewerId = Guid.NewGuid(), Name = "Dr. Smith", Specialty = "SLP", CurrentWorkload = 3, MaxWorkload = 8, IsAvailable = true },
            new() { ReviewerId = Guid.NewGuid(), Name = "Dr. Johnson", Specialty = "OT", CurrentWorkload = 5, MaxWorkload = 8, IsAvailable = true },
            new() { ReviewerId = Guid.NewGuid(), Name = "Dr. Williams", Specialty = "PT", CurrentWorkload = 2, MaxWorkload = 8, IsAvailable = true },
            new() { ReviewerId = Guid.NewGuid(), Name = "Dr. Brown", Specialty = "ABA", CurrentWorkload = 4, MaxWorkload = 8, IsAvailable = true },
            new() { ReviewerId = Guid.NewGuid(), Name = "Dr. Davis", Specialty = "General", CurrentWorkload = 6, MaxWorkload = 10, IsAvailable = true }
        };
    }

    private async Task<List<IntelligentAssignmentDto>> RunIntelligentAssignment(
        List<Resource> resources,
        List<ReviewerProfileDto> reviewers,
        AutoAssignmentRequest request)
    {
        var assignments = new List<IntelligentAssignmentDto>();
        var availableReviewers = reviewers.Where(r => r.IsAvailable && r.CurrentWorkload < r.MaxWorkload).ToList();

        foreach (var resource in resources)
        {
            var assignment = await AssignOptimalReviewer(resource, availableReviewers, request);
            if (assignment != null)
            {
                assignments.Add(assignment);

                // Update workload for subsequent assignments
                var reviewer = availableReviewers.First(r => r.ReviewerId == assignment.ReviewerId);
                reviewer.CurrentWorkload++;

                // Remove from available list if at capacity
                if (reviewer.CurrentWorkload >= reviewer.MaxWorkload)
                {
                    availableReviewers.Remove(reviewer);
                }
            }
        }

        await Task.CompletedTask;
        return assignments;
    }

    private async Task<IntelligentAssignmentDto?> AssignOptimalReviewer(
        Resource resource,
        List<ReviewerProfileDto> availableReviewers,
        AutoAssignmentRequest request)
    {
        if (!availableReviewers.Any()) return null;

        var resourceSpecialty = DetermineResourceSpecialty(resource);
        var isUrgent = IsUrgentResource(resource);
        var complexity = DetermineResourceComplexity(resource);

        // Score each reviewer based on assignment criteria
        var reviewerScores = new List<(ReviewerProfileDto Reviewer, double Score, string Reason)>();

        foreach (var reviewer in availableReviewers)
        {
            double score = 0;
            var reasons = new List<string>();

            // Specialty match (High weight)
            if (reviewer.Specialty == resourceSpecialty || reviewer.Specialty == "General")
            {
                score += reviewer.Specialty == resourceSpecialty ? 100 : 50;
                reasons.Add(reviewer.Specialty == resourceSpecialty ? "Specialty match" : "General specialist");
            }

            // Workload balance (Medium weight)
            var workloadScore = (reviewer.MaxWorkload - reviewer.CurrentWorkload) * 10;
            score += workloadScore;
            reasons.Add($"Workload: {reviewer.CurrentWorkload}/{reviewer.MaxWorkload}");

            // Urgency handling (Medium weight)
            if (isUrgent && reviewer.CurrentWorkload <= 3)
            {
                score += 30;
                reasons.Add("Available for urgent review");
            }

            // Complexity handling (Low weight)
            if (complexity == "High" && reviewer.Specialty != "General")
            {
                score += 10;
                reasons.Add("Experienced with complex resources");
            }

            reviewerScores.Add((reviewer, score, string.Join(", ", reasons)));
        }

        // Select the highest scoring reviewer
        var bestMatch = reviewerScores.OrderByDescending(rs => rs.Score).First();

        await Task.CompletedTask;

        return new IntelligentAssignmentDto
        {
            ResourceId = resource.ResourceId,
            ResourceTitle = resource.Title,
            ReviewerId = bestMatch.Reviewer.ReviewerId,
            ReviewerName = bestMatch.Reviewer.Name,
            ReviewerSpecialty = bestMatch.Reviewer.Specialty,
            Priority = isUrgent ? ReviewPriority.Urgent : ReviewPriority.Normal,
            AssignmentReason = bestMatch.Reason,
            AssignmentScore = bestMatch.Score
        };
    }

    private string DetermineResourceSpecialty(Resource resource)
    {
        // Analyze resource content to determine most appropriate specialty
        var skillAreas = resource.GetSkillAreas();

        if (skillAreas.ContainsKey("areas"))
        {
            var areas = skillAreas["areas"].ToString()?.ToLower() ?? "";

            if (areas.Contains("speech") || areas.Contains("language") || areas.Contains("communication"))
                return "SLP";
            if (areas.Contains("fine motor") || areas.Contains("sensory") || areas.Contains("occupational"))
                return "OT";
            if (areas.Contains("gross motor") || areas.Contains("physical") || areas.Contains("mobility"))
                return "PT";
            if (areas.Contains("behavior") || areas.Contains("autism") || areas.Contains("aba"))
                return "ABA";
        }

        return "General";
    }

    private bool IsUrgentResource(Resource resource)
    {
        // Determine if resource requires urgent review
        var daysSinceSubmission = (DateTime.UtcNow - resource.CreatedAt).TotalDays;
        return daysSinceSubmission > 5 || resource.Title.ToLower().Contains("urgent");
    }

    private string DetermineResourceComplexity(Resource resource)
    {
        // Assess resource complexity for appropriate reviewer assignment
        var hasMultipleLanguages = resource.LanguagesAvailable.Count > 1;
        var hasHighEvidenceLevel = resource.EvidenceLevel >= 4;
        var isInteractive = resource.IsInteractive;

        if (hasMultipleLanguages && hasHighEvidenceLevel && isInteractive)
            return "High";
        if (hasHighEvidenceLevel || isInteractive)
            return "Medium";

        return "Low";
    }

    private async Task<WorkloadSummaryDto> GetWorkloadSummary(List<ReviewerProfileDto> reviewers)
    {
        await Task.CompletedTask;

        return new WorkloadSummaryDto
        {
            TotalReviewers = reviewers.Count,
            AvailableReviewers = reviewers.Count(r => r.IsAvailable && r.CurrentWorkload < r.MaxWorkload),
            AverageWorkload = reviewers.Average(r => (double)r.CurrentWorkload / r.MaxWorkload * 100),
            ReviewerDetails = reviewers.Select(r => new ReviewerWorkloadDto
            {
                ReviewerId = r.ReviewerId,
                Name = r.Name,
                Specialty = r.Specialty,
                CurrentWorkload = r.CurrentWorkload,
                MaxWorkload = r.MaxWorkload,
                CapacityPercent = (double)r.CurrentWorkload / r.MaxWorkload * 100,
                IsAvailable = r.IsAvailable
            }).ToList()
        };
    }

    /// <summary>
    /// Manages content categorization taxonomy for resource organization
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<CategoryCreationResponse>> CreateCategory([FromBody] CategoryCreationRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            // Create main category
            var category = new UPTRMS.Api.Models.Domain.Category
            {
                CategoryId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                IsActive = true,
                DisplayOrder = request.DisplayOrder ?? 999
            };

            // Validate taxonomy consistency
            var validationResult = await ValidateTaxonomyConsistency(category);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = "Taxonomy validation failed", details = validationResult.Errors });
            }

            // Save category
            var createdCategory = await CreateCategoryInDatabase(category);

            // Create subcategories if provided
            var subcategories = new List<UPTRMS.Api.Models.Domain.Category>();
            if (request.Subcategories?.Any() == true)
            {
                foreach (var subcat in request.Subcategories)
                {
                    var subcategory = new UPTRMS.Api.Models.Domain.Category
                    {
                        CategoryId = Guid.NewGuid(),
                        Name = subcat.Name,
                        Description = subcat.Description,
                        ParentCategoryId = createdCategory.CategoryId,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        DisplayOrder = subcat.DisplayOrder ?? 999
                    };

                    var createdSubcategory = await CreateCategoryInDatabase(subcategory);
                    subcategories.Add(createdSubcategory);
                }
            }

            // Initialize usage tracking
            await InitializeCategoryUsageTracking(createdCategory, subcategories);

            _logger.LogInformation("Created category '{CategoryName}' with {SubcategoryCount} subcategories by user {UserId}",
                request.Name, subcategories.Count, userId);

            return Ok(new CategoryCreationResponse
            {
                CategoryId = createdCategory.CategoryId,
                Name = createdCategory.Name,
                Description = createdCategory.Description,
                ParentCategoryId = createdCategory.ParentCategoryId,
                SubcategoriesCreated = subcategories.Count,
                Subcategories = subcategories.Select(s => new CategorySummaryDto
                {
                    CategoryId = s.CategoryId,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive
                }).ToList(),
                IsAvailableForTagging = true,
                Message = "Category created successfully and is available for content tagging"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category '{CategoryName}'", request.Name);
            return StatusCode(500, new { error = "An error occurred while creating the category" });
        }
    }

    /// <summary>
    /// Gets category taxonomy with usage statistics
    /// </summary>
    [HttpGet("categories/taxonomy")]
    public async Task<ActionResult<TaxonomyResponse>> GetCategoryTaxonomy([FromQuery] bool includeUsageStats = false)
    {
        try
        {
            var categories = await GetAllCategories();
            var taxonomy = BuildTaxonomyHierarchy(categories);

            if (includeUsageStats)
            {
                await PopulateUsageStatistics(taxonomy);
            }

            return Ok(new TaxonomyResponse
            {
                TotalCategories = categories.Count,
                TopLevelCategories = taxonomy.Count,
                Taxonomy = taxonomy,
                LastUpdated = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category taxonomy");
            return StatusCode(500, new { error = "An error occurred while retrieving taxonomy" });
        }
    }

    /// <summary>
    /// Reviews existing content for new category assignment
    /// </summary>
    [HttpPost("categories/{categoryId}/review-content")]
    public async Task<ActionResult<CategoryReviewResponse>> ReviewContentForCategory(Guid categoryId, [FromBody] CategoryReviewRequest request)
    {
        try
        {
            var category = await GetCategoryById(categoryId);
            if (category == null)
            {
                return NotFound($"Category {categoryId} not found");
            }

            // Get content that might fit this category
            var candidateResources = await FindCandidateResourcesForCategory(category, request);
            var suggestions = await GenerateCategorySuggestions(candidateResources, category);

            _logger.LogInformation("Generated {Count} category assignment suggestions for category '{CategoryName}'",
                suggestions.Count, category.Name);

            return Ok(new CategoryReviewResponse
            {
                CategoryId = categoryId,
                CategoryName = category.Name,
                ResourcesReviewed = candidateResources.Count,
                SuggestionsGenerated = suggestions.Count,
                Suggestions = suggestions,
                ReviewedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing content for category {CategoryId}", categoryId);
            return StatusCode(500, new { error = "An error occurred while reviewing content for category" });
        }
    }

    private async Task<TaxonomyValidationResult> ValidateTaxonomyConsistency(UPTRMS.Api.Models.Domain.Category category)
    {
        var result = new TaxonomyValidationResult { IsValid = true, Errors = new List<string>() };

        // Check for duplicate names at the same level
        var existingCategories = await GetCategoriesAtLevel(category.ParentCategoryId);
        if (existingCategories.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
        {
            result.IsValid = false;
            result.Errors.Add($"Category name '{category.Name}' already exists at this level");
        }

        // Check parent exists if specified
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await GetCategoryById(category.ParentCategoryId.Value);
            if (parent == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Parent category {category.ParentCategoryId} does not exist");
            }
        }

        // Check for circular references
        if (await WouldCreateCircularReference(category))
        {
            result.IsValid = false;
            result.Errors.Add("Category would create a circular reference in taxonomy");
        }

        return result;
    }

    private async Task<UPTRMS.Api.Models.Domain.Category> CreateCategoryInDatabase(UPTRMS.Api.Models.Domain.Category category)
    {
        // In a real implementation, this would use a CategoryRepository
        await Task.CompletedTask;
        return category; // Placeholder
    }

    private async Task InitializeCategoryUsageTracking(UPTRMS.Api.Models.Domain.Category category, List<UPTRMS.Api.Models.Domain.Category> subcategories)
    {
        // Initialize usage tracking for new categories
        await Task.CompletedTask; // Placeholder
    }

    private async Task<List<Category>> GetAllCategories()
    {
        // Placeholder - would query database
        await Task.CompletedTask;
        return new List<Category>();
    }

    private List<TaxonomyNodeDto> BuildTaxonomyHierarchy(List<Category> categories)
    {
        // Build hierarchical taxonomy structure
        var topLevel = categories.Where(c => !c.ParentCategoryId.HasValue).ToList();
        return topLevel.Select(c => BuildTaxonomyNode(c, categories)).ToList();
    }

    private TaxonomyNodeDto BuildTaxonomyNode(Category category, List<Category> allCategories)
    {
        var children = allCategories.Where(c => c.ParentCategoryId == category.CategoryId).ToList();
        return new TaxonomyNodeDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            UsageCount = 0, // Would be populated from usage tracking
            Children = children.Select(c => BuildTaxonomyNode(c, allCategories)).ToList()
        };
    }

    private async Task PopulateUsageStatistics(List<TaxonomyNodeDto> taxonomy)
    {
        // Populate usage statistics for taxonomy nodes
        await Task.CompletedTask; // Placeholder
    }

    private async Task<Category?> GetCategoryById(Guid categoryId)
    {
        // Placeholder - would query database
        await Task.CompletedTask;
        return null;
    }

    private async Task<List<Category>> GetCategoriesAtLevel(Guid? parentId)
    {
        // Placeholder - would query database
        await Task.CompletedTask;
        return new List<Category>();
    }

    private async Task<bool> WouldCreateCircularReference(Category category)
    {
        // Check for circular references in taxonomy
        await Task.CompletedTask;
        return false; // Placeholder
    }

    private async Task<List<Resource>> FindCandidateResourcesForCategory(Category category, CategoryReviewRequest request)
    {
        // Find resources that might belong to this category
        await Task.CompletedTask;
        return new List<Resource>(); // Placeholder
    }

    private async Task<List<CategorySuggestionDto>> GenerateCategorySuggestions(List<Resource> resources, Category category)
    {
        // Generate suggestions for categorizing resources
        await Task.CompletedTask;
        return new List<CategorySuggestionDto>(); // Placeholder
    }

    private ResourceReviewDto MapToReviewDto(Resource resource)
    {
        return new ResourceReviewDto
        {
            ResourceId = resource.ResourceId,
            Title = resource.Title,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            SkillAreas = resource.GetSkillAreas(),
            GradeLevels = resource.GetGradeLevels(),
            LanguagesAvailable = resource.LanguagesAvailable,
            GenerationMethod = resource.GenerationMethod,
            SubmittedAt = resource.CreatedAt,
            LastUpdated = resource.UpdatedAt,
            Status = resource.ClinicalReviewStatus,
            CreatedByUserId = resource.CreatedByUserId,
            FileUrl = resource.FileUrl,
            PreviewUrl = resource.PreviewUrl
        };
    }

    private ReviewEvaluationDto MapToEvaluationDto(ReviewEvaluation evaluation)
    {
        return new ReviewEvaluationDto
        {
            EvaluationId = evaluation.EvaluationId,
            ResourceId = evaluation.ResourceId,
            ReviewerId = evaluation.ReviewerId,
            ClinicalAccuracy = evaluation.ClinicalAccuracy,
            AgeAppropriateness = evaluation.AgeAppropriateness,
            EvidenceLevel = evaluation.EvidenceLevel,
            ApprovalStatus = evaluation.ApprovalStatus,
            Comments = evaluation.Comments,
            RequiredChanges = evaluation.RequiredChanges,
            ReviewedAt = evaluation.ReviewedAt
        };
    }

    private int DetermineEvidenceLevel(ReviewEvaluation evaluation)
    {
        // Simple algorithm to determine evidence level based on review scores
        var averageScore = (evaluation.ClinicalAccuracy + evaluation.AgeAppropriateness + evaluation.EvidenceLevel) / 3.0;

        return averageScore switch
        {
            >= 4.5 => 5,
            >= 3.5 => 4,
            >= 2.5 => 3,
            >= 1.5 => 2,
            _ => 1
        };
    }

    private async Task NotifyResourceCreator(Resource resource, ReviewEvaluation evaluation)
    {
        // Send notification email to resource creator about review outcome
        if (resource.CreatedByUserId.HasValue)
        {
            var creator = await _userRepository.GetByIdAsync(resource.CreatedByUserId.Value);
            if (creator != null)
            {
                var isApproved = evaluation.ApprovalStatus == ReviewApprovalStatus.Approved;
                var subject = isApproved ? "Your resource has been approved!" : "Your resource needs revisions";
                var message = isApproved
                    ? $"Great news! Your resource '{resource.Title}' has been approved and is now available."
                    : $"Your resource '{resource.Title}' needs some changes. Feedback: {evaluation.Comments}";

                await _emailService.SendSellerNotificationAsync(creator.Email, creator.FirstName, "ResourceReview", message);
            }
        }
        _logger.LogInformation("Notification sent to creator of resource {ResourceId}", resource.ResourceId);
    }

    private async Task NotifyUsersOfRetirement(Resource resource)
    {
        // Notify users who have favorited or recently downloaded this resource
        // Query users who may be affected by resource retirement
        // When UserFavorites and ResourceDownloads tables are added:
        // var affectedUsers = await _context.UserFavorites
        //     .Where(f => f.ResourceId == resource.ResourceId)
        //     .Select(f => f.User)
        //     .Distinct()
        //     .ToListAsync();

        // var recentDownloaders = await _context.ResourceDownloads
        //     .Where(d => d.ResourceId == resource.ResourceId && d.DownloadedAt > DateTime.UtcNow.AddDays(-30))
        //     .Select(d => d.User)
        //     .Distinct()
        //     .ToListAsync();

        // For now, notify the resource creator if available
        var usersToNotify = new List<User>();
        if (resource.CreatedByUserId.HasValue)
        {
            var creator = await _userRepository.GetByIdAsync(resource.CreatedByUserId.Value);
            if (creator != null) usersToNotify.Add(creator);
        }

        foreach (var user in usersToNotify)
        {
            var alternatives = resource.SuggestedAlternatives != null && resource.SuggestedAlternatives.Any()
                ? $" Suggested alternatives: {resource.SuggestedAlternatives.Count} alternative resources available"
                : "";

            var message = $"The resource '{resource.Title}' has been retired. Reason: {resource.RetiredReason}.{alternatives}";

            await _emailService.SendResourceSharedEmailAsync(
                user.Email,
                user.FirstName,
                "UPTRMS System",
                "Resource Retirement Notice",
                $"{_configuration["App:BaseUrl"]}/resources");
        }

        _logger.LogInformation("Retirement notification sent for resource {ResourceId} to {UserCount} users",
            resource.ResourceId, usersToNotify.Count());
    }

    private RetiredResourceDto MapToRetiredDto(Resource resource)
    {
        return new RetiredResourceDto
        {
            ResourceId = resource.ResourceId,
            Title = resource.Title,
            Description = resource.Description,
            RetiredAt = resource.RetiredAt ?? DateTime.MinValue,
            RetiredReason = resource.RetiredReason,
            RetiredBy = resource.RetiredBy,
            SuggestedAlternatives = resource.SuggestedAlternatives,
            OriginalFileUrl = resource.FileUrl // Still accessible for existing downloads
        };
    }

    private async Task<ImageCopyrightResult> CheckImageCopyright(List<string> imageUrls)
    {
        // Placeholder implementation - would integrate with copyright detection services
        await Task.Delay(100); // Simulate API call

        var violations = new List<string>();
        var checkedImages = new List<string>();

        foreach (var imageUrl in imageUrls)
        {
            checkedImages.Add(imageUrl);
            // Simulate copyright check - in reality would use services like TinEye, Google Vision API, etc.
            if (imageUrl.Contains("shutterstock") || imageUrl.Contains("getty"))
            {
                violations.Add($"Potential copyright violation detected in image: {imageUrl}");
            }
        }

        return new ImageCopyrightResult
        {
            HasViolations = violations.Any(),
            Violations = violations,
            CheckedImages = checkedImages,
            CheckedAt = DateTime.UtcNow
        };
    }

    private async Task<TextCopyrightResult> CheckTextCopyright(string textContent)
    {
        // Placeholder implementation - would integrate with plagiarism detection
        await Task.Delay(50);

        var violations = new List<string>();

        // Simple check for common copyrighted phrases
        var copyrightedPhrases = new[] { "copyrighted material", "all rights reserved", "© 2024" };
        foreach (var phrase in copyrightedPhrases)
        {
            if (textContent.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add($"Potential copyrighted text detected: '{phrase}'");
            }
        }

        return new TextCopyrightResult
        {
            HasViolations = violations.Any(),
            Violations = violations,
            TextLength = textContent.Length,
            CheckedAt = DateTime.UtcNow
        };
    }

    private async Task<RightsVerificationResult> VerifyContentRights(string licenseInfo)
    {
        await Task.Delay(25);

        var isVerified = !string.IsNullOrEmpty(licenseInfo) &&
                        (licenseInfo.Contains("Creative Commons") ||
                         licenseInfo.Contains("Public Domain") ||
                         licenseInfo.Contains("Original Work"));

        return new RightsVerificationResult
        {
            IsVerified = isVerified,
            LicenseType = licenseInfo,
            VerificationNotes = isVerified ? "License verified" : "License information insufficient",
            VerifiedAt = DateTime.UtcNow
        };
    }

    private async Task StoreCopyrightViolation(CopyrightViolation violation)
    {
        // Store copyright violation in audit log until dedicated table is added
        var auditEntry = new
        {
            Type = "CopyrightViolation",
            ResourceId = violation.ResourceId,
            ViolationType = "Combined",
            ImageViolations = violation.ImageViolations,
            TextViolations = violation.TextViolations,
            DetectedAt = violation.DetectedAt,
            Status = violation.Status
        };

        _logger.LogInformation("Copyright violation stored for resource {ResourceId}: {@Violation}",
            violation.ResourceId, auditEntry);

        // When CopyrightViolations table is added to DbContext:
        // _context.CopyrightViolations.Add(violation);
        // await _context.SaveChangesAsync();

        await Task.CompletedTask;
    }

    private async Task NotifyUserOfCopyrightConcern(Guid userId, Resource resource, CopyrightViolation violation)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            var message = $@"A copyright concern has been raised regarding your resource '{resource.Title}'.

Image Violations: {string.Join(", ", violation.ImageViolations)}
Text Violations: {string.Join(", ", violation.TextViolations)}

Please review and address this concern. The resource has been temporarily suspended pending resolution.

For more information, visit your seller dashboard.";

            await _emailService.SendSellerNotificationAsync(
                user.Email,
                user.FirstName,
                "CopyrightConcern",
                message);
        }

        _logger.LogInformation("Copyright concern notification sent to user {UserId} for resource {ResourceId}",
            userId, resource.ResourceId);
    }

    private async Task<List<string>> GetAlternativeContent(Guid resourceId)
    {
        await Task.CompletedTask;
        return new List<string>
        {
            "Consider using Creative Commons licensed images from Unsplash",
            "Create original illustrations or diagrams",
            "Use public domain resources from government websites",
            "Purchase royalty-free stock images from licensed providers"
        };
    }

    private async Task<CsvValidationResult> ValidateCsvMetadata(IFormFile csvFile)
    {
        var result = new CsvValidationResult();

        try
        {
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine))
            {
                result.Errors.Add("CSV file is empty");
                return result;
            }

            var expectedHeaders = new[] { "Title", "Description", "ResourceType", "SkillAreas", "GradeLevels", "Languages", "FileName" };
            var headers = headerLine.Split(',');

            foreach (var expectedHeader in expectedHeaders)
            {
                if (!headers.Contains(expectedHeader))
                {
                    result.Errors.Add($"Missing required header: {expectedHeader}");
                }
            }

            var records = new List<Dictionary<string, string>>();
            var lineNumber = 1;

            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(',');
                if (values.Length != headers.Length)
                {
                    result.Errors.Add($"Line {lineNumber}: Column count mismatch");
                    continue;
                }

                var record = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    record[headers[i]] = values[i].Trim('"');
                }

                // Validate required fields
                if (string.IsNullOrEmpty(record.GetValueOrDefault("Title")))
                {
                    result.Errors.Add($"Line {lineNumber}: Title is required");
                }
                if (string.IsNullOrEmpty(record.GetValueOrDefault("FileName")))
                {
                    result.Errors.Add($"Line {lineNumber}: FileName is required");
                }

                records.Add(record);
            }

            result.Metadata = records;
            result.RecordCount = records.Count;
            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading CSV file: {ex.Message}");
        }

        return result;
    }

    private ZipValidationResult ValidateZipFile(IFormFile zipFile)
    {
        var result = new ZipValidationResult();

        try
        {
            using var stream = zipFile.OpenReadStream();
            using var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);

            var files = new List<string>();
            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith('/')) // Skip directories
                {
                    files.Add(entry.Name);

                    // Validate file size (max 50MB per file)
                    if (entry.Length > 50 * 1024 * 1024)
                    {
                        result.Errors.Add($"File {entry.Name} exceeds 50MB limit");
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".mp4", ".mp3" };
                    var extension = Path.GetExtension(entry.Name).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        result.Errors.Add($"File {entry.Name} has unsupported extension: {extension}");
                    }
                }
            }

            result.Files = files;
            result.FileCount = files.Count;
            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading ZIP file: {ex.Message}");
        }

        return result;
    }

    private async Task StartBulkUploadProcessing(BulkUploadJob job, List<Dictionary<string, string>> metadata, List<string> files)
    {
        // In a real implementation, this would be processed in a background service
        // For now, simulate the process starting
        await Task.CompletedTask;
        _logger.LogInformation("Started background processing for bulk upload job {JobId}", job.JobId);
    }

    private async Task<BulkUploadJob?> GetBulkUploadJob(Guid jobId)
    {
        // Placeholder - would retrieve from database
        await Task.CompletedTask;
        return new BulkUploadJob
        {
            JobId = jobId,
            Status = "Completed",
            TotalFiles = 50,
            ProcessedFiles = 50,
            SuccessCount = 47,
            ErrorCount = 3,
            CompletedAt = DateTime.UtcNow.AddMinutes(-5)
        };
    }

    private async Task<List<string>> GetJobErrors(Guid jobId)
    {
        await Task.CompletedTask;
        return new List<string>
        {
            "File 'worksheet_23.pdf' failed: Unsupported format",
            "File 'activity_45.docx' failed: Copyright validation failed",
            "File 'video_12.mp4' failed: File corrupted"
        };
    }

    private async Task<List<Resource>> GetVersionsForResource(Guid resourceId)
    {
        var versions = new List<Resource>();
        var currentResource = await _resourceRepository.GetByIdAsync(resourceId);

        if (currentResource == null) return versions;

        // Find the root version (no previous version)
        while (currentResource.PreviousVersionId.HasValue)
        {
            currentResource = await _resourceRepository.GetByIdAsync(currentResource.PreviousVersionId.Value);
            if (currentResource == null) break;
        }

        // Collect all versions starting from root
        if (currentResource != null)
        {
            versions.Add(currentResource);
            await CollectSubsequentVersions(currentResource.ResourceId, versions);
        }

        return versions.OrderBy(v => v.CreatedAt).ToList();
    }

    private async Task CollectSubsequentVersions(Guid currentVersionId, List<Resource> versions)
    {
        var subsequentVersions = await _resourceRepository.GetResourcesAsync(r => r.PreviousVersionId == currentVersionId);
        foreach (var version in subsequentVersions)
        {
            versions.Add(version);
            await CollectSubsequentVersions(version.ResourceId, versions);
        }
    }

    private ResourceVersionDto CreateVersionDto(Resource resource)
    {
        return new ResourceVersionDto
        {
            ResourceId = resource.ResourceId,
            Version = resource.Version ?? "1.0",
            Title = resource.Title,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
            IsSuperseded = resource.IsSuperseded,
            SupersededAt = resource.SupersededAt,
            IsLatest = !resource.IsSuperseded && resource.LatestVersionId == resource.ResourceId,
            ChangeLog = "Version update", // Could be enhanced to store actual change logs
            ReviewStatus = resource.ClinicalReviewStatus.ToString()
        };
    }

    private async Task NotifyUsersOfVersionUpdate(Guid oldVersionId, Guid newVersionId)
    {
        // In a real implementation, this would:
        // 1. Find all users who downloaded the old version
        // 2. Send them notifications about the new version
        // 3. Potentially provide automatic updates or download links

        _logger.LogInformation("Sending version update notifications for resource upgrade {OldId} -> {NewId}",
            oldVersionId, newVersionId);

        // Placeholder for notification logic
        await Task.CompletedTask;
    }


    /// <summary>
    /// Validate content metadata quality according to platform standards
    /// </summary>
    [HttpPost("validate-metadata")]
    public async Task<ActionResult<MetadataValidationResponse>> ValidateMetadata([FromBody] MetadataValidationRequest request)
    {
        try
        {
            var response = new MetadataValidationResponse
            {
                ResourceId = request.ResourceId,
                OverallScore = 0,
                IsValid = true
            };

            // Validate Title
            var titleValidation = ValidateTitleField(request.Title);
            response.FieldValidations.Add(titleValidation);

            // Validate Description
            var descriptionValidation = ValidateDescriptionField(request.Description);
            response.FieldValidations.Add(descriptionValidation);

            // Validate Skill Areas
            var skillAreasValidation = await ValidateSkillAreasField(request.SkillAreas);
            response.FieldValidations.Add(skillAreasValidation);

            // Validate Grade Levels
            var gradeLevelsValidation = ValidateGradeLevelsField(request.GradeLevels);
            response.FieldValidations.Add(gradeLevelsValidation);

            // Validate Evidence Level
            var evidenceLevelValidation = ValidateEvidenceLevelField(request.EvidenceLevel, request.EvidenceJustification);
            response.FieldValidations.Add(evidenceLevelValidation);

            // Validate Languages
            var languagesValidation = ValidateLanguagesField(request.Languages);
            response.FieldValidations.Add(languagesValidation);

            // Calculate overall quality score (average of all field scores)
            response.OverallScore = response.FieldValidations.Average(f => f.QualityScore);
            response.IsValid = response.FieldValidations.All(f => f.IsValid);

            // Generate improvement suggestions for fields scoring below 80%
            response.ImprovementSuggestions = response.FieldValidations
                .Where(f => f.QualityScore < 80)
                .SelectMany(f => f.Issues)
                .ToList();

            _logger.LogInformation("Metadata validation completed for resource {ResourceId} with overall score {Score}",
                request.ResourceId, response.OverallScore);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating metadata for resource {ResourceId}", request.ResourceId);
            return StatusCode(500, new { error = "An error occurred while validating metadata" });
        }
    }

    private FieldValidationResult ValidateTitleField(string title)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Title",
            IsValid = true,
            QualityScore = 100
        };

        if (string.IsNullOrWhiteSpace(title))
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("Title is required");
            return result;
        }

        if (title.Length > 100)
        {
            result.IsValid = false;
            result.QualityScore -= 30;
            result.Issues.Add("Title must be under 100 characters");
        }

        if (title.Length < 10)
        {
            result.QualityScore -= 20;
            result.Issues.Add("Title should be more descriptive (at least 10 characters)");
        }

        // Check for descriptiveness (not just generic words)
        var genericWords = new[] { "worksheet", "activity", "exercise", "practice", "resource" };
        if (genericWords.Any(word => title.ToLower().Contains(word)) && title.Split(' ').Length < 4)
        {
            result.QualityScore -= 15;
            result.Issues.Add("Title should be more specific about the skill or content area");
        }

        return result;
    }

    private FieldValidationResult ValidateDescriptionField(string description)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Description",
            IsValid = true,
            QualityScore = 100
        };

        if (string.IsNullOrWhiteSpace(description))
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("Description is required");
            return result;
        }

        if (description.Length < 50)
        {
            result.IsValid = false;
            result.QualityScore -= 40;
            result.Issues.Add("Description must be at least 50 characters");
        }

        if (description.Length > 500)
        {
            result.IsValid = false;
            result.QualityScore -= 30;
            result.Issues.Add("Description must be under 500 characters");
        }

        // Check for completeness indicators
        var qualityIndicators = new[] { "age", "skill", "goal", "outcome", "instruction" };
        var indicatorCount = qualityIndicators.Count(indicator =>
            description.ToLower().Contains(indicator));

        if (indicatorCount < 2)
        {
            result.QualityScore -= 20;
            result.Issues.Add("Description should include more details about age range, skills addressed, or learning goals");
        }

        return result;
    }

    private async Task<FieldValidationResult> ValidateSkillAreasField(List<string> skillAreas)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Skill Areas",
            IsValid = true,
            QualityScore = 100
        };

        if (skillAreas == null || !skillAreas.Any())
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("At least one skill area must be specified");
            return result;
        }

        // In a real implementation, this would check against approved taxonomy
        var approvedSkillAreas = new[]
        {
            "Fine Motor", "Gross Motor", "Speech", "Language", "Articulation",
            "Sensory Integration", "Bilateral Coordination", "Visual Perception",
            "Cognitive", "Social Skills", "Handwriting", "Self-Care", "AAC",
            "Feeding", "Oral Motor", "Phonological Awareness"
        };

        var invalidAreas = skillAreas.Where(area =>
            !approvedSkillAreas.Any(approved =>
                string.Equals(approved, area, StringComparison.OrdinalIgnoreCase))).ToList();

        if (invalidAreas.Any())
        {
            result.QualityScore -= 25 * Math.Min(invalidAreas.Count, 4);
            result.Issues.Add($"Invalid skill areas: {string.Join(", ", invalidAreas)}. Must use approved taxonomy.");
        }

        if (skillAreas.Count > 5)
        {
            result.QualityScore -= 10;
            result.Issues.Add("Consider limiting to 5 or fewer skill areas for better categorization");
        }

        await Task.CompletedTask;
        return result;
    }

    private FieldValidationResult ValidateGradeLevelsField(List<string> gradeLevels)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Grade Levels",
            IsValid = true,
            QualityScore = 100
        };

        if (gradeLevels == null || !gradeLevels.Any())
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("At least one grade level must be specified");
            return result;
        }

        var validGradeLevels = new[]
        {
            "Birth-3", "PreK", "Kindergarten", "1st Grade", "2nd Grade", "3rd Grade",
            "4th Grade", "5th Grade", "6th Grade", "7th Grade", "8th Grade",
            "9th Grade", "10th Grade", "11th Grade", "12th Grade", "Adult"
        };

        var invalidLevels = gradeLevels.Where(level =>
            !validGradeLevels.Any(valid =>
                string.Equals(valid, level, StringComparison.OrdinalIgnoreCase))).ToList();

        if (invalidLevels.Any())
        {
            result.IsValid = false;
            result.QualityScore -= 40;
            result.Issues.Add($"Invalid grade levels: {string.Join(", ", invalidLevels)}");
        }

        return result;
    }

    private FieldValidationResult ValidateEvidenceLevelField(int evidenceLevel, string? evidenceJustification)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Evidence Level",
            IsValid = true,
            QualityScore = 100
        };

        if (evidenceLevel < 1 || evidenceLevel > 5)
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("Evidence level must be between 1 and 5");
            return result;
        }

        // For evidence levels 4-5, justification should be provided
        if (evidenceLevel >= 4 && string.IsNullOrWhiteSpace(evidenceJustification))
        {
            result.QualityScore -= 30;
            result.Issues.Add("Evidence levels 4-5 should include justification or research citations");
        }

        if (evidenceLevel >= 4 && !string.IsNullOrWhiteSpace(evidenceJustification) && evidenceJustification.Length < 50)
        {
            result.QualityScore -= 15;
            result.Issues.Add("Evidence justification should provide more detail for high evidence levels");
        }

        return result;
    }

    private FieldValidationResult ValidateLanguagesField(List<string> languages)
    {
        var result = new FieldValidationResult
        {
            FieldName = "Languages",
            IsValid = true,
            QualityScore = 100
        };

        if (languages == null || !languages.Any())
        {
            result.IsValid = false;
            result.QualityScore = 0;
            result.Issues.Add("At least one language must be specified");
            return result;
        }

        // Check for valid ISO language codes (simplified validation)
        var validLanguageCodes = new[]
        {
            "en", "es", "fr", "de", "it", "pt", "ru", "zh", "ja", "ko", "ar", "hi"
        };

        var validLanguageNames = new[]
        {
            "English", "Spanish", "French", "German", "Italian", "Portuguese",
            "Russian", "Chinese", "Japanese", "Korean", "Arabic", "Hindi"
        };

        var invalidLanguages = languages.Where(lang =>
            !validLanguageCodes.Any(code => string.Equals(code, lang, StringComparison.OrdinalIgnoreCase)) &&
            !validLanguageNames.Any(name => string.Equals(name, lang, StringComparison.OrdinalIgnoreCase))).ToList();

        if (invalidLanguages.Any())
        {
            result.QualityScore -= 25;
            result.Issues.Add($"Invalid language codes/names: {string.Join(", ", invalidLanguages)}. Use ISO codes or standard language names.");
        }

        return result;
    }

    private int? ParseGradeLevel(string gradeLevel)
    {
        return gradeLevel.ToLower() switch
        {
            "prek" or "pre-k" or "preschool" => -1,
            "kindergarten" or "k" => 0,
            "1st" or "1" or "first" => 1,
            "2nd" or "2" or "second" => 2,
            "3rd" or "3" or "third" => 3,
            "4th" or "4" or "fourth" => 4,
            "5th" or "5" or "fifth" => 5,
            "6th" or "6" or "sixth" => 6,
            "7th" or "7" or "seventh" => 7,
            "8th" or "8" or "eighth" => 8,
            "9th" or "9" or "ninth" => 9,
            "10th" or "10" or "tenth" => 10,
            "11th" or "11" or "eleventh" => 11,
            "12th" or "12" or "twelfth" => 12,
            _ => int.TryParse(gradeLevel, out var grade) ? grade : null
        };
    }
}

// DTOs for content management
public class ContentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public List<string> SkillAreas { get; set; } = new();
    public List<string> GradeLevels { get; set; } = new();
    public int EvidenceLevel { get; set; } = 1;
    public List<string> Languages { get; set; } = new();
    public string LicenseType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}

public class ResourceUploadResponse
{
    public Guid ResourceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EstimatedReviewTime { get; set; } = string.Empty;
}

public class RetireResourceRequest
{
    public Guid ResourceId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<Guid>? SuggestedAlternatives { get; set; }
}

public class RetiredResourceDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime RetiredAt { get; set; }
    public string? RetiredReason { get; set; }
    public Guid? RetiredBy { get; set; }
    public List<Guid> SuggestedAlternatives { get; set; } = new();
    public string? OriginalFileUrl { get; set; }
}

public class CopyrightVerificationRequest
{
    public Guid ResourceId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string TextContent { get; set; } = string.Empty;
    public string LicenseInfo { get; set; } = string.Empty;
}

public class CopyrightVerificationResponse
{
    public Guid ResourceId { get; set; }
    public bool HasViolations { get; set; }
    public ImageCopyrightResult ImageResults { get; set; } = new();
    public TextCopyrightResult TextResults { get; set; } = new();
    public RightsVerificationResult RightsVerification { get; set; } = new();
    public string NextSteps { get; set; } = string.Empty;
    public List<string> SuggestedAlternatives { get; set; } = new();
}

public class ImageCopyrightResult
{
    public bool HasViolations { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> CheckedImages { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class TextCopyrightResult
{
    public bool HasViolations { get; set; }
    public List<string> Violations { get; set; } = new();
    public int TextLength { get; set; }
    public DateTime CheckedAt { get; set; }
}

public class RightsVerificationResult
{
    public bool IsVerified { get; set; }
    public string LicenseType { get; set; } = string.Empty;
    public string VerificationNotes { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
}

public class CopyrightViolation
{
    public Guid ViolationId { get; set; } = Guid.NewGuid();
    public Guid ResourceId { get; set; }
    public DateTime DetectedAt { get; set; }
    public List<string> ImageViolations { get; set; } = new();
    public List<string> TextViolations { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BulkUploadRequest
{
    public IFormFile MetadataCsv { get; set; } = null!;
    public IFormFile ResourcesZip { get; set; } = null!;
}

public class BulkUploadResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;
    public string TrackingUrl { get; set; } = string.Empty;
}

public class BulkUploadStatusResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
    public int ProcessedFiles { get; set; }
    public int TotalFiles { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime? CompletedAt { get; set; }
}

public class BulkUploadJob
{
    public Guid JobId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
}

public class CsvValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Dictionary<string, string>> Metadata { get; set; } = new();
    public int RecordCount { get; set; }
}

public class ZipValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Files { get; set; } = new();
    public int FileCount { get; set; }
}

public class ResourceReviewDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType ResourceType { get; set; }
    public Dictionary<string, object> SkillAreas { get; set; } = new();
    public List<int> GradeLevels { get; set; } = new();
    public List<string> LanguagesAvailable { get; set; } = new();
    public GenerationMethod GenerationMethod { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? LastUpdated { get; set; }
    public ClinicalReviewStatus Status { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? FileUrl { get; set; }
    public string? PreviewUrl { get; set; }
}

public class AssignReviewerRequest
{
    public Guid ResourceId { get; set; }
    public Guid ReviewerId { get; set; }
    public string? Notes { get; set; }
}

public class SubmitReviewRequest
{
    public Guid ResourceId { get; set; }
    public int ClinicalAccuracy { get; set; } // 1-5 scale
    public int AgeAppropriateness { get; set; } // 1-5 scale
    public int SafetyCompliance { get; set; } // 1-5 scale
    public int TherapeuticValue { get; set; } // 1-5 scale
    public bool OverallApproval { get; set; }
    public string? Comments { get; set; }
}

