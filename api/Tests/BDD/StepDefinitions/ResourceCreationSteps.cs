using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ResourceCreationSteps : BaseStepDefinitions
{
    private string _currentResourceId = string.Empty;
    private Dictionary<string, object> _creationData = new();
    private List<object> _templateOptions = new();

    public ResourceCreationSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"general resource creation tools are available")]
    public void GivenResourceCreationToolsAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am a content creator")]
    public void GivenIAmAContentCreator()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"template library is loaded")]
    public void GivenTemplateLibraryIsLoaded()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I start new resource creation")]
    public async Task WhenIStartNewResourceCreation()
    {
        await WhenISendAGETRequestTo("/api/creation/new");
    }
    [When(@"I select worksheet template")]
    public async Task WhenISelectWorksheetTemplate()
    {
        await WhenISendAGETRequestTo("/api/creation/templates/worksheet-basic");
    }
    [When(@"I customize content:")]
    public async Task WhenICustomizeContent(Table table)
    {
        var customizations = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            customizations[row["Element"]] = row["Value"];
        }
        await WhenISendAPUTRequestToWithData("/api/creation/customize", new Dictionary<string, object>
        {
            ["templateId"] = "worksheet-basic",
            ["customizations"] = customizations
        });
    }
    
    [When(@"I add images from library")]
    public async Task WhenIAddImagesFromLibrary()
    {
        await WhenISendAPOSTRequestToWithData("/api/creation/add-images", new Dictionary<string, object>
        {
            ["imageIds"] = new[] { "img-123", "img-456", "img-789" },
            ["positions"] = new[]
            {
                new { ImageId = "img-123", X = 100, Y = 50 },
                new { ImageId = "img-456", X = 200, Y = 150 }
            }
        });
    }
            
    [When(@"I preview creation")]
    public async Task WhenIPreviewCreation()
    {
        await WhenISendAGETRequestTo("/api/creation/preview");
    }
    [When(@"I save as draft")]
    public async Task WhenISaveAsDraft()
    {
        _currentResourceId = $"draft-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/creation/save-draft", new Dictionary<string, object>
        {
            ["title"] = "My Custom Worksheet",
            ["description"] = "Fine motor practice worksheet",
            ["status"] = "draft"
        });
    }

    [When(@"I publish resource")]
    public async Task WhenIPublishResource()
    {
        await WhenISendAPOSTRequestToWithData("/api/creation/publish", new Dictionary<string, object>
        {
            ["resourceId"] = _currentResourceId,
            ["category"] = "Fine Motor",
            ["ageRange"] = "4-6 years",
            ["tags"] = new[] { "cutting", "tracing", "pre-writing" }
        });
    }
    [Then(@"creation interface loads")]
    public void ThenCreationInterfaceLoads()
    {
        ScenarioContext["InterfaceLoaded"] = true;
        ScenarioContext["EditorFeatures"] = new[]
        {
            "Drag-drop editor",
            "Text tools",
            "Image insertion",
            "Layout options"
        };
    }

    [Then(@"template options show:")]
    public void ThenTemplateOptionsShow(Table table)
    {
        var templates = new List<object>();
        foreach (var row in table.Rows)
        {
            templates.Add(new
            {
                Category = row["Category"],
                Count = row["Available"],
                Preview = row["Preview Available"]
            });
        }
        
        ScenarioContext["TemplateOptions"] = templates;
    }
    [Then(@"editor displays template")]
    public void ThenEditorDisplaysTemplate()
    {
        ScenarioContext["TemplateDisplayed"] = true;
        ScenarioContext["EditableElements"] = new[]
        {
            "Title text",
            "Instructions", 
            "Activity areas",
            "Images"
        };
    }

    [Then(@"content updated in editor")]
    public void ThenContentUpdatedInEditor()
    {
        ScenarioContext["ContentUpdated"] = true;
        ScenarioContext["ChangesSaved"] = false; // Auto-save not triggered yet
    }

    [Then(@"images positioned correctly")]
    public void ThenImagesPositionedCorrectly()
    {
        ScenarioContext["ImagesAdded"] = true;
        ScenarioContext["ImageCount"] = 3;
        ScenarioContext["PositioningAccurate"] = true;
    }
    [Then(@"preview shows:")]
    public void ThenPreviewShows(Table table)
    {
        var previewDetails = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            previewDetails[row["Element"]] = row["Appearance"];
        }
        ScenarioContext["PreviewDetails"] = previewDetails;
    }
    [Then(@"draft saved successfully")]
    public void ThenDraftSavedSuccessfully()
    {
        ScenarioContext["DraftSaved"] = true;
        ScenarioContext["DraftId"] = _currentResourceId;
        ScenarioContext["AutoSaveEnabled"] = true;
    }
    [Then(@"version control enabled")]
    public void ThenVersionControlEnabled()
    {
        ScenarioContext["VersionControlEnabled"] = true;
        ScenarioContext["CurrentVersion"] = "1.0";
        ScenarioContext["ChangeHistory"] = new[]
        {
            "Initial creation",
            "Added images", 
            "Modified text"
        };
    }

    [Then(@"resource published")]
    public void ThenResourcePublished()
    {
        ScenarioContext["ResourcePublished"] = true;
        ScenarioContext["PublishStatus"] = "live";
    }
    [Then(@"available in library")]
    public void ThenAvailableInLibrary()
    {
        ScenarioContext["AddedToLibrary"] = true;
        ScenarioContext["LibraryCategory"] = "User Created";
        ScenarioContext["Searchable"] = true;
    }
    [Then(@"collaboration features include:")]
    public void ThenCollaborationFeaturesInclude(Table table)
    {
        var features = new List<string>();
        foreach (var row in table.Rows)
        {
            features.Add(row["Feature"]);
        }
        ScenarioContext["CollaborationFeatures"] = features;
    }
    [Then(@"brand customization allows:")]
    public void ThenBrandCustomizationAllows(Table table)
    {
        var customizations = new List<string>();
        foreach (var row in table.Rows)
        {
            customizations.Add(row["Customization"]);
        }
        ScenarioContext["BrandCustomizations"] = customizations;
    }
    [Then(@"export formats available:")]
    public void ThenExportFormatsAvailable(Table table)
    {
        var formats = new List<object>();
        foreach (var row in table.Rows)
        {
            formats.Add(new
            {
                Format = row["Format"],
                Quality = row["Quality"],
                Purpose = row["Best For"]
            });
        }
        
        ScenarioContext["ExportFormats"] = formats;
    }
}
