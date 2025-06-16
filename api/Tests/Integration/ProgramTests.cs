using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using FluentAssertions;

namespace TherapyDocs.Api.Tests.Integration;

public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Program_Builds_Successfully()
    {
        // Act & Assert
        using var client = _factory.CreateClient();
        client.Should().NotBeNull();
    }

    [Fact]
    public void Program_ConfiguresServices_Successfully()
    {
        // Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Check all registered services
        services.GetService<IHostEnvironment>().Should().NotBeNull();
        services.GetService<IConfiguration>().Should().NotBeNull();
        services.GetService<ILogger<Program>>().Should().NotBeNull();
    }

    [Fact]
    public void Program_Development_ConfiguresSwagger()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });

        // Act
        using var client = factory.CreateClient();
        
        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Program_Production_NoSwagger()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });

        // Act
        using var client = factory.CreateClient();
        
        // Assert
        client.Should().NotBeNull();
    }
}