using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models;

namespace TherapyDocs.Api.Repositories;

public class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PasswordHistoryRepository> _logger;

    public PasswordHistoryRepository(IConfiguration configuration, ILogger<PasswordHistoryRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
        _logger = logger;
    }

    public async Task<bool> IsPasswordReusedAsync(Guid userId, string passwordHash)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@user_id", userId);
            parameters.Add("@password_hash", passwordHash);
            parameters.Add("@is_reused", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_check_password_history", parameters, 
                commandType: CommandType.StoredProcedure);

            return parameters.Get<bool>("@is_reused");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password history for user {UserId}", userId);
            throw;
        }
    }

    public async Task AddPasswordToHistoryAsync(Guid userId, string passwordHash)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("sp_add_password_history",
                new
                {
                    user_id = userId,
                    password_hash = passwordHash
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding password to history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PasswordChangeRequirement> CheckPasswordChangeRequiredAsync(Guid userId)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@user_id", userId);
            parameters.Add("@change_required", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@days_until_expiry", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_check_password_change_required", parameters, 
                commandType: CommandType.StoredProcedure);

            return new PasswordChangeRequirement
            {
                ChangeRequired = parameters.Get<bool>("@change_required"),
                DaysUntilExpiry = parameters.Get<int>("@days_until_expiry")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password change requirement for user {UserId}", userId);
            throw;
        }
    }
}