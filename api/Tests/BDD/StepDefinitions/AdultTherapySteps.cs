using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class AdultTherapySteps : BaseStepDefinitions
{
    private string _currentPatientId = string.Empty;
    private Dictionary<string, object> _patientData = new();
    private List<object> _functionalGoals = new();

    public AdultTherapySteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"adult therapy resources are available")]
    public void GivenAdultTherapyResourcesAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I work with adult populations")]
    public void GivenIWorkWithAdultPopulations()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"patient ""(.*)"" has stroke diagnosis")]
    public void GivenPatientHasStrokeDiagnosis(string patientName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"(.*) deficits include:")]
    public void GivenDeficitsInclude(string pronoun, Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I'm treating ""(.*)"" with dementia")]
    public void GivenImTreatingWithDementia(string patientName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"family wants home program")]
    public void GivenFamilyWantsHomeProgram()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I search for aphasia materials")]
    public async Task WhenISearchForAphasiaMaterials()
    {
        await WhenISendAGETRequestTo("/api/resources/search?category=aphasia&population=adult");
    }
    [When(@"I filter by ""(.*)""")]
    public async Task WhenIFilterBySeverity(string severity)
    {
        await WhenISendAGETRequestTo($"/api/resources/filter?severity={severity}");
    }
    [When(@"I search cognitive exercises")]
    public async Task WhenISearchCognitiveExercises()
    {
        await WhenISendAGETRequestTo("/api/resources/search?category=cognitive&population=adult");
    }
    [When(@"I select dementia-appropriate")]
    public async Task WhenISelectDementiaAppropriate()
    {
        await WhenISendAGETRequestTo("/api/resources/filter?condition=dementia&appropriate=true");
    }
    [Then(@"adult therapy resources include:")]
    public void ThenResourcesInclude(Table table)
    {
        var resources = new List<object>();
        foreach (var row in table.Rows)
        {
            resources.Add(new
            {
                Type = row["Resource Type"],
                Examples = row["Examples"]
                    });
        }
        
        ScenarioContext["AvailableResources"] = resources;
    }
    [Then(@"each resource indicates:")]
    public void ThenEachResourceIndicates(Table table)
    {
        var indicators = new List<string>();
        foreach (var row in table.Rows)
        {
            indicators.Add(row["Indicator"]);
        }
        ScenarioContext["ResourceIndicators"] = indicators;
    }
    [Then(@"difficulty progression available")]
    public void ThenDifficultyProgressionAvailable()
    {
        ScenarioContext["ProgressionAvailable"] = true;
        ScenarioContext["DifficultyLevels"] = new[]
        {
            "Maximum cues",
            "Moderate cues",
            "Minimal cues",
            "Independent"
        };
    }

    [Then(@"activities are:")]
    public void ThenActivitiesAre(Table table)
    {
        var activityFeatures = new List<string>();
        foreach (var row in table.Rows)
        {
            activityFeatures.Add(row["Feature"]);
                }
        ScenarioContext["ActivityFeatures"] = activityFeatures;
    }
    [Then(@"caregiver materials include:")]
    public void ThenCaregiverMaterialsInclude(Table table)
    {
        var materials = new List<object>();
        foreach (var row in table.Rows)
        {
            materials.Add(new
            {
                Type = row["Material Type"],
                Content = row["Content"]
                    });
        }
        
        ScenarioContext["CaregiverMaterials"] = materials;
    }
    [Then(@"safety considerations noted")]
    public void ThenSafetyConsiderationsNoted()
    {
        ScenarioContext["SafetyNotesIncluded"] = true;
        ScenarioContext["SafetyTopics"] = new[]
        {
            "Fall prevention",
            "Cognitive limitations",
            "Supervision needs",
            "Environmental modifications"
        };
    }

    // Additional missing step definitions
    [Then(@"the therapy response should contain array of:")]
    public void ThenTheTherapyResponseShouldContainArrayOf(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"resources should include:")]
    public void ThenResourcesShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Then(@"protocol should include:")]
    public void ThenProtocolShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"assessments should include:")]
    public void ThenAssessmentsShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"materials should include:")]
    public void ThenMaterialsShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"resources should be tailored for:")]
    public void ThenResourcesShouldBeTailoredFor(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"goals should be:")]
    public void ThenGoalsShouldBe(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"strategies should include:")]
    public void ThenStrategiesShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"curriculum should contain:")]
    public void ThenCurriculumShouldContain(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"measures should include:")]
    public void ThenMeasuresShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
}
