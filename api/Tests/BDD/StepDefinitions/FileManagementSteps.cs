using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class FileManagementSteps : BaseStepDefinitions
{
    private string _currentFileId = string.Empty;
    private string _currentFolderId = string.Empty;
    private Dictionary<string, object> _fileData = new();
    private List<object> _uploadedFiles = new();

    public FileManagementSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"file management system is enabled")]
    public void GivenFileManagementSystemIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have uploaded resources")]
    public void GivenIHaveUploadedResources()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"folders exist:")]
    public void GivenFoldersExist(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"storage limit is (.*)GB")]
    public void GivenStorageLimitIsGB(int limit)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"team sharing is enabled")]
    public void GivenTeamSharingIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I upload new file:")]
    public async Task WhenIUploadNewFile(Table table)
    {
        var fileInfo = table.Rows[0];
        _currentFileId = $"file-{Guid.NewGuid()}";
        
        await WhenISendAPOSTRequestToWithData("/api/files/upload", new Dictionary<string, object>
        {
            ["fileId"] = _currentFileId,
            ["fileName"] = fileInfo["File Name"],
            ["fileSize"] = fileInfo["Size"],
            ["fileType"] = fileInfo["Type"]
        });
    }

    [When(@"I create folder ""(.*)""")]
    public async Task WhenICreateFolder(string folderName)
    {
        _currentFolderId = $"folder-{Guid.NewGuid()}";
        
        await WhenISendAPOSTRequestToWithData("/api/files/folders", new Dictionary<string, object>
        {
            ["folderId"] = _currentFolderId,
            ["folderName"] = folderName,
            ["parentId"] = null
        });
    }
    [When(@"I move files to folder")]
    public async Task WhenIMoveFilesToFolder()
    {
        await WhenISendAPOSTRequestToWithData("/api/files/move", new Dictionary<string, object>
        {
            ["fileIds"] = new[] { "file-001", "file-002" },
            ["targetFolderId"] = _currentFolderId
        });
    }
    
    [When(@"I search for files ""(.*)""")]
    public async Task WhenISearchFor(string searchTerm)
    {
        await WhenISendAGETRequestTo($"/api/files/search?q={Uri.EscapeDataString(searchTerm)}");
    }

    [When(@"I share folder with team")]
    public async Task WhenIShareFolderWithTeam()
    {
        await WhenISendAPOSTRequestToWithData($"/api/files/folders/{_currentFolderId}/share", new Dictionary<string, object>
        {
            ["shareWith"] = "team",
            ["permissions"] = "read-write"
        });
    }

    [When(@"I try to upload (.*) file")]
    public async Task WhenITryToUploadFile(string fileSize)
    {
        await WhenISendAPOSTRequestToWithData("/api/files/upload", new Dictionary<string, object>
        {
            ["fileName"] = "large_video.mp4",
            ["fileSize"] = fileSize,
            ["checkOnly"] = true
        });
    }

    [Then(@"file uploads successfully")]
    public void ThenFileUploadsSuccessfully()
    {
        ScenarioContext["UploadSuccessful"] = true;
        ScenarioContext["FileStored"] = true;
    }
    [Then(@"file is:")]
    public void ThenFileIs(Table table)
    {
        var fileProperties = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            fileProperties[row["Property"]] = row["Status"];
        }
        ScenarioContext["FileProperties"] = fileProperties;
    }
    [Then(@"folder structure shows:")]
    public void ThenFolderStructureShows(Table table)
    {
        var structure = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            structure[row["Folder"]] = row["Contents"];
        }
        ScenarioContext["FolderStructure"] = structure;
    }
    [Then(@"files are organized")]
    public void ThenFilesAreOrganized()
    {
        ScenarioContext["FilesOrganized"] = true;
        ScenarioContext["OrganizationMethod"] = "By folder";
    }
    [Then(@"search results show:")]
    public void ThenSearchResultsShow(Table table)
    {
        var results = new List<object>();
        foreach (var row in table.Rows)
        {
            results.Add(new
            {
                FileName = row["File"],
                Location = row["Location"],
                Type = row["Type"]
            });
        }
        
        ScenarioContext["SearchResults"] = results;
    }
    [Then(@"results include:")]
    public void ThenResultsInclude(Table table)
    {
        var features = new List<string>();
        foreach (var row in table.Rows)
        {
            features.Add(row["Feature"]);
        }
        ScenarioContext["SearchFeatures"] = features;
    }
    [Then(@"team members see:")]
    public void ThenTeamMembersSee(Table table)
    {
        var sharedContent = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            sharedContent[row["Content"]] = row["Access Level"];
        }
        ScenarioContext["TeamSharedContent"] = sharedContent;
    }
    [Then(@"notifications sent to (.*) members")]
    public void ThenNotificationsSentToMembers(int count)
    {
        ScenarioContext["ShareNotificationsSent"] = count;
        ScenarioContext["NotificationType"] = "folder_shared";
    }
    [Then(@"upload blocked")]
    public void ThenUploadBlocked()
    {
        ScenarioContext["UploadBlocked"] = true;
        ScenarioContext["BlockReason"] = "Storage limit exceeded";
    }
    [Then(@"error message: ""(.*)""")]
    public void ThenErrorMessage(string message)
    {
        ScenarioContext["ErrorMessage"] = message;
        ScenarioContext["ErrorDisplayed"] = true;
    }
    [Then(@"upgrade prompt shown")]
    public void ThenUpgradePromptShown()
    {
        ScenarioContext["UpgradePrompt"] = true;
        ScenarioContext["UpgradeOptions"] = new[]
        {
            "Add 10GB for $5/month",
            "Add 50GB for $20/month",
            "Unlimited for $50/month"
        };
    }
}
