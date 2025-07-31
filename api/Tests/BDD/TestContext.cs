using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechTalk.SpecFlow;
using UPTRMS.Api.Tests.Mocks;

namespace UPTRMS.Api.Tests.BDD;

[Binding]
public class TestContext
{
    private static WebApplicationFactory<Program>? _factory;
    
    public static WebApplicationFactory<Program> Factory => _factory ??= CreateFactory();

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new TestWebApplicationFactory<Program>();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        // Initialize scenario-specific context
        scenarioContext["TestStartTime"] = DateTime.UtcNow;
        
        // Clear mock state between scenarios
        MockAuthenticationService.ClearState();
        MockUserRepository.ClearState();
    }

    [AfterScenario]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        // Cleanup scenario-specific resources
        if (scenarioContext.ContainsKey("CreatedResourceIds"))
        {
            var resourceIds = scenarioContext.Get<List<string>>("CreatedResourceIds");
            // Cleanup created test data (no-op for now since all endpoints return 501)
        }
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Global test initialization
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        // Global cleanup
        _factory?.Dispose();
    }
}