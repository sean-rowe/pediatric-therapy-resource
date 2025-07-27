using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Policy = "TherapistOnly")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> GetStudents()
    {
        throw new NotImplementedException("GetStudents endpoint not yet implemented");
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDetailDto>> GetStudent(Guid id)
    {
        throw new NotImplementedException("GetStudent endpoint not yet implemented");
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        throw new NotImplementedException("CreateStudent endpoint not yet implemented");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request)
    {
        throw new NotImplementedException("UpdateStudent endpoint not yet implemented");
    }

    [HttpPost("{id}/goals")]
    public async Task<ActionResult<StudentGoalDto>> AddGoal(Guid id, [FromBody] CreateStudentGoalRequest request)
    {
        throw new NotImplementedException("AddGoal endpoint not yet implemented");
    }

    [HttpPost("{id}/discharge")]
    public async Task<IActionResult> DischargeStudent(Guid id, [FromBody] DischargeStudentRequest request)
    {
        throw new NotImplementedException("DischargeStudent endpoint not yet implemented");
    }

    private string GenerateAccessCode()
    {
        throw new NotImplementedException("GenerateAccessCode not yet implemented");
    }
}

// DTOs
public class CreateStudentRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    public int? GradeLevel { get; set; }
    
    [EmailAddress]
    public string? ParentEmail { get; set; }
}

public class UpdateStudentRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? GradeLevel { get; set; }
    [EmailAddress]
    public string? ParentEmail { get; set; }
}

public class StudentDetailDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<StudentGoalDto> Goals { get; set; } = new();
    public List<SessionSummaryDto> RecentSessions { get; set; } = new();
}

public class StudentGoalDto
{
    public Guid GoalId { get; set; }
    public string GoalText { get; set; } = string.Empty;
    public GoalStatus Status { get; set; }
    public decimal Progress { get; set; }
    public DateTime? TargetDate { get; set; }
}

public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public int Duration { get; set; }
    public string SessionType { get; set; } = string.Empty;
}

public class CreateStudentGoalRequest
{
    [Required]
    public string GoalText { get; set; } = string.Empty;
    public string GoalArea { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
}

public class DischargeStudentRequest
{
    public string? Reason { get; set; }
}

