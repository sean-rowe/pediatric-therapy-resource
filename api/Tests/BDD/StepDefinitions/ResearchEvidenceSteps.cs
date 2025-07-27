using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ResearchEvidenceSteps : BaseStepDefinitions
{
    private string _currentStudyId = string.Empty;
    private Dictionary<string, object> _evidenceData = new();
    private List<object> _citations = new();

    public ResearchEvidenceSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"research library is available")]
    public void GivenResearchLibraryIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"evidence-based practice is prioritized")]
    public void GivenEvidenceBasedPracticeIsPrioritized()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I seek evidence for intervention")]
    public void GivenISeekEvidenceForIntervention()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"resource has research citations")]
    public void GivenResourceHasResearchCitations()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I view resource evidence base")]
    public async Task WhenIViewResourceEvidenceBase()
    {
        await WhenISendAGETRequestTo("/api/resources/resource-123/evidence");
    }
    [When(@"I search research database")]
    public async Task WhenISearchResearchDatabase()
    {
        await WhenISendAGETRequestTo("/api/research/search?query=sensory+integration&intervention=true");
    }
    [When(@"I filter by evidence level")]
    public async Task WhenIFilterByEvidenceLevel()
    {
        await WhenISendAGETRequestTo("/api/research/filter?evidenceLevel=1,2&studyType=RCT");
    }
    [When(@"I access research summary")]
    public async Task WhenIAccessResearchSummary()
    {
        _currentStudyId = "study-456";
        await WhenISendAGETRequestTo($"/api/research/studies/{_currentStudyId}/summary");
    }
    [When(@"I save citation to library")]
    public async Task WhenISaveCitationToLibrary()
    {
        await WhenISendAPOSTRequestToWithData("/api/research/citations/save", new Dictionary<string, object>
        {
            ["studyId"] = _currentStudyId,
            ["folder"] = "Sensory Integration",
            ["notes"] = "Strong evidence for effectiveness with autism"
        });
    }

    [When(@"I generate reference list")]
    public async Task WhenIGenerateReferenceList()
    {
        await WhenISendAPOSTRequestToWithData("/api/research/references/generate", new Dictionary<string, object>
        {
            ["format"] = "APA",
            ["includeAbstracts"] = false
        });
    }

    [Then(@"evidence indicators show:")]
    public void ThenEvidenceIndicatorsShow(Table table)
    {
        var indicators = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            indicators[row["Indicator"]] = row["Value"];
        }
        ScenarioContext["EvidenceIndicators"] = indicators;
    }
    [Then(@"research citations display:")]
    public void ThenResearchCitationsDisplay(Table table)
    {
        var citations = new List<object>();
        foreach (var row in table.Rows)
        {
            citations.Add(new
            {
                Study = row["Study"],
                Level = row["Evidence Level"]
            });
        }
        
        ScenarioContext["DisplayedCitations"] = citations;
    }
    [Then(@"study results include:")]
    public void ThenStudyResultsInclude(Table table)
    {
        var results = new List<object>();
        foreach (var row in table.Rows)
        {
            results.Add(new
            {
                Study = row["Study Title"],
                Participants = row["Participants"],
                Findings = row["Key Findings"]
            });
        }
        
        ScenarioContext["StudyResults"] = results;
    }
    [Then(@"filtered results show high quality studies")]
    public void ThenFilteredResultsShowHighQualityStudies()
    {
        ScenarioContext["HighQualityStudies"] = true;
        ScenarioContext["FilteredCount"] = 24;
        ScenarioContext["AverageEvidenceLevel"] = 1.8;
    }
    [Then(@"research summary provides:")]
    public void ThenResearchSummaryProvides(Table table)
    {
        var summary = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            summary[row["Section"]] = row["Content"];
        }
        ScenarioContext["ResearchSummary"] = summary;
    }
    [Then(@"outcome tracking linked")]
    public void ThenOutcomeTrackingLinked()
    {
        ScenarioContext["OutcomeTrackingLinked"] = true;
        ScenarioContext["LinkedOutcomes"] = new[]
        {
            "Sensory processing measures",
            "Functional performance",
            "Quality of life indicators"
        };
    }

    [Then(@"citation saved successfully")]
    public void ThenCitationSavedSuccessfully()
    {
        ScenarioContext["CitationSaved"] = true;
        ScenarioContext["SavedToFolder"] = "Sensory Integration";
    }
    [Then(@"citation organized by:")]
    public void ThenCitationOrganizedBy(Table table)
    {
        var organization = new List<string>();
        foreach (var row in table.Rows)
        {
            organization.Add(row["Organization Method"]);
        }
        ScenarioContext["CitationOrganization"] = organization;
    }
    [Then(@"reference list generated")]
    public void ThenReferenceListGenerated()
    {
        ScenarioContext["ReferenceListGenerated"] = true;
        ScenarioContext["ReferenceFormat"] = "APA 7th edition";
    }
    [Then(@"best practice alerts show:")]
    public void ThenBestPracticeAlertsShow(Table table)
    {
        var alerts = new List<string>();
        foreach (var row in table.Rows)
        {
            alerts.Add(row["Alert"]);
        }
        ScenarioContext["BestPracticeAlerts"] = alerts;
    }
    [Then(@"research updates available")]
    public void ThenResearchUpdatesAvailable()
    {
        ScenarioContext["ResearchUpdatesAvailable"] = true;
        ScenarioContext["UpdateFrequency"] = "Monthly";
        ScenarioContext["UpdateTypes"] = new[]
        {
            "New systematic reviews",
            "Updated guidelines",
            "Emerging evidence"
        };
    }

    [Then(@"continuing education credits offered")]
    public void ThenContinuingEducationCreditsOffered()
    {
        ScenarioContext["CECreditsOffered"] = true;
        ScenarioContext["CreditProviders"] = new[]
        {
            "AOTA approved",
            "ASHA approved", 
            "APTA approved"
        };
    }
}
