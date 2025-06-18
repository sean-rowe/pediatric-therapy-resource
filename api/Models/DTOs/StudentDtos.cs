namespace TherapyDocs.Api.Models.DTOs;

public class CreateStudentRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid TherapistId { get; set; }
    public string? StudentId { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeLevel { get; set; }
    public string? PrimaryDiagnosis { get; set; }
    public string? SecondaryDiagnoses { get; set; }
    public string? Goals { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStudentRequest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? StudentId { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeLevel { get; set; }
    public string? PrimaryDiagnosis { get; set; }
    public string? SecondaryDiagnoses { get; set; }
    public string? Goals { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class StudentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public StudentDto? Student { get; set; }
}

public class StudentsListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<StudentDto> Students { get; set; } = new();
    public int TotalCount { get; set; }
}

public class StudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid TherapistId { get; set; }
    public string? StudentId { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeLevel { get; set; }
    public string? PrimaryDiagnosis { get; set; }
    public string? SecondaryDiagnoses { get; set; }
    public string? Goals { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? Therapist { get; set; }
}

public class StudentSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; }
}