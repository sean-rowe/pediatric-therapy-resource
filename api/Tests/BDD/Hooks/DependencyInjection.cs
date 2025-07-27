using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;
using BoDi;

namespace UPTRMS.Api.Tests.BDD.Hooks;

[Binding]
public class DependencyInjection
{
    private readonly IObjectContainer _container;

    public DependencyInjection(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeScenario]
    public void RegisterDependencies()
    {
        // Register the test factory as a singleton for the scenario
        _container.RegisterInstanceAs(TestContext.Factory);
        
        // Register any other scenario-scoped dependencies here
    }
}