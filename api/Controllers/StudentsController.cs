using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Policy = "TherapistOnly")]
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentRepository studentRepository, ISessionRepository sessionRepository, ILogger<StudentsController> logger)
    {
        _studentRepository = studentRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> GetStudents()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            IEnumerable<Student> students = await _studentRepository.GetByTherapistAsync(userId, includeInactive: false);

            List<StudentDto> studentDtos = students.Select(s => new StudentDto
            {
                StudentId = s.StudentId,
                FirstName = s.FirstNameEncrypted,
                LastName = s.LastNameEncrypted,
                GradeLevel = s.GradeLevel,
                IsActive = s.IsActive,
                AccessCode = s.AccessCode
            }).ToList();

            return Ok(studentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students");
            return StatusCode(500, new { message = "An error occurred while retrieving students" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDetailDto>> GetStudent(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Student? student = await _studentRepository.GetByIdAsync(id);

            if (student == null || student.TherapistId != userId || !student.IsActive)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Get goals
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                studentId: id,
                therapistId: userId);

            // Get recent sessions
            IEnumerable<Session> recentSessions = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                studentId: id,
                skip: 0,
                take: 5);

            StudentDetailDto dto = new StudentDetailDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstNameEncrypted,
                LastName = student.LastNameEncrypted,
                AccessCode = student.AccessCode,
                GradeLevel = student.GradeLevel,
                IsActive = student.IsActive,
                CreatedAt = student.CreatedAt,
                Goals = goals.Select(g => new StudentGoalDto
                {
                    GoalId = g.GoalId,
                    GoalText = g.GoalDescription,
                    Status = g.Status,
                    Progress = g.CurrentProgress,
                    TargetDate = g.TargetDate
                }).ToList(),
                RecentSessions = recentSessions.Select(s => new SessionSummaryDto
                {
                    SessionId = s.SessionId,
                    SessionDate = s.SessionDate,
                    Duration = s.DurationMinutes,
                    SessionType = s.SessionType.ToString()
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student {StudentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the student" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Student student = new Student
            {
                TherapistId = userId,
                FirstNameEncrypted = request.FirstName, // Would be encrypted in production
                LastNameEncrypted = request.LastName, // Would be encrypted in production
                DateOfBirthEncrypted = request.DateOfBirth?.ToString() ?? string.Empty, // Would be encrypted
                GradeLevel = request.GradeLevel?.ToString(),
                ParentEmailEncrypted = request.ParentEmail, // Would be encrypted
                AccessCode = await _studentRepository.GenerateUniqueAccessCodeAsync(),
                IsActive = true
            };

            student = await _studentRepository.AddAsync(student);

            StudentDto dto = new StudentDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstNameEncrypted,
                LastName = student.LastNameEncrypted,
                GradeLevel = student.GradeLevel?.ToString(),
                IsActive = student.IsActive,
                AccessCode = student.AccessCode
            };

            return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return StatusCode(500, new { message = "An error occurred while creating the student" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Student? student = await _studentRepository.GetByIdAsync(id);

            if (student == null || student.TherapistId != userId || !student.IsActive)
            {
                return NotFound(new { message = "Student not found" });
            }

            if (!string.IsNullOrEmpty(request.FirstName))
                student.FirstNameEncrypted = request.FirstName; // Would be encrypted
            if (!string.IsNullOrEmpty(request.LastName))
                student.LastNameEncrypted = request.LastName; // Would be encrypted
            if (request.GradeLevel.HasValue)
                student.GradeLevel = request.GradeLevel.Value.ToString();
            if (!string.IsNullOrEmpty(request.ParentEmail))
                student.ParentEmailEncrypted = request.ParentEmail; // Would be encrypted

            student.UpdatedAt = DateTime.UtcNow;
            await _studentRepository.UpdateAsync(student);

            StudentDto dto = new StudentDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstNameEncrypted,
                LastName = student.LastNameEncrypted,
                GradeLevel = student.GradeLevel?.ToString(),
                IsActive = student.IsActive,
                AccessCode = student.AccessCode
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {StudentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the student" });
        }
    }

    [HttpPost("{id}/goals")]
    public async Task<ActionResult<StudentGoalDto>> AddGoal(Guid id, [FromBody] CreateStudentGoalRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Student? student = await _studentRepository.GetByIdAsync(id);

            if (student == null || student.TherapistId != userId || !student.IsActive)
            {
                return NotFound(new { message = "Student not found" });
            }

            StudentGoal goal = new StudentGoal
            {
                StudentId = id,
                GoalArea = request.GoalArea,
                GoalDescription = request.GoalText,
                Objectives = "", // Add objectives if needed
                Baseline = 0,
                Target = 100,
                CurrentProgress = 0,
                TargetDate = request.TargetDate ?? DateTime.UtcNow.AddMonths(6),
                Frequency = "Weekly",
                Status = GoalStatus.Active
            };

            goal = await _studentRepository.CreateGoalAsync(goal);

            StudentGoalDto dto = new StudentGoalDto
            {
                GoalId = goal.GoalId,
                GoalText = goal.GoalDescription,
                Status = goal.Status,
                Progress = goal.CurrentProgress,
                TargetDate = goal.TargetDate
            };

            return CreatedAtAction(nameof(GoalsController.GetGoal), "Goals", new { id = goal.GoalId }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding goal for student {StudentId}", id);
            return StatusCode(500, new { message = "An error occurred while adding the goal" });
        }
    }

    [HttpPost("{id}/discharge")]
    public async Task<IActionResult> DischargeStudent(Guid id, [FromBody] DischargeStudentRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Student? student = await _studentRepository.GetByIdAsync(id);

            if (student == null || student.TherapistId != userId || !student.IsActive)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Mark student as inactive
            student.IsActive = false;
            student.UpdatedAt = DateTime.UtcNow;
            await _studentRepository.UpdateAsync(student);

            // Get and update all active goals
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                studentId: id,
                therapistId: userId,
                status: GoalStatus.Active);

            foreach (StudentGoal goal in goals)
            {
                goal.Status = GoalStatus.Discontinued;
                goal.UpdatedAt = DateTime.UtcNow;
                await _studentRepository.UpdateGoalAsync(goal);
            }

            // Log the discharge reason
            _logger.LogInformation("Student {StudentId} discharged. Reason: {Reason}", id, request.Reason ?? "No reason provided");

            return Ok(new { message = "Student discharged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discharging student {StudentId}", id);
            return StatusCode(500, new { message = "An error occurred while discharging the student" });
        }
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

public class StudentDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public bool IsActive { get; set; }
    public string AccessCode { get; set; } = string.Empty;
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

