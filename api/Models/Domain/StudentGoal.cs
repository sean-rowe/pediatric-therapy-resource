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
    [MaxLength(200)]
    public string GoalArea { get; set; } = string.Empty;

    [Required]
    public string GoalDescription { get; set; } = string.Empty;
    
    // Alias for backward compatibility
    public string GoalText 
    { 
        get => GoalDescription; 
        set => GoalDescription = value; 
    }

    [Required]
    public string Objectives { get; set; } = string.Empty;

    [Required]
    public int Baseline { get; set; } = 0;

    [Required]
    public int Target { get; set; } = 100;

    [Required]
    public int CurrentProgress { get; set; } = 0;
    
    // Alias for backward compatibility
    [Range(0, 100)]
    public decimal Progress 
    { 
        get => CurrentProgress; 
        set => CurrentProgress = (int)value; 
    }

    [Required]
    public DateTime TargetDate { get; set; } = DateTime.UtcNow.AddMonths(6);

    [Required]
    [MaxLength(100)]
    public string Frequency { get; set; } = "Weekly";

    public GoalStatus Status { get; set; } = GoalStatus.Active;

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