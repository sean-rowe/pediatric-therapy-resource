using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.DTOs;

public class CategoryCreationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int? DisplayOrder { get; set; }

    public List<SubcategoryRequest>? Subcategories { get; set; }
}

public class SubcategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? DisplayOrder { get; set; }
}

public class CategoryCreationResponse
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int SubcategoriesCreated { get; set; }
    public List<CategorySummaryDto> Subcategories { get; set; } = new();
    public bool IsAvailableForTagging { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CategorySummaryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class TaxonomyResponse
{
    public int TotalCategories { get; set; }
    public int TopLevelCategories { get; set; }
    public List<TaxonomyNodeDto> Taxonomy { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class TaxonomyNodeDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
    public List<TaxonomyNodeDto> Children { get; set; } = new();
}

public class CategoryReviewRequest
{
    public int? MaxResourcesToReview { get; set; } = 100;
    public bool IncludeRecentlyAdded { get; set; } = true;
    public List<string>? SkillAreasToInclude { get; set; }
}

public class CategoryReviewResponse
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ResourcesReviewed { get; set; }
    public int SuggestionsGenerated { get; set; }
    public List<CategorySuggestionDto> Suggestions { get; set; } = new();
    public DateTime ReviewedAt { get; set; }
}

public class CategorySuggestionDto
{
    public Guid ResourceId { get; set; }
    public string ResourceTitle { get; set; } = string.Empty;
    public string SuggestionReason { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public bool AutoAssignRecommended { get; set; }
}

public class TaxonomyValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class CategoryDto
{
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DisplayOrder { get; set; }
}