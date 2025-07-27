using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Models.DTOs;

public class StudentDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}