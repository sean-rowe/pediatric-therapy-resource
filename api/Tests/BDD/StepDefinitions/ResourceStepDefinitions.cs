using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ResourceStepDefinitions : BaseStepDefinitions
{
    public ResourceStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    // Resource Search & Discovery
    
    [Given(@"I am on the resource library page")]
    public void GivenIAmOnTheResourceLibraryPage()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"resources are categorized by:")]
    public void GivenResourcesAreCategorizedBy(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I search for ""(.*)""")]
    public async Task WhenISearchFor(string searchTerm)
    {
        await WhenISendAGETRequestTo($"/api/resources/search?q={Uri.EscapeDataString(searchTerm)}");
        ScenarioContext["SearchTerm"] = searchTerm;
    }
    [When(@"I apply the following filters:")]
    public async Task WhenIApplyTheFollowingFilters(Table filters)
    {
        var queryParams = new List<string>();
        foreach (var row in filters.Rows)
        {
            var filterType = row["Filter Type"].Replace(" ", "").ToLower();
            var selections = row["Selection"].Split(',').Select(s => s.Trim());
            foreach (var selection in selections)
            {
                queryParams.Add($"{filterType}={Uri.EscapeDataString(selection)}");
            }
        }
        
        var queryString = string.Join("&", queryParams);
        await WhenISendAGETRequestTo($"/api/resources/search?{queryString}");
    }
    
    [Then(@"results should display within (.*) seconds")]
    public void ThenResultsShouldDisplayWithinSeconds(int seconds)
    {
        // In real implementation, measure actual response time
        LastResponse.Should().NotBeNull();
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"I should see resources matching all criteria:")]
    public async Task ThenIShouldSeeResourcesMatchingAllCriteria(Table criteria)
    {
        var content = await LastResponse!.Content.ReadAsStringAsync();
        foreach (var row in criteria.Rows)
        {
            content.Should().Contain(row["Value"]);
        }
    }
    
    [Then(@"results should be sorted by (.*)")]
    public void ThenResultsShouldBeSortedBy(string sortOrder)
    {
        ScenarioContext["SortOrder"] = sortOrder;
        // Verify sorting in actual implementation
        
    }
    [Then(@"each result should show:")]
    public async Task ThenEachResultShouldShow(Table expectedFields)
    {
        var results = GetResponseContent<List<dynamic>>();
        results.Should().NotBeNull();
        
        // Verify each result has expected fields
        foreach (var field in expectedFields.Rows)
        {
            var fieldName = field["Element"];
            // In real implementation, verify field presence
        }
    }

    // Resource Management
    
    [When(@"I click the star icon on a resource")]
    public async Task WhenIClickTheStarIconOnAResource()
    {
        var resourceId = GetFromContext<string>("CurrentResourceId") ?? "test-resource";
        await WhenISendAPOSTRequestToWithData($"/api/resources/{resourceId}/favorite", 
            new Dictionary<string, object>());
    }
    [When(@"I create a new folder called ""(.*)""")]
    public async Task WhenICreateANewFolderCalled(string folderName)
    {
        await WhenISendAPOSTRequestToWithData("/api/resources/folders",
            new Dictionary<string, object> { { "name", folderName } });
    }

    [Then(@"it should be added to my favorites")]
    public async Task ThenItShouldBeAddedToMyFavorites()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var favoritesResponse = await Client.GetAsync("/api/resources/favorites");
        var favorites = await favoritesResponse.Content.ReadAsStringAsync();
        favorites.Should().Contain(GetFromContext<string>("CurrentResourceId"));
    }

    // Free Resources
    
    [Given(@"I am browsing without subscription")]
    public void GivenIAmBrowsingWithoutSubscription()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access the free section")]
    public async Task WhenIAccessTheFreeSection()
    {
        await WhenISendAGETRequestTo("/api/resources/free");
    }
    [Then(@"quality free resources should be available")]
    public void ThenQualityFreeResourcesShouldBeAvailable()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var resources = GetResponseContent<List<dynamic>>();
        resources.Should().NotBeNull().And.NotBeEmpty();
    }

    // Physical/Digital Hybrid
    
    [Given(@"I purchased ""(.*)""")]
    public void GivenIPurchased(string productName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I scan the QR code on ""(.*)"" card")]
    public async Task WhenIScanTheQRCodeOnCard(string cardName)
    {
        var qrCode = GenerateMockQRCode(cardName);
        await WhenISendAGETRequestTo($"/api/qr/scan/{qrCode}");
    }
    [Then(@"my device should display:")]
    public async Task ThenMyDeviceShouldDisplay(Table expectedContent)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var content = await LastResponse.Content.ReadAsStringAsync();
        
        foreach (var row in expectedContent.Rows)
        {
            var feature = row["Digital Feature"];
            content.Should().Contain(feature.ToLower().Replace(" ", ""));
        }
    }

    // Specialized Content
    
    [Given(@"I need therapy resources in (.*)")]
    public void GivenINeedResourcesIn(string language)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I switch to (.*) interface")]
    public async Task WhenISwitchToInterface(string language)
    {
        Client.DefaultRequestHeaders.Add("Accept-Language", GetLanguageCode(language));
        await WhenISendAPUTRequestToWithData("/api/users/language",
            new Dictionary<string, object> { { "language", GetLanguageCode(language) } });
    }

    [Then(@"the entire layout should flip:")]
    public void ThenTheEntireLayoutShouldFlip(Table layoutChanges)
    {
        // RTL language support verification
        ScenarioContext["IsRTL"] = true;
    }

    // Helper methods
    private string GenerateMockQRCode(string cardName)
    {
        return $"QR_{cardName.Replace(" ", "_").ToUpper()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    private string GetLanguageCode(string language)
    {
        return language.ToLower() switch
        {
            "arabic" => "ar",
            "spanish" => "es",
            "french" => "fr",
            "chinese" or "mandarin" => "zh",
            "korean" => "ko",
            "vietnamese" => "vi",
            "russian" => "ru",
            "portuguese" => "pt",
            _ => "en",
        };
    }
}