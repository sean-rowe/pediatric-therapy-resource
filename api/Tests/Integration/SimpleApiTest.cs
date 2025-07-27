using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace UPTRMS.Api.Tests.Integration;

public class SimpleApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SimpleApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Health_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task Get_AuthSsoProviders_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/sso/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("google");
        content.Should().Contain("clever");
        content.Should().Contain("classlink");
    }
}