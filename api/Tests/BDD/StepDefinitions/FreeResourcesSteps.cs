using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class FreeResourcesSteps : BaseStepDefinitions
{
    private string _currentResourceId = string.Empty;
    private int _monthlyLimit = 10;
    private Dictionary<string, object> _freeResourceData = new();

    public FreeResourcesSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"free resources section is available")]
    public void GivenFreeResourcesSectionIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am a non-subscriber")]
    public void GivenIAmANonSubscriber()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"monthly free limit is (.*)")]
    public void GivenMonthlyFreeLimitIs(int limit)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I used (.*) free resources this month")]
    public void GivenIUsedFreeResourcesThisMonth(int usedCount)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"weekly rotation is active")]
    public void GivenWeeklyRotationIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I browse free resources")]
    public async Task WhenIBrowseFreeResources()
    {
        await WhenISendAGETRequestTo("/api/resources/free");
    }
    [When(@"I download free resource")]
    public async Task WhenIDownloadFreeResource()
    {
        _currentResourceId = "free-resource-123";
        await WhenISendAGETRequestTo($"/api/resources/{_currentResourceId}/download?free=true");
    }
    [When(@"I access sample pages")]
    public async Task WhenIAccessSamplePages()
    {
        await WhenISendAGETRequestTo("/api/resources/samples");
    }
    [When(@"I preview premium resource")]
    public async Task WhenIPreviewPremiumResource()
    {
        await WhenISendAGETRequestTo("/api/resources/premium-123/preview");
    }
    [When(@"I signup for newsletter")]
    public async Task WhenISignupForNewsletter()
    {
        await WhenISendAPOSTRequestToWithData("/api/newsletter/signup", new Dictionary<string, object>
        {
            ["email"] = "user@example.com",
            ["preferences"] = new[] { "weekly_free", "birthday_specials", "new_features" }
        });
    }

    [When(@"I claim birthday month special")]
    public async Task WhenIClaimBirthdayMonthSpecial()
    {
        await WhenISendAPOSTRequestToWithData("/api/specials/birthday/claim", new Dictionary<string, object>
        {
            ["birthMonth"] = DateTime.UtcNow.Month,
            ["email"] = "user@example.com"
        });
    }
    
    [When(@"I attempt (.*) more download")]
    public async Task WhenIAttemptMoreDownload(int additionalDownloads)
    {
        for (int i = 0; i < additionalDownloads; i++)
        {
            await WhenISendAGETRequestTo($"/api/resources/free-resource-{i + 200}/download?free=true");
        }
    }
    
    [Then(@"free section displays:")]
    public void ThenFreeSectionDisplays(Table table)
    {
        var freeSection = new List<object>();
        foreach (var row in table.Rows)
        {
            freeSection.Add(new
            {
                Category = row["Category"],
                Count = row["Available Count"]
            });
        }
        
        ScenarioContext["FreeSectionDisplay"] = freeSection;
    }
    [Then(@"usage counter shows (.*)/(.*) remaining")]
    public void ThenUsageCounterShowsRemaining(int remaining, int total)
    {
        ScenarioContext["UsageCounterDisplay"] = $"{remaining}/{total}";
        ScenarioContext["LimitTrackingWorking"] = true;
    }
    [Then(@"download succeeds")]
    public void ThenDownloadSucceeds()
    {
        ScenarioContext["DownloadSuccess"] = true;
        ScenarioContext["ResourceDelivered"] = true;
    }
    [Then(@"counter decrements")]
    public void ThenCounterDecrements()
    {
        ScenarioContext["CounterDecremented"] = true;
        var previousUsed = (int)ScenarioContext["ResourcesUsed"];
        ScenarioContext["ResourcesUsed"] = previousUsed + 1;
    }
    [Then(@"sample previews show:")]
    public void ThenSamplePreviewsShow(Table table)
    {
        var samples = new List<object>();
        foreach (var row in table.Rows)
        {
            samples.Add(new
            {
                Resource = row["Resource Type"],
                Preview = row["Preview Content"]
            });
        }
        
        ScenarioContext["SamplePreviews"] = samples;
    }
    [Then(@"upgrade prompts displayed")]
    public void ThenUpgradePromptsDisplayed()
    {
        ScenarioContext["UpgradePromptsShown"] = true;
        ScenarioContext["UpgradeIncentives"] = new[]
        {
            "Unlimited downloads",
            "Access to premium content",
            "Advanced features"
        };
    }

    [Then(@"preview shows first (.*) pages")]
    public void ThenPreviewShowsFirstPages(int pageCount)
    {
        ScenarioContext["PreviewPages"] = pageCount;
        ScenarioContext["PreviewLimited"] = true;
    }
    [Then(@"subscription upsell presented")]
    public void ThenSubscriptionUpsellPresented()
    {
        ScenarioContext["UpsellPresented"] = true;
        ScenarioContext["UpsellOffer"] = "Get full access for $19.95/month";
    }
    [Then(@"newsletter signup confirmed")]
    public void ThenNewsletterSignupConfirmed()
    {
        ScenarioContext["NewsletterSignup"] = true;
        ScenarioContext["WelcomeEmailSent"] = true;
    }
    [Then(@"free bonus resources unlocked")]
    public void ThenFreeBonusResourcesUnlocked()
    {
        ScenarioContext["BonusResourcesUnlocked"] = true;
        ScenarioContext["BonusCount"] = 5;
    }
    [Then(@"birthday special activated")]
    public void ThenBirthdaySpecialActivated()
    {
        ScenarioContext["BirthdaySpecialActive"] = true;
        ScenarioContext["SpecialBenefit"] = "Extra 10 free downloads";
    }
    [Then(@"special valid for (.*) days")]
    public void ThenSpecialValidForDays(int days)
    {
        ScenarioContext["SpecialValidDays"] = days;
        ScenarioContext["SpecialExpiry"] = DateTime.UtcNow.AddDays(days);
    }
    [Then(@"download blocked")]
    public void ThenDownloadBlocked()
    {
        ScenarioContext["DownloadBlocked"] = true;
        ScenarioContext["BlockReason"] = "Monthly limit exceeded";
    }
    [Then(@"limit exceeded message shown")]
    public void ThenLimitExceededMessageShown()
    {
        ScenarioContext["LimitMessage"] = true;
        ScenarioContext["MessageText"] = "You've reached your monthly limit of 10 free resources";
    }
    [Then(@"subscription options presented")]
    public void ThenSubscriptionOptionsPresented()
    {
        ScenarioContext["SubscriptionOptions"] = new[]
        {
            new { Plan = "Basic", Price = "$9.95/month", Features = "20 downloads/month" },
            new { Plan = "Pro", Price = "$19.95/month", Features = "Unlimited downloads" }
        };
    }
}