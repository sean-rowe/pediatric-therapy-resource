using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/caseload")]
[Authorize(Policy = "TherapistOnly")]
public class CaseloadController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CaseloadController> _logger;

    public CaseloadController(ApplicationDbContext context, ILogger<CaseloadController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<CaseloadOverviewDto>> GetCaseloadOverview()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var activeStudents = await _context.Students
                .Where(s => s.TherapistId == userId && s.Status == StudentStatus.Active)
                .CountAsync();

            var scheduledSessions = await _context.Sessions
                .Where(s => s.TherapistId == userId && 
                       s.Status == SessionStatus.Scheduled &&
                       s.SessionDate >= DateTime.UtcNow.Date)
                .CountAsync();

            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var completedThisWeek = await _context.Sessions
                .Where(s => s.TherapistId == userId && 
                       s.Status == SessionStatus.Completed &&
                       s.SessionDate >= weekStart &&
                       s.SessionDate < weekEnd)
                .CountAsync();

            var goalsProgress = await _context.Set<StudentGoal>()
                .Where(g => g.Student.TherapistId == userId && g.Status == GoalStatus.Active)
                .AverageAsync(g => (double?)g.Progress) ?? 0;

            var overview = new CaseloadOverviewDto
            {
                ActiveStudents = activeStudents,
                ScheduledSessions = scheduledSessions,
                CompletedSessionsThisWeek = completedThisWeek,
                AverageGoalProgress = (decimal)goalsProgress,
                AlertsCount = await GetAlertsCountAsync(userId)
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving caseload overview");
            return StatusCode(500, new { message = "An error occurred while retrieving caseload overview" });
        }
    }

    [HttpGet("students")]
    public async Task<ActionResult<List<CaseloadStudentDto>>> GetCaseloadStudents(
        [FromQuery] StudentStatus? status = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var query = _context.Students
                .Include(s => s.Goals)
                .Include(s => s.Sessions)
                .Where(s => s.TherapistId == userId);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s => 
                    s.FirstNameEncrypted.ToLower().Contains(searchLower) ||
                    s.LastNameEncrypted.ToLower().Contains(searchLower));
            }

            var students = await query
                .Select(s => new CaseloadStudentDto
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstNameEncrypted,
                    LastName = s.LastNameEncrypted,
                    GradeLevel = s.GradeLevel,
                    Status = s.Status,
                    LastSessionDate = s.Sessions
                        .Where(sess => sess.Status == SessionStatus.Completed)
                        .OrderByDescending(sess => sess.SessionDate)
                        .Select(sess => (DateTime?)sess.SessionDate)
                        .FirstOrDefault(),
                    NextSessionDate = s.Sessions
                        .Where(sess => sess.Status == SessionStatus.Scheduled && sess.SessionDate >= DateTime.UtcNow)
                        .OrderBy(sess => sess.SessionDate)
                        .Select(sess => (DateTime?)sess.SessionDate)
                        .FirstOrDefault(),
                    ActiveGoalsCount = s.Goals.Count(g => g.Status == GoalStatus.Active),
                    AverageGoalProgress = s.Goals
                        .Where(g => g.Status == GoalStatus.Active)
                        .Average(g => (decimal?)g.Progress) ?? 0,
                    HasAlerts = false // TODO: Implement alert logic
                })
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving caseload students");
            return StatusCode(500, new { message = "An error occurred while retrieving caseload students" });
        }
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<List<ScheduleItemDto>>> GetSchedule(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddDays(7);

            var sessions = await _context.Sessions
                .Include(s => s.Student)
                .Where(s => s.TherapistId == userId &&
                       s.SessionDate >= start &&
                       s.SessionDate < end)
                .OrderBy(s => s.SessionDate)
                .Select(s => new ScheduleItemDto
                {
                    SessionId = s.SessionId,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FirstNameEncrypted + " " + s.Student.LastNameEncrypted,
                    SessionDate = s.SessionDate,
                    DurationMinutes = s.DurationMinutes,
                    SessionType = s.SessionType,
                    Status = s.Status,
                    Location = s.SessionType == SessionType.Teletherapy ? "Virtual" : "In-Person"
                })
                .ToListAsync();

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule");
            return StatusCode(500, new { message = "An error occurred while retrieving schedule" });
        }
    }

    [HttpGet("productivity")]
    public async Task<ActionResult<ProductivityReportDto>> GetProductivityReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow.Date.AddDays(1);

            var sessions = await _context.Sessions
                .Where(s => s.TherapistId == userId &&
                       s.SessionDate >= start &&
                       s.SessionDate < end)
                .ToListAsync();

            var totalSessions = sessions.Count;
            var completedSessions = sessions.Count(s => s.Status == SessionStatus.Completed);
            var cancelledSessions = sessions.Count(s => s.Status == SessionStatus.Cancelled);
            var noShowSessions = sessions.Count(s => s.Status == SessionStatus.NoShow);
            var totalMinutes = sessions.Where(s => s.Status == SessionStatus.Completed).Sum(s => s.DurationMinutes);

            var report = new ProductivityReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                CancelledSessions = cancelledSessions,
                NoShowSessions = noShowSessions,
                TotalTherapyMinutes = totalMinutes,
                AverageSessionsPerDay = totalSessions / Math.Max(1, (end - start).Days),
                CompletionRate = totalSessions > 0 ? (decimal)completedSessions / totalSessions * 100 : 0,
                SessionsByType = sessions
                    .GroupBy(s => s.SessionType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating productivity report");
            return StatusCode(500, new { message = "An error occurred while generating productivity report" });
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<CaseloadAlertDto>>> GetAlerts()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());
            
            var alerts = new List<CaseloadAlertDto>();

            // Check for students without recent sessions
            var twoWeeksAgo = DateTime.UtcNow.Date.AddDays(-14);
            var studentsWithoutRecentSessions = await _context.Students
                .Where(s => s.TherapistId == userId && 
                       s.Status == StudentStatus.Active &&
                       !s.Sessions.Any(sess => sess.SessionDate > twoWeeksAgo && sess.Status == SessionStatus.Completed))
                .Select(s => new { s.StudentId, StudentName = s.FirstNameEncrypted + " " + s.LastNameEncrypted })
                .ToListAsync();

            foreach (var student in studentsWithoutRecentSessions)
            {
                alerts.Add(new CaseloadAlertDto
                {
                    AlertId = Guid.NewGuid(),
                    Type = AlertType.NoRecentSession,
                    Severity = AlertSeverity.Medium,
                    Message = $"{student.StudentName} has not had a session in over 2 weeks",
                    StudentId = student.StudentId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Check for goals nearing target date
            var thirtyDaysFromNow = DateTime.UtcNow.Date.AddDays(30);
            var goalsNearingDeadline = await _context.Set<StudentGoal>()
                .Include(g => g.Student)
                .Where(g => g.Student.TherapistId == userId &&
                       g.Status == GoalStatus.Active &&
                       g.TargetDate.HasValue &&
                       g.TargetDate.Value <= thirtyDaysFromNow &&
                       g.Progress < 80)
                .Select(g => new { 
                    g.GoalId, 
                    g.StudentId, 
                    StudentName = g.Student.FirstNameEncrypted + " " + g.Student.LastNameEncrypted,
                    g.GoalText,
                    g.TargetDate,
                    g.Progress
                })
                .ToListAsync();

            foreach (var goal in goalsNearingDeadline)
            {
                alerts.Add(new CaseloadAlertDto
                {
                    AlertId = Guid.NewGuid(),
                    Type = AlertType.GoalDeadline,
                    Severity = AlertSeverity.High,
                    Message = $"Goal for {goal.StudentName} is at {goal.Progress}% with deadline {goal.TargetDate:MM/dd/yyyy}",
                    StudentId = goal.StudentId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return Ok(alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts");
            return StatusCode(500, new { message = "An error occurred while retrieving alerts" });
        }
    }

    private async Task<int> GetAlertsCountAsync(Guid therapistId)
    {
        // Simplified version - in real implementation would be more comprehensive
        var twoWeeksAgo = DateTime.UtcNow.Date.AddDays(-14);
        return await _context.Students
            .Where(s => s.TherapistId == therapistId && 
                   s.Status == StudentStatus.Active &&
                   !s.Sessions.Any(sess => sess.SessionDate > twoWeeksAgo && sess.Status == SessionStatus.Completed))
            .CountAsync();
    }
}

// DTOs
public class CaseloadOverviewDto
{
    public int ActiveStudents { get; set; }
    public int ScheduledSessions { get; set; }
    public int CompletedSessionsThisWeek { get; set; }
    public decimal AverageGoalProgress { get; set; }
    public int AlertsCount { get; set; }
}

public class CaseloadStudentDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public StudentStatus Status { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public DateTime? NextSessionDate { get; set; }
    public int ActiveGoalsCount { get; set; }
    public decimal AverageGoalProgress { get; set; }
    public bool HasAlerts { get; set; }
}

public class ScheduleItemDto
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int DurationMinutes { get; set; }
    public SessionType SessionType { get; set; }
    public SessionStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class ProductivityReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int CancelledSessions { get; set; }
    public int NoShowSessions { get; set; }
    public int TotalTherapyMinutes { get; set; }
    public double AverageSessionsPerDay { get; set; }
    public decimal CompletionRate { get; set; }
    public Dictionary<string, int> SessionsByType { get; set; } = new();
}

public class CaseloadAlertDto
{
    public Guid AlertId { get; set; }
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

