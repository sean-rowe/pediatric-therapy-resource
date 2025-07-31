using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.DTOs;

public class VersionUpdateRequest
{
    [Required]
    public Guid OriginalResourceId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Version { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    public List<string>? Languages { get; set; }

    public int? EvidenceLevel { get; set; }
}

public class VersionUpdateResponse
{
    public Guid NewVersionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EstimatedReviewTime { get; set; } = string.Empty;
}

public class VersionApprovalRequest
{
    [Required]
    public Guid NewVersionId { get; set; }

    public string? ApprovalNotes { get; set; }
}

public class VersionApprovalResponse
{
    public Guid ApprovedVersionId { get; set; }
    public Guid SupersededVersionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool NotificationsSent { get; set; }
}

public class ResourceVersionDto
{
    public Guid ResourceId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsSuperseded { get; set; }
    public DateTime? SupersededAt { get; set; }
    public bool IsLatest { get; set; }
    public string ChangeLog { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
}