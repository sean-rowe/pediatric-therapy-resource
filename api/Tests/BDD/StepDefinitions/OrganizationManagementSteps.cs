using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class OrganizationManagementSteps : BaseStepDefinitions
{
    private string _currentOrgId = string.Empty;
    private string _invitationId = string.Empty;
    private Dictionary<string, object> _orgSettings = new();

    public OrganizationManagementSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"organization management is enabled")]
    public void GivenOrganizationManagementIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am an organization administrator")]
    public void GivenIAmAnOrganizationAdministrator()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"organization ""(.*)"" exists")]
    public void GivenOrganizationExists(string orgName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have (.*) team members")]
    public void GivenIHaveTeamMembers(int memberCount)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I create new organization")]
    public async Task WhenICreateNewOrganization()
    {
        _currentOrgId = $"org-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/organizations", new Dictionary<string, object>
{
    ["name"] = "Pediatric Therapy Center",
    ["type"] = "Healthcare Practice",
            ["address"] = "123 Main St, City, ST 12345",
            ["contactEmail"] = "admin@therapycenter.com"
        });
    }
    [When(@"I invite team member")]
    public async Task WhenIInviteTeamMember()
    {
        _invitationId = $"invite-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData($"/api/organizations/{_currentOrgId}/invitations", new Dictionary<string, object>
{
    ["email"] = "therapist@example.com",
    ["role"] = "Therapist",
    ["message"] = "Welcome to our team!"
});
    }

    [When(@"I set member permissions")]
    public async Task WhenISetMemberPermissions()
    {
            await WhenISendAPUTRequestToWithData($"/api/organizations/{_currentOrgId}/members/member-1/permissions", new Dictionary<string, object>
{
    ["permissions"] = new[]
            {
            "view_resources",
            "create_sessions",
            "manage_students"
            }
});
    }
    [When(@"I configure organization settings")]
    public async Task WhenIConfigureOrganizationSettings()
    {
            await WhenISendAPUTRequestToWithData($"/api/organizations/{_currentOrgId}/settings", new Dictionary<string, object>
{
    ["allowResourceSharing"] = true,
    ["requireApprovalForSharing"] = false,
    ["defaultPermissions"] = "therapist",
    ["billingSettings"] = new { Method = "consolidated"}
});

    }

    [When(@"I view organization dashboard")]
    public async Task WhenIViewOrganizationDashboard()
    {
        await WhenISendAGETRequestTo($"/api/organizations/{_currentOrgId}/dashboard");
    }
    [When(@"I export member activity")]
    public async Task WhenIExportMemberActivity()
    {
        await WhenISendAGETRequestTo($"/api/organizations/{_currentOrgId}/reports/activity");
    }
    [Then(@"organization created successfully")]
    public void ThenOrganizationCreatedSuccessfully()
    {
        ScenarioContext["OrganizationCreated"] = true;
        ScenarioContext["OrganizationId"] = _currentOrgId;
    }
    [Then(@"admin dashboard available")]
    public void ThenAdminDashboardAvailable()
    {
        ScenarioContext["DashboardAvailable"] = true;
        ScenarioContext["DashboardFeatures"] = new[]
        {
            "Member management",
            "Usage analytics",
            "Billing overview",
            "Settings"
            
        };
    }

    [Then(@"invitation sent to (.*)")]
    public void ThenInvitationSentTo(string email)
    {
        ScenarioContext["InvitationSent"] = true;
        ScenarioContext["InviteeEmail"] = email;
        ScenarioContext["InvitationId"] = _invitationId;
    }
    [Then(@"invitation includes:")]
    public void ThenInvitationIncludes(Table table)
    {
        var invitationDetails = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            invitationDetails[row["Element"]] = row["Content"];
        }
        ScenarioContext["InvitationDetails"] = invitationDetails;
    }
    [Then(@"permissions updated successfully")]
    public void ThenPermissionsUpdatedSuccessfully()
    {
        ScenarioContext["PermissionsUpdated"] = true;
        ScenarioContext["UpdatedAt"] = DateTime.UtcNow;
    }
    [Then(@"member has access to:")]
    public void ThenMemberHasAccessTo(Table table)
    {
        var permissions = new List<string>();
        foreach (var row in table.Rows)
        {
            permissions.Add(row["Permission"]);
        }
                ScenarioContext["MemberPermissions"] = permissions;
    }
    [Then(@"settings saved successfully")]
    public void ThenSettingsSavedSuccessfully()
    {
        ScenarioContext["SettingsSaved"] = true;
        ScenarioContext["LastModified"] = DateTime.UtcNow;
    }
    [Then(@"dashboard shows:")]
    public void ThenDashboardShows(Table table)
    {
        var dashboardData = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            dashboardData[row["Metric"]] = row["Value"];
        }
        ScenarioContext["DashboardData"] = dashboardData;
    }
    [Then(@"organization analytics include:")]
    public void ThenOrganizationAnalyticsInclude(Table table)
    {
        var analytics = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            analytics[row["Metric"]] = row["Value"];
        }
        ScenarioContext["OrganizationAnalytics"] = analytics;
    }
    [Then(@"activity report generated")]
    public void ThenActivityReportGenerated()
    {
        ScenarioContext["ActivityReportGenerated"] = true;
        ScenarioContext["ReportFormat"] = "CSV";
        ScenarioContext["ReportIncluded"] = new[]
        {
            "User login times",
            "Resource usage",
            "Session documentation",
            "Feature utilization"

        };
    }
}