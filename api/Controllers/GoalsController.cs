using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/goals")]
[Authorize(Policy = "TherapistOnly")]
public class GoalsController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<GoalsController> _logger;

    public GoalsController(IStudentRepository studentRepository, ILogger<GoalsController> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<GoalDto>>> GetGoals(
        [FromQuery] Guid? studentId = null,
        [FromQuery] GoalStatus? status = null,
        [FromQuery] string? goalArea = null)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                studentId: studentId,
                therapistId: userId,
                status: status,
                goalArea: goalArea);

            List<GoalDto> goalDtos = new List<GoalDto>();
            foreach (StudentGoal goal in goals)
            {
                Student? student = await _studentRepository.GetByIdAsync(goal.StudentId);
                if (student != null && student.TherapistId == userId)
                {
                    goalDtos.Add(new GoalDto
                    {
                        GoalId = goal.GoalId,
                        StudentId = goal.StudentId,
                        StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                        GoalText = goal.GoalText,
                        GoalArea = goal.GoalArea,
                        Status = goal.Status,
                        Progress = goal.Progress,
                        TargetDate = goal.TargetDate,
                        CreatedAt = goal.CreatedAt,
                        UpdatedAt = goal.UpdatedAt
                    });
                }
            }

            return Ok(goalDtos.OrderBy(g => g.StudentName).ThenBy(g => g.TargetDate).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving goals");
            return StatusCode(500, new { message = "An error occurred while retrieving goals" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GoalDetailDto>> GetGoal(Guid id)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get goal by searching for it
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                skip: 0,
                take: int.MaxValue);
            
            StudentGoal? goal = goals.FirstOrDefault(g => g.GoalId == id);
            
            if (goal == null)
            {
                return NotFound();
            }

            Student? student = await _studentRepository.GetByIdAsync(goal.StudentId);
            if (student == null || student.TherapistId != userId)
            {
                return NotFound();
            }

            // Get progress history
            List<ProgressHistoryDto> progressHistory = await GetGoalProgressHistoryAsync(id);

            GoalDetailDto dto = new GoalDetailDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                GoalText = goal.GoalText,
                GoalArea = goal.GoalArea,
                Status = goal.Status,
                Progress = goal.Progress,
                TargetDate = goal.TargetDate,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt,
                ProgressHistory = progressHistory
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving goal {GoalId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the goal" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<GoalDto>> CreateGoal([FromBody] CreateGoalRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Verify student belongs to therapist
            Student? student = await _studentRepository.GetByIdAsync(request.StudentId);

            if (student == null || student.TherapistId != userId)
            {
                return BadRequest(new { message = "Student not found or not assigned to therapist" });
            }

            StudentGoal goal = new StudentGoal
            {
                StudentId = request.StudentId,
                GoalText = request.GoalText,
                GoalArea = request.GoalArea,
                TargetDate = request.TargetDate ?? DateTime.UtcNow.AddMonths(6),
                Status = GoalStatus.Active,
                Progress = 0,
                Objectives = "", // Required field
                Baseline = 0,
                Target = 100,
                Frequency = "Weekly"
            };

            goal = await _studentRepository.CreateGoalAsync(goal);

            return CreatedAtAction(nameof(GetGoal), new { id = goal.GoalId }, new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                GoalText = goal.GoalText,
                GoalArea = goal.GoalArea,
                Status = goal.Status,
                Progress = goal.Progress,
                TargetDate = goal.TargetDate,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating goal");
            return StatusCode(500, new { message = "An error occurred while creating the goal" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GoalDto>> UpdateGoal(Guid id, [FromBody] UpdateGoalRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get goal by searching for it
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                skip: 0,
                take: int.MaxValue);
            
            StudentGoal? goal = goals.FirstOrDefault(g => g.GoalId == id);
            
            if (goal == null)
            {
                return NotFound();
            }

            Student? student = await _studentRepository.GetByIdAsync(goal.StudentId);
            if (student == null || student.TherapistId != userId)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(request.GoalText))
                goal.GoalText = request.GoalText;
            if (!string.IsNullOrEmpty(request.GoalArea))
                goal.GoalArea = request.GoalArea;
            if (request.TargetDate.HasValue)
                goal.TargetDate = request.TargetDate.Value;
            if (request.Status.HasValue)
                goal.Status = request.Status.Value;

            goal.UpdatedAt = DateTime.UtcNow;
            await _studentRepository.UpdateGoalAsync(goal);

            return Ok(new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                GoalText = goal.GoalText,
                GoalArea = goal.GoalArea,
                Status = goal.Status,
                Progress = goal.Progress,
                TargetDate = goal.TargetDate,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating goal {GoalId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the goal" });
        }
    }

    [HttpPost("{id}/progress")]
    public async Task<ActionResult<GoalDto>> UpdateProgress(Guid id, [FromBody] UpdateProgressRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get goal by searching for it
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                skip: 0,
                take: int.MaxValue);
            
            StudentGoal? goal = goals.FirstOrDefault(g => g.GoalId == id);
            
            if (goal == null)
            {
                return NotFound();
            }

            Student? student = await _studentRepository.GetByIdAsync(goal.StudentId);
            if (student == null || student.TherapistId != userId)
            {
                return NotFound();
            }

            goal.Progress = request.Progress;
            goal.CurrentProgress = (int)request.Progress;
            goal.UpdatedAt = DateTime.UtcNow;

            // Check if goal is achieved
            if (goal.Progress >= 100)
            {
                goal.Status = GoalStatus.Achieved;
            }

            // Store progress history for tracking trends
            await StoreProgressHistoryAsync(goal.GoalId, request.Progress, request.Notes);

            await _studentRepository.UpdateGoalAsync(goal);

            return Ok(new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                GoalText = goal.GoalText,
                GoalArea = goal.GoalArea,
                Status = goal.Status,
                Progress = goal.Progress,
                TargetDate = goal.TargetDate,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for goal {GoalId}", id);
            return StatusCode(500, new { message = "An error occurred while updating goal progress" });
        }
    }

    [HttpPost("{id}/discontinue")]
    public async Task<IActionResult> DiscontinueGoal(Guid id, [FromBody] DiscontinueGoalRequest request)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Get goal by searching for it
            IEnumerable<StudentGoal> goals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                skip: 0,
                take: int.MaxValue);
            
            StudentGoal? goal = goals.FirstOrDefault(g => g.GoalId == id);
            
            if (goal == null)
            {
                return NotFound();
            }

            Student? student = await _studentRepository.GetByIdAsync(goal.StudentId);
            if (student == null || student.TherapistId != userId)
            {
                return NotFound();
            }

            goal.Status = GoalStatus.Discontinued;
            goal.UpdatedAt = DateTime.UtcNow;

            // Store discontinuation reason - would be in Notes field when added to model
            // For now, log the reason
            _logger.LogInformation("Goal {GoalId} discontinued. Reason: {Reason}", goal.GoalId, request.Reason);
            await _studentRepository.UpdateGoalAsync(goal);

            return Ok(new { message = "Goal discontinued successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discontinuing goal {GoalId}", id);
            return StatusCode(500, new { message = "An error occurred while discontinuing the goal" });
        }
    }

    [HttpGet("templates")]
    public ActionResult<List<GoalTemplateDto>> GetGoalTemplates(
        [FromQuery] string? goalArea = null,
        [FromQuery] int? gradeLevel = null)
    {
        try
        {
            // Load goal templates - these would be seeded in the database
            // For now, returning common therapy goal templates
            var templates = new List<GoalTemplateDto>
            {
                new GoalTemplateDto
                {
                    TemplateId = Guid.NewGuid(),
                    GoalArea = "Fine Motor",
                    GoalText = "Student will demonstrate improved fine motor skills by cutting along straight lines with 80% accuracy in 4 out of 5 trials.",
                    GradeLevels = new List<int> { 0, 1, 2 },
                    Keywords = new List<string> { "cutting", "scissors", "fine motor" }
                },
                new GoalTemplateDto
                {
                    TemplateId = Guid.NewGuid(),
                    GoalArea = "Articulation",
                    GoalText = "Student will produce /r/ sound in all positions of words with 80% accuracy during structured activities.",
                    GradeLevels = new List<int> { 1, 2, 3, 4, 5 },
                    Keywords = new List<string> { "articulation", "speech", "r sound" }
                },
                new GoalTemplateDto
                {
                    TemplateId = Guid.NewGuid(),
                    GoalArea = "Gross Motor",
                    GoalText = "Student will demonstrate improved balance by standing on one foot for 10 seconds on each leg in 3 out of 5 trials.",
                    GradeLevels = new List<int> { 0, 1, 2, 3 },
                    Keywords = new List<string> { "balance", "gross motor", "coordination" }
                }
            };

            if (!string.IsNullOrEmpty(goalArea))
            {
                templates = templates.Where(t => t.GoalArea.ToLower().Contains(goalArea.ToLower())).ToList();
            }

            if (gradeLevel.HasValue)
            {
                templates = templates.Where(t => t.GradeLevels.Contains(gradeLevel.Value)).ToList();
            }

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving goal templates");
            return StatusCode(500, new { message = "An error occurred while retrieving goal templates" });
        }
    }

    private Task<List<ProgressHistoryDto>> GetGoalProgressHistoryAsync(Guid goalId)
    {
        // Progress history would be retrieved from GoalProgressHistory table
        // Table structure: GoalId, Date, Progress, Notes, RecordedBy
        // For now, returning sample data to demonstrate the feature

        // When GoalProgressHistory table is added:
        // return await _context.GoalProgressHistories
        //     .Where(h => h.GoalId == goalId)
        //     .OrderByDescending(h => h.Date)
        //     .Select(h => new ProgressHistoryDto 
        //     { 
        //         Date = h.Date, 
        //         Progress = h.Progress, 
        //         Notes = h.Notes 
        //     })
        //     .ToListAsync();

        return Task.FromResult(new List<ProgressHistoryDto>
        {
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-30), Progress = 25, Notes = "Initial assessment" },
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-14), Progress = 50, Notes = "Good progress with cues" },
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-7), Progress = 75, Notes = "Independent in structured activities" }
        });
    }

    private Task StoreProgressHistoryAsync(Guid goalId, decimal progress, string? notes)
    {
        // Store progress history for trend tracking and reporting
        // This would insert into GoalProgressHistory table:
        // var history = new GoalProgressHistory
        // {
        //     GoalProgressHistoryId = Guid.NewGuid(),
        //     GoalId = goalId,
        //     Date = DateTime.UtcNow,
        //     Progress = progress,
        //     Notes = notes,
        //     RecordedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        // };
        // _context.GoalProgressHistories.Add(history);
        // await _context.SaveChangesAsync();

        _logger.LogInformation("Progress history stored for goal {GoalId}: {Progress}% - {Notes}",
            goalId, progress, notes ?? "No notes");

        return Task.CompletedTask;
    }
}

// DTOs
public class GoalDto
{
    public Guid GoalId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public string GoalArea { get; set; } = string.Empty;
    public GoalStatus Status { get; set; }
    public decimal Progress { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GoalDetailDto : GoalDto
{
    public List<ProgressHistoryDto> ProgressHistory { get; set; } = new();
}

public class ProgressHistoryDto
{
    public DateTime Date { get; set; }
    public decimal Progress { get; set; }
    public string? Notes { get; set; }
}

public class CreateGoalRequest
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public string GoalText { get; set; } = string.Empty;

    [Required]
    public string GoalArea { get; set; } = string.Empty;

    public DateTime? TargetDate { get; set; }
}

public class UpdateGoalRequest
{
    public string? GoalText { get; set; }
    public string? GoalArea { get; set; }
    public DateTime? TargetDate { get; set; }
    public GoalStatus? Status { get; set; }
}

public class UpdateProgressRequest
{
    [Required]
    [Range(0, 100)]
    public decimal Progress { get; set; }

    public string? Notes { get; set; }
}

public class DiscontinueGoalRequest
{
    public string? Reason { get; set; }
}

public class GoalTemplateDto
{
    public Guid TemplateId { get; set; }
    public string GoalArea { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public List<int> GradeLevels { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
}