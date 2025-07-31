using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UPTRMS.Api.Data;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.Mocks;

namespace UPTRMS.Api.Tests.BDD;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove any existing DbContext registration
            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryDbForTesting_{Guid.NewGuid()}");
            });

            // Replace real services with mocks
            var authServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthenticationService));
            if (authServiceDescriptor != null)
            {
                services.Remove(authServiceDescriptor);
            }
            services.AddScoped<IAuthenticationService, MockAuthenticationService>();

            // Replace IUserRepository with mock
            var userRepoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(UPTRMS.Api.Repositories.IUserRepository));
            if (userRepoDescriptor != null)
            {
                services.Remove(userRepoDescriptor);
            }
            services.AddScoped<UPTRMS.Api.Repositories.IUserRepository, MockUserRepository>();

            // Replace other services with mocks
            var emailServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));
            if (emailServiceDescriptor != null)
            {
                services.Remove(emailServiceDescriptor);
            }
            services.AddScoped<IEmailService, MockEmailService>();

            var tokenServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITokenService));
            if (tokenServiceDescriptor != null)
            {
                services.Remove(tokenServiceDescriptor);
            }
            services.AddScoped<ITokenService, MockTokenService>();

            // IPasswordService doesn't exist in the current API
            // Commenting out for now
            // var passwordServiceDescriptor = services.SingleOrDefault(
            //     d => d.ServiceType == typeof(Services.IPasswordService));
            // if (passwordServiceDescriptor != null)
            // {
            //     services.Remove(passwordServiceDescriptor);
            // }
        });
    }
}