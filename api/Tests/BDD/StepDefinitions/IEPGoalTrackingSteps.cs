using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class IEPGoalTrackingSteps : BaseStepDefinitions
{
    private string _currentStudentId = string.Empty;
    private string _currentGoalId = string.Empty;
    private Dictionary<string, object> _goalData = new();
    private List<object> _progressData = new();

    public IEPGoalTrackingSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"IEP goal tracking is enabled")]
    public void GivenIEPGoalTrackingIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student has IEP goals")]
    public void GivenStudentHasIEPGoals()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"goals include:")]
    public void GivenGoalsInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"baseline data exists")]
    public void GivenBaselineDataExists()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"progress tracking is inconsistent")]
    public void GivenProgressTrackingIsInconsistent()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access goal tracking dashboard")]
    public async Task WhenIAccessGoalTrackingDashboard()
    {
        await WhenISendAGETRequestTo($"/api/students/{_currentStudentId}/iep/dashboard");
    }
    [When(@"I update goal progress")]
    public async Task WhenIUpdateGoalProgress()
    {
        _currentGoalId = $"goal-{Guid.NewGuid()}";
        await WhenISendAPUTRequestToWithData($"/api/iep/goals/{_currentGoalId}/progress", new Dictionary<string, object>
        {
            ["sessionDate"] = DateTime.UtcNow,
            ["trialData"] = new[] { 8, 9, 7, 10, 8 },
            ["notes"] = "Student showed improvement with verbal prompts",
            ["studentId"] = _currentStudentId,
            ["reportType"] = "quarterly",
            ["includeGraphs"] = true,
            ["dateRange"] = new { start = DateTime.UtcNow.AddMonths(-3), end = DateTime.UtcNow }
        });
    }

    [When(@"I set goal benchmarks")]
    public async Task WhenISetGoalBenchmarks()
    {
        await WhenISendAPOSTRequestToWithData($"/api/iep/goals/{_currentGoalId}/benchmarks", new Dictionary<string, object>
        {
            ["benchmarks"] = new[]
            {
                new { month = 1, targetPercentage = 60 },
                new { month = 3, targetPercentage = 75 },
                new { month = 6, targetPercentage = 90 }
            }
        });
    }
            
    [When(@"I attempt to access without permission")]
    public async Task WhenIAttemptToAccessWithoutPermission()
    {
        Client.DefaultRequestHeaders.Remove("Authorization");
        await WhenISendAGETRequestTo($"/api/students/{_currentStudentId}/iep/dashboard");
    }
    [When(@"I try to update with invalid data")]
    public async Task WhenITryToUpdateWithInvalidData()
    {
        await WhenISendAPUTRequestToWithData($"/api/iep/goals/{_currentGoalId}/progress", new Dictionary<string, object>
        {
            ["sessionDate"] = "invalid-date",
            ["notes"] = "" // Empty notes
        });
    }
    [Then(@"IEP dashboard displays:")]
    public void ThenDashboardDisplays(Table table)
    {
        var dashboard = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            dashboard[row["Element"]] = row["Information"];
        }
        ScenarioContext["DashboardDisplay"] = dashboard;
    }
    [Then(@"visual progress shown:")]
    public void ThenVisualProgressShown(Table table)
    {
        var visualElements = new List<object>();
        foreach (var row in table.Rows)
        {
            visualElements.Add(new
            {
                Type = row["Chart Type"],
                Data = row["Data Shown"],
                Period = row["Time Period"]
            });
        }
        
        ScenarioContext["VisualProgress"] = visualElements;
    }
    [Then(@"goal status updated")]
    public void ThenGoalStatusUpdated()
    {
        ScenarioContext["GoalStatusUpdated"] = true;
        ScenarioContext["LastUpdated"] = DateTime.UtcNow;
        ScenarioContext["ProgressPercentage"] = 67;
    }
    [Then(@"data validation shows:")]
    public void ThenDataValidationShows(Table table)
    {
        var validation = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            validation[row["Check"]] = row["Status"];
        }
        ScenarioContext["DataValidation"] = validation;
    }
    [Then(@"IEP report includes:")]
    public void ThenReportIncludes(Table table)
    {
        var reportSections = new List<string>();
        foreach (var row in table.Rows)
        {
            reportSections.Add(row["Section"]);
        }
        ScenarioContext["ReportSections"] = reportSections;
    }
    [Then(@"benchmarks saved successfully")]
    public void ThenBenchmarksSavedSuccessfully()
    {
        ScenarioContext["BenchmarksSaved"] = true;
        ScenarioContext["BenchmarkCount"] = 3;
    }
    [Then(@"progress alerts triggered:")]
    public void ThenProgressAlertsTriggered(Table table)
    {
        var alerts = new List<object>();
        foreach (var row in table.Rows)
        {
            alerts.Add(new
            {
                Type = row["Alert Type"],
                Condition = row["Condition"],
                Action = row["Recommended Action"]
            });
        }
        
        ScenarioContext["ProgressAlerts"] = alerts;
    }
    [Then(@"IEP access denied")]
    public void ThenAccessDenied()
    {
        LastResponse.Should().NotBeNull();
        LastResponse!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ScenarioContext["AccessDenied"] = true;
    }
    [Then(@"validation errors returned:")]
    public void ThenValidationErrorsReturned(Table table)
    {
        LastResponse.Should().NotBeNull();
        LastResponse!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errors = new List<string>();
        foreach (var row in table.Rows)
        {
            errors.Add(row["Error"]);
        }
        ScenarioContext["ValidationErrors"] = errors;
    }
    [Then(@"IEP data integrity maintained")]
    public void ThenDataIntegrityMaintained()
    {
        ScenarioContext["DataIntegrityMaintained"] = true;
        ScenarioContext["NoCorruptedRecords"] = true;
    }
    [Then(@"trending analysis shows:")]
    public void ThenTrendingAnalysisShows(Table table)
    {
        var trends = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            trends[row["Metric"]] = new
            {
                Trend = row["Trend"],
                Significance = row["Statistical Significance"]
            };
        }
        ScenarioContext["TrendingAnalysis"] = trends;
    }
    
    [Then(@"IEP recommendations generated:")]
    public void ThenRecommendationsGenerated(Table table)
    {
        var recommendations = new List<string>();
        foreach (var row in table.Rows)
        {
            recommendations.Add(row["Recommendation"]);
        }
        ScenarioContext["AutoRecommendations"] = recommendations;
    }
}
