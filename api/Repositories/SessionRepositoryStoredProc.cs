using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using Newtonsoft.Json;

namespace UPTRMS.Api.Repositories;

public class SessionRepositoryStoredProc : ISessionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public SessionRepositoryStoredProc(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Session?> GetByIdAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessionById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SessionId", id);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapSessionFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await GetAllAsync(0, int.MaxValue);
    }

    public async Task<IEnumerable<Session>> GetAllAsync(int skip, int take)
    {
        List<Session> sessions = new List<Session>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", DBNull.Value); // Admin access
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            sessions.Add(MapSessionFromReader(reader));
        }
        
        return sessions;
    }

    public async Task<Session> AddAsync(Session entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateSession", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter sessionIdParam = new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier);
        sessionIdParam.Direction = ParameterDirection.Output;
        sessionIdParam.Value = entity.SessionId == Guid.Empty ? DBNull.Value : entity.SessionId;
        command.Parameters.Add(sessionIdParam);
        
        command.Parameters.AddWithValue("@TherapistId", entity.TherapistId);
        command.Parameters.AddWithValue("@StudentId", entity.StudentId);
        command.Parameters.AddWithValue("@SessionDate", entity.SessionDate);
        command.Parameters.AddWithValue("@DurationMinutes", entity.DurationMinutes);
        command.Parameters.AddWithValue("@SessionType", (int)entity.SessionType);
        command.Parameters.AddWithValue("@ResourcesUsed", JsonConvert.SerializeObject(entity.ResourcesUsed));
        command.Parameters.AddWithValue("@DataPoints", entity.DataPoints);
        command.Parameters.AddWithValue("@NotesEncrypted", (object?)entity.NotesEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)entity.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsBillable", entity.IsBillable);
        command.Parameters.AddWithValue("@BillingCode", (object?)entity.BillingCode ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        entity.SessionId = (Guid)sessionIdParam.Value;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return entity;
    }

    public async Task UpdateAsync(Session entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateSession", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@SessionId", entity.SessionId);
        command.Parameters.AddWithValue("@TherapistId", entity.TherapistId);
        command.Parameters.AddWithValue("@SessionDate", entity.SessionDate);
        command.Parameters.AddWithValue("@DurationMinutes", entity.DurationMinutes);
        command.Parameters.AddWithValue("@SessionType", (int)entity.SessionType);
        command.Parameters.AddWithValue("@ResourcesUsed", JsonConvert.SerializeObject(entity.ResourcesUsed));
        command.Parameters.AddWithValue("@DataPoints", entity.DataPoints);
        command.Parameters.AddWithValue("@NotesEncrypted", (object?)entity.NotesEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)entity.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsBillable", entity.IsBillable);
        command.Parameters.AddWithValue("@BillingCode", (object?)entity.BillingCode ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_DeleteSession", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@SessionId", id);
        command.Parameters.AddWithValue("@TherapistId", DBNull.Value); // Admin access

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsAsync(
        Guid therapistId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? studentId = null,
        SessionType? sessionType = null,
        int skip = 0,
        int take = 20)
    {
        List<Session> sessions = new List<Session>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", therapistId);
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@StudentId", (object?)studentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SessionType", (object?)sessionType ?? DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            sessions.Add(MapSessionFromReader(reader));
        }
        
        return sessions;
    }

    public async Task<SessionStatisticsDto> GetSessionStatisticsAsync(
        Guid therapistId,
        DateTime startDate,
        DateTime endDate,
        Guid? studentId = null)
    {
        SessionStatisticsDto stats = new SessionStatisticsDto();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessionStatistics", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", therapistId);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);
        command.Parameters.AddWithValue("@StudentId", (object?)studentId ?? DBNull.Value);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        // First result set: Sessions by type
        while (await reader.ReadAsync())
        {
            stats.SessionsByType.Add(new SessionTypeStats
            {
                SessionType = (SessionType)reader.GetInt32(reader.GetOrdinal("SessionType")),
                SessionCount = reader.GetInt32(reader.GetOrdinal("SessionCount")),
                TotalMinutes = reader.GetInt32(reader.GetOrdinal("TotalMinutes"))
            });
        }
        
        // Second result set: Summary statistics
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            stats.TotalSessions = reader.GetInt32(reader.GetOrdinal("TotalSessions"));
            stats.TotalMinutes = reader.GetInt32(reader.GetOrdinal("TotalMinutes"));
            stats.TotalHours = reader.GetDouble(reader.GetOrdinal("TotalHours"));
            stats.AverageMinutesPerSession = reader.GetDouble(reader.GetOrdinal("AverageMinutesPerSession"));
        }
        
        return stats;
    }

    public async Task<IEnumerable<Session>> GetSessionsByResourceAsync(
        Guid resourceId,
        Guid? therapistId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20)
    {
        List<Session> sessions = new List<Session>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessionsByResource", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", resourceId);
        command.Parameters.AddWithValue("@TherapistId", (object?)therapistId ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            sessions.Add(MapSessionFromReader(reader));
        }
        
        return sessions;
    }

    public async Task RecordSessionProgressAsync(
        Guid sessionId,
        Guid goalId,
        int progressValue,
        string? notes = null)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_RecordSessionProgress", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@GoalId", goalId);
        command.Parameters.AddWithValue("@ProgressValue", progressValue);
        command.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Session>> GetUpcomingSessionsAsync(
        Guid therapistId,
        int days = 7,
        int skip = 0,
        int take = 20)
    {
        List<Session> sessions = new List<Session>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUpcomingSessions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", therapistId);
        command.Parameters.AddWithValue("@Days", days);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            sessions.Add(MapSessionFromReader(reader));
        }
        
        return sessions;
    }

    public async Task<Session> CloneSessionAsync(Guid sourceSessionId, DateTime newSessionDate)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CloneSession", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@SourceSessionId", sourceSessionId);
        command.Parameters.AddWithValue("@NewSessionDate", newSessionDate);
        
        SqlParameter newSessionIdParam = new SqlParameter("@NewSessionId", SqlDbType.UniqueIdentifier);
        newSessionIdParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(newSessionIdParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        Guid newSessionId = (Guid)newSessionIdParam.Value;
        Session? newSession = await GetByIdAsync(newSessionId);
        
        return newSession ?? throw new InvalidOperationException("Failed to clone session");
    }

    public async Task<Session?> GetSessionWithDetailsAsync(Guid sessionId, Guid? therapistId = null)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSessionById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@TherapistId", (object?)therapistId ?? DBNull.Value);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            Session session = MapSessionFromReader(reader);
            
            // The stored procedure includes student information
            if (reader.GetOrdinal("FirstNameEncrypted") >= 0)
            {
                session.Student = new Student
                {
                    StudentId = session.StudentId,
                    FirstNameEncrypted = reader.GetString(reader.GetOrdinal("FirstNameEncrypted")),
                    LastNameEncrypted = reader.GetString(reader.GetOrdinal("LastNameEncrypted")),
                    GradeLevel = reader.IsDBNull(reader.GetOrdinal("GradeLevel")) 
                        ? null 
                        : reader.GetInt32(reader.GetOrdinal("GradeLevel")).ToString()
                };
            }
            
            return session;
        }
        
        return null;
    }

    private Session MapSessionFromReader(SqlDataReader reader)
    {
        Session session = new Session
        {
            SessionId = reader.GetGuid(reader.GetOrdinal("SessionId")),
            TherapistId = reader.GetGuid(reader.GetOrdinal("TherapistId")),
            StudentId = reader.GetGuid(reader.GetOrdinal("StudentId")),
            SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),
            DurationMinutes = reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
            SessionType = (SessionType)reader.GetInt32(reader.GetOrdinal("SessionType")),
            DataPoints = reader.GetString(reader.GetOrdinal("DataPoints")),
            IsBillable = reader.GetBoolean(reader.GetOrdinal("IsBillable")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

        // Parse ResourcesUsed JSON
        string resourcesUsedJson = reader.GetString(reader.GetOrdinal("ResourcesUsed"));
        session.ResourcesUsed = JsonConvert.DeserializeObject<List<Guid>>(resourcesUsedJson) ?? new List<Guid>();

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("NotesEncrypted");
        if (!reader.IsDBNull(ordinal))
            session.NotesEncrypted = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("Location");
        if (!reader.IsDBNull(ordinal))
            session.Location = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("BillingCode");
        if (!reader.IsDBNull(ordinal))
            session.BillingCode = reader.GetString(ordinal);

        return session;
    }

    // Additional IRepository<Session> interface methods
    public async Task<IEnumerable<Session>> FindAsync(Expression<Func<Session, bool>> predicate)
    {
        List<Session> allSessions = (await GetAllAsync()).ToList();
        return allSessions.AsQueryable().Where(predicate).ToList();
    }

    public async Task DeleteAsync(Session entity)
    {
        await DeleteAsync(entity.SessionId);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Session, bool>> predicate)
    {
        List<Session> allSessions = (await GetAllAsync()).ToList();
        return allSessions.AsQueryable().Any(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Session, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return (await GetAllAsync()).Count();
        }
        else
        {
            List<Session> allSessions = (await GetAllAsync()).ToList();
            return allSessions.AsQueryable().Count(predicate);
        }
    }

    public IQueryable<Session> Query()
    {
        throw new NotSupportedException("Query() is not supported with stored procedures. Use specific methods like GetByIdAsync, GetSessionsAsync, etc.");
    }
}