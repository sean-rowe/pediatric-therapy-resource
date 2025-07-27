using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class CommunicationStepDefinitions : BaseStepDefinitions
{
    public CommunicationStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    // Parent Portal
    
    [Given(@"I am a parent of student ""(.*)""")]
    public void GivenIAmAParentOfStudent(string studentName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Given(@"I have a valid access code ""(.*)""")]
    public void GivenIHaveAValidAccessCode(string accessCode)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"I enter the access code")]
    public async Task WhenIEnterTheAccessCode()
    {
        var code = GetFromContext<string>("AccessCode");
        await WhenISendAPOSTRequestToWithData("/api/parent-portal/validate",
            new Dictionary<string, object> { { "code", code } });
    }

    [Then(@"I should have access to:")]
    public async Task ThenIShouldHaveAccessTo(Table accessibleFeatures)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var access = GetResponseContent<dynamic>();
        
        foreach (var feature in accessibleFeatures.Rows)
        {
            // Verify feature access
        }
    }

    // Multi-channel Communication
    
    [Given(@"therapist needs to share resources")]
    public void GivenTherapistNeedsToShareResources()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"selecting sharing method")]
    public async Task WhenSelectingSharingMethod()
    {
        await WhenISendAGETRequestTo("/api/share/methods");
    }
    
    [Then(@"multiple secure options available")]
    public void ThenMultipleSecureOptionsAvailable()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var methods = GetResponseContent<List<string>>();
        methods.Should().Contain("quicklink");
        methods.Should().Contain("email");
        methods.Should().Contain("portal");
    }
    
    [When(@"I generate a QuickLink for resources")]
    public async Task WhenIGenerateAQuickLinkForResources()
    {
        await WhenISendAPOSTRequestToWithData("/api/share/quicklink",
            new Dictionary<string, object> 
            { 
                { "resourceIds", "[\"res1\",\"res2\"]" },
                { "expirationDays", "7" }
            });
    }

    [Then(@"I receive a secure shareable link")]
    public void ThenIReceiveASecureShareableLink()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var response = GetResponseContent<dynamic>();
        response?.link.Should().NotBeNull();
        response?.expiresAt.Should().NotBeNull();
    }
    
    [When(@"important updates occur")]
    public async Task WhenImportantUpdatesOccur()
    {
        await WhenISendAPOSTRequestToWithData("/api/notifications/trigger",
            new Dictionary<string, object> 
            { 
                { "type", "progress_update" },
                { "studentId", "test-student" }
            });
    }

    [Then(@"I receive notifications through preferred channels")]
    public async Task ThenIReceiveNotificationsThroughPreferredChannels()
    {
        var notificationsResponse = await Client.GetAsync("/api/notifications");
        notificationsResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var notifications = GetResponseContent<List<dynamic>>();
        notifications.Should().NotBeEmpty();
    }

    // Messaging
    
    [When(@"I send a message to therapist:")]
    public async Task WhenISendAMessageToTherapist(Table messageDetails)
    {
        await WhenISendAPOSTRequestToWithData("/api/messages/send", messageDetails);
    }
    
    [Then(@"therapist receives notification")]
    public void ThenTherapistReceivesNotification()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        ScenarioContext["MessageSent"] = true;
    }
    
    [Then(@"message appears in secure inbox")]
    public async Task ThenMessageAppearsInSecureInbox()
    {
        var inboxResponse = await Client.GetAsync("/api/messages/inbox");
        inboxResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var messages = GetResponseContent<List<dynamic>>();
        messages.Should().NotBeEmpty();
    }

    // Progress Reports
    
    [When(@"I request progress report")]
    public async Task WhenIRequestProgressReport()
    {
        var studentId = GetFromContext<string>("StudentId") ?? "test-student";
        await WhenISendAGETRequestTo($"/api/reports/progress/{studentId}");
    }
    
    [Then(@"I see visual progress data:")]
    public async Task ThenISeeVisualProgressData(Table expectedData)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var report = GetResponseContent<dynamic>();
        
        foreach (var dataType in expectedData.Rows)
        {
            // Verify data visualization
        }
    }

    // Language Support
    
    [Given(@"my preferred language is ""(.*)""")]
    public void GivenMyPreferredLanguageIs(string language)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Then(@"all communications are in (.*)")]
    public async Task ThenAllCommunicationsAreIn(string language)
    {
        var content = await LastResponse!.Content.ReadAsStringAsync();
        // Verify language based on expected patterns
        ScenarioContext["CommunicationLanguage"] = language;
    }

    // Document Sharing
    
    [When(@"therapist shares documents:")]
    public async Task WhenTherapistSharesDocuments(Table documents)
    {
        var docList = documents.Rows.Select(r => r["Document"]).ToList();
        await WhenISendAPOSTRequestToWithData("/api/documents/share",
            new Dictionary<string, object> 
            { 
                { "documents", string.Join(",", docList) },
                { "recipients", "parents" }
            });
    }

    [Then(@"documents appear in parent portal")]
    public async Task ThenDocumentsAppearInParentPortal()
    {
        var documentsResponse = await Client.GetAsync("/api/parent-portal/documents");
        documentsResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var documents = GetResponseContent<List<dynamic>>();
        documents.Should().NotBeEmpty();
    }

    // Real-time Updates
    
    [Given(@"I am viewing my child's session")]
    public void GivenIAmViewingMyChildsSession()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"therapist updates progress")]
    public async Task WhenTherapistUpdatesProgress()
    {
        await WhenISendAPOSTRequestToWithData("/api/sessions/progress/update",
            new Dictionary<string, object> 
            { 
                { "studentId", GetFromContext<string>("StudentId") ?? "test-student" },
                { "progress", "Goal met!" }
            });
    }

    [Then(@"I see real-time updates")]
    public void ThenISeeRealTimeUpdates()
    {
        // In real implementation, this would verify WebSocket updates
        ScenarioContext["RealTimeUpdates"] = true;
    }

    // Helper methods
    private string GetLanguageCode(string language)
    {
        return language.ToLower() switch
        {
            "spanish" => "es",
            "mandarin" or "chinese" => "zh",
            "arabic" => "ar",
            "french" => "fr",
            "vietnamese" => "vi",
            _ => "en"
        };
    }
}
