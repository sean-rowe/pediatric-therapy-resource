using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.DTOs;

public class MetadataValidationRequest
{
    public Guid ResourceId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public List<string> SkillAreas { get; set; } = new();

    [Required]
    public List<string> GradeLevels { get; set; } = new();

    [Range(1, 5)]
    public int EvidenceLevel { get; set; }

    public string? EvidenceJustification { get; set; }

    [Required]
    public List<string> Languages { get; set; } = new();
}

public class MetadataValidationResponse
{
    public Guid ResourceId { get; set; }
    public bool IsValid { get; set; }
    public double OverallScore { get; set; }
    public List<FieldValidationResult> FieldValidations { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

public class FieldValidationResult
{
    public string FieldName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public double QualityScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

public class MetadataQualityStandards
{
    public TitleStandards Title { get; set; } = new();
    public DescriptionStandards Description { get; set; } = new();
    public SkillAreaStandards SkillAreas { get; set; } = new();
    public GradeLevelStandards GradeLevels { get; set; } = new();
    public EvidenceLevelStandards EvidenceLevel { get; set; } = new();
    public LanguageStandards Languages { get; set; } = new();
}

public class TitleStandards
{
    public int MinLength { get; set; } = 10;
    public int MaxLength { get; set; } = 100;
    public bool RequireDescriptive { get; set; } = true;
    public List<string> GenericWords { get; set; } = new() { "worksheet", "activity", "exercise", "practice", "resource" };
}

public class DescriptionStandards
{
    public int MinLength { get; set; } = 50;
    public int MaxLength { get; set; } = 500;
    public List<string> RequiredElements { get; set; } = new() { "age", "skill", "goal", "outcome", "instruction" };
    public int MinRequiredElements { get; set; } = 2;
}

public class SkillAreaStandards
{
    public int MaxAllowed { get; set; } = 5;
    public List<string> ApprovedSkillAreas { get; set; } = new()
    {
        "Fine Motor", "Gross Motor", "Speech", "Language", "Articulation",
        "Sensory Integration", "Bilateral Coordination", "Visual Perception",
        "Cognitive", "Social Skills", "Handwriting", "Self-Care", "AAC",
        "Feeding", "Oral Motor", "Phonological Awareness"
    };
}

public class GradeLevelStandards
{
    public List<string> ValidGradeLevels { get; set; } = new()
    {
        "Birth-3", "PreK", "Kindergarten", "1st Grade", "2nd Grade", "3rd Grade",
        "4th Grade", "5th Grade", "6th Grade", "7th Grade", "8th Grade",
        "9th Grade", "10th Grade", "11th Grade", "12th Grade", "Adult"
    };
}

public class EvidenceLevelStandards
{
    public int MinLevel { get; set; } = 1;
    public int MaxLevel { get; set; } = 5;
    public int JustificationRequiredLevel { get; set; } = 4;
    public int MinJustificationLength { get; set; } = 50;
}

public class LanguageStandards
{
    public List<string> ValidLanguageCodes { get; set; } = new()
    {
        "en", "es", "fr", "de", "it", "pt", "ru", "zh", "ja", "ko", "ar", "hi"
    };
    public List<string> ValidLanguageNames { get; set; } = new()
    {
        "English", "Spanish", "French", "German", "Italian", "Portuguese",
        "Russian", "Chinese", "Japanese", "Korean", "Arabic", "Hindi"
    };
}