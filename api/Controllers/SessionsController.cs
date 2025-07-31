using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize(Policy = "TherapistOnly")]
public class SessionsController : ControllerBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISessionRepository sessionRepository, IStudentRepository studentRepository, ILogger<SessionsController> logger)
    {
        _sessionRepository = sessionRepository;
        _studentRepository = studentRepository;
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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            IEnumerable<Session> sessions = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                studentId: studentId,
                startDate: startDate,
                endDate: endDate);

            List<SessionDto> sessionDtos = new List<SessionDto>();
            foreach (Session session in sessions)
            {
                Student? student = null;
                if (session.Student == null)
                {
                    student = await _studentRepository.GetByIdAsync(session.StudentId);
                }
                else
                {
                    student = session.Student;
                }

                sessionDtos.Add(new SessionDto
                {
                    SessionId = session.SessionId,
                    StudentId = session.StudentId,
                    StudentName = student != null ? $"{student.FirstNameEncrypted} {student.LastNameEncrypted}" : "Unknown",
                    SessionDate = session.SessionDate,
                    DurationMinutes = session.DurationMinutes,
                    SessionType = session.SessionType,
                    Status = session.Status,
                    CreatedAt = session.CreatedAt
                });
            }

            return Ok(sessionDtos.OrderByDescending(s => s.SessionDate).ToList());
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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Session? session = await _sessionRepository.GetSessionWithDetailsAsync(id, userId);

            if (session == null)
            {
                return NotFound();
            }

            Student? student = session.Student ?? await _studentRepository.GetByIdAsync(session.StudentId);
            
            SessionDetailDto dto = new SessionDetailDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = student != null ? $"{student.FirstNameEncrypted} {student.LastNameEncrypted}" : "Unknown",
                SessionDate = session.SessionDate,
                DurationMinutes = session.DurationMinutes,
                SessionType = session.SessionType,
                Status = session.Status,
                NotesEncrypted = session.NotesEncrypted ?? string.Empty,
                CreatedAt = session.CreatedAt,
                Resources = new List<SessionResourceDto>(), // TODO: Load from SessionResources join table
                Goals = new List<SessionGoalDto>(), // TODO: Load session goals
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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            // Verify student belongs to therapist
            Student? student = await _studentRepository.GetByIdAsync(request.StudentId);
            
            if (student == null || student.TherapistId != userId)
            {
                return BadRequest(new { message = "Student not found or not assigned to therapist" });
            }

            Session session = new Session
            {
                TherapistId = userId,
                StudentId = request.StudentId,
                SessionDate = request.SessionDate,
                DurationMinutes = request.DurationMinutes,
                SessionType = request.SessionType,
                Status = SessionStatus.Scheduled,
                NotesEncrypted = request.Notes ?? string.Empty,
                Location = request.Location ?? "Clinic",
                IsBillable = request.IsBillable ?? true
            };

            if (request.DataPoints != null && request.DataPoints.Any())
            {
                session.DataPointsJson = System.Text.Json.JsonSerializer.Serialize(request.DataPoints);
            }

            session = await _sessionRepository.AddAsync(session);

            return CreatedAtAction(nameof(GetSession), new { id = session.SessionId }, new SessionDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null || session.TherapistId != userId)
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
            await _sessionRepository.UpdateAsync(session);

            Student? student = await _studentRepository.GetByIdAsync(session.StudentId);

            return Ok(new SessionDto
            {
                SessionId = session.SessionId,
                StudentId = session.StudentId,
                StudentName = student != null ? $"{student.FirstNameEncrypted} {student.LastNameEncrypted}" : "Unknown",
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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null || session.TherapistId != userId)
            {
                return NotFound();
            }

            session.Status = SessionStatus.Completed;
            session.IsCompleted = true;
            session.NotesEncrypted = request.Notes;
            session.UpdatedAt = DateTime.UtcNow;

            if (request.DataPoints != null && request.DataPoints.Any())
            {
                session.DataPointsJson = System.Text.Json.JsonSerializer.Serialize(request.DataPoints);
            }

            await _sessionRepository.UpdateAsync(session);

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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null || session.TherapistId != userId)
            {
                return NotFound();
            }

            session.Status = SessionStatus.Cancelled;
            session.NotesEncrypted = request.Reason ?? "Session cancelled";
            session.UpdatedAt = DateTime.UtcNow;

            await _sessionRepository.UpdateAsync(session);

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
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            Session? session = await _sessionRepository.GetByIdAsync(id);

            if (session == null || session.TherapistId != userId)
            {
                return NotFound();
            }

            // Add resource to ResourcesUsed list
            if (session.ResourcesUsed == null)
            {
                session.ResourcesUsed = new List<Guid>();
            }
            
            if (!session.ResourcesUsed.Contains(request.ResourceId))
            {
                session.ResourcesUsed.Add(request.ResourceId);
                await _sessionRepository.UpdateAsync(session);
            }

            _logger.LogInformation("Resource {ResourceId} added to session {SessionId}", request.ResourceId, id);
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
    
    public string? Location { get; set; }
    
    public bool? IsBillable { get; set; }
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

