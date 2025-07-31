using System;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.Domain;

/// <summary>
/// Category for organizing resources
/// </summary>
public class Category
{
    [Key]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [MaxLength(50)]
    public string? IconName { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<ResourceCategory> Resources { get; set; } = new List<ResourceCategory>();
}

/// <summary>
/// Junction table for resource-category many-to-many relationship
/// </summary>
public class ResourceCategory
{
    [Key]
    public Guid ResourceCategoryId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ResourceId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public bool IsPrimary { get; set; } = false;

    // Navigation properties
    public virtual Resource Resource { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}