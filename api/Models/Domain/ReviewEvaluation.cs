using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

public class ReviewEvaluation
{
    [Key]
    public Guid ReviewEvaluationId { get; set; }
    
    // Alias for compatibility
    public Guid EvaluationId 
    { 
        get => ReviewEvaluationId; 
        set => ReviewEvaluationId = value; 
    }
    
    [Required]
    public Guid ReviewAssignmentId { get; set; }
    
    [Required]
    public Guid ResourceId { get; set; }
    
    [Required]
    public Guid ReviewerUserId { get; set; }
    
    // Alias for compatibility
    public Guid ReviewerId 
    { 
        get => ReviewerUserId; 
        set => ReviewerUserId = value; 
    }
    
    [Required]
    public ReviewApprovalStatus ApprovalStatus { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int ClinicalAccuracy { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int AgeAppropriateness { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int EvidenceLevel { get; set; }
    
    [Required]
    public string Comments { get; set; } = string.Empty;
    
    public string? RequiredChanges { get; set; }
    
    [Required]
    public DateTime ReviewedAt { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual ReviewAssignment ReviewAssignment { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual User ReviewerUser { get; set; } = null!;
}

public enum ReviewApprovalStatus
{
    Rejected = 0,
    ApprovedWithChanges = 1,
    Approved = 2
}