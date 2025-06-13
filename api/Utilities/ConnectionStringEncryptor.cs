using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TherapyDocs.Api.Utilities;

// Utility program to encrypt connection strings
public class ConnectionStringEncryptor
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run --project ConnectionStringEncryptor <connection-string>");
            Console.WriteLine("Example: dotnet run --project ConnectionStringEncryptor \"Server=localhost;Database=TherapyDocs;User Id=sa;Password=YourPassword123!\"");
            return;
        }

        var connectionString = args[0];
        
        // Build a minimal host for data protection
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDataProtection()
                    .SetApplicationName("TherapyDocs")
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "TherapyDocs",
                        "DataProtection-Keys")));
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector("ConnectionStrings");
            
            var encrypted = protector.Protect(connectionString);
            
            Console.WriteLine("Original connection string:");
            Console.WriteLine(MaskPassword(connectionString));
            Console.WriteLine();
            Console.WriteLine("Encrypted connection string (add this to appsettings.json):");
            Console.WriteLine($"\"DefaultConnection\": \"encrypted:{encrypted}\"");
            Console.WriteLine();
            Console.WriteLine("To verify decryption:");
            
            try
            {
                var decrypted = protector.Unprotect(encrypted);
                Console.WriteLine("Decryption successful: " + (decrypted == connectionString ? "✓" : "✗"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption failed: {ex.Message}");
            }
        }
    }
    
    private static string MaskPassword(string connectionString)
    {
        // Mask password in connection string for display
        var parts = connectionString.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("Password", StringComparison.OrdinalIgnoreCase) ||
                parts[i].Trim().StartsWith("Pwd", StringComparison.OrdinalIgnoreCase))
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length == 2)
                {
                    parts[i] = $"{keyValue[0]}=****";
                }
            }
        }
        return string.Join(";", parts);
    }
}