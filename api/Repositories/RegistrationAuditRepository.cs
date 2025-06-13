using Microsoft.Data.SqlClient;
using System.Data;

namespace TherapyDocs.Api.Repositories;

public class RegistrationAuditRepository : IRegistrationAuditRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegistrationAuditRepository> _logger;

    public RegistrationAuditRepository(IConfiguration configuration, ILogger<RegistrationAuditRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    public async Task LogRegistrationAttemptAsync(string email, string? licenseNumber, string? licenseState, 
        bool success, string? failureReason, string? ipAddress, string? userAgent)
    {
        try
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var command = new SqlCommand("sp_registration_audit_log", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@license_number", (object?)licenseNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@license_state", (object?)licenseState ?? DBNull.Value);
            command.Parameters.AddWithValue("@success", success);
            command.Parameters.AddWithValue("@failure_reason", (object?)failureReason ?? DBNull.Value);
            command.Parameters.AddWithValue("@ip_address", (object?)ipAddress ?? DBNull.Value);
            command.Parameters.AddWithValue("@user_agent", (object?)userAgent ?? DBNull.Value);
            
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging registration attempt for email: {Email}", email);
            // Don't throw here as this is audit logging - we don't want to fail registration due to audit issues
        }
    }
}