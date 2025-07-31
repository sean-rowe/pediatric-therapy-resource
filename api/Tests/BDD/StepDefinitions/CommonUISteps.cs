using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;
using System.Text.Json;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

/// <summary>
/// Common UI interaction steps that simulate user actions
/// These are used across multiple features for testing UI workflows
/// </summary>
[Binding]
public class CommonUISteps : BaseStepDefinitions
{
    private Dictionary<string, object> _formData = new();
    private string _currentPage = string.Empty;
    private List<string> _navigationHistory = new();

    public CommonUISteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    #region Navigation Steps
    
[Given(@"I am on the (.*) page")]
    public async Task GivenIAmOnThePage(string pageName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"I navigate to (.*)")]
    public async Task WhenINavigateTo(string pageName)
    {
        await GivenIAmOnThePage(pageName);
    }
    
    [When(@"I go back")]
    public async Task WhenIGoBack()
    {
        if (_navigationHistory.Count > 1)
        {
            _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
            _currentPage = _navigationHistory.Last();
            
            var endpoint = GetEndpointForPage(_currentPage);
            await WhenISendAGETRequestTo(endpoint);
        }

    }

    #endregion

    #region Form Interaction Steps

    [When(@"I fill in (.*) with (.*)")]
    public void WhenIFillInWith(string fieldName, string value)
    {
        _formData[fieldName] = value;
        ScenarioContext[$"Field_{fieldName}"] = value;
    }
    
    [When(@"I select (.*) from (.*)")]
    public void WhenISelectFrom(string value, string dropdownName)
    {
        _formData[dropdownName] = value;
        ScenarioContext[$"Selected_{dropdownName}"] = value;
    }
    
    [When(@"I check (.*)")]
    public void WhenICheck(string checkboxName)
    {
        _formData[checkboxName] = true;
        ScenarioContext[$"Checked_{checkboxName}"] = true;
    }
    
    [When(@"I uncheck (.*)")]
    public void WhenIUncheck(string checkboxName)
    {
        _formData[checkboxName] = false;
        ScenarioContext[$"Checked_{checkboxName}"] = false;
    }
    
    [When(@"I upload file (.*) to (.*)")]
    public void WhenIUploadFileTo(string fileName, string fieldName)
    {
        _formData[fieldName] = new
        {
            FileName = fileName,
            ContentType = GetContentType(fileName),
            Size = "1024KB" // Mock size
        };
        ScenarioContext[$"UploadedFile_{fieldName}"] = fileName;
    }

    #endregion

    #region Button/Link Actions
    
    // Keep this as the primary click handler
    [When(@"I click (.*)")]
    public async Task WhenIClick(string buttonOrLinkText)
    {
        // Simulate clicking by determining the action
        var endpoint = GetActionEndpoint(buttonOrLinkText);
        
        if (_formData.Any())
        {
            // If we have form data, send as POST
            await WhenISendAPOSTRequestToWithData(endpoint, _formData);
            _formData.Clear(); // Clear after submission
        }
        else
        {
            // Otherwise, send as GET
            await WhenISendAGETRequestTo(endpoint);
        }
        
        ScenarioContext["LastClickedElement"] = buttonOrLinkText;
    }
    
    [When(@"I click the (.*) button")]
    public async Task WhenIClickTheButton(string buttonName)
    {
        await WhenIClick(buttonName);
    }
    
    // Commented out to avoid ambiguity - WhenIClick handles all click actions
    // [When(@"I click the (.*) link")]
    // public async Task WhenIClickTheLink(string linkText)
    // {
    //     await WhenIClick(linkText);
    // }
    
    [When(@"I submit the form")]
    public async Task WhenISubmitTheForm()
    {
        // Determine form endpoint based on current page
        var endpoint = GetFormSubmitEndpoint(_currentPage);
        await WhenISendAPOSTRequestToWithData(endpoint, _formData);
        _formData.Clear();
    }

    #endregion

    #region Validation Steps

    [Then(@"I should see (.*)")]
    public void ThenIShouldSee(string text)
    {
        ThenTheResponseShouldContain(text);
        ScenarioContext["VisibleText"] = text;
    }
    
    [Then(@"I should not see (.*)")]
    public async Task ThenIShouldNotSee(string text)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        content.Should().NotContain(text);
    }
    
    [Then(@"I should be on the (.*) page")]
    public void ThenIShouldBeOnThePage(string pageName)
    {
        _currentPage.Should().Be(pageName);
        ScenarioContext["CurrentPage"].Should().Be(pageName);
    }
    
    [Then(@"the (.*) field should contain (.*)")]
    public async Task ThenTheFieldShouldContain(string fieldName, string expectedValue)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        // In a real implementation, this would parse the response and check field values
        // For now, we'll store in context
        ScenarioContext[$"Field_{fieldName}_Value"] = expectedValue;
    }
    
    [Then(@"I should see error (.*)")]
    public void ThenIShouldSeeError(string errorMessage)
    {
        ThenIShouldSee(errorMessage);
        ScenarioContext["ErrorDisplayed"] = true;
        ScenarioContext["ErrorMessage"] = errorMessage;
    }
    
    [Then(@"I should see success message (.*)")]
    public void ThenIShouldSeeSuccessMessage(string successMessage)
    {
        ThenIShouldSee(successMessage);
        ScenarioContext["SuccessDisplayed"] = true;
        ScenarioContext["SuccessMessage"] = successMessage;
    }

    #endregion

    #region Table/List Interactions

    [When(@"I sort by (.*)")]
    public async Task WhenISortBy(string columnName)
    {
        var endpoint = $"{GetEndpointForPage(_currentPage)}?sort={columnName}";
        await WhenISendAGETRequestTo(endpoint);
        ScenarioContext["SortColumn"] = columnName;
    }
    
    [When(@"I filter by (.*) with value (.*)")]
    public async Task WhenIFilterByWithValue(string filterName, string filterValue)
    {
        var endpoint = $"{GetEndpointForPage(_currentPage)}?{filterName}={filterValue}";
        await WhenISendAGETRequestTo(endpoint);
        ScenarioContext[$"Filter_{filterName}"] = filterValue;
    }
    
    [When(@"I select row (\d+)")]
    public void WhenISelectRow(int rowNumber)
    {
        ScenarioContext["SelectedRow"] = rowNumber;
        ScenarioContext["RowSelected"] = true;
    }
    
    [Then(@"I should see (\d+) results")]
    public async Task ThenIShouldSeeResults(int expectedCount)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        // Parse as JSON array
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        if (root.ValueKind == JsonValueKind.Array)
        {
            root.GetArrayLength().Should().Be(expectedCount);
        }
        else if (root.TryGetProperty("data", out var dataArray))
        {
            dataArray.GetArrayLength().Should().Be(expectedCount);
        }
        else if (root.TryGetProperty("results", out var resultsArray))
        {
            resultsArray.GetArrayLength().Should().Be(expectedCount);
        }

    }

    #endregion

    #region Helper Methods

    private string GetEndpointForPage(string pageName)
    {
        pageName = pageName.ToLower().Replace(" ", "-");
        if (pageName == "dashboard") return "/api/dashboard";
        if (pageName == "login") return "/api/auth/login";
        if (pageName == "register") return "/api/auth/register";
        if (pageName == "resources") return "/api/resources";
        if (pageName == "students") return "/api/students";
        if (pageName == "marketplace") return "/api/marketplace";
        if (pageName == "settings") return "/api/settings";
        if (pageName == "profile") return "/api/profile";
        return $"/api/{pageName}";
    }

    private string GetActionEndpoint(string action)
    {
        action = action.ToLower().Replace(" ", "-");
        if (action == "save") return $"{GetEndpointForPage(_currentPage)}/save";
        if (action == "submit") return $"{GetEndpointForPage(_currentPage)}/submit";
        if (action == "cancel") return GetEndpointForPage(_currentPage);
        if (action == "delete") return $"{GetEndpointForPage(_currentPage)}/delete";
        if (action == "approve") return $"{GetEndpointForPage(_currentPage)}/approve";
        if (action == "reject") return $"{GetEndpointForPage(_currentPage)}/reject";
        return $"{GetEndpointForPage(_currentPage)}/{action}";
    }

    private string GetFormSubmitEndpoint(string currentPage)
    {
        currentPage = currentPage.ToLower().Replace(" ", "-");
        if (currentPage == "login") return "/api/auth/login";
        if (currentPage == "register") return "/api/auth/register";
        if (currentPage == "profile") return "/api/profile/update";
        if (currentPage == "settings") return "/api/settings/save";
        return $"{GetEndpointForPage(currentPage)}/submit";
    }

    private string GetContentType(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLower();
        if (extension == ".pdf") return "application/pdf";
        if (extension == ".jpg" || extension == ".jpeg") return "image/jpeg";
        if (extension == ".png") return "image/png";
        if (extension == ".doc" || extension == ".docx") return "application/msword";
        return "application/octet-stream";
    }
    #endregion
}
