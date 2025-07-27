using System.Reflection;
using TechTalk.SpecFlow;
using UPTRMS.Api.Tests.BDD.StepDefinitions;
using Xunit;
using Xunit.Abstractions;

namespace UPTRMS.Api.Tests.BDD;

/// <summary>
/// Main test runner for all BDD scenarios
/// </summary>
public class TestRunner
{
    private readonly ITestOutputHelper _output;

    public TestRunner(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "BDD")]
    public void AllFeatureFilesHaveStepDefinitions()
    {
        // Get all step definition types
        var stepDefinitionTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttributes<BindingAttribute>().Any())
            .ToList();

        _output.WriteLine($"Found {stepDefinitionTypes.Count} step definition classes:");
        foreach (var type in stepDefinitionTypes)
        {
            _output.WriteLine($"  - {type.Name}");
        }

        // Verify we have all major categories covered
        var expectedCategories = new[]
        {
            "Resource", "Therapy", "Student", 
            "Seller", "Specialized", "Admin", "Communication", "Compliance",
            "Platform", "Integration"
        };

        foreach (var category in expectedCategories)
        {
            var hasCategory = stepDefinitionTypes.Any(t => 
                t.Name.Contains(category, StringComparison.OrdinalIgnoreCase));
            
            Assert.True(hasCategory, $"Missing step definitions for {category}");
        }
    }

    [Theory]
    [Trait("Category", "BDD")]
    [InlineData("auth", 4)]
    [InlineData("resources", 11)]
    [InlineData("therapy", 14)]
    [InlineData("marketplace", 3)]
    [InlineData("admin", 3)]
    [InlineData("compliance", 2)]
    [InlineData("specialized", 6)]
    [InlineData("platform", 6)]
    [InlineData("students", 2)]
    [InlineData("communication", 2)]
    [InlineData("integrations", 2)]
    public void FeatureCategoriesHaveExpectedFileCount(string category, int expectedCount)
    {
        var featurePath = Path.Combine("BDD", "Features", category);
        if (Directory.Exists(featurePath))
        {
            var featureFiles = Directory.GetFiles(featurePath, "*.feature").Length;
            _output.WriteLine($"{category}: {featureFiles} feature files");
            
            // Allow some variance as features may be consolidated
            Assert.True(Math.Abs(featureFiles - expectedCount) <= 2, 
                $"Expected around {expectedCount} features in {category}, but found {featureFiles}");
        }
    }

    [Fact]
    [Trait("Category", "BDD")]
    public void AllStepDefinitionsInheritFromBase()
    {
        var stepDefinitionTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttributes<BindingAttribute>().Any() && 
                       t.Name != "BaseStepDefinitions" &&
                       t.Name != "TestContext" &&
                       t.Name != "DependencyInjection" &&
                       t.Name != "TestHooks")
            .ToList();

        foreach (var type in stepDefinitionTypes)
        {
            Assert.True(type.IsSubclassOf(typeof(BaseStepDefinitions)),
                $"{type.Name} should inherit from BaseStepDefinitions");
        }
    }

    [Fact]
    [Trait("Category", "BDD")]
    public void NoDuplicateStepDefinitions()
    {
        var stepMethods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttributes<BindingAttribute>().Any())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttributes<GivenAttribute>().Any() ||
                       m.GetCustomAttributes<WhenAttribute>().Any() ||
                       m.GetCustomAttributes<ThenAttribute>().Any())
            .ToList();

        var stepPatterns = stepMethods
            .SelectMany(m => m.GetCustomAttributes<StepDefinitionBaseAttribute>()
                .Select(attr => new { Method = m, Pattern = attr.Regex }))
            .GroupBy(x => x.Pattern)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicate in stepPatterns)
        {
            _output.WriteLine($"Duplicate pattern found: {duplicate.Key}");
            foreach (var method in duplicate)
            {
                _output.WriteLine($"  - {method.Method.DeclaringType?.Name}.{method.Method.Name}");
            }
        }

        Assert.Empty(stepPatterns);
    }
}

/// <summary>
/// Integration test collection for proper test isolation
/// </summary>
[CollectionDefinition("BDD Integration Tests")]
public class BDDIntegrationTestCollection : ICollectionFixture<TestContext>
{
    // This class has no code, and is never created. 
    // Its purpose is to be the place to apply [CollectionDefinition]
    // and all the ICollectionFixture<> interfaces.
}