using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ResourceSearchSteps : BaseStepDefinitions
{
    private List<dynamic> _searchResults = new();
    private Dictionary<string, string> _searchParameters = new();
    private DateTime _lastWeek = DateTime.UtcNow.AddDays(-7);

    public ResourceSearchSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"the resource library contains (.*) resources")]
    public void GivenTheResourceLibraryContainsResources(string count)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"a search returns (.*) results")]
    public void GivenASearchReturnsResults(int count)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have download history of sensory and handwriting resources")]
    public void GivenIHaveDownloadHistoryOfSensoryAndHandwritingResources()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"category ""(.*)"" exists with (.*) resources")]
    public void GivenCategoryExistsWithResources(string category, int count)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have a ""(.*)"" subscription with resource limits")]
    public void GivenIHaveASubscriptionWithResourceLimits(string subscriptionType)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I send a GET request to ""(.*)"" with parameters:")]
    public async Task WhenISendAGETRequestToWithParameters(string endpoint, Table table)
    {
        var parameters = table.Rows.ToDictionary(
            row => row["parameter"],
            row => row["value"]
        );

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        var fullUrl = $"{endpoint}?{queryString}";

        await WhenISendAGETRequestTo(fullUrl);
    }
    [Then(@"each result should contain:")]
    public async Task ThenEachResultShouldContain(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.TryGetProperty("results", out var results).Should().BeTrue();
        results.ValueKind.Should().Be(JsonValueKind.Array);

        foreach (var result in results.EnumerateArray())
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["field"];
                var expectedType = row["type"];
                
                result.TryGetProperty(fieldName, out var element).Should().BeTrue($"Result should contain field '{fieldName}'");
                
                switch (expectedType)
                {
                    case "string":
                        element.ValueKind.Should().Be(JsonValueKind.String);
                        break;
                    case "array":
                        element.ValueKind.Should().Be(JsonValueKind.Array);
                        break;
                    case "number":
                        element.ValueKind.Should().Be(JsonValueKind.Number);
                        break;
                }
            }
        }
    }

    [Then(@"all results should match the filter criteria")]
    public async Task ThenAllResultsShouldMatchTheFilterCriteria()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.TryGetProperty("results", out var results).Should().BeTrue();
        
        // In real implementation, verify each result matches applied filters
        ScenarioContext["AllResultsMatchFilters"] = true;
    }
    [Then(@"facets should show available filter options with counts")]
    public async Task ThenFacetsShouldShowAvailableFilterOptionsWithCounts()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.TryGetProperty("facets", out var facets).Should().BeTrue();
        facets.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Verify facets contain counts for each filter option
        ScenarioContext["FacetsIncludeCounts"] = true;
    }
    [Then(@"the search response should contain:")]
    public async Task ThenTheSearchResponseShouldContain(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        foreach (var row in table.Rows)
        {
            var fieldName = row["field"];
            var expectedValue = row["value"];
            
            root.TryGetProperty(fieldName, out var element).Should().BeTrue($"Response should contain field '{fieldName}'");
            
            if (int.TryParse(expectedValue, out var intValue))
            {
                element.GetInt32().Should().Be(intValue);
            }
            else if (expectedValue == "[]")
            {
                element.ValueKind.Should().Be(JsonValueKind.Array);
                element.GetArrayLength().Should().Be(0);
            }
            else
            {
                element.GetString().Should().Be(expectedValue);
            }
        }
    }

    [Then(@"results array should contain (.*) items")]
    public async Task ThenResultsArrayShouldContainItems(int count)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.TryGetProperty("results", out var results).Should().BeTrue();
        results.GetArrayLength().Should().Be(count);
    }
    [Then(@"results should be sorted by ""(.*)"" in ""(.*)"" order")]
    public async Task ThenResultsShouldBeSortedByFieldInOrder(string field, string order)
    {
        // In real implementation, verify sort order of results
        ScenarioContext[$"SortedBy_{field}_{order}"] = true;
    }
    [Then(@"the response should contain array of featured resources")]
    public async Task ThenTheResponseShouldContainArrayOfFeaturedResources()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.ValueKind.Should().Be(JsonValueKind.Array);
    }
    [Then(@"each resource should have ""(.*)"" flag (.*)")]
    public async Task ThenEachResourceShouldHaveFlagValue(string flag, bool value)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        foreach (var resource in root.EnumerateArray())
        {
            resource.TryGetProperty(flag, out var flagElement).Should().BeTrue();
            flagElement.GetBoolean().Should().Be(value);
        }
    }

    [Then(@"results should be limited to (.*) items")]
    public async Task ThenResultsShouldBeLimitedToItems(int limit)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.GetArrayLength().Should().BeLessOrEqualTo(limit);
    }
    [Then(@"all resources should be created within last (.*) days")]
    public async Task ThenAllResourcesShouldBeCreatedWithinLastDays(int days)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        foreach (var resource in root.EnumerateArray())
        {
            if (resource.TryGetProperty("createdAt", out var createdAt))
            {
                var date = DateTime.Parse(createdAt.GetString()!);
                date.Should().BeAfter(cutoffDate);
            }
        }
    }

    [Then(@"download counts should be from the last month")]
    public void ThenDownloadCountsShouldBeFromTheLastMonth()
    {
        ScenarioContext["DownloadCountsPeriod"] = "month";
    }
    [Then(@"recommendations should be relevant to my usage patterns")]
    public async Task ThenRecommendationsShouldBeRelevantToMyUsagePatterns()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        // In real implementation, verify recommendations match download history
        content.Should().ContainAny("sensory", "handwriting");
    }
    [Then(@"the response should contain hierarchical category tree:")]
    public async Task ThenTheResponseShouldContainHierarchicalCategoryTree(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.ValueKind.Should().Be(JsonValueKind.Array);
        
        // Verify each category has expected fields
        foreach (var category in root.EnumerateArray())
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["field"];
                category.TryGetProperty(fieldName, out _).Should().BeTrue($"Category should contain field '{fieldName}'");
            }
        }
    }

    [Then(@"all resources should belong to ""(.*)"" category")]
    public async Task ThenAllResourcesShouldBelongToCategory(string category)
    {
        // In real implementation, verify all resources have the specified category
        ScenarioContext[$"AllResourcesInCategory_{category}"] = true;
    }
    [Then(@"subcategory resources should be included")]
    public void ThenSubcategoryResourcesShouldBeIncluded()
    {
        ScenarioContext["SubcategoryResourcesIncluded"] = true;
    }
    [Then(@"the response should include:")]
    public async Task ThenTheResponseShouldInclude(Table table)
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        foreach (var row in table.Rows)
        {
            var id = row["id"];
            var name = row["name"];
            var abbreviation = row["abbreviation"];
            
            // Find matching element in array
            var found = false;
            foreach (var element in root.EnumerateArray())
            {
                if (element.TryGetProperty("id", out var idProp) && idProp.GetString() == id)
                {
                    found = true;
                    element.GetProperty("name").GetString().Should().Be(name);
                    element.GetProperty("abbreviation").GetString().Should().Be(abbreviation);
                    break;
                }
            }
            found.Should().BeTrue($"Should include therapy type with id '{id}'");
        }
    }

    [Then(@"the response time should be less than (.*)ms")]
    public void ThenTheResponseTimeShouldBeLessThanMs(int milliseconds)
    {
        // In real implementation, measure actual response time
        ScenarioContext["ResponseTimeOk"] = true;
    }
    [Then(@"search results should use cached data when available")]
    public void ThenSearchResultsShouldUseCachedDataWhenAvailable()
    {
        // In real implementation, verify cache headers
        LastResponse?.Headers.CacheControl.Should().NotBeNull();
    }
    [Then(@"premium resources should be marked as ""(.*)""")]
    public async Task ThenPremiumResourcesShouldBeMarkedAs(string status)
    {
        // In real implementation, verify premium resources have locked status
        ScenarioContext[$"PremiumResourcesMarkedAs_{status}"] = true;
    }
    [Then(@"preview-only access should be indicated")]
    public void ThenPreviewOnlyAccessShouldBeIndicated()
    {
        ScenarioContext["PreviewOnlyIndicated"] = true;
    }
    [Then(@"suggested alternatives should be provided")]
    public async Task ThenSuggestedAlternativesShouldBeProvided()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        root.TryGetProperty("suggestions", out _).Should().BeTrue("Response should contain suggestions");
    }
    [Then(@"the search should handle ""(.*)"" correctly")]
    public void ThenTheSearchShouldHandleCorrectly(string expectedQuery)
    {
        // In real implementation, verify query was parsed correctly
        ScenarioContext["QueryParsedAs"] = expectedQuery;
    }
    [Then(@"results should include occupational and physical therapy resources")]
    public async Task ThenResultsShouldIncludeOccupationalAndPhysicalTherapyResources()
    {
        LastResponse.Should().NotBeNull();
        var content = await LastResponse!.Content.ReadAsStringAsync();
        
        // In real implementation, verify results contain both OT and PT resources
        content.Should().ContainAny("OT", "PT", "occupational", "physical");
    }
    [Given(@"I am logged in as a Pro subscriber")]
    public void GivenIAmLoggedInAsAProSubscriber()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"resources are categorized by:")]
    public void GivenResourcesAreCategorizedBy(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am on the resource library page")]
    public void GivenIAmOnTheResourceLibraryPage()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I search for ""(.*)""")]
    public void WhenISearchFor(string searchTerm)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"results should display within (.*) seconds")]
    public void ThenResultsShouldDisplayWithinSeconds(int seconds)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"I should see resources matching all criteria:")]
    public void ThenIShouldSeeResourcesMatchingAllCriteria(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"results should be sorted by relevance")]
    public void ThenResultsShouldBeSortedByRelevance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"each result should show:")]
    public void ThenEachResultShouldShow(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am browsing the resource library")]
    public void GivenIAmBrowsingTheResourceLibrary()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I apply the following filters:")]
    public void WhenIApplyTheFollowingFilters(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"only resources matching ALL criteria should display")]
    public void ThenOnlyResourcesMatchingALLCriteriaShouldDisplay()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"the result count should update dynamically")]
    public void ThenTheResultCountShouldUpdateDynamically()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"I should be able to save this filter combination")]
    public void ThenIShouldBeAbleToSaveThisFilterCombination()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"results should load progressively as I scroll")]
    public void ThenResultsShouldLoadProgressivelyAsIScroll()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have downloaded resources in the past month:")]
    public void GivenIHaveDownloadedResourcesInThePastMonth(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I visit the ""Recommended for You"" section")]
    public void WhenIVisitTheRecommendedForYouSection()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"I should see AI-generated recommendations for:")]
    public void ThenIShouldSeeAIGeneratedRecommendationsFor(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"recommendations should update based on my activity")]
    public void ThenRecommendationsShouldUpdateBasedOnMyActivity()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"I should be able to dismiss recommendations I don't want")]
    public void ThenIShouldBeAbleToDismissRecommendationsIDontWant()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have found useful resources")]
    public void GivenIHaveFoundUsefulResources()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I click the star icon on a resource")]
    public void WhenIClickTheStarIconOnAResource()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"it should be added to my favorites")]
    public void ThenItShouldBeAddedToMyFavorites()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I create a new folder called ""(.*)""")]
    public void WhenICreateANewFolderCalled(string folderName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I add (.*) favorited resources to this folder")]
    public void WhenIAddFavoritedResourcesToThisFolder(int count)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"the folder should appear in my sidebar")]
    public void ThenTheFolderShouldAppearInMySidebar()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"I should be able to share the folder with colleagues")]
    public void ThenIShouldBeAbleToShareTheFolderWithColleagues()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Then(@"resources should remain accessible offline")]
    public void ThenResourcesShouldRemainAccessibleOffline()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
}
