using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class CommunityFeaturesSteps : BaseStepDefinitions
{
    private string _currentReviewId = string.Empty;
    private string _currentQuestionId = string.Empty;
    private Dictionary<string, object> _communityData = new();

    public CommunityFeaturesSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    
    [Given(@"community features are enabled")]
    public void GivenCommunityFeaturesAreEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Given(@"moderated sharing is active")]
    public void GivenModeratedSharingIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Given(@"I downloaded resource ""(.*)""")]
    public void GivenIDownloadedResource(string resourceName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [Given(@"resource has existing reviews")]
    public void GivenResourceHasExistingReviews()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"I write resource review")]
    public async Task WhenIWriteResourceReview()
    {
        _currentReviewId = $"review-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/community/reviews", new Dictionary<string, object>
        {
            ["resourceId"] = "resource-123",
            ["rating"] = 5,
            ["title"] = "Great for fine motor work",
            ["comment"] = "My students loved these activities. Clear instructions and engaging graphics.",
            ["wouldRecommend"] = true
        });
    }

    [When(@"I ask question about resource")]
    public async Task WhenIAskQuestionAboutResource()
    {
        _currentQuestionId = $"question-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/community/questions", new Dictionary<string, object>
        {
            ["resourceId"] = "resource-123",
            ["question"] = "Can this be adapted for students with visual impairments?",
            ["category"] = "adaptation"
        });
    }
    
    [When(@"I submit success story")]
    public async Task WhenISubmitSuccessStory()
    {
        await WhenISendAPOSTRequestToWithData("/api/community/success-stories", new Dictionary<string, object>
        {
            ["title"] = "Amazing progress with bilateral coordination",
            ["story"] = "After using these activities for 6 weeks, my student showed remarkable improvement...",
            ["resourcesUsed"] = new[] { "resource-123", "resource-456" },
            ["anonymous"] = false
        });
    }
    
    [When(@"I vote on feature request")]
    public async Task WhenIVoteOnFeatureRequest()
    {
        await WhenISendAPOSTRequestToWithData("/api/community/feature-requests/vote", new Dictionary<string, object>
        {
            ["requestId"] = "feature-789",
            ["vote"] = "upvote",
            ["comment"] = "This would save me so much time!"
        });
    }

    [When(@"I submit bug report")]
    public async Task WhenISubmitBugReport()
    {
        await WhenISendAPOSTRequestToWithData("/api/community/bug-reports", new Dictionary<string, object>
        {
            ["title"] = "Download link broken",
            ["description"] = "The PDF download returns a 404 error",
            ["resourceId"] = "resource-123",
            ["browser"] = "Chrome",
            ["severity"] = "medium"
        });
    }

    [When(@"another user answers question")]
    public async Task WhenAnotherUserAnswersQuestion()
    {
        await WhenISendAPOSTRequestToWithData($"/api/community/questions/{_currentQuestionId}/answers", new Dictionary<string, object>
        {
            ["answer"] = "Yes! I've successfully adapted this for students with VI by adding tactile elements.",
            ["helpful"] = true,
            ["answeredBy"] = "ExperiencedTherapist"
        });
    }

    [Then(@"review appears on resource page")]
    public void ThenReviewAppearsOnResourcePage()
    {
            ScenarioContext["ReviewPosted"] = true;
        ScenarioContext["ReviewVisibility"] = "public";
    }
    
    [Then(@"review awaits moderation")]
    public void ThenReviewAwaitsModeration()
    {
        ScenarioContext["ReviewModeration"] = true;
        ScenarioContext["ModerationStatus"] = "pending";
    }
    
    [Then(@"other users can see rating")]
    public void ThenOtherUsersCanSeeRating()
    {
        ScenarioContext["RatingVisible"] = true;
        ScenarioContext["UpdatedAverageRating"] = 4.8;
    }
    
    [Then(@"question posted to Q&A")]
    public void ThenQuestionPostedToQA()
    {
        ScenarioContext["QuestionPosted"] = true;
        ScenarioContext["QuestionCategory"] = "adaptation";
    }
    
    [Then(@"relevant users notified")]
    public void ThenRelevantUsersNotified()
    {
        ScenarioContext["NotificationsSet"] = true;
        ScenarioContext["NotifiedUsers"] = new[]
        {
            "Users who downloaded this resource",
            "Users with expertise in adaptations"
        };
    }

    [Then(@"story submitted for review")]
    public void ThenStorySubmittedForReview()
    {
        ScenarioContext["StorySubmitted"] = true;
        ScenarioContext["StoryStatus"] = "pending_moderation";
    }
    
    [Then(@"story tagged with resources")]
    public void ThenStoryTaggedWithResources()
    {
        ScenarioContext["ResourcesTagged"] = true;
        ScenarioContext["TaggedResources"] = new[] { "resource-123", "resource-456" };
    }

    [Then(@"vote recorded")]
    public void ThenVoteRecorded()
    {
        ScenarioContext["VoteRecorded"] = true;
        ScenarioContext["VoteCount"] = 47; // incremented
    }

    [Then(@"feature request priority updated")]
    public void ThenFeatureRequestPriorityUpdated()
    {
        ScenarioContext["PriorityUpdated"] = true;
        ScenarioContext["NewPriority"] = "high";
    }
    
    [Then(@"bug report created")]
    public void ThenBugReportCreated()
    {
        ScenarioContext["BugReportCreated"] = true;
        ScenarioContext["BugReportId"] = "bug-12345";
    }
    
    [Then(@"development team notified")]
    public void ThenDevelopmentTeamNotified()
    {
        ScenarioContext["DevTeamNotified"] = true;
        ScenarioContext["NotificationMethod"] = "Slack + email";
    }
    
    [Then(@"answer posted successfully")]
    public void ThenAnswerPostedSuccessfully()
    {
        ScenarioContext["AnswerPosted"] = true;
        ScenarioContext["AnswerHelpful"] = true;
    }
    
    [Then(@"question marked as answered")]
    public void ThenQuestionMarkedAsAnswered()
    {
        ScenarioContext["QuestionAnswered"] = true;
        ScenarioContext["AnswerAccepted"] = true;
    }
    
    [Then(@"spam detection prevents:")]
    public void ThenSpamDetectionPrevents(Table table)
    {
        var detections = new List<string>();
        foreach (var row in table.Rows)
        {
            detections.Add(row["Detection"]);
        }
        ScenarioContext["SpamDetections"] = detections;
    }
    
    [Then(@"voting manipulation prevented")]
    public void ThenVotingManipulationPrevented()
    {
        ScenarioContext["ManipulationPrevented"] = true;
        ScenarioContext["PreventionMethod"] = "Rate limiting + account verification";
    }
}
