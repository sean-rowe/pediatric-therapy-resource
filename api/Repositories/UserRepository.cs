using Microsoft.Data.SqlClient;
using System.Data;
using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_get_by_email", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@email", email);
            
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_get_by_id", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@user_id", id);
            
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapUserFromReader(reader);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_email_exists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@email", email);
            
            var existsParam = new SqlParameter("@exists", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(existsParam);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return (bool)existsParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email exists: {Email}", email);
            throw;
        }
    }

    public async Task<bool> LicenseExistsAsync(string licenseNumber, string licenseState)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_license_exists", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@license_number", licenseNumber);
            command.Parameters.AddWithValue("@license_state", licenseState);
            
            var existsParam = new SqlParameter("@exists", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(existsParam);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return (bool)existsParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking license exists: {LicenseNumber}, {State}", licenseNumber, licenseState);
            throw;
        }
    }

    public async Task<Guid> CreateUserAsync(User user)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_register", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
            command.Parameters.AddWithValue("@first_name", user.FirstName);
            command.Parameters.AddWithValue("@last_name", user.LastName);
            command.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@license_number", (object?)user.LicenseNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@license_state", (object?)user.LicenseState ?? DBNull.Value);
            command.Parameters.AddWithValue("@service_type", user.ServiceType);
            
            var userIdParam = new SqlParameter("@user_id", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(userIdParam);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return (Guid)userIdParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_update", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@user_id", user.Id);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@first_name", user.FirstName);
            command.Parameters.AddWithValue("@last_name", user.LastName);
            command.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@email_verified", user.EmailVerified);
            command.Parameters.AddWithValue("@status", user.Status);
            command.Parameters.AddWithValue("@is_active", user.IsActive);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> VerifyEmailAsync(Guid userId)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_user_verify_email", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@user_id", userId);
            
            var successParam = new SqlParameter("@success", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(successParam);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return (bool)successParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for user: {UserId}", userId);
            throw;
        }
    }

    private static User MapUserFromReader(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetGuid("id"),
            Email = reader.GetString("email"),
            PasswordHash = reader.GetString("password_hash"),
            FirstName = reader.GetString("first_name"),
            LastName = reader.GetString("last_name"),
            Role = reader.GetString("role"),
            LicenseNumber = reader.IsDBNull("license_number") ? null : reader.GetString("license_number"),
            LicenseState = reader.IsDBNull("license_state") ? null : reader.GetString("license_state"),
            ServiceType = reader.GetString("service_type"),
            SubscriptionTier = reader.GetString("subscription_tier"),
            SubscriptionExpires = reader.IsDBNull("subscription_expires") ? null : reader.GetDateTime("subscription_expires"),
            MonthlyContentGenerated = reader.GetInt32("monthly_content_generated"),
            ContentGenerationLimit = reader.GetInt32("content_generation_limit"),
            Timezone = reader.GetString("timezone"),
            PreferredNoteTemplate = reader.GetString("preferred_note_template"),
            AutoSaveNotes = reader.GetBoolean("auto_save_notes"),
            OfflineSyncEnabled = reader.GetBoolean("offline_sync_enabled"),
            PushNotifications = reader.GetBoolean("push_notifications"),
            IsActive = reader.GetBoolean("is_active"),
            EmailVerified = reader.GetBoolean("email_verified"),
            LastLogin = reader.IsDBNull("last_login") ? null : reader.GetDateTime("last_login"),
            CreatedAt = reader.GetDateTime("created_at"),
            UpdatedAt = reader.GetDateTime("updated_at"),
            Phone = reader.IsDBNull("phone") ? null : reader.GetString("phone"),
            Status = reader.GetString("status")
        };
    }
}