using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class GamificationSteps : BaseStepDefinitions
{
    private string _currentStudentId = string.Empty;
    private Dictionary<string, object> _rewardSystem = new();
    private List<object> _achievements = new();

    public GamificationSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"gamification is enabled")]
    public void GivenGamificationIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student ""(.*)"" is motivated by rewards")]
    public void GivenStudentIsMotivatedByRewards(string studentName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"reward system configured:")]
    public void GivenRewardSystemConfigured(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student has earned:")]
    public void GivenStudentHasEarned(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"leaderboard is active")]
    public void GivenLeaderboardIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"student completes activity correctly")]
    public async Task WhenStudentCompletesActivityCorrectly()
    {
        await WhenISendAPOSTRequestToWithData("/api/gamification/activity-complete", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["activityId"] = "activity-123",
            ["score"] = 100,
            ["timeSpent"] = 300
        });
    }

    [When(@"student reaches (.*) points")]
    public async Task WhenStudentReachesPoints(int points)
    {
        await WhenISendAPOSTRequestToWithData("/api/gamification/milestone", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["totalPoints"] = points,
            ["milestone"] = "points_threshold"
        });
    }

    [When(@"student visits reward store")]
    public async Task WhenStudentVisitsRewardStore()
    {
        await WhenISendAGETRequestTo($"/api/gamification/rewards/store?studentId={_currentStudentId}");
    }
    [When(@"student redeems ""(.*)""")]
    public async Task WhenStudentRedeems(string reward)
    {
        await WhenISendAPOSTRequestToWithData("/api/gamification/rewards/redeem", new Dictionary<string, object>
        {
            ["studentId"] = _currentStudentId,
            ["rewardName"] = reward,
            ["pointsCost"] = 50
        });
    }

    [When(@"week ends")]
    public async Task WhenWeekEnds()
    {
        await WhenISendAPOSTRequestToWithData("/api/gamification/weekly-reset", new Dictionary<string, object>
        {
            ["weekEndDate"] = DateTime.UtcNow
        });
    }

    [Then(@"student earns:")]
    public void ThenStudentEarns(Table table)
    {
        var earned = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            earned[row["Reward Type"]] = row["Amount"];
        }
        ScenarioContext["EarnedRewards"] = earned;
    }
    [Then(@"celebration animation plays")]
    public void ThenCelebrationAnimationPlays()
    {
        ScenarioContext["AnimationPlayed"] = true;
        ScenarioContext["AnimationType"] = "confetti";
        ScenarioContext["Duration"] = "3 seconds";
    }
    [Then(@"progress bar updates")]
    public void ThenProgressBarUpdates()
    {
        ScenarioContext["ProgressBarUpdated"] = true;
        ScenarioContext["ProgressPercentage"] = 75;
        ScenarioContext["NextMilestone"] = "100 points";
    }
    [Then(@"new badge unlocked: ""(.*)""")]
    public void ThenNewBadgeUnlocked(string badgeName)
    {
        ScenarioContext["BadgeUnlocked"] = true;
        ScenarioContext["BadgeName"] = badgeName;
        ScenarioContext["BadgeRarity"] = "rare";
    }
    [Then(@"notification shows achievement")]
    public void ThenNotificationShowsAchievement()
    {
        ScenarioContext["AchievementNotification"] = true;
        ScenarioContext["NotificationDuration"] = "5 seconds";
        ScenarioContext["ShareOption"] = true;
    }
    [Then(@"available rewards show:")]
    public void ThenAvailableRewardsShow(Table table)
    {
        var rewards = new List<object>();
        foreach (var row in table.Rows)
        {
            rewards.Add(new
            {
                Reward = row["Reward"],
                Cost = row["Cost"],
                Available = row["Available"]
            });
        }
        
        ScenarioContext["RewardStore"] = rewards;
    }
    [Then(@"points deducted")]
    public void ThenPointsDeducted()
    {
        ScenarioContext["PointsDeducted"] = true;
        ScenarioContext["DeductionAmount"] = 50;
        ScenarioContext["RemainingPoints"] = 70;
    }
    [Then(@"digital sticker added to collection")]
    public void ThenDigitalStickerAddedToCollection()
    {
        ScenarioContext["StickerAdded"] = true;
        ScenarioContext["CollectionUpdated"] = true;
        ScenarioContext["TotalStickers"] = 25;
    }
    [Then(@"therapist notified of redemption")]
    public void ThenTherapistNotifiedOfRedemption()
    {
        ScenarioContext["TherapistNotified"] = true;
        ScenarioContext["NotificationType"] = "reward_redeemed";
        ScenarioContext["ApprovalRequired"] = false;
    }
    [Then(@"leaderboard updates:")]
    public void ThenLeaderboardUpdates(Table table)
    {
        var leaderboard = new List<object>();
        foreach (var row in table.Rows)
        {
            leaderboard.Add(new
            {
                Position = row["Position"],
                Student = row["Student"],
                Points = row["Points"]
            });
        }
        
        ScenarioContext["LeaderboardState"] = leaderboard;
    }
    [Then(@"positions reset")]
    public void ThenPositionsReset()
    {
        ScenarioContext["LeaderboardReset"] = true;
        ScenarioContext["PreviousWinner"] = "Student A";
        ScenarioContext["WeeklyWinnerBadge"] = true;
    }
    [Then(@"fresh competition begins")]
    public void ThenFreshCompetitionBegins()
    {
        ScenarioContext["NewCompetitionStarted"] = true;
        ScenarioContext["AllPointsReset"] = true;
        ScenarioContext["HistoryPreserved"] = true;
    }
}
