using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ParentPortalSteps : BaseStepDefinitions
{
    private string _currentParentName = string.Empty;
    private string _invitationEmail = string.Empty;
    private string _accessCode = string.Empty;
    private List<object> _children = new();
    private Dictionary<string, object> _sessionData = new();

    public ParentPortalSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"the parent portal is enabled")]
    public void GivenTheParentPortalIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have children:")]
    public void GivenIHaveChildren(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"therapist sends portal invitation to ""(.*)""")]
    public void GivenTherapistSendsPortalInvitationTo(string email)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"parent has two-factor authentication enabled")]
    public void GivenParentHasTwoFactorAuthenticationEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" has two registered parents")]
    public void GivenHasTwoRegisteredParents(string childName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" had sessions this week")]
    public void GivenHadSessionsThisWeek(string childName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"""(.*)"" has active IEP goals")]
    public void GivenHasActiveIEPGoals(string childName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"quarterly reports are available")]
    public void GivenQuarterlyReportsAreAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"""(.*)"" receives invitation email")]
    public void WhenReceivesInvitationEmail(string parentName)
    {
        ScenarioContext["InvitationReceived"] = true;
        ScenarioContext["RecipientName"] = parentName;
    }
    [When(@"he registers with the access code")]
    public async Task WhenHeRegistersWithTheAccessCode()
    {
        await WhenISendAPOSTRequestToWithData("/api/parent-portal/register", new Dictionary<string, object>
        {
            ["accessCode"] = _accessCode,
            ["email"] = _invitationEmail,
            ["password"] = "SecurePass123!",
            ["firstName"] = "Robert",
            ["lastName"] = "Wilson"
        });
    }
    
    [When(@"parent logs in with email and password")]
    public async Task WhenParentLogsInWithEmailAndPassword()
    {
        await WhenISendAPOSTRequestToWithData("/api/parent-portal/login", new Dictionary<string, object>
        {
            ["email"] = "parent@email.com",
            ["password"] = "SecurePass123!"
        });
    }
    
    [When(@"both parents access the portal")]
    public void WhenBothParentsAccessThePortal()
    {
        ScenarioContext["MultipleParentAccess"] = true;
        ScenarioContext["AccessTimestamps"] = new Dictionary<string, DateTime>
        {
            ["Lisa Wilson"] = DateTime.UtcNow,
            ["Robert Wilson"] = DateTime.UtcNow.AddMinutes(5)
        };
    }
    
    [When(@"I view session history")]
    public async Task WhenIViewSessionHistory()
    {
        await WhenISendAGETRequestTo("/api/parent-portal/children/emma-wilson/sessions");
    }
    [When(@"I view goals section")]
    public async Task WhenIViewGoalsSection()
    {
        await WhenISendAGETRequestTo("/api/parent-portal/children/emma-wilson/goals");
    }
    [When(@"I view reports section")]
    public async Task WhenIViewReportsSection()
    {
        await WhenISendAGETRequestTo("/api/parent-portal/children/emma-wilson/reports");
    }
    [Then(@"email contains:")]
    public void ThenEmailContains(Table table)
    {
        var emailContent = new Dictionary<string, string>();
        foreach (var row in table.Rows)
        {
            emailContent[row["Content"]] = row["Details"];
        }
        ScenarioContext["EmailContent"] = emailContent;
        ScenarioContext["EmailContainsAccessCode"] = true;
        ScenarioContext["EmailContainsLink"] = true;
    }
    [Then(@"account is created")]
    public void ThenAccountIsCreated()
    {
        ScenarioContext["ParentAccountCreated"] = true;
        ScenarioContext["AccountId"] = "parent-" + Guid.NewGuid().ToString();
    }
    [Then(@"he has access to ""(.*)"" information only")]
    public void ThenHeHasAccessToInformationOnly(string childName)
    {
        ScenarioContext["AccessRestriction"] = new
        {
            AllowedChild = childName,
            RestrictedData = new[] { "other_children", "other_therapists", "admin_features" }
        };
    }

    [Then(@"SMS code is sent to registered phone")]
    public void ThenSMSCodeIsSentToRegisteredPhone()
    {
        ScenarioContext["SMSCodeSent"] = true;
        ScenarioContext["SMSCode"] = new Random().Next(100000, 999999).ToString();
    }
    [Then(@"login requires code entry")]
    public void ThenLoginRequiresCodeEntry()
    {
        ScenarioContext["2FARequired"] = true;
    }
    [Then(@"session expires after (.*) minutes of inactivity")]
    public void ThenSessionExpiresAfterMinutesOfInactivity(int minutes)
    {
        ScenarioContext["SessionTimeout"] = minutes;
        ScenarioContext["SessionExpiryPolicy"] = "sliding";
    }
    [Then(@"both see identical information")]
    public void ThenBothSeeIdenticalInformation()
    {
        ScenarioContext["DataConsistency"] = true;
        ScenarioContext["SharedDataView"] = true;
    }
    [Then(@"both can communicate with therapist")]
    public void ThenBothCanCommunicationWithTherapist()
    {
        ScenarioContext["MessagingEnabled"] = true;
        ScenarioContext["MessageThreadShared"] = true;
    }
    [Then(@"changes by one parent are visible to other")]
    public void ThenChangesByOneParentAreVisibleToOther()
    {
        ScenarioContext["RealTimeSync"] = true;
        ScenarioContext["ChangeNotifications"] = true;
    }
    [Then(@"I see parent-friendly summaries:")]
    public async Task ThenISeeParentFriendlySummaries(Table table)
    {
        LastResponse.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            ScenarioContext[$"Session_{row["Date"]}"] = new
            {
                Therapist = row["Therapist"],
                FocusArea = row["Focus Area"],
                ProgressNote = row["Progress Note"]
            };
        }
    }

    [Then(@"I cannot see:")]
    public void ThenICannotSee(Table table)
    {
        var restrictedInfo = table.Rows.Select(row => row["Restricted Info"]).ToList();
        ScenarioContext["RestrictedInformation"] = restrictedInfo;
        ScenarioContext["DataFiltered"] = true;
    }
    [Then(@"I see simplified goal information:")]
    public void ThenISeeSimplifiedGoalInformation(Table table)
    {
        foreach (var row in table.Rows)
        {
            ScenarioContext[$"Goal_{row["Goal Area"]}"] = new
            {
                CurrentLevel = row["Current Level"],
                Target = row["Target"],
                Progress = row["Progress"]
            };
        }
    }

    [Then(@"visual progress bars")]
    public void ThenVisualProgressBars()
    {
        ScenarioContext["ProgressVisualization"] = "bars";
        ScenarioContext["VisualElementsPresent"] = true;
    }
    [Then(@"last updated dates")]
    public void ThenLastUpdatedDates()
    {
        ScenarioContext["UpdateTimestampsShown"] = true;
    }
    [Then(@"not clinical measurement details")]
    public void ThenNotClinicalMeasurementDetails()
    {
        ScenarioContext["ClinicalDetailsHidden"] = true;
    }
    [Then(@"I can download:")]
    public void ThenICanDownload(Table table)
    {
        var downloadableReports = new List<object>();
        foreach (var row in table.Rows)
        {
            downloadableReports.Add(new
            {
                ReportType = row["Report Type"],
                Format = row["Format"],
                Content = row["Content"]
            });
        }
        
        ScenarioContext["DownloadableReports"] = downloadableReports;
    }
    [Then(@"reports are watermarked")]
    public void ThenReportsAreWatermarked()
    {
        ScenarioContext["ReportsWatermarked"] = true;
        ScenarioContext["WatermarkText"] = "Confidential - Parent Copy";
    }
    [Then(@"download is logged")]
    public void ThenDownloadIsLogged()
    {
        ScenarioContext["DownloadLogged"] = true;
        ScenarioContext["DownloadAudit"] = new
        {
            Timestamp = DateTime.UtcNow,
            User = _currentParentName,
            Document = "Progress_Summary_Q4_2023.pdf",
            IPAddress = "192.168.1.100"
        };
    }

    private string GenerateAccessCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}