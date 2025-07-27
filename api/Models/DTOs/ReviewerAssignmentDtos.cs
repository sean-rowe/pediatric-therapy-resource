using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.DTOs;

public class AutoAssignmentRequest
{
    public int? MaxResourcesPerRun { get; set; } = 20;
    public bool PrioritizeUrgent { get; set; } = true;
    public bool BalanceWorkload { get; set; } = true;
    public bool MatchSpecialty { get; set; } = true;
}

public class ReviewerAssignmentResponse
{
    public int AssignmentsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AssignmentDetailDto> AssignmentDetails { get; set; } = new();
    public WorkloadSummaryDto WorkloadSummary { get; set; } = new();
}

public class AssignmentDetailDto
{
    public Guid ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerSpecialty { get; set; } = string.Empty;
    public ReviewPriority Priority { get; set; }
    public string AssignmentReason { get; set; } = string.Empty;
    public DateTime EstimatedCompletionDate { get; set; }
}

public class IntelligentAssignmentDto
{
    public Guid ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerSpecialty { get; set; } = string.Empty;
    public ReviewPriority Priority { get; set; }
    public string AssignmentReason { get; set; } = string.Empty;
    public double AssignmentScore { get; set; }
}

public class ReviewerProfileDto
{
    public Guid ReviewerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int CurrentWorkload { get; set; }
    public int MaxWorkload { get; set; }
    public bool IsAvailable { get; set; }
}

public class WorkloadSummaryDto
{
    public int TotalReviewers { get; set; }
    public int AvailableReviewers { get; set; }
    public double AverageWorkload { get; set; }
    public List<ReviewerWorkloadDto> ReviewerDetails { get; set; } = new();
}

public class ReviewerWorkloadDto
{
    public Guid ReviewerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int CurrentWorkload { get; set; }
    public int MaxWorkload { get; set; }
    public double CapacityPercent { get; set; }
    public bool IsAvailable { get; set; }
}

public enum ReviewPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}