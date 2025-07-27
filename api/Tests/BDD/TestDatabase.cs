using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UPTRMS.Api.Tests.BDD;

public class TestDatabase
{
    private readonly string _connectionString;
    
    public TestDatabase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TestDatabase") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=TherapyDocsTest;Trusted_Connection=true;TrustServerCertificate=true";
    }
    
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
    
    public async Task ResetDatabase()
    {
        using var connection = CreateConnection();
        connection.Open();
        
        // Clear test data
        var commands = new[]
        {
            "DELETE FROM audit_log",
            "DELETE FROM registration_audit_log", 
            "DELETE FROM password_history",
            "DELETE FROM email_verification_tokens",
            "DELETE FROM users"
        };
        
        foreach (var sql in commands)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            try
            {
                await Task.Run(() => command.ExecuteNonQuery());
            }
            catch
            {
                // Table might not exist yet
            }
        }
    }
    
    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }
        
        using var reader = await Task.Run(() => command.ExecuteReader());
        if (reader.Read())
        {
            // Simple mapping - in production use Dapper
            return (T)reader[0];
        }
        
        return default!;
    }
    
    private void AddParameters(IDbCommand command, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{prop.Name}";
            parameter.Value = prop.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}