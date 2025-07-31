namespace UPTRMS.Api.Models.DTOs;

public class ReviewStatisticsDto
{
    public OverallStatistics Overall { get; set; } = new();
    public ApprovalStatistics Approval { get; set; } = new();
    public List<TopReviewer> TopReviewers { get; set; } = new();
    public List<MonthlyReviewStats> MonthlyStats { get; set; } = new();
    
    // Backward compatibility properties
    public int ResourcesInQueue => Overall?.PendingReviews ?? 0;
    public double AverageReviewTimeHours => Overall?.AvgReviewTimeHours ?? 0;
    public double ApprovalRate => Approval != null && (Approval.ApprovedCount + Approval.ApprovedWithChangesCount + Approval.RejectedCount) > 0 
        ? (double)(Approval.ApprovedCount + Approval.ApprovedWithChangesCount) / (Approval.ApprovedCount + Approval.ApprovedWithChangesCount + Approval.RejectedCount) * 100 
        : 0;
    public Dictionary<string, int> ReviewerWorkload { get; set; } = new();
}

public class OverallStatistics
{
    public int TotalAssignments { get; set; }
    public int CompletedReviews { get; set; }
    public int PendingReviews { get; set; }
    public int InProgressReviews { get; set; }
    public double? AvgReviewTimeHours { get; set; }
}

public class ApprovalStatistics
{
    public int ApprovedCount { get; set; }
    public int ApprovedWithChangesCount { get; set; }
    public int RejectedCount { get; set; }
    public double AvgClinicalAccuracy { get; set; }
    public double AvgAgeAppropriateness { get; set; }
    public double AvgEvidenceLevel { get; set; }
}

public class TopReviewer
{
    public Guid UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewsCompleted { get; set; }
    public double? AvgReviewTimeHours { get; set; }
}

public class MonthlyReviewStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int ReviewCount { get; set; }
    public double AvgEvidenceLevel { get; set; }
}