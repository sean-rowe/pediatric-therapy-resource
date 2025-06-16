using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Models.DTOs;

namespace TherapyDocs.Api.Repositories;

public class AccountLockoutRepository : IAccountLockoutRepository
{
    private readonly string _connectionString;
    private readonly ILogger<AccountLockoutRepository> _logger;

    public AccountLockoutRepository(IConfiguration configuration, ILogger<AccountLockoutRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
        _logger = logger;
    }

    public async Task RecordFailedLoginAttemptAsync(string email, string? ipAddress, string? userAgent)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("sp_record_failed_login",
                new
                {
                    email,
                    ip_address = ipAddress,
                    user_agent = userAgent
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording failed login attempt");
            throw;
        }
    }

    public async Task<AccountLockoutStatus> CheckAccountLockoutAsync(string email)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@email", email);
            parameters.Add("@is_locked", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@locked_until", dbType: DbType.DateTime2, direction: ParameterDirection.Output);
            parameters.Add("@remaining_attempts", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_check_account_lockout", parameters, 
                commandType: CommandType.StoredProcedure);

            return new AccountLockoutStatus
            {
                IsLocked = parameters.Get<bool>("@is_locked"),
                LockedUntil = parameters.Get<DateTime?>("@locked_until"),
                RemainingAttempts = parameters.Get<int>("@remaining_attempts")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking account lockout status");
            throw;
        }
    }

    public async Task ClearFailedLoginAttemptsAsync(string email)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("sp_clear_failed_login_attempts",
                new { email },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing failed login attempts");
            throw;
        }
    }
}