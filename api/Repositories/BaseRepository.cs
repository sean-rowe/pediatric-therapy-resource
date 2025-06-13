using Microsoft.Data.SqlClient;

namespace TherapyDocs.Api.Repositories;

public abstract class BaseRepository
{
    protected readonly string ConnectionString;
    protected readonly ILogger Logger;

    protected BaseRepository(ISecureConfigurationService secureConfiguration, ILogger logger)
    {
        ConnectionString = secureConfiguration.GetConnectionString("DefaultConnection");
        Logger = logger;
    }

    protected SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}