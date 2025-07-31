using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

public class ReviewAssignment
{
    [Key]
    public Guid ReviewAssignmentId { get; set; }
    
    // Alias for compatibility
    public Guid AssignmentId 
    { 
        get => ReviewAssignmentId; 
        set => ReviewAssignmentId = value; 
    }
    
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
    public Guid AssignedByUserId { get; set; }
    
    [Required]
    public DateTime AssignedAt { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    [Required]
    public ReviewAssignmentStatus Status { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual User ReviewerUser { get; set; } = null!;
    public virtual User AssignedByUser { get; set; } = null!;
    public virtual ReviewEvaluation? ReviewEvaluation { get; set; }
}

public enum ReviewAssignmentStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}