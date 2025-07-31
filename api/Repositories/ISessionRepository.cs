using System.Linq.Expressions;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public interface ISessionRepository : IRepository<Session>
{
    Task<IEnumerable<Session>> GetSessionsAsync(
        Guid therapistId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? studentId = null,
        SessionType? sessionType = null,
        int skip = 0,
        int take = 20);
    Task<SessionStatisticsDto> GetSessionStatisticsAsync(
        Guid therapistId,
        DateTime startDate,
        DateTime endDate,
        Guid? studentId = null);
    Task<IEnumerable<Session>> GetSessionsByResourceAsync(
        Guid resourceId,
        Guid? therapistId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20);
    Task RecordSessionProgressAsync(
        Guid sessionId,
        Guid goalId,
        int progressValue,
        string? notes = null);
    Task<IEnumerable<Session>> GetUpcomingSessionsAsync(
        Guid therapistId,
        int days = 7,
        int skip = 0,
        int take = 20);
    Task<Session> CloneSessionAsync(Guid sourceSessionId, DateTime newSessionDate);
    Task<Session?> GetSessionWithDetailsAsync(Guid sessionId, Guid? therapistId = null);
}

public class SessionStatisticsDto
{
    public int TotalSessions { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalHours { get; set; }
    public double AverageMinutesPerSession { get; set; }
    public List<SessionTypeStats> SessionsByType { get; set; } = new();
}

public class SessionTypeStats
{
    public SessionType SessionType { get; set; }
    public int SessionCount { get; set; }
    public int TotalMinutes { get; set; }
}