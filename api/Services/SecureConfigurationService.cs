using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using TherapyDocs.Api.Interfaces;

namespace TherapyDocs.Api.Services;

public class SecureConfigurationService : ISecureConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _protector;
    private readonly ILogger<SecureConfigurationService> _logger;

    public SecureConfigurationService(
        IConfiguration configuration,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SecureConfigurationService> logger)
    {
        _configuration = configuration;
        _protector = dataProtectionProvider.CreateProtector("ConnectionStrings");
        _logger = logger;
    }

    public string GetConnectionString(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{name}' not found");
        }

        // Check if connection string is encrypted (starts with "encrypted:")
        if (connectionString.StartsWith("encrypted:"))
        {
            try
            {
                var encryptedValue = connectionString.Substring("encrypted:".Length);
                connectionString = _protector.Unprotect(encryptedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt connection string '{Name}'", name);
                throw new InvalidOperationException($"Failed to decrypt connection string '{name}'", ex);
            }
        }

        return connectionString;
    }

    public string EncryptConnectionString(string connectionString)
    {
        return "encrypted:" + _protector.Protect(connectionString);
    }
}

// Extension method to register secure configuration
public static class SecureConfigurationExtensions
{
    public static IServiceCollection AddSecureConfiguration(this IServiceCollection services)
    {
        services.AddDataProtection()
            .SetApplicationName("TherapyDocs")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TherapyDocs",
                "DataProtection-Keys")));

        services.AddSingleton<ISecureConfigurationService, SecureConfigurationService>();
        
        return services;
    }
}