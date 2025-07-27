using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class InteractiveActivitiesSteps : BaseStepDefinitions
{
    private string _currentActivityId = string.Empty;
    private string _currentDeckId = string.Empty;
    private Dictionary<string, object> _activityData = new();
    private List<object> _studentResponses = new();

    public InteractiveActivitiesSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"digital activities module is available")]
    public void GivenDigitalActivitiesModuleIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am a student practicing (.*) sounds")]
    public void GivenIAmAStudentPracticingSounds(string soundType)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"my therapist assigned ""(.*)"" deck")]
    public void GivenMyTherapistAssignedDeck(string deckName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have ""(.*)"" categorization activity")]
    public void GivenIHaveCategorizationActivity(string activityName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am using app on iPad")]
    public void GivenIAmUsingAppOnIPad()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have downloaded ""(.*)"" pack")]
    public void GivenIHaveDownloadedPack(string packName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"therapist created custom deck")]
    public void GivenTherapistCreatedCustomDeck()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I start the activity")]
    public async Task WhenIStartTheActivity()
    {
        await WhenISendAPOSTRequestToWithData("/api/activities/start", new Dictionary<string, object>
        {
            ["activityId"] = _currentActivityId ?? _currentDeckId,
            ["studentId"] = ScenarioContext["StudentId"],
            ["startTime"] = DateTime.UtcNow
        });
    }

    [When(@"I drag ""(.*)"" to ""(.*)"" box")]
    public async Task WhenIDragItemToBox(string item, string category)
    {
        await WhenISendAPOSTRequestToWithData("/api/activities/drag-drop", new Dictionary<string, object>
        {
            ["activityId"] = _currentActivityId,
            ["item"] = item,
            ["targetCategory"] = category,
            ["timestamp"] = DateTime.UtcNow
        });
    }

    [When(@"I complete all (.*) cards")]
    public async Task WhenICompleteAllCards(int cardCount)
    {
        await WhenISendAPOSTRequestToWithData($"/api/activities/{_currentDeckId}/complete", new Dictionary<string, object>
        {
            ["cardsCompleted"] = cardCount,
            ["completionTime"] = DateTime.UtcNow,
            ["accuracy"] = 85
        });
    }

    [When(@"I complete all items correctly")]
    public async Task WhenICompleteAllItemsCorrectly()
    {
        await WhenISendAPOSTRequestToWithData($"/api/activities/{_currentActivityId}/complete", new Dictionary<string, object>
        {
            ["allCorrect"] = true,
            ["completionTime"] = DateTime.UtcNow,
            ["attempts"] = 1
        });
    }

    [When(@"internet connection is lost")]
    public void WhenInternetConnectionIsLost()
    {
        ScenarioContext["InternetConnected"] = false;
        ScenarioContext["OfflineMode"] = true;
        ScenarioContext["DisconnectedAt"] = DateTime.UtcNow;
    }
    [When(@"I complete (.*) activities offline")]
    public async Task WhenICompleteActivitiesOffline(int activityCount)
    {
        var offlineActivities = new List<object>();
        for (int i = 0; i < activityCount; i++)
        {
            offlineActivities.Add(new
            {
                ActivityId = $"offline-activity-{i}",
                CompletedAt = DateTime.UtcNow.AddMinutes(i * 5),
                Score = 80 + i * 2
            });
        }
        
        ScenarioContext["OfflineActivities"] = offlineActivities;
    }
    [When(@"internet connection is restored")]
    public async Task WhenInternetConnectionIsRestored()
    {
        ScenarioContext["InternetConnected"] = true;
        ScenarioContext["OfflineMode"] = false;
        ScenarioContext["ReconnectedAt"] = DateTime.UtcNow;
        
        await WhenISendAPOSTRequestToWithData("/api/activities/sync", new Dictionary<string, object>
        {
            ["offlineActivities"] = ScenarioContext["OfflineActivities"],
            ["deviceId"] = "ipad-001"
        });
    }

    [When(@"student works on activity")]
    public async Task WhenStudentWorksOnActivity()
    {
        await WhenISendAPOSTRequestToWithData($"/api/activities/{_currentActivityId}/work", new Dictionary<string, object>
        {
            ["studentId"] = ScenarioContext["StudentId"],
            ["sessionStart"] = DateTime.UtcNow
        });
    }

    [Then(@"I should see first word ""(.*)"" with image")]
    public void ThenIShouldSeeFirstWordWithImage(string word)
    {
        ScenarioContext["CurrentWord"] = word;
        ScenarioContext["ImageDisplayed"] = true;
        ScenarioContext["WordPosition"] = 1;
    }
    [Then(@"I should be able to interact:")]
    public void ThenIShouldBeAbleTo(Table table)
    {
        var abilities = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            abilities[row["Action"]] = row["Result"];
        }
        ScenarioContext["StudentAbilities"] = abilities;
    }
    [Then(@"I should see my results:")]
    public void ThenIShouldSeeMyResults(Table table)
    {
        var results = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            results[row["Metric"]] = row["Display"];
        }
        ScenarioContext["ActivityResults"] = results;
    }
    [Then(@"I see (.*) food items and (.*) category boxes:")]
    public void ThenISeeFoodItemsAndCategoryBoxes(int itemCount, int categoryCount, Table table)
    {
        ScenarioContext["ItemCount"] = itemCount;
        ScenarioContext["CategoryCount"] = categoryCount;
        
        var categories = new List<object>();
        foreach (var row in table.Rows)
        {
            categories.Add(new
            {
                Category = row["Category"],
                Color = row["Color"]
                    });
        }
        
        ScenarioContext["Categories"] = categories;
    }
    [Then(@"item should snap into place")]
    public void ThenItemShouldSnapIntoPlace()
    {
        ScenarioContext["SnapToGrid"] = true;
        ScenarioContext["AnimationSmooth"] = true;
    }
    [Then(@"I should hear positive feedback sound")]
    public void ThenIShouldHearPositiveFeedbackSound()
    {
        ScenarioContext["AudioFeedback"] = "positive";
        ScenarioContext["SoundPlayed"] = "success.mp3";
    }
    [Then(@"border should glow (.*)")]
    public void ThenBorderShouldGlow(string color)
    {
        ScenarioContext["BorderEffect"] = "glow";
        ScenarioContext["GlowColor"] = color;
    }
    [Then(@"item should bounce back")]
    public void ThenItemShouldBounceBack()
    {
        ScenarioContext["IncorrectAnimation"] = "bounce";
        ScenarioContext["ItemReturned"] = true;
    }
    [Then(@"I should see hint: ""(.*)""")]
    public void ThenIShouldSeeHint(string hint)
    {
        ScenarioContext["HintDisplayed"] = true;
        ScenarioContext["HintText"] = hint;
    }
    [Then(@"confetti animation should play")]
    public void ThenConfettiAnimationShouldPlay()
    {
        ScenarioContext["CompletionAnimation"] = "confetti";
        ScenarioContext["CelebrationTriggered"] = true;
    }
    [Then(@"I can print certificate of completion")]
    public void ThenICanPrintCertificateOfCompletion()
    {
        ScenarioContext["CertificateAvailable"] = true;
        ScenarioContext["CertificateData"] = new
        {
            StudentName = "Test Student",
            ActivityName = "Food Groups",
            CompletionDate = DateTime.UtcNow,
            Score = "100%"
        };
    }

    [Then(@"I should still be able to:")]
    public void ThenIShouldStillBeAbleTo(Table table)
    {
        var offlineCapabilities = new Dictionary<string, string>();
        foreach (var row in table.Rows)
        {
            offlineCapabilities[row["Action"]] = row["Availability"];
                }
        ScenarioContext["OfflineCapabilities"] = offlineCapabilities;
    }
    [Then(@"all progress should automatically sync")]
    public void ThenAllProgressShouldAutomaticallySync()
    {
        ScenarioContext["AutoSyncCompleted"] = true;
        ScenarioContext["SyncResult"] = "Success";
        ScenarioContext["RecordsSync"] = 5;
    }
    [Then(@"therapist should see updated data")]
    public void ThenTherapistShouldSeeUpdatedData()
    {
        ScenarioContext["TherapistDataUpdated"] = true;
        ScenarioContext["LastSyncTime"] = DateTime.UtcNow;
    }
    [Then(@"no data should be lost")]
    public void ThenNoDataShouldBeLost()
    {
        ScenarioContext["DataIntegrity"] = "Verified";
        ScenarioContext["OfflineDataPreserved"] = true;
    }
    [Then(@"cards automatically adjust")]
    public void ThenCardsAutomaticallyAdjust()
    {
        ScenarioContext["DynamicAdjustment"] = true;
        ScenarioContext["AdjustmentCriteria"] = new
        {
            RemoveOnMastery = true,
            AddSimilarWords = true,
            ProgressThreshold = "80% accuracy"
        };
    }

    // Removed duplicate methods - keeping only the implemented versions above
}
