using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/caseload")]
[Authorize(Policy = "TherapistOnly")]
public class CaseloadController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<CaseloadController> _logger;

    public CaseloadController(IStudentRepository studentRepository, ISessionRepository sessionRepository, ILogger<CaseloadController> logger)
    {
        _studentRepository = studentRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<CaseloadOverviewDto>> GetCaseloadOverview()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException());

            var students = await _studentRepository.GetByTherapistAsync(userId);
            var activeStudents = students.Count(s => s.Status == StudentStatus.Active);

            var sessions = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                startDate: DateTime.UtcNow.Date);
            var scheduledSessions = sessions.Count(s => s.Status == SessionStatus.Scheduled);

            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var weekSessions = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                startDate: weekStart,
                endDate: weekEnd);
            var completedThisWeek = weekSessions.Count(s => s.Status == SessionStatus.Completed);

            var goals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                status: GoalStatus.Active);
            var goalsProgress = goals.Any() ? goals.Average(g => (double)g.Progress) : 0;

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

            var allStudents = await _studentRepository.GetByTherapistAsync(userId);
            
            // Apply filters
            IEnumerable<Student> filteredStudents = allStudents;
            
            if (status.HasValue)
                filteredStudents = filteredStudents.Where(s => s.Status == status.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                filteredStudents = filteredStudents.Where(s =>
                    s.FirstNameEncrypted.ToLower().Contains(searchLower) ||
                    s.LastNameEncrypted.ToLower().Contains(searchLower));
            }

            var students = new List<CaseloadStudentDto>();
            
            foreach (var student in filteredStudents)
            {
                // Get sessions for this student
                var studentSessions = await _sessionRepository.GetSessionsAsync(
                    therapistId: userId,
                    studentId: student.StudentId);
                    
                // Get goals for this student
                var studentGoals = await _studentRepository.GetStudentGoalsAsync(
                    studentId: student.StudentId,
                    therapistId: userId);
                
                var completedSessions = studentSessions
                    .Where(sess => sess.Status == SessionStatus.Completed)
                    .OrderByDescending(sess => sess.SessionDate);
                    
                var scheduledSessions = studentSessions
                    .Where(sess => sess.Status == SessionStatus.Scheduled && sess.SessionDate >= DateTime.UtcNow)
                    .OrderBy(sess => sess.SessionDate);
                    
                var activeGoals = studentGoals.Where(g => g.Status == GoalStatus.Active).ToList();
                
                var dto = new CaseloadStudentDto
                {
                    StudentId = student.StudentId,
                    FirstName = student.FirstNameEncrypted,
                    LastName = student.LastNameEncrypted,
                    GradeLevel = student.GradeLevel,
                    Status = student.Status,
                    LastSessionDate = completedSessions.Select(s => (DateTime?)s.SessionDate).FirstOrDefault(),
                    NextSessionDate = scheduledSessions.Select(s => (DateTime?)s.SessionDate).FirstOrDefault(),
                    ActiveGoalsCount = activeGoals.Count,
                    AverageGoalProgress = activeGoals.Any() ? activeGoals.Average(g => g.Progress) : 0,
                    HasAlerts = studentGoals.Any(g => g.TargetDate < DateTime.UtcNow && g.Progress < 100) ||
                               !studentSessions.Any(sess => sess.SessionDate > DateTime.UtcNow.AddDays(-14) && sess.Status == SessionStatus.Completed)
                };
                
                students.Add(dto);
            }
            
            students = students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

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

            var sessions = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                startDate: start,
                endDate: end);
                
            var scheduleItems = new List<ScheduleItemDto>();
            
            foreach (var session in sessions.OrderBy(s => s.SessionDate))
            {
                var student = await _studentRepository.GetByIdAsync(session.StudentId);
                
                scheduleItems.Add(new ScheduleItemDto
                {
                    SessionId = session.SessionId,
                    StudentId = session.StudentId,
                    StudentName = student != null ? $"{student.FirstNameEncrypted} {student.LastNameEncrypted}" : "Unknown",
                    SessionDate = session.SessionDate,
                    DurationMinutes = session.DurationMinutes,
                    SessionType = session.SessionType,
                    Status = session.Status,
                    Location = session.SessionType == SessionType.Teletherapy ? "Virtual" : "In-Person"
                });
            }

            return Ok(scheduleItems);
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

            var sessionsEnum = await _sessionRepository.GetSessionsAsync(
                therapistId: userId,
                startDate: start,
                endDate: end);
            var sessions = sessionsEnum.ToList();

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
            var activeStudents = await _studentRepository.GetByTherapistAsync(userId);
            activeStudents = activeStudents.Where(s => s.Status == StudentStatus.Active).ToList();
            
            var studentsWithoutRecentSessions = new List<dynamic>();
            
            foreach (var student in activeStudents)
            {
                var recentSessions = await _sessionRepository.GetSessionsAsync(
                    therapistId: userId,
                    studentId: student.StudentId,
                    startDate: twoWeeksAgo);
                    
                if (!recentSessions.Any(sess => sess.Status == SessionStatus.Completed))
                {
                    studentsWithoutRecentSessions.Add(new { StudentId = student.StudentId, StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}" });
                }
            }

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
            var activeGoals = await _studentRepository.GetStudentGoalsAsync(
                therapistId: userId,
                status: GoalStatus.Active);
                
            var goalsNearingDeadline = new List<dynamic>();
            
            foreach (var goal in activeGoals.Where(g => g.TargetDate <= thirtyDaysFromNow && g.Progress < 80))
            {
                var student = await _studentRepository.GetByIdAsync(goal.StudentId);
                if (student != null)
                {
                    goalsNearingDeadline.Add(new
                    {
                        GoalId = goal.GoalId,
                        StudentId = goal.StudentId,
                        StudentName = $"{student.FirstNameEncrypted} {student.LastNameEncrypted}",
                        GoalText = goal.GoalText,
                        TargetDate = goal.TargetDate,
                        Progress = goal.Progress
                    });
                }
            }

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
        var activeStudents = await _studentRepository.GetByTherapistAsync(therapistId);
        activeStudents = activeStudents.Where(s => s.Status == StudentStatus.Active).ToList();
        
        int count = 0;
        foreach (var student in activeStudents)
        {
            var recentSessions = await _sessionRepository.GetSessionsAsync(
                therapistId: therapistId,
                studentId: student.StudentId,
                startDate: twoWeeksAgo);
                
            if (!recentSessions.Any(sess => sess.Status == SessionStatus.Completed))
            {
                count++;
            }
        }
        
        return count;
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

