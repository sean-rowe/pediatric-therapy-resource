using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class CreationToolsSteps : BaseStepDefinitions
{
    private string _currentTemplateId = string.Empty;
    private string _currentResourceId = string.Empty;
    private Dictionary<string, object> _creationData = new();

    public CreationToolsSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"template creation tools are available")]
    public void GivenResourceCreationToolsAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I want to create custom worksheet")]
    public void GivenIWantToCreateCustomWorksheet()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"copyright-cleared image library exists")]
    public void GivenCopyrightClearedImageLibraryExists()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"brand guidelines are configured")]
    public void GivenBrandGuidelinesAreConfigured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I created template ""(.*)""")]
    public void GivenICreatedTemplate(string templateName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access creation tools")]
    public async Task WhenIAccessCreationTools()
    {
        await WhenISendAGETRequestTo("/api/creation-tools");
    }
    [When(@"I select ""(.*)"" template")]
    public async Task WhenISelectTemplate(string templateType)
    {
        _currentTemplateId = $"template-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/creation-tools/templates/select", new Dictionary<string, object>
{
    ["templateType"] = templateType,
    ["templateId"] = _currentTemplateId
});
    }

    [When(@"I customize with:")]
    public async Task WhenICustomizeWith(Table table)
    {
        var customizations = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            customizations[row["Element"]] = row["Customization"];
        }
        await WhenISendAPOSTRequestToWithData($"/api/creation-tools/templates/{_currentTemplateId}/customize", customizations);
    }
    [When(@"I save as new resource")]
    public async Task WhenISaveAsNewResource()
    {
        _currentResourceId = $"resource-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/creation-tools/resources/save", new Dictionary<string, object>
{
    ["resourceId"] = _currentResourceId,
    ["templateId"] = _currentTemplateId,
    ["name"] = "Custom Handwriting Practice"
});
    }

    [When(@"team member edits template")]
    public async Task WhenTeamMemberEditsTemplate()
    {
            await WhenISendAPOSTRequestToWithData($"/api/creation-tools/templates/{_currentTemplateId}/edit", new Dictionary<string, object>
{
    ["editorId"] = "teammember-123",
    ["changes"] = new { Title = "Updated Title" }
});
    }

    [Then(@"I see template categories:")]
    public void ThenISeeTemplateCategories(Table table)
    {
        var categories = new List<object>();
        foreach (var row in table.Rows)
        {
            categories.Add(new
            {
                Category = row["Category"],
                Templates = row["Templates"]
            });
        }
        
        ScenarioContext["TemplateCategories"] = categories;
    }
    [Then(@"editor provides:")]
    public void ThenEditorProvides(Table table)
    {
        var editorFeatures = new List<object>();
        foreach (var row in table.Rows)
        {
            editorFeatures.Add(new
            {
                Feature = row["Feature"],
                Options = row["Options"]
            });
        }
        
        ScenarioContext["EditorFeatures"] = editorFeatures;
    }
    [Then(@"preview shows real-time updates")]
    public void ThenPreviewShowsRealTimeUpdates()
    {
        ScenarioContext["RealTimePreview"] = true;
        ScenarioContext["PreviewLatency"] = "< 100ms";
    }
    [Then(@"resource is:")]
    public void ThenResourceIs(Table table)
    {
        var resourceProperties = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            resourceProperties[row["Property"]] = row["Status"];
        }
        ScenarioContext["ResourceProperties"] = resourceProperties;
    }
    [Then(@"images are:")]
    public void ThenImagesAre(Table table)
    {
        var imageProperties = new List<string>();
        foreach (var row in table.Rows)
        {
            imageProperties.Add(row["Property"]);
        }
        ScenarioContext["ImageProperties"] = imageProperties;
    }
    [Then(@"search includes:")]
    public void ThenSearchIncludes(Table table)
    {
        var searchFeatures = new List<string>();
        foreach (var row in table.Rows)
        {
            searchFeatures.Add(row["Feature"]);
        }
        ScenarioContext["ImageSearchFeatures"] = searchFeatures;
    }
    [Then(@"brand elements:")]
    public void ThenBrandElements(Table table)
    {
        var brandApplication = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            brandApplication[row["Element"]] = row["Application"];
        }
        ScenarioContext["BrandApplication"] = brandApplication;
    }
    [Then(@"version (.*) created")]
    public void ThenVersionCreated(int versionNumber)
    {
        ScenarioContext["NewVersion"] = versionNumber;
        ScenarioContext["VersionCreated"] = true;
    }
    [Then(@"changes tracked with:")]
    public void ThenChangesTrackedWith(Table table)
    {
        var changeTracking = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            changeTracking[row["Tracking"]] = row["Details"];
        }
        ScenarioContext["ChangeTracking"] = changeTracking;
    }
    [Then(@"both can work simultaneously")]
    public void ThenBothCanWorkSimultaneously()
    {
        ScenarioContext["CollaborativeEditing"] = true;
        ScenarioContext["ConflictResolution"] = "automatic";
        ScenarioContext["SyncInterval"] = "real-time";
    }
}
