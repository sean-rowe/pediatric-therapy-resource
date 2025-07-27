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
[Route("api/goals")]
[Authorize(Policy = "TherapistOnly")]
public class GoalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GoalsController> _logger;

    public GoalsController(ApplicationDbContext context, ILogger<GoalsController> logger)
    {
        _context = context;
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var query = _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .Where(g => g.Student.TherapistId == userId);

            if (studentId.HasValue)
                query = query.Where(g => g.StudentId == studentId.Value);

            if (status.HasValue)
                query = query.Where(g => g.Status == status.Value);

            if (!string.IsNullOrEmpty(goalArea))
                query = query.Where(g => g.GoalArea.ToLower().Contains(goalArea.ToLower()));

            var goals = await query
                .OrderBy(g => g.Student.LastNameEncrypted)
                .ThenBy(g => g.TargetDate)
                .Select(g => new GoalDto
                {
                    GoalId = g.GoalId,
                    StudentId = g.StudentId,
                    StudentName = g.Student.FirstNameEncrypted + " " + g.Student.LastNameEncrypted,
                    GoalText = g.GoalText,
                    GoalArea = g.GoalArea,
                    Status = g.Status,
                    Progress = g.Progress,
                    TargetDate = g.TargetDate,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(goals);
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var goal = await _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .FirstOrDefaultAsync(g => g.GoalId == id && g.Student.TherapistId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            // Get progress history
            var progressHistory = await GetGoalProgressHistoryAsync(id);

            var dto = new GoalDetailDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = goal.Student.FirstNameEncrypted + " " + goal.Student.LastNameEncrypted,
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            // Verify student belongs to therapist
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == request.StudentId && s.TherapistId == userId);
                
            if (student == null)
            {
                return BadRequest(new { message = "Student not found or not assigned to therapist" });
            }

            var goal = new StudentGoal
            {
                StudentId = request.StudentId,
                GoalText = request.GoalText,
                GoalArea = request.GoalArea,
                TargetDate = request.TargetDate,
                Status = GoalStatus.Active,
                Progress = 0
            };

            _context.Set<StudentGoal>().Add(goal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGoal), new { id = goal.GoalId }, new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = student.FirstNameEncrypted + " " + student.LastNameEncrypted,
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var goal = await _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .FirstOrDefaultAsync(g => g.GoalId == id && g.Student.TherapistId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(request.GoalText))
                goal.GoalText = request.GoalText;
            if (!string.IsNullOrEmpty(request.GoalArea))
                goal.GoalArea = request.GoalArea;
            if (request.TargetDate.HasValue)
                goal.TargetDate = request.TargetDate;
            if (request.Status.HasValue)
                goal.Status = request.Status.Value;

            goal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = goal.Student.FirstNameEncrypted + " " + goal.Student.LastNameEncrypted,
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var goal = await _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .FirstOrDefaultAsync(g => g.GoalId == id && g.Student.TherapistId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Progress = request.Progress;
            goal.UpdatedAt = DateTime.UtcNow;

            // Check if goal is achieved
            if (goal.Progress >= 100)
            {
                goal.Status = GoalStatus.Achieved;
            }

            // TODO: Store progress history
            await StoreProgressHistoryAsync(goal.GoalId, request.Progress, request.Notes);

            await _context.SaveChangesAsync();

            return Ok(new GoalDto
            {
                GoalId = goal.GoalId,
                StudentId = goal.StudentId,
                StudentName = goal.Student.FirstNameEncrypted + " " + goal.Student.LastNameEncrypted,
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
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var goal = await _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .FirstOrDefaultAsync(g => g.GoalId == id && g.Student.TherapistId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Status = GoalStatus.Discontinued;
            goal.UpdatedAt = DateTime.UtcNow;

            // TODO: Store discontinuation reason
            await _context.SaveChangesAsync();

            return Ok(new { message = "Goal discontinued successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discontinuing goal {GoalId}", id);
            return StatusCode(500, new { message = "An error occurred while discontinuing the goal" });
        }
    }

    [HttpGet("templates")]
    public async Task<ActionResult<List<GoalTemplateDto>>> GetGoalTemplates(
        [FromQuery] string? goalArea = null,
        [FromQuery] int? gradeLevel = null)
    {
        try
        {
            // TODO: Implement goal templates from database
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

    private async Task<List<ProgressHistoryDto>> GetGoalProgressHistoryAsync(Guid goalId)
    {
        // TODO: Implement actual progress history from database
        return new List<ProgressHistoryDto>
        {
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-30), Progress = 25, Notes = "Initial assessment" },
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-14), Progress = 50, Notes = "Good progress with cues" },
            new ProgressHistoryDto { Date = DateTime.UtcNow.AddDays(-7), Progress = 75, Notes = "Independent in structured activities" }
        };
    }

    private async Task StoreProgressHistoryAsync(Guid goalId, decimal progress, string? notes)
    {
        // TODO: Implement actual storage of progress history
        await Task.CompletedTask;
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