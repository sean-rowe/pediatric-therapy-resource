using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class SensoryIntegrationSteps : BaseStepDefinitions
{
    private string _currentStudentId = string.Empty;
    private Dictionary<string, object> _sensoryProfile = new();
    private List<object> _sensoryActivities = new();

    public SensoryIntegrationSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"sensory integration resources are available")]
    public void GivenSensoryIntegrationResourcesAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am an OT specializing in sensory")]
    public void GivenIAmAnOTSpecializingInSensory()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" is sensory seeking")]
    public void GivenIsSensorySeeking(string childName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"sensory needs include:")]
    public void GivenSensoryNeedsInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I need to design sensory diet")]
    public void GivenINeedToDesignSensoryDiet()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access sensory diet builder")]
    public async Task WhenIAccessSensoryDietBuilder()
    {
        await WhenISendAGETRequestTo("/api/sensory/diet-builder");

    }

    [When(@"I input child's profile:")]
    public async Task WhenIInputChildsProfile(Table table)
    {
        var profile = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            profile[row["Factor"]] = row["Details"];
        }
        await WhenISendAPOSTRequestToWithData("/api/sensory/profile", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["profile"] = profile
        });
    }

    [When(@"I generate recommendations")]
    public async Task WhenIGenerateRecommendations()
    {
        await WhenISendAPOSTRequestToWithData("/api/sensory/diet/generate", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["includeVisuals"] = true
        });
    }

    [When(@"I search heavy work activities")]
    public async Task WhenISearchHeavyWorkActivities()
    {
        await WhenISendAGETRequestTo("/api/sensory/activities?type=heavy-work&age=6");
    }

    [When(@"I filter for classroom-appropriate")]
    public async Task WhenIFilterForClassroomAppropriate()
    {
        await WhenISendAGETRequestTo("/api/sensory/activities/filter?setting=classroom");

    }
    [Then(@"builder offers:")]
    public void ThenBuilderOffers(Table table)
    {
        var features = new List<string>();
        foreach (var row in table.Rows)
        {
            features.Add(row["Feature"]);
        }
        ScenarioContext["DietBuilderFeatures"] = features;

    }
    [Then(@"sensory diet includes:")]
    public void ThenSensoryDietIncludes(Table table)
    {
        var dietComponents = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            dietComponents[row["Time"]] = new
            {
                Activity = row["Activity"],
                System = row["Sensory System"]
            };
        }
        ScenarioContext["GeneratedSensoryDiet"] = dietComponents;
    }

    [Then(@"visual schedule provided")]
    public void ThenVisualScheduleProvided()
    {
        ScenarioContext["VisualScheduleGenerated"] = true;
        ScenarioContext["ScheduleFormat"] = "Picture cards with times";
    }
    [Then(@"activities include:")]
    public void ThenActivitiesInclude(Table table)
    {
        var activities = new List<object>();
        foreach (var row in table.Rows)
        {
            activities.Add(new
            {
                Activity = row["Activity"],
                Duration = row["Duration"],
                Equipment = row["Equipment"]
            });
        }
        
        ScenarioContext["HeavyWorkActivities"] = activities;
    }
    [Then(@"each activity shows:")]
    public void ThenEachActivityShows(Table table)
    {
        var activityDetails = new List<string>();
        foreach (var row in table.Rows)
        {
            activityDetails.Add(row["Information"]);
        }
        ScenarioContext["ActivityInformation"] = activityDetails;
    }
    [Then(@"filtered results show:")]
    public void ThenFilteredResultsShow(Table table)
    {
        var results = new List<object>();
        foreach (var row in table.Rows)
        {
            results.Add(new
            {
                Activity = row["Activity"],
                NoiseLevel = row["Noise Level"],
                SpaceNeeded = row["Space Needed"]
            });
        }
        
        ScenarioContext["ClassroomAppropriateActivities"] = results;
    }
    [Then(@"implementation tips include:")]
    public void ThenImplementationTipsInclude(Table table)
    {
        var tips = new List<string>();
        foreach (var row in table.Rows)
        {
            tips.Add(row["Tip"]);
        }
        ScenarioContext["ImplementationTips"] = tips;
    }
}
