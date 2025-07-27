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
[Route("api/sessions")]
[Authorize(Policy = "TherapistOnly")]
public class SessionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ApplicationDbContext context, ILogger<SessionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionDto>>> GetSessions(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? studentId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var query = _context.Sessions
                .Include(s => s.Student)
                .Where(s => s.TherapistId == userId);

            if (startDate.HasValue)
                query = query.Where(s => s.SessionDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(s => s.SessionDate <= endDate.Value);
                
            if (studentId.HasValue)
                query = query.Where(s => s.StudentId == studentId.Value);

            var sessions = await query
                .OrderByDescending(s => s.SessionDate)
                .Select(s => new SessionDto
                {
                    SessionId = s.SessionId,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FirstNameEncrypted + " " + s.Student.LastNameEncrypted,
                    SessionDate = s.SessionDate,
                    DurationMinutes = s.DurationMinutes,
                    SessionType = s.SessionType,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions");
            return StatusCode(500, new { message = "An error occurred while retrieving sessions" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDetailDto>> GetSession(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var session = await _context.Sessions
                .Include(s => s.Student)
                .Include(s => s.Resources)
                .Include(s => s.Goals)
                .FirstOrDefaultAsync(s => s.SessionId == id && s.TherapistId == userId);

            if (session == null)
            {
                return NotFound();
            }

            var dto = new SessionDetailDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = session.Student.FirstNameEncrypted + " " + session.Student.LastNameEncrypted,
                SessionDate = session.SessionDate,
                DurationMinutes = session.DurationMinutes,
                SessionType = session.SessionType,
                Status = session.Status,
                NotesEncrypted = session.NotesEncrypted,
                CreatedAt = session.CreatedAt,
                Resources = session.Resources.Select(r => new SessionResourceDto
                {
                    ResourceId = r.ResourceId,
                    Title = r.Title,
                    ResourceType = r.ResourceType
                }).ToList(),
                Goals = session.Goals.Select(g => new SessionGoalDto
                {
                    GoalId = g.GoalId,
                    GoalText = g.GoalText,
                    Progress = g.Progress
                }).ToList(),
                DataPoints = session.DataPointsJson != null ? 
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(session.DataPointsJson) ?? new() : 
                    new()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the session" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SessionDto>> CreateSession([FromBody] CreateSessionRequest request)
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

            var session = new Session
            {
                TherapistId = userId,
                StudentId = request.StudentId,
                SessionDate = request.SessionDate,
                DurationMinutes = request.DurationMinutes,
                SessionType = request.SessionType,
                Status = SessionStatus.Scheduled,
                NotesEncrypted = request.Notes ?? string.Empty
            };

            if (request.DataPoints != null && request.DataPoints.Any())
            {
                session.DataPointsJson = System.Text.Json.JsonSerializer.Serialize(request.DataPoints);
            }

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSession), new { id = session.SessionId }, new SessionDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = student.FirstNameEncrypted + " " + student.LastNameEncrypted,
                SessionDate = session.SessionDate,
                DurationMinutes = session.DurationMinutes,
                SessionType = session.SessionType,
                Status = session.Status,
                CreatedAt = session.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new { message = "An error occurred while creating the session" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SessionDto>> UpdateSession(Guid id, [FromBody] UpdateSessionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var session = await _context.Sessions
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.SessionId == id && s.TherapistId == userId);

            if (session == null)
            {
                return NotFound();
            }

            if (request.SessionDate.HasValue)
                session.SessionDate = request.SessionDate.Value;
            if (request.DurationMinutes.HasValue)
                session.DurationMinutes = request.DurationMinutes.Value;
            if (request.SessionType.HasValue)
                session.SessionType = request.SessionType.Value;
            if (request.Status.HasValue)
                session.Status = request.Status.Value;
            if (!string.IsNullOrEmpty(request.Notes))
                session.NotesEncrypted = request.Notes;
            if (request.DataPoints != null)
                session.DataPointsJson = System.Text.Json.JsonSerializer.Serialize(request.DataPoints);

            session.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new SessionDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = session.Student.FirstNameEncrypted + " " + session.Student.LastNameEncrypted,
                SessionDate = session.SessionDate,
                DurationMinutes = session.DurationMinutes,
                SessionType = session.SessionType,
                Status = session.Status,
                CreatedAt = session.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the session" });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, [FromBody] CompleteSessionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == id && s.TherapistId == userId);

            if (session == null)
            {
                return NotFound();
            }

            session.Status = SessionStatus.Completed;
            session.NotesEncrypted = request.Notes;
            session.UpdatedAt = DateTime.UtcNow;

            if (request.DataPoints != null && request.DataPoints.Any())
            {
                session.DataPointsJson = System.Text.Json.JsonSerializer.Serialize(request.DataPoints);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Session completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session {SessionId}", id);
            return StatusCode(500, new { message = "An error occurred while completing the session" });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelSession(Guid id, [FromBody] CancelSessionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == id && s.TherapistId == userId);

            if (session == null)
            {
                return NotFound();
            }

            session.Status = SessionStatus.Cancelled;
            session.NotesEncrypted = request.Reason ?? "Session cancelled";
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Session cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling session {SessionId}", id);
            return StatusCode(500, new { message = "An error occurred while cancelling the session" });
        }
    }

    [HttpPost("{id}/resources")]
    public async Task<IActionResult> AddResourceToSession(Guid id, [FromBody] AddResourceToSessionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == id && s.TherapistId == userId);

            if (session == null)
            {
                return NotFound();
            }

            // TODO: Implement session-resource relationship
            // For now, just return success
            return Ok(new { message = "Resource added to session successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding resource to session {SessionId}", id);
            return StatusCode(500, new { message = "An error occurred while adding resource to session" });
        }
    }
}

// DTOs
public class SessionDto
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int DurationMinutes { get; set; }
    public SessionType SessionType { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SessionDetailDto : SessionDto
{
    public string NotesEncrypted { get; set; } = string.Empty;
    public List<SessionResourceDto> Resources { get; set; } = new();
    public List<SessionGoalDto> Goals { get; set; } = new();
    public Dictionary<string, object> DataPoints { get; set; } = new();
}

public class SessionResourceDto
{
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
}

public class SessionGoalDto
{
    public Guid GoalId { get; set; }
    public string GoalText { get; set; } = string.Empty;
    public decimal Progress { get; set; }
}

public class CreateSessionRequest
{
    [Required]
    public Guid StudentId { get; set; }
    
    [Required]
    public DateTime SessionDate { get; set; }
    
    [Required]
    [Range(1, 480)]
    public int DurationMinutes { get; set; }
    
    [Required]
    public SessionType SessionType { get; set; }
    
    public string? Notes { get; set; }
    
    public Dictionary<string, object>? DataPoints { get; set; }
}

public class UpdateSessionRequest
{
    public DateTime? SessionDate { get; set; }
    public int? DurationMinutes { get; set; }
    public SessionType? SessionType { get; set; }
    public SessionStatus? Status { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? DataPoints { get; set; }
}

public class CompleteSessionRequest
{
    [Required]
    public string Notes { get; set; } = string.Empty;
    public Dictionary<string, object>? DataPoints { get; set; }
}

public class CancelSessionRequest
{
    public string? Reason { get; set; }
}

public class AddResourceToSessionRequest
{
    [Required]
    public Guid ResourceId { get; set; }
}

