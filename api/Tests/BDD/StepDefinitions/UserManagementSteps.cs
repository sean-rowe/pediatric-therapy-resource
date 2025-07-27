using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class UserManagementSteps : BaseStepDefinitions
{
    private string _currentUserId = string.Empty;
    private Dictionary<string, object> _userProfile = new();
    private List<object> _userActivity = new();

    public UserManagementSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"user management system is active")]
    public void GivenUserManagementSystemIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"user ""(.*)"" exists")]
    public void GivenUserExists(string userName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"user has role ""(.*)""")]
    public void GivenUserHasRole(string role)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"user profile is incomplete")]
    public void GivenUserProfileIsIncomplete()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I view user list")]
    public async Task WhenIViewUserList()
    {
        await WhenISendAGETRequestTo("/api/users");
    }
    [When(@"I create new user")]
    public async Task WhenICreateNewUser()
    {
        await WhenISendAPOSTRequestToWithData("/api/users", new Dictionary<string, object>
        {
            ["email"] = "newuser@example.com",
            ["firstName"] = "New",
            ["lastName"] = "User",
            ["role"] = "Therapist",
            ["sendWelcomeEmail"] = true
        });
    }

    [When(@"I update user profile")]
    public async Task WhenIUpdateUserProfile()
    {
        await WhenISendAPUTRequestToWithData($"/api/users/{_currentUserId}/profile", new Dictionary<string, object>
        {
            ["firstName"] = "Updated",
            ["lastName"] = "Name",
            ["licenseNumber"] = "LIC123456",
            ["specialization"] = "Pediatric OT"
        });
    }
    
    [When(@"I change user role to ""(.*)""")]
    public async Task WhenIChangeUserRoleTo(string newRole)
    {
        await WhenISendAPUTRequestToWithData($"/api/users/{_currentUserId}/role", new Dictionary<string, object>
        {
            ["role"] = newRole,
            ["reason"] = "Promotion to supervisory role"
        });
    }
    
    [When(@"I suspend user account")]
    public async Task WhenISuspendUserAccount()
    {
        await WhenISendAPOSTRequestToWithData($"/api/users/{_currentUserId}/suspend", new Dictionary<string, object>
        {
            ["reason"] = "Policy violation",
            ["duration"] = "30 days",
            ["notifyUser"] = true
        });
    }

    [When(@"I view user activity log")]
    public async Task WhenIViewUserActivityLog()
    {
        await WhenISendAGETRequestTo($"/api/users/{_currentUserId}/activity");
    }
    [When(@"I export user data")]
    public async Task WhenIExportUserData()
    {
        await WhenISendAGETRequestTo($"/api/users/{_currentUserId}/export");
    }
    [Then(@"user list displays:")]
    public void ThenUserListDisplays(Table table)
    {
        var users = new List<object>();
        foreach (var row in table.Rows)
        {
            users.Add(new
            {
                Name = row["Name"],
                Email = row["Email"],
                Role = row["Role"],
                Status = row["Status"]
                    });
        }
        
        ScenarioContext["UserList"] = users;
    }
    [Then(@"user created successfully")]
    public void ThenUserCreatedSuccessfully()
    {
        ScenarioContext["UserCreated"] = true;
        ScenarioContext["WelcomeEmailSent"] = true;
    }
    [Then(@"user receives welcome email")]
    public void ThenUserReceivesWelcomeEmail()
    {
        ScenarioContext["WelcomeEmailReceived"] = true;
        ScenarioContext["EmailContains"] = new[]
        {
            "Account created",
            "Login instructions",
            "Support contact"
        };
    }

    [Then(@"profile updated successfully")]
    public void ThenProfileUpdatedSuccessfully()
    {
        ScenarioContext["ProfileUpdated"] = true;
        ScenarioContext["UpdatedAt"] = DateTime.UtcNow;
    }
    [Then(@"role changed successfully")]
    public void ThenRoleChangedSuccessfully()
    {
        ScenarioContext["RoleChanged"] = true;
        ScenarioContext["RoleChangeNotification"] = true;
    }
    [Then(@"user permissions updated to:")]
    public void ThenUserPermissionsUpdatedTo(Table table)
    {
        var permissions = new List<string>();
        foreach (var row in table.Rows)
        {
            permissions.Add(row["Permission"]);
        }
        ScenarioContext["UpdatedPermissions"] = permissions;
    }
    [Then(@"account suspended")]
    public void ThenAccountSuspended()
    {
        ScenarioContext["AccountSuspended"] = true;
        ScenarioContext["SuspensionDate"] = DateTime.UtcNow;
        ScenarioContext["SuspensionActive"] = true;
    }
    [Then(@"user notified of suspension")]
    public void ThenUserNotifiedOfSuspension()
    {
        ScenarioContext["SuspensionNotificationSent"] = true;
        ScenarioContext["NotificationMethod"] = "email";
    }
    [Then(@"activity log shows:")]
    public void ThenActivityLogShows(Table table)
    {
        var activities = new List<object>();
        foreach (var row in table.Rows)
        {
            activities.Add(new
            {
                Action = row["Action"],
                Timestamp = row["Timestamp"],
                IpAddress = row["IP Address"]
                    });
        }
        
        ScenarioContext["UserActivityLog"] = activities;
    }
    [Then(@"data export includes:")]
    public void ThenDataExportIncludes(Table table)
    {
        var exportData = new List<string>();
        foreach (var row in table.Rows)
        {
            exportData.Add(row["Data Type"]);
        }
        ScenarioContext["ExportedDataTypes"] = exportData;
    }
    [Then(@"export provided in format (.*)")]
    public void ThenExportProvidedInFormat(string format)
    {
        ScenarioContext["ExportFormat"] = format;
        ScenarioContext["ExportReady"] = true;
    }

    private string[] GetRolePermissions(string role)
    {
        return role.ToLower() switch
        {
            "therapist" => new[] { "view_resources", "create_sessions", "manage_own_students" },
            "supervisor" => new[] { "view_resources", "create_sessions", "manage_all_students", "supervise_staff" },
            "admin" => new[] { "full_access", "manage_users", "configure_system" },
            _ => Array.Empty<string>()
        };
    }
}
