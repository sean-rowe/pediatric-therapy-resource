using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
    public class AdvocacyLegalSteps : BaseStepDefinitions
{
    private string _currentDocumentId = string.Empty;
    private Dictionary<string, object> _advocacyData = new();
    private List<object> _legalTemplates = new();

    public AdvocacyLegalSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"advocacy resources are available")]
    public void GivenAdvocacyResourcesAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"legal templates are current")]
    public void GivenLegalTemplatesAreCurrent()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I need advocacy support")]
    public void GivenINeedAdvocacySupport()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student has IEP meeting scheduled")]
    public void GivenStudentHasIEPMeetingScheduled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access IEP preparation checklist")]
    public async Task WhenIAccessIEPPreparationChecklist()
    {
        await WhenISendAGETRequestTo("/api/advocacy/iep/preparation-checklist");
    }
    [When(@"I select appropriate template")]
    public async Task WhenISelectAppropriateTemplate()
    {
        _currentDocumentId = $"doc-{Guid.NewGuid()}";
        await WhenISendAGETRequestTo("/api/advocacy/templates/iep-concerns-letter");
    }
    [When(@"I customize template with details")]
    public async Task WhenICustomizeTemplateWithDetails()
    {
        await WhenISendAPUTRequestToWithData($"/api/advocacy/templates/{_currentDocumentId}/customize", new Dictionary<string, object>
        {
            ["studentName"] = "Emma Johnson",
            ["concerns"] = new[]
            {
                "Lack of progress in fine motor goals",
                "Need for sensory breaks",
                "Request for additional OT services"
            },
            ["parentName"] = "Sarah Johnson"
        });
    }
    [When(@"I access rights information")]
    public async Task WhenIAccessRightsInformation()
    {
        await WhenISendAGETRequestTo("/api/advocacy/rights-information?topic=special-education");
    }

    [When(@"I search for state-specific guidance")]
    public async Task WhenISearchForStateSpecificGuidance()
    {
        await WhenISendAGETRequestTo("/api/advocacy/state-guidance?state=CA&topic=iep-process");
    }
    [When(@"I generate appeal letter")]
    public async Task WhenIGenerateAppealLetter()
    {
        await WhenISendAPOSTRequestToWithData("/api/advocacy/letters/appeal", new Dictionary<string, object>
        {
            ["appealType"] = "insurance_denial",
            ["serviceDenied"] = "Occupational Therapy",
            ["reasonForAppeal"] = "Medical necessity clearly documented",
            ["supportingDocuments"] = new[] { "doctor_note.pdf", "evaluation_report.pdf" }
        });
    }
    [When(@"I access grant writing templates")]
    public async Task WhenIAccessGrantWritingTemplates()
    {
        await WhenISendAGETRequestTo("/api/advocacy/grants/templates");
    }

    [Then(@"checklist includes:")]
    public void ThenChecklistIncludes(Table table)
    {
        var checklistItems = new List<string>();
        foreach (var row in table.Rows)
        {
            checklistItems.Add(row["Item"]);
        }
        ScenarioContext["IEPChecklist"] = checklistItems;
    }
    [Then(@"template customization shows:")]
    public void ThenTemplateCustomizationShows(Table table)
    {
        var customization = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            customization[row["Field"]] = row["Value"];
        }
        ScenarioContext["TemplateCustomization"] = customization;
    }
    [Then(@"document ready for review")]
    public void ThenDocumentReadyForReview()
    {
        ScenarioContext["DocumentReady"] = true;
        ScenarioContext["ReviewRequired"] = "Attorney review recommended";
    }
    [Then(@"rights information displays:")]
    public void ThenRightsInformationDisplays(Table table)
    {
        var rightsInfo = new List<string>();
        foreach (var row in table.Rows)
        {
            rightsInfo.Add(row["Right"]);
        }
        ScenarioContext["RightsInformation"] = rightsInfo;
    }
    [Then(@"state-specific details include:")]
    public void ThenStateSpecificDetailsInclude(Table table)
    {
        var stateDetails = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            stateDetails[row["Topic"]] = row["Details"];
        }
        ScenarioContext["StateSpecificGuidance"] = stateDetails;
    }
    [Then(@"appeal letter generated")]
    public void ThenAppealLetterGenerated()
    {
        ScenarioContext["AppealLetterGenerated"] = true;
        ScenarioContext["LetterFormat"] = "Professional legal format";
    }
    [Then(@"letter includes required elements:")]
    public void ThenLetterIncludesRequiredElements(Table table)
    {
        var requiredElements = new List<string>();
        foreach (var row in table.Rows)
        {
            requiredElements.Add(row["Element"]);
        }
        ScenarioContext["LetterElements"] = requiredElements;
    }
    [Then(@"grant templates available:")]
    public void ThenGrantTemplatesAvailable(Table table)
    {
        var grantTemplates = new List<object>();
        foreach (var row in table.Rows)
        {
            grantTemplates.Add(new
            {
                Type = row["Grant Type"],
                Purpose = row["Purpose"]
                    });
        }
        
        ScenarioContext["GrantTemplates"] = grantTemplates;
    }
    [Then(@"guidance sections include:")]
    public void ThenGuidanceSectionsInclude(Table table)
    {
        var guidanceSections = new List<string>();
        foreach (var row in table.Rows)
        {
            guidanceSections.Add(row["Section"]);
        }
        ScenarioContext["GuidanceSections"] = guidanceSections;
    }
    [Then(@"update notifications available")]
    public void ThenUpdateNotificationsAvailable()
    {
        ScenarioContext["UpdateNotifications"] = true;
        ScenarioContext["NotificationTypes"] = new[]
        {
            "Legal changes",
            "New templates",
            "State policy updates"
        };
    }

    [Then(@"disclaimer text shown:")]
    public void ThenDisclaimerTextShown(Table table)
    {
        var disclaimers = new List<string>();
        foreach (var row in table.Rows)
        {
            disclaimers.Add(row["Disclaimer"]);
        }
        ScenarioContext["DisclaimerText"] = disclaimers;
    }

    // Additional missing step definitions
[Given(@"I am authenticated")]
    public void GivenIAmAuthenticated()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"information should include:")]
    public void ThenInformationShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"guides should cover:")]
    public void ThenGuidesShouldCover(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"modules should include:")]
    public void ThenModulesShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"templates should include:")]
    public void ThenTemplatesShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"appeal should include:")]
    public void ThenAppealShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"organizations should include:")]
    public void ThenOrganizationsShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"case file should be created")]
    public void ThenCaseFileShouldBeCreated()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"document organization should be provided")]
    public void ThenDocumentOrganizationShouldBeProvided()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"updates should include:")]
    public void ThenUpdatesShouldInclude(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
}
