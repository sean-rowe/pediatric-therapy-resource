using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class TransitionPlanningSteps : BaseStepDefinitions
{
    private string _currentStudentId = string.Empty;
    private Dictionary<string, object> _transitionData = new();
    private List<object> _assessmentResults = new();

    public TransitionPlanningSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"transition planning tools are available")]
    public void GivenTransitionPlanningToolsAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I work with transition-age students")]
    public void GivenIWorkWithTransitionAgeStudents()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" is (.*) years old")]
    public void GivenStudentIsYearsOld(string studentName, int age)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"post-graduation goals include:")]
    public void GivenPostGraduationGoalsInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access vocational assessments")]
    public async Task WhenIAccessVocationalAssessments()
    {
        await WhenISendAGETRequestTo("/api/transition/assessments/vocational");
    }
    [When(@"I administer interest inventory")]
    public async Task WhenIAdministerInterestInventory()
    {
        await WhenISendAPOSTRequestToWithData("/api/transition/assessments/interest", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["assessmentType"] = "career_interest"
        });
    }

    [When(@"student (.*) completes assessment")]
    public async Task WhenStudentCompletesAssessment(string studentName)
    {
        await WhenISendAPOSTRequestToWithData($"/api/transition/assessments/{_currentStudentId}/complete", new Dictionary<string, object>
        {
            ["completedBy"] = studentName,
            ["responses"] = new Dictionary<string, int>
            {
                ["working_with_hands"] = 8,
                ["helping_others"] = 9,
                ["organizing_things"] = 5,
                ["creative_tasks"] = 7
            }
        });
    }

    [When(@"I search life skills curricula")]
    public async Task WhenISearchLifeSkillsCurricula()
    {
        await WhenISendAGETRequestTo("/api/transition/resources/life-skills");
    }
    [When(@"I filter for ""(.*)""")]
    public async Task WhenIFilterFor(string filterCriteria)
    {
        await WhenISendAGETRequestTo($"/api/transition/resources/filter?criteria={Uri.EscapeDataString(filterCriteria)}");
    }
    [Then(@"assessments include:")]
    public void ThenAssessmentsInclude(Table table)
    {
        var assessments = new List<object>();
        foreach (var row in table.Rows)
        {
            assessments.Add(new
            {
                Type = row["Assessment Type"],
                Purpose = row["Purpose"]
            });
        }
        
        ScenarioContext["AvailableAssessments"] = assessments;
    }
    [Then(@"each assessment provides:")]
    public void ThenEachAssessmentProvides(Table table)
    {
        var features = new List<string>();
        foreach (var row in table.Rows)
        {
            features.Add(row["Feature"]);
        }
        ScenarioContext["AssessmentFeatures"] = features;
    }
    [Then(@"results indicate:")]
    public void ThenResultsIndicate(Table table)
    {
        var results = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            results[row["Area"]] = row["Interest Level"];
        }
        ScenarioContext["InterestInventoryResults"] = results;
    }
    [Then(@"career suggestions include:")]
    public void ThenCareerSuggestionsInclude(Table table)
    {
        var careers = new List<string>();
        foreach (var row in table.Rows)
        {
            careers.Add(row["Career Path"]);
        }
        ScenarioContext["SuggestedCareers"] = careers;
    }
    [Then(@"next steps recommended:")]
    public void ThenNextStepsRecommended(Table table)
    {
        var nextSteps = new List<string>();
        foreach (var row in table.Rows)
        {
            nextSteps.Add(row["Action"]);
        }
        ScenarioContext["RecommendedNextSteps"] = nextSteps;
    }
    [Then(@"curricula includes:")]
    public void ThenCurriculaIncludes(Table table)
    {
        var curricula = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            curricula[row["Skill Area"]] = row["Topics"];
        }
        ScenarioContext["LifeSkillsCurricula"] = curricula;
    }
    [Then(@"materials are:")]
    public void ThenMaterialsAre(Table table)
    {
        var materialFeatures = new List<string>();
        foreach (var row in table.Rows)
        {
            materialFeatures.Add(row["Feature"]);
        }
        ScenarioContext["MaterialFeatures"] = materialFeatures;
    }
    [Then(@"progress tracking includes:")]
    public void ThenProgressTrackingIncludes(Table table)
    {
        var trackingFeatures = new List<string>();
        foreach (var row in table.Rows)
        {
            trackingFeatures.Add(row["Tracking Feature"]);
        }
        ScenarioContext["ProgressTrackingFeatures"] = trackingFeatures;
    }
}
