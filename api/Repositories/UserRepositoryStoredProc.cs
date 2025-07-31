using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using Newtonsoft.Json;

namespace UPTRMS.Api.Repositories;

/// <summary>
/// User repository implementation using stored procedures
/// </summary>
public class UserRepositoryStoredProc : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public UserRepositoryStoredProc(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUserById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", id);
        command.Parameters.AddWithValue("@IncludeDeleted", false);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUsersByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    public async Task<User> AddAsync(User entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateUser", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Email", entity.Email);
        command.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
        command.Parameters.AddWithValue("@FirstName", entity.FirstName);
        command.Parameters.AddWithValue("@LastName", entity.LastName);
        command.Parameters.AddWithValue("@LicenseNumber", (object?)entity.LicenseNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@LicenseState", entity.LicenseState);
        command.Parameters.AddWithValue("@LicenseType", (object?)entity.LicenseType ?? DBNull.Value);
        command.Parameters.AddWithValue("@Languages", JsonConvert.SerializeObject(entity.Languages));
        command.Parameters.AddWithValue("@Specialties", JsonConvert.SerializeObject(entity.Specialties));
        
        SqlParameter userIdParam = new SqlParameter("@UserId", SqlDbType.UniqueIdentifier);
        userIdParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(userIdParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        entity.UserId = (Guid)userIdParam.Value;
        return entity;
    }

    public async Task UpdateAsync(User entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateUser", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@UserId", entity.UserId);
        command.Parameters.AddWithValue("@FirstName", entity.FirstName);
        command.Parameters.AddWithValue("@LastName", entity.LastName);
        command.Parameters.AddWithValue("@LicenseNumber", (object?)entity.LicenseNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@LicenseState", entity.LicenseState);
        command.Parameters.AddWithValue("@LicenseType", (object?)entity.LicenseType ?? DBNull.Value);
        command.Parameters.AddWithValue("@Languages", JsonConvert.SerializeObject(entity.Languages));
        command.Parameters.AddWithValue("@Specialties", JsonConvert.SerializeObject(entity.Specialties));
        command.Parameters.AddWithValue("@PreferredLanguage", entity.PreferredLanguage);
        command.Parameters.AddWithValue("@Timezone", (object?)entity.Timezone ?? DBNull.Value);
        command.Parameters.AddWithValue("@EmailNotificationsEnabled", entity.EmailNotificationsEnabled);
        command.Parameters.AddWithValue("@Theme", (object?)entity.Theme ?? DBNull.Value);
        command.Parameters.AddWithValue("@DefaultView", (object?)entity.DefaultView ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_SoftDeleteUser", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@UserId", id);
        command.Parameters.AddWithValue("@DeletedByUserId", DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUsersByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SearchTerm", email);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", 1);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            User user = MapUserFromReader(reader);
            // Ensure exact email match (stored proc does LIKE search)
            if (user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }
        }
        
        return null;
    }

    public async Task<User?> GetByIdWithOrganizationAsync(Guid userId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUserWithOrganization", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            User user = MapUserFromReader(reader);
            
            // Map organization if present
            if (!reader.IsDBNull(reader.GetOrdinal("Org_OrganizationId")))
            {
                user.Organization = new Organization
                {
                    OrganizationId = reader.GetGuid(reader.GetOrdinal("Org_OrganizationId")),
                    Name = reader.GetString(reader.GetOrdinal("Org_Name")),
                    Type = Enum.Parse<OrganizationType>(reader.GetString(reader.GetOrdinal("Org_Type"))),
                    SubscriptionTier = (SubscriptionTier)reader.GetInt32(reader.GetOrdinal("Org_SubscriptionTier")),
                    LicenseCount = reader.GetInt32(reader.GetOrdinal("Org_LicenseCount")),
                    UsedLicenses = reader.GetInt32(reader.GetOrdinal("Org_UsedLicenses")),
                    SsoEnabled = reader.GetBoolean(reader.GetOrdinal("Org_SsoEnabled")),
                    SsoProvider = reader.IsDBNull(reader.GetOrdinal("Org_SsoProvider")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("Org_SsoProvider")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("Org_IsActive"))
                };
            }
            
            return user;
        }
        
        return null;
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(Guid organizationId)
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUsersByOrganization", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@OrganizationId", organizationId);
        command.Parameters.AddWithValue("@IncludeInactive", false);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    public async Task<IEnumerable<User>> GetSellersAsync(bool approvedOnly = true)
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetSellers", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ApprovedOnly", approvedOnly);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        User? existingUser = await GetByEmailAsync(email);
        
        if (existingUser == null)
            return true;
            
        if (excludeUserId.HasValue && existingUser.UserId == excludeUserId.Value)
            return true;
            
        return false;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20)
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUsersByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SearchTerm", searchTerm);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    public async Task<IEnumerable<User>> GetAllAsync(int skip, int take)
    {
        List<User> users = new List<User>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUsersByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }
        
        return users;
    }

    private User MapUserFromReader(SqlDataReader reader)
    {
        User user = new User
        {
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            SubscriptionTier = (SubscriptionTier)reader.GetInt32(reader.GetOrdinal("SubscriptionTier")),
            SubscriptionStatus = (SubscriptionStatus)reader.GetInt32(reader.GetOrdinal("SubscriptionStatus")),
            Role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            EmailVerified = reader.GetBoolean(reader.GetOrdinal("EmailVerified")),
            TwoFactorEnabled = reader.GetBoolean(reader.GetOrdinal("TwoFactorEnabled")),
            PreferredLanguage = reader.GetString(reader.GetOrdinal("PreferredLanguage")),
            LicenseState = reader.GetString(reader.GetOrdinal("LicenseState")),
            IsSellerApproved = reader.GetBoolean(reader.GetOrdinal("IsSellerApproved")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            EmailNotificationsEnabled = reader.GetBoolean(reader.GetOrdinal("EmailNotificationsEnabled"))
        };

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("LicenseNumber");
        if (!reader.IsDBNull(ordinal))
            user.LicenseNumber = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("LicenseType");
        if (!reader.IsDBNull(ordinal))
            user.LicenseType = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("LicenseVerified");
        user.LicenseVerified = reader.GetBoolean(ordinal);
        
        ordinal = reader.GetOrdinal("LicenseVerifiedAt");
        if (!reader.IsDBNull(ordinal))
            user.LicenseVerifiedAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("LicenseExpirationDate");
        if (!reader.IsDBNull(ordinal))
            user.LicenseExpirationDate = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("OrganizationId");
        if (!reader.IsDBNull(ordinal))
            user.OrganizationId = reader.GetGuid(ordinal);
            
        ordinal = reader.GetOrdinal("SubscriptionStartDate");
        if (!reader.IsDBNull(ordinal))
            user.SubscriptionStartDate = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("SubscriptionEndDate");
        if (!reader.IsDBNull(ordinal))
            user.SubscriptionEndDate = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("LastLoginAt");
        if (!reader.IsDBNull(ordinal))
            user.LastLoginAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("DeletedAt");
        if (!reader.IsDBNull(ordinal))
            user.DeletedAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("ProfileImageUrl");
        if (!reader.IsDBNull(ordinal))
            user.ProfileImageUrl = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("RefreshToken");
        if (!reader.IsDBNull(ordinal))
            user.RefreshToken = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("RefreshTokenExpiresAt");
        if (!reader.IsDBNull(ordinal))
            user.RefreshTokenExpiresAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("Timezone");
        if (!reader.IsDBNull(ordinal))
            user.Timezone = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("Theme");
        if (!reader.IsDBNull(ordinal))
            user.Theme = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("DefaultView");
        if (!reader.IsDBNull(ordinal))
            user.DefaultView = reader.GetString(ordinal);

        // JSON fields
        ordinal = reader.GetOrdinal("Languages");
        string languagesJson = reader.GetString(ordinal);
        user.Languages = JsonConvert.DeserializeObject<List<string>>(languagesJson) ?? new List<string>();
        
        ordinal = reader.GetOrdinal("Specialties");
        string specialtiesJson = reader.GetString(ordinal);
        user.Specialties = JsonConvert.DeserializeObject<List<string>>(specialtiesJson) ?? new List<string>();

        return user;
    }

    // Additional IRepository<User> interface methods
    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    {
        // Stored procedures don't support expression trees
        // For complex queries, fall back to loading all and filtering in memory
        // In production, create specific stored procedures for common queries
        List<User> allUsers = (await GetAllAsync()).ToList();
        return allUsers.AsQueryable().Where(predicate).ToList();
    }

    public async Task DeleteAsync(User entity)
    {
        await DeleteAsync(entity.UserId);
    }

    public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
    {
        // Similar limitation as FindAsync
        List<User> allUsers = (await GetAllAsync()).ToList();
        return allUsers.AsQueryable().Any(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            // Could create sp_GetUserCount for better performance
            return (await GetAllAsync()).Count();
        }
        else
        {
            List<User> allUsers = (await GetAllAsync()).ToList();
            return allUsers.AsQueryable().Count(predicate);
        }
    }

    public IQueryable<User> Query()
    {
        // Stored procedures don't support IQueryable
        // This is a limitation of the stored procedure approach
        throw new NotSupportedException("Query() is not supported with stored procedures. Use specific methods like GetByIdAsync, SearchUsersAsync, etc.");
    }
}