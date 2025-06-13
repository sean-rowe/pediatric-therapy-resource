using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailVerificationRepository> _logger;

    public EmailVerificationRepository(IConfiguration configuration, ILogger<EmailVerificationRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    public async Task<string> CreateVerificationTokenAsync(Guid userId)
    {
        try
        {
            var token = GenerateSecureToken();
            
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_email_verification_token_create", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddDays(1));
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating verification token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<EmailVerificationToken?> GetTokenAsync(string token)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_email_verification_token_get", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@token", token);
            
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new EmailVerificationToken
                {
                    Id = reader.GetGuid("id"),
                    UserId = reader.GetGuid("user_id"),
                    Token = reader.GetString("token"),
                    ExpiresAt = reader.GetDateTime("expires_at"),
                    UsedAt = reader.IsDBNull("used_at") ? null : reader.GetDateTime("used_at"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification token: {Token}", token);
            throw;
        }
    }

    public async Task<bool> MarkTokenUsedAsync(string token)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_email_verification_token_use", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@token", token);
            
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
            _logger.LogError(ex, "Error marking token as used: {Token}", token);
            throw;
        }
    }

    public async Task<bool> HasValidTokenAsync(Guid userId)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_email_verification_token_has_valid", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@user_id", userId);
            
            var hasValidParam = new SqlParameter("@has_valid", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(hasValidParam);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            
            return (bool)hasValidParam.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking valid token for user: {UserId}", userId);
            throw;
        }
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}