using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;

namespace UPTRMS.Api.Repositories;

public class StudentRepositoryStoredProc : IStudentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public StudentRepositoryStoredProc(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetStudentById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@StudentId", id);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapStudentFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await GetAllAsync(0, int.MaxValue);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(int skip, int take)
    {
        List<Student> students = new List<Student>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetStudentsByTherapist", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", DBNull.Value); // Admin access
        command.Parameters.AddWithValue("@IncludeInactive", true);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            students.Add(MapStudentFromReader(reader));
        }
        
        return students;
    }

    public async Task<Student> AddAsync(Student entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateStudent", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter studentIdParam = new SqlParameter("@StudentId", SqlDbType.UniqueIdentifier);
        studentIdParam.Direction = ParameterDirection.Output;
        studentIdParam.Value = entity.StudentId == Guid.Empty ? DBNull.Value : entity.StudentId;
        command.Parameters.Add(studentIdParam);
        
        command.Parameters.AddWithValue("@TherapistId", entity.TherapistId);
        command.Parameters.AddWithValue("@FirstNameEncrypted", entity.FirstNameEncrypted);
        command.Parameters.AddWithValue("@LastNameEncrypted", entity.LastNameEncrypted);
        command.Parameters.AddWithValue("@DateOfBirthEncrypted", (object?)entity.DateOfBirthEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@GradeLevel", 
            string.IsNullOrEmpty(entity.GradeLevel) || !int.TryParse(entity.GradeLevel, out int gradeLevel) 
                ? DBNull.Value 
                : gradeLevel);
        command.Parameters.AddWithValue("@ParentEmailEncrypted", (object?)entity.ParentEmailEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@IepGoalsEncrypted", (object?)entity.IepGoalsEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@AccessCode", entity.AccessCode);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        entity.StudentId = (Guid)studentIdParam.Value;
        entity.CreatedAt = DateTime.UtcNow;
        return entity;
    }

    public async Task UpdateAsync(Student entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateStudent", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@StudentId", entity.StudentId);
        command.Parameters.AddWithValue("@TherapistId", entity.TherapistId);
        command.Parameters.AddWithValue("@FirstNameEncrypted", entity.FirstNameEncrypted);
        command.Parameters.AddWithValue("@LastNameEncrypted", entity.LastNameEncrypted);
        command.Parameters.AddWithValue("@DateOfBirthEncrypted", (object?)entity.DateOfBirthEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@GradeLevel", 
            string.IsNullOrEmpty(entity.GradeLevel) || !int.TryParse(entity.GradeLevel, out int gradeLevel) 
                ? DBNull.Value 
                : gradeLevel);
        command.Parameters.AddWithValue("@ParentEmailEncrypted", (object?)entity.ParentEmailEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@IepGoalsEncrypted", (object?)entity.IepGoalsEncrypted ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_DeleteStudent", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@StudentId", id);
        command.Parameters.AddWithValue("@TherapistId", DBNull.Value); // Admin access

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Student>> GetByTherapistAsync(Guid therapistId, bool includeInactive = false)
    {
        List<Student> students = new List<Student>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetStudentsByTherapist", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", therapistId);
        command.Parameters.AddWithValue("@IncludeInactive", includeInactive);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            students.Add(MapStudentFromReader(reader));
        }
        
        return students;
    }

    public async Task<IEnumerable<Student>> SearchStudentsAsync(
        Guid therapistId,
        string? searchTerm = null,
        int? gradeLevel = null,
        bool? isActive = null,
        int skip = 0,
        int take = 20)
    {
        List<Student> students = new List<Student>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_SearchStudents", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@TherapistId", therapistId);
        command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        command.Parameters.AddWithValue("@GradeLevel", (object?)gradeLevel ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            students.Add(MapStudentFromReader(reader));
        }
        
        return students;
    }

    public async Task<string> GenerateUniqueAccessCodeAsync()
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GenerateUniqueAccessCode", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter accessCodeParam = new SqlParameter("@AccessCode", SqlDbType.NVarChar, 10);
        accessCodeParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(accessCodeParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        return accessCodeParam.Value?.ToString() ?? throw new InvalidOperationException("Failed to generate access code");
    }

    public async Task<IEnumerable<StudentGoal>> GetStudentGoalsAsync(
        Guid? studentId = null,
        Guid? therapistId = null,
        GoalStatus? status = null,
        string? goalArea = null,
        int skip = 0,
        int take = 20)
    {
        List<StudentGoal> goals = new List<StudentGoal>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetStudentGoals", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@StudentId", (object?)studentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@TherapistId", (object?)therapistId ?? DBNull.Value);
        command.Parameters.AddWithValue("@GoalStatus", (object?)status ?? DBNull.Value);
        command.Parameters.AddWithValue("@GoalArea", (object?)goalArea ?? DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            goals.Add(MapGoalFromReader(reader));
        }
        
        return goals;
    }

    public async Task<StudentGoal> CreateGoalAsync(StudentGoal goal)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateStudentGoal", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        SqlParameter goalIdParam = new SqlParameter("@GoalId", SqlDbType.UniqueIdentifier);
        goalIdParam.Direction = ParameterDirection.Output;
        goalIdParam.Value = goal.GoalId == Guid.Empty ? DBNull.Value : goal.GoalId;
        command.Parameters.Add(goalIdParam);
        
        command.Parameters.AddWithValue("@StudentId", goal.StudentId);
        command.Parameters.AddWithValue("@GoalArea", goal.GoalArea);
        command.Parameters.AddWithValue("@GoalDescription", goal.GoalDescription);
        command.Parameters.AddWithValue("@Objectives", goal.Objectives);
        command.Parameters.AddWithValue("@Baseline", goal.Baseline);
        command.Parameters.AddWithValue("@Target", goal.Target);
        command.Parameters.AddWithValue("@TargetDate", goal.TargetDate);
        command.Parameters.AddWithValue("@Frequency", goal.Frequency);
        command.Parameters.AddWithValue("@Status", (int)goal.Status);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        goal.GoalId = (Guid)goalIdParam.Value;
        goal.CreatedAt = DateTime.UtcNow;
        return goal;
    }

    public async Task UpdateGoalAsync(StudentGoal goal)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateStudentGoal", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@GoalId", goal.GoalId);
        command.Parameters.AddWithValue("@GoalArea", goal.GoalArea);
        command.Parameters.AddWithValue("@GoalDescription", goal.GoalDescription);
        command.Parameters.AddWithValue("@Objectives", goal.Objectives);
        command.Parameters.AddWithValue("@Baseline", goal.Baseline);
        command.Parameters.AddWithValue("@Target", goal.Target);
        command.Parameters.AddWithValue("@CurrentProgress", goal.CurrentProgress);
        command.Parameters.AddWithValue("@TargetDate", goal.TargetDate);
        command.Parameters.AddWithValue("@Frequency", goal.Frequency);
        command.Parameters.AddWithValue("@Status", (int)goal.Status);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteGoalAsync(Guid goalId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_DeleteStudentGoal", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@GoalId", goalId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private Student MapStudentFromReader(SqlDataReader reader)
    {
        Student student = new Student
        {
            StudentId = reader.GetGuid(reader.GetOrdinal("StudentId")),
            TherapistId = reader.GetGuid(reader.GetOrdinal("TherapistId")),
            FirstNameEncrypted = reader.GetString(reader.GetOrdinal("FirstNameEncrypted")),
            LastNameEncrypted = reader.GetString(reader.GetOrdinal("LastNameEncrypted")),
            GradeLevel = reader.IsDBNull(reader.GetOrdinal("GradeLevel")) 
                ? null 
                : reader.GetInt32(reader.GetOrdinal("GradeLevel")).ToString(),
            AccessCode = reader.GetString(reader.GetOrdinal("AccessCode")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("DateOfBirthEncrypted");
        if (!reader.IsDBNull(ordinal))
            student.DateOfBirthEncrypted = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("ParentEmailEncrypted");
        if (!reader.IsDBNull(ordinal))
            student.ParentEmailEncrypted = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("IepGoalsEncrypted");
        if (!reader.IsDBNull(ordinal))
            student.IepGoalsEncrypted = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(ordinal))
            student.UpdatedAt = reader.GetDateTime(ordinal);

        return student;
    }

    private StudentGoal MapGoalFromReader(SqlDataReader reader)
    {
        StudentGoal goal = new StudentGoal
        {
            GoalId = reader.GetGuid(reader.GetOrdinal("GoalId")),
            StudentId = reader.GetGuid(reader.GetOrdinal("StudentId")),
            GoalArea = reader.GetString(reader.GetOrdinal("GoalArea")),
            GoalDescription = reader.GetString(reader.GetOrdinal("GoalDescription")),
            Objectives = reader.GetString(reader.GetOrdinal("Objectives")),
            Baseline = reader.GetInt32(reader.GetOrdinal("Baseline")),
            Target = reader.GetInt32(reader.GetOrdinal("Target")),
            CurrentProgress = reader.GetInt32(reader.GetOrdinal("CurrentProgress")),
            TargetDate = reader.GetDateTime(reader.GetOrdinal("TargetDate")),
            Frequency = reader.GetString(reader.GetOrdinal("Frequency")),
            Status = (GoalStatus)reader.GetInt32(reader.GetOrdinal("Status")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        int ordinal = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(ordinal))
            goal.UpdatedAt = reader.GetDateTime(ordinal);

        return goal;
    }

    // Additional IRepository<Student> interface methods
    public async Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate)
    {
        // Stored procedures don't support expression trees
        // For complex queries, fall back to loading all and filtering in memory
        List<Student> allStudents = (await GetAllAsync()).ToList();
        return allStudents.AsQueryable().Where(predicate).ToList();
    }

    public async Task DeleteAsync(Student entity)
    {
        await DeleteAsync(entity.StudentId);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Student, bool>> predicate)
    {
        List<Student> allStudents = (await GetAllAsync()).ToList();
        return allStudents.AsQueryable().Any(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Student, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return (await GetAllAsync()).Count();
        }
        else
        {
            List<Student> allStudents = (await GetAllAsync()).ToList();
            return allStudents.AsQueryable().Count(predicate);
        }
    }

    public IQueryable<Student> Query()
    {
        throw new NotSupportedException("Query() is not supported with stored procedures. Use specific methods like GetByIdAsync, SearchStudentsAsync, etc.");
    }
}