using System;
using System.Collections.Generic;

namespace UPTRMS.Api.Models.DTOs;

public class QualityMetricsResponse
{
    public double AverageReviewTimeDays { get; set; }
    public double ApprovalRatePercent { get; set; }
    public int ResourcesInQueue { get; set; }
    public double AverageReviewerWorkload { get; set; }
    public Dictionary<string, int> ReviewerWorkloadDistribution { get; set; } = new();
    public QualityTrendsDto QualityTrends { get; set; } = new();
    public List<BottleneckDto> Bottlenecks { get; set; } = new();
    public List<QualityAlertDto> Alerts { get; set; } = new();
    public QualityTargetsDto Targets { get; set; } = new();
}

public class QualityTrendsDto
{
    public double LastWeekApprovalRate { get; set; }
    public double LastMonthApprovalRate { get; set; }
    public double ReviewTimeImprovement { get; set; } // Negative = improvement
    public int QueueSizeChange { get; set; } // Positive = increase
}

public class BottleneckDto
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class QualityAlertDto
{
    public string Type { get; set; } = string.Empty; // Info, Warning, Critical
    public string Message { get; set; } = string.Empty;
    public string ActionRequired { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class QualityTargetsDto
{
    public double TargetReviewTimeDays { get; set; }
    public double TargetApprovalRate { get; set; }
    public int TargetMaxQueueSize { get; set; }
    public int TargetMaxReviewerWorkload { get; set; }
}