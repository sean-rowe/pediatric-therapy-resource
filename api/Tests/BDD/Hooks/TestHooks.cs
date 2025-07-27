using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechTalk.SpecFlow;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.BDD.Mocks;
using UPTRMS.Api.Tests.Mocks;

namespace UPTRMS.Api.Tests.BDD.Hooks
{
    [Binding]
    public class TestHooks
    {
        private static WebApplicationFactory<Program>? _factory;
        
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Set environment variables for JWT configuration before creating the factory
            Environment.SetEnvironmentVariable("Jwt__Secret", "super-secret-key-that-is-at-least-32-characters-long-for-testing-purposes");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "TherapyDocsTest");
            Environment.SetEnvironmentVariable("Jwt__Audience", "TherapyDocsTest");
            Environment.SetEnvironmentVariable("Encryption__Key", "test-encryption-key-for-unit-tests-32bytes-long!");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
            
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                    
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Clear existing configuration sources to ensure our test config takes precedence
                        config.Sources.Clear();
                        
                        // Add test configuration
                        config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["Encryption:Key"] = "test-encryption-key-for-unit-tests-32bytes-long!",
                            ["Jwt:Secret"] = "super-secret-key-that-is-at-least-32-characters-long-for-testing-purposes",
                            ["Jwt:Issuer"] = "TherapyDocsTest",
                            ["Jwt:Audience"] = "TherapyDocsTest"
                        });
                        
                        // Add the test appsettings file
                        config.AddJsonFile("appsettings.Test.json", optional: true);
                    });
                    
                    builder.ConfigureServices(services =>
                    {
                        // Remove any existing EncryptionService registration
                        var encryptionDescriptors = services.Where(
                            d => d.ServiceType == typeof(IEncryptionService)).ToList();
                        foreach (var descriptor in encryptionDescriptors)
                        {
                            services.Remove(descriptor);
                        }
                        
                        // Add mock encryption service as singleton to ensure it's available early
                        services.AddSingleton<IEncryptionService, MockEncryptionService>();
                        
                        // Remove any existing AuthenticationService registration
                        var authDescriptors = services.Where(
                            d => d.ServiceType == typeof(IAuthenticationService)).ToList();
                        foreach (var descriptor in authDescriptors)
                        {
                            services.Remove(descriptor);
                        }
                        
                        // Add mock authentication service
                        services.AddScoped<IAuthenticationService, MockAuthenticationService>();
                        
                        // Remove the app's ApplicationDbContext registration
                        var dbContextDescriptor = services.SingleOrDefault(
                            d => d.ServiceType ==
                                typeof(DbContextOptions<UPTRMS.Api.Data.ApplicationDbContext>));

                        if (dbContextDescriptor != null)
                        {
                            services.Remove(dbContextDescriptor);
                        }

                        // Add ApplicationDbContext using an in-memory database for tests
                        services.AddDbContext<UPTRMS.Api.Data.ApplicationDbContext>((serviceProvider, options) =>
                        {
                            options.UseInMemoryDatabase("InMemoryDbForTesting");
                            // Ensure the encryption service is available for the context
                            options.UseApplicationServiceProvider(serviceProvider);
                        });
                    });
                });
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _factory?.Dispose();
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            // Clear MockAuthenticationService state before each scenario
            MockAuthenticationService.ClearState();
            
            scenarioContext["Factory"] = _factory;
            
            // Create a new HttpClient for each scenario
            var client = _factory!.CreateClient();
            scenarioContext["HttpClient"] = client;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            // Cleanup after scenario if needed
        }
    }
}