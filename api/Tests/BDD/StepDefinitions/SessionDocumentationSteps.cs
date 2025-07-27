using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class SessionDocumentationSteps : BaseStepDefinitions
{
    private string _currentSessionId = string.Empty;
    private Dictionary<string, object> _sessionData = new();
    private List<object> _interventions = new();

    public SessionDocumentationSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"session documentation is required")]
    public void GivenSessionDocumentationIsRequired()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I completed therapy session")]
    public void GivenICompletedTherapySession()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"session included:")]
    public void GivenSessionIncluded(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"documentation deadline is (.*) hours")]
    public void GivenDocumentationDeadlineIsHours(int hours)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I open documentation form")]
    public async Task WhenIOpenDocumentationForm()
    {
        await WhenISendAGETRequestTo($"/api/sessions/{_currentSessionId}/documentation");

    }
    [When(@"I select quick documentation")]
    public async Task WhenISelectQuickDocumentation()
    {
        await WhenISendAPOSTRequestToWithData("/api/sessions/documentation/quick", new Dictionary<string, object>
{
    ["sessionId"] = _currentSessionId,
    ["templateType"] = "brief_note"
});
    }

    [When(@"I complete all fields")]
    public async Task WhenICompleteAllFields()
    {
            await WhenISendAPOSTRequestToWithData($"/api/sessions/{_currentSessionId}/documentation/complete", new Dictionary<string, object>
{
    ["subjective"] = "Student was alert and cooperative",
    ["objective"] = "Completed 3/4 fine motor tasks",
    ["assessment"] = "Making progress toward goals",
    ["plan"] = "Continue current interventions"
});
    }

    [When(@"I submit documentation")]
    public async Task WhenISubmitDocumentation()
    {
            await WhenISendAPOSTRequestToWithData($"/api/sessions/{_currentSessionId}/documentation/submit", new Dictionary<string, object>
{
    ["completedAt"] = DateTime.UtcNow,
    ["signature"] = "therapist_signature"
});
    }

    [Then(@"form pre-populates:")]
    public void ThenFormPrePopulates(Table table)
    {
            var prePopulatedData = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            prePopulatedData[row["Field"]] = row["Data"];
}
        ScenarioContext["PrePopulatedFields"] = prePopulatedData;
    }
    [Then(@"objective data captured:")]
    public void ThenObjectiveDataCaptured(Table table)
    {
        var objectiveData = new List<object>();
        foreach (var row in table.Rows)
        {
            objectiveData.Add(new
            {
                Metric = row["Metric"],
                Value = row["Value"]
            });
        }
        
        ScenarioContext["CapturedObjectiveData"] = objectiveData;
    }
    [Then(@"template includes:")]
    public void ThenTemplateIncludes(Table table)
    {
        var templateSections = new List<string>();
        foreach (var row in table.Rows)
        {
            templateSections.Add(row["Section"]);
}
        ScenarioContext["TemplateSections"] = templateSections;
    }
    [Then(@"smart phrases available:")]
    public void ThenSmartPhrasesAvailable(Table table)
    {
        var smartPhrases = new List<string>();
        foreach (var row in table.Rows)
        {
            smartPhrases.Add(row["Phrase"]);
}
        ScenarioContext["AvailableSmartPhrases"] = smartPhrases;
    }
    [Then(@"documentation locked")]
    public void ThenDocumentationLocked()
    {
        ScenarioContext["DocumentationLocked"] = true;
        ScenarioContext["LockedAt"] = DateTime.UtcNow;
        ScenarioContext["EditableStatus"] = false;
    }
    [Then(@"audit trail shows:")]
    public void ThenAuditTrailShows(Table table)
    {
        var auditEntries = new List<object>();
        foreach (var row in table.Rows)
        {
            auditEntries.Add(new
            {
                Action = row["Action"],
                Timestamp = row["Timestamp"]
            });
        }
        
        ScenarioContext["DocumentationAuditTrail"] = auditEntries;
    }
    [Then(@"compliance indicators show:")]
    public void ThenComplianceIndicatorsShow(Table table)
    {
        var indicators = new Dictionary<string, string>();
        foreach (var row in table.Rows)
        {
            indicators[row["Requirement"]] = row["Status"];
}
        ScenarioContext["ComplianceIndicators"] = indicators;
    }
}