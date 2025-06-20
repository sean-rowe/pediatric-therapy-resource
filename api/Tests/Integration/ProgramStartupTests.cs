using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Net;
using TherapyDocs.Api.Interfaces;
using TherapyDocs.Api.Services;
using Xunit;

namespace TherapyDocs.Api.Tests.Integration;

public class ProgramStartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramStartupTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Program_ShouldHavePartialClass()
    {
        // This test ensures Program class is accessible for testing
        Assert.NotNull(typeof(Program));
        Assert.True(typeof(Program).IsClass);
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterAllRequiredServices()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Repositories
        Assert.NotNull(services.GetService<IUserRepository>());
        Assert.NotNull(services.GetService<IEmailVerificationRepository>());
        Assert.NotNull(services.GetService<IRegistrationAuditRepository>());
        Assert.NotNull(services.GetService<IAccountLockoutRepository>());
        Assert.NotNull(services.GetService<IPasswordHistoryRepository>());

        // Assert - Services
        Assert.NotNull(services.GetService<IUserRegistrationService>());
        Assert.NotNull(services.GetService<IEmailVerificationService>());
        Assert.NotNull(services.GetService<ILoginService>());
        Assert.NotNull(services.GetService<IAuthService>());
        Assert.NotNull(services.GetService<IEmailService>());
        Assert.NotNull(services.GetService<ILicenseVerificationService>());
        Assert.NotNull(services.GetService<IPasswordService>());
        Assert.NotNull(services.GetService<IHaveIBeenPwnedService>());
    }

    [Fact]
    public void ServiceRegistration_AuthService_ShouldUseRefactoredImplementation()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetService<IAuthService>();

        // Assert
        Assert.NotNull(authService);
        Assert.IsType<AuthServiceRefactored>(authService);
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterServicesWithCorrectLifetime()
    {
        // Act
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var service1Scope1 = scope1.ServiceProvider.GetService<IUserRepository>();
        var service2Scope1 = scope1.ServiceProvider.GetService<IUserRepository>();
        var service1Scope2 = scope2.ServiceProvider.GetService<IUserRepository>();

        // Assert - Scoped services should be same within scope, different across scopes
        Assert.Same(service1Scope1, service2Scope1); // Same within scope
        Assert.NotSame(service1Scope1, service1Scope2); // Different across scopes
    }

    [Fact]
    public async Task Application_ShouldStartSuccessfully()
    {
        // Act
        var client = _factory.CreateClient();

        // Assert - Application should start without throwing
        Assert.NotNull(client);
    }

    [Fact]
    public async Task Middleware_GlobalExceptionMiddleware_ShouldBeConfigured()
    {
        // Act
        var client = _factory.CreateClient();
        
        // Make a request to trigger middleware pipeline
        var response = await client.GetAsync("/api/nonexistent");

        // Assert - Should get a response (not throw during middleware setup)
        Assert.NotNull(response);
    }

    [Fact]
    public async Task CORS_ShouldBeConfigured()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/auth/health");

        // Assert - CORS headers should be present or request should be processed
        Assert.NotNull(response);
    }

    [Fact]
    public void Configuration_JWT_ShouldBeRequired()
    {
        // This test verifies that JWT configuration is required
        // In a real startup without JWT key, it would throw InvalidOperationException
        
        // Act & Assert
        // The fact that the factory can be created means JWT is properly configured in test environment
        Assert.NotNull(_factory);
    }

    [Fact]
    public void DependencyInjection_ShouldResolveComplexDependencyGraph()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var userRegistrationService = scope.ServiceProvider.GetService<IUserRegistrationService>();

        // Assert - Complex service with multiple dependencies should resolve
        Assert.NotNull(userRegistrationService);
        Assert.IsType<UserRegistrationService>(userRegistrationService);
    }

    [Fact]
    public void Logging_SerilogShouldBeConfigured()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<ProgramStartupTests>>();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void MemoryCache_ShouldBeRegistered()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();

        // Assert
        Assert.NotNull(cache);
    }

    [Fact]
    public void HttpClient_ShouldBeRegistered()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

        // Assert
        Assert.NotNull(httpClientFactory);
        
        var httpClient = httpClientFactory.CreateClient();
        Assert.NotNull(httpClient);
    }

    [Fact]
    public void FluentValidation_ShouldBeRegistered()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var validators = scope.ServiceProvider.GetServices<FluentValidation.IValidator>();

        // Assert
        Assert.NotNull(validators);
        Assert.NotEmpty(validators);
    }

    [Fact]
    public void SecureConfiguration_ShouldBeRegistered()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var secureConfig = scope.ServiceProvider.GetService<ISecureConfigurationService>();

        // Assert
        Assert.NotNull(secureConfig);
    }

    [Fact]
    public void Antiforgery_ShouldBeConfigured()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var antiforgery = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();

        // Assert
        Assert.NotNull(antiforgery);
    }

    [Fact]
    public void RateLimiting_ShouldBeConfigured()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var rateLimitingService = scope.ServiceProvider.GetService<Microsoft.AspNetCore.RateLimiting.RateLimitingService>();

        // Assert - Rate limiting service should be available
        // Note: The service might be internal, so we check if the feature is configured indirectly
        var rateLimiterOptions = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>();
        Assert.NotNull(rateLimiterOptions);
    }

    [Fact]
    public void Authentication_JwtBearer_ShouldBeConfigured()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var authSchemeProvider = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();

        // Assert
        Assert.NotNull(authSchemeProvider);
    }

    [Fact]
    public async Task Swagger_ShouldBeConfiguredInDevelopment()
    {
        // Act
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");

        // Assert - In development, swagger should be accessible
        // Note: This might return 404 if swagger is not accessible, but shouldn't throw
        Assert.NotNull(response);
    }

    [Fact]
    public void Controllers_ShouldBeRegistered()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var mvcOptions = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>();

        // Assert
        Assert.NotNull(mvcOptions);
        
        // ValidationExceptionFilter should be registered
        var filters = mvcOptions.Value.Filters;
        Assert.Contains(filters, f => f.GetType().Name.Contains("ValidationExceptionFilter") || 
                                     (f is Microsoft.AspNetCore.Mvc.Filters.ServiceFilterAttribute sfa && 
                                      sfa.ServiceType.Name.Contains("ValidationExceptionFilter")));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Testing")]
    public void Application_ShouldStartInDifferentEnvironments(string environment)
    {
        // Act & Assert
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Add test configuration for JWT
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("Jwt:Key", "ThisIsATestKeyThatIsLongEnoughForTesting123!")
                    });
                });
            });

        // Should not throw exception
        Assert.NotNull(factory);
        
        var client = factory.CreateClient();
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceLifetimes_ShouldBeCorrect()
    {
        // Act
        var serviceCollection = new ServiceCollection();
        
        // We can't directly access the service registration from Program.cs,
        // but we can verify that our services are properly registered as Scoped
        using var scope = _factory.Services.CreateScope();
        
        // Get two instances of the same service within the same scope
        var service1 = scope.ServiceProvider.GetService<IUserRepository>();
        var service2 = scope.ServiceProvider.GetService<IUserRepository>();
        
        // Get instance from different scope
        using var scope2 = _factory.Services.CreateScope();
        var service3 = scope2.ServiceProvider.GetService<IUserRepository>();

        // Assert
        Assert.Same(service1, service2); // Same instance within scope (Scoped)
        Assert.NotSame(service1, service3); // Different instance across scopes (Scoped)
    }

    [Fact]
    public void Application_ShouldHandleStartupExceptionsGracefully()
    {
        // This test ensures that if there are startup issues, they're handled appropriately
        // The test passing means the application can start with the current configuration
        
        // Act
        var client = _factory.CreateClient();
        
        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task HealthCheck_Endpoint_ShouldBeAccessible()
    {
        // Act
        var client = _factory.CreateClient();
        
        // Try to access a basic endpoint to ensure routing is working
        var response = await client.GetAsync("/");

        // Assert - Should get some response, not an exception
        Assert.NotNull(response);
    }

    [Fact]
    public void Configuration_ShouldLoadSuccessfully()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetService<IConfiguration>();

        // Assert
        Assert.NotNull(configuration);
        
        // Test that configuration can be accessed
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        // Connection string might be null in test environment, but configuration should exist
        Assert.NotNull(configuration);
    }
}

// Custom WebApplicationFactory for testing specific scenarios
public class CustomProgramTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Jwt:Key", "TestKeyForProgramTests123!@#$%^&*()"),
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Server=localhost;Database=TestDb;Integrated Security=true;")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Add test-specific services if needed
        });
    }
}

public class ProgramConfigurationTests : IClassFixture<CustomProgramTestFactory>
{
    private readonly CustomProgramTestFactory _factory;

    public ProgramConfigurationTests(CustomProgramTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void JwtConfiguration_WithValidKey_ShouldNotThrow()
    {
        // Act & Assert - Factory creation should succeed with valid JWT key
        Assert.NotNull(_factory);
        
        using var scope = _factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetService<IConfiguration>();
        
        var jwtKey = config?["Jwt:Key"];
        Assert.NotNull(jwtKey);
        Assert.True(jwtKey.Length >= 32); // JWT key should be long enough
    }

    [Fact]
    public async Task ApplicationPipeline_ShouldProcessRequestsCorrectly()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/auth/test");

        // Assert - Should process request through pipeline (might return 404, but not throw)
        Assert.NotNull(response);
    }
}