using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Models.DTOs;

public class ReviewEvaluationDto
{
    public Guid EvaluationId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid ReviewerId { get; set; }
    public int ClinicalAccuracy { get; set; }
    public int AgeAppropriateness { get; set; }
    public int EvidenceLevel { get; set; }
    public ReviewApprovalStatus ApprovalStatus { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string? RequiredChanges { get; set; }
    public DateTime ReviewedAt { get; set; }
}