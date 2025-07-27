using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class FeedingTherapySteps : BaseStepDefinitions
{
    private string _currentPatientId = string.Empty;
    private Dictionary<string, object> _feedingData = new();
    private List<object> _foodLog = new();

    public FeedingTherapySteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"feeding therapy resources are available")]
    public void GivenFeedingTherapyResourcesAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I work on feeding skills")]
    public void GivenIWorkOnFeedingSkills()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" has selective eating")]
    public void GivenHasSelectiveEating(string childName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"accepted foods limited to:")]
    public void GivenAcceptedFoodsLimitedTo(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access food chaining protocol")]
    public async Task WhenIAccessFoodChainingProtocol()
    {
        await WhenISendAGETRequestTo("/api/feeding/protocols/food-chaining");
    }
    [When(@"I create chain starting with ""(.*)""")]
    public async Task WhenICreateChainStartingWith(string baseFood)
    {
        await WhenISendAPOSTRequestToWithData("/api/feeding/chains/create", new Dictionary<string, object>
{
    ["patientId"] = _currentPatientId,
    ["baseFood"] = baseFood,
    ["targetCategory"] = "vegetables"
});
    }

    [When(@"I track food exposures")]
    public async Task WhenITrackFoodExposures()
    {
            await WhenISendAPOSTRequestToWithData("/api/feeding/tracking/exposures", new Dictionary<string, object>
{
    ["patientId"] = _currentPatientId,
    ["date"] = DateTime.UtcNow,
    ["exposures"] = new[]
    {
        new { Food = "carrots", Response = "touched" },
        new { Food = "sweet potato", Response = "licked" }
    }
});
        }
        
    [Then(@"protocol includes:")]
    public void ThenProtocolIncludes(Table table)
    {
        var protocolSteps = new List<object>();
        foreach (var row in table.Rows)
        {
            protocolSteps.Add(new
            {
                Step = row["Step"],
                Description = row["Description"]
                    });
        }
        
        ScenarioContext["FoodChainingProtocol"] = protocolSteps;
    }
    [Then(@"chain progression shows:")]
    public void ThenChainProgressionShows(Table table)
    {
        var progression = new List<object>();
        foreach (var row in table.Rows)
        {
            progression.Add(new
            {
                Stage = row["Stage"],
                Food = row["Food"],
                Similarity = row["Similarity"]
                    });
        }
        
        ScenarioContext["FoodChainProgression"] = progression;
    }
    [Then(@"parent materials explain:")]
    public void ThenParentMaterialsExplain(Table table)
    {
        var explanations = new List<string>();
        foreach (var row in table.Rows)
        {
            explanations.Add(row["Concept"]);
        }
        ScenarioContext["ParentEducationContent"] = explanations;
    }
    [Then(@"feeding progress tracking includes:")]
    public void ThenProgressTrackingIncludes(Table table)
    {
        var trackingFeatures = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            trackingFeatures[row["Metric"]] = row["Measurement"];
        }
        ScenarioContext["FeedingProgressTracking"] = trackingFeatures;
    }
    [Then(@"visual supports show:")]
    public void ThenVisualSupportsShow(Table table)
    {
        var visuals = new List<string>();
        foreach (var row in table.Rows)
        {
            visuals.Add(row["Visual Type"]);
        }
        ScenarioContext["FeedingVisualSupports"] = visuals;
    }
}
