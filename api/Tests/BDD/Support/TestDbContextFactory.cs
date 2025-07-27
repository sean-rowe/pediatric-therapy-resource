using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UPTRMS.Api.Data;
using UPTRMS.Api.Services;
using UPTRMS.Api.Tests.BDD.Mocks;

namespace UPTRMS.Api.Tests.BDD.Support;

public class TestDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseInMemoryDatabase("TestDb");

        // Create a mock encryption service for tests
        var encryptionService = new MockEncryptionService();

        return new ApplicationDbContext(optionsBuilder.Options, encryptionService);
    }
}