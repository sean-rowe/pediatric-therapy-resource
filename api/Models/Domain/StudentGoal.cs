using System;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

public class StudentGoal
{
    [Key]
    public Guid GoalId { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid StudentId { get; set; }
    
    [Required]
    public string GoalText { get; set; } = string.Empty;
    
    [Required]
    public string GoalArea { get; set; } = string.Empty;
    
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    
    [Range(0, 100)]
    public decimal Progress { get; set; }
    
    public DateTime? TargetDate { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Student Student { get; set; } = null!;
}

public enum GoalStatus
{
    Active,
    Achieved,
    Modified,
    Discontinued
}