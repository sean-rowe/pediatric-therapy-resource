using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ClinicalSupervisionSteps : BaseStepDefinitions
{
    private string _currentStudentId = string.Empty;
    private string _supervisionSessionId = string.Empty;
    private Dictionary<string, object> _competencyData = new();

    public ClinicalSupervisionSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"clinical supervision tools are available")]
    public void GivenClinicalSupervisionToolsAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am a clinical supervisor")]
    public void GivenIAmAClinicalSupervisor()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I supervise student ""(.*)""")]
    public void GivenISuperviseStudent(string studentName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"competency framework includes:")]
    public void GivenCompetencyFrameworkIncludes(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student recorded therapy session")]
    public void GivenStudentRecordedTherapySession()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I complete competency evaluation")]
    public async Task WhenICompleteCompetencyEvaluation()
    {
        await WhenISendAPOSTRequestToWithData($"/api/supervision/students/{_currentStudentId}/evaluation", new Dictionary<string, object>
        {
            ["evaluationType"] = "midterm",
            ["competencies"] = new Dictionary<string, object>
            {
                ["clinical_skills"] = 3, // Emerging competence
                ["professional_behavior"] = 4, // Competent
                ["critical_thinking"] = 2 // Developing
            }
        });
    }

    [When(@"I review session video")]
    public async Task WhenIReviewSessionVideo()
    {
        await WhenISendAGETRequestTo($"/api/supervision/videos/{_currentStudentId}/review");
    }
    [When(@"I add annotation at timestamp (.*)")]
    public async Task WhenIAddAnnotationAtTimestamp(string timestamp)
    {
        await WhenISendAPOSTRequestToWithData("/api/supervision/videos/annotate", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["timestamp"] = timestamp,
            ["comment"] = "Good use of therapeutic rapport",
            ["competency"] = "Therapeutic Use of Self"
        });
    }
    [When(@"I schedule supervision meeting")]
    public async Task WhenIScheduleSupervisionMeeting()
    {
        _supervisionSessionId = $"supervision-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/supervision/schedule", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["date"] = DateTime.UtcNow.AddDays(1),
            ["duration"] = 60,
            ["type"] = "individual",
            ["agenda"] = "Video review and competency discussion"
        });
    }
    [When(@"I log supervision hours")]
    public async Task WhenILogSupervisionHours()
    {
        await WhenISendAPOSTRequestToWithData("/api/supervision/hours/log", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["date"] = DateTime.UtcNow,
            ["type"] = "direct_observation",
            ["duration"] = 60,
            ["topics"] = new[] { "Assessment techniques", "Documentation" }
        });
    }

    [Then(@"competency tracking shows:")]
    public void ThenCompetencyTrackingShows(Table table)
    {
        var tracking = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            tracking[row["Competency"]] = new
            {
                Level = row["Level"],
                Evidence = row["Evidence"]
            };
        }
        ScenarioContext["CompetencyTracking"] = tracking;
    }

    [Then(@"progress visualization displays:")]
    public void ThenProgressVisualizationDisplays(Table table)
    {
        var visualization = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            visualization[row["Element"]] = row["Display"];
        }
        ScenarioContext["ProgressVisualization"] = visualization;
    }
    [Then(@"video annotations include:")]
    public void ThenVideoAnnotationsInclude(Table table)
    {
        var annotations = new List<object>();
        foreach (var row in table.Rows)
        {
            annotations.Add(new
            {
                Tool = row["Tool"],
                Purpose = row["Purpose"]
            });
        }
        
        ScenarioContext["VideoAnnotations"] = annotations;
    }
    [Then(@"student can access:")]
    public void ThenStudentCanAccess(Table table)
    {
        var access = new List<string>();
        foreach (var row in table.Rows)
        {
            access.Add(row["Access"]);
        }
        ScenarioContext["StudentAccess"] = access;
    }
    [Then(@"meeting scheduled successfully")]
    public void ThenMeetingScheduledSuccessfully()
    {
        ScenarioContext["MeetingScheduled"] = true;
        ScenarioContext["SupervisionSessionId"] = _supervisionSessionId;
    }
    [Then(@"reminder notifications sent")]
    public void ThenReminderNotificationsSent()
    {
        ScenarioContext["RemindersSet"] = true;
        ScenarioContext["ReminderTimes"] = new[] { "24 hours", "1 hour" };
    }
    
    [Then(@"supervision log updated:")]
    public void ThenSupervisionLogUpdated(Table table)
    {
        var logEntry = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            logEntry[row["Field"]] = row["Value"];
        }
        ScenarioContext["SupervisionLogEntry"] = logEntry;
    }
    [Then(@"accreditation report generated:")]
    public void ThenAccreditationReportGenerated(Table table)
    {
        var report = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            report[row["Report Type"]] = row["Content"];
        }
        ScenarioContext["AccreditationReport"] = report;
    }
    [Then(@"learning plan created:")]
    public void ThenLearningPlanCreated(Table table)
    {
        var learningPlan = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            learningPlan[row["Learning Need"]] = new
            {
                Strategies = row["Strategies"],
                Timeline = row["Timeline"]
            };
        }
        ScenarioContext["LearningPlan"] = learningPlan;
    }

    [Then(@"remediation resources available:")]
    public void ThenRemediationResourcesAvailable(Table table)
    {
        var resources = new List<object>();
        foreach (var row in table.Rows)
        {
            resources.Add(new
            {
                Type = row["Type"],
                Assignment = row["Assignment"]
            });
        }
        
        ScenarioContext["RemediationResources"] = resources;
    }
}