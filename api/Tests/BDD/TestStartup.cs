using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UPTRMS.Api.Data;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Tests.BDD;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase("TestDb");
                // The encryption service is required in the constructor
                var encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();
            });

            // Ensure encryption service is registered
            services.AddScoped<IEncryptionService, EncryptionService>();
            
            // Replace AuthenticationService with MockAuthenticationService for tests
            ServiceDescriptor? authDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthenticationService));
            if (authDescriptor != null)
            {
                services.Remove(authDescriptor);
            }
            services.AddScoped<IAuthenticationService, UPTRMS.Api.Tests.Mocks.MockAuthenticationService>();

            // Configure JSON options for both minimal API and MVC
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });
            
            // Also configure MVC JSON options for controller model binding
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            // Build the service provider
            System.IServiceProvider sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices
                    .GetRequiredService<ILogger<TestWebApplicationFactory<TProgram>>>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data if needed
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                }
            }
        });

        builder.UseEnvironment("Test");
    }
}