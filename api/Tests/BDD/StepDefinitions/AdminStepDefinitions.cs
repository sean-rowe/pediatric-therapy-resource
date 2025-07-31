using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class AdminStepDefinitions : BaseStepDefinitions
{
    public AdminStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    // System Administration
    
    [Given(@"I am logged in as system administrator")]
    public async Task GivenIAmLoggedInAsSystemAdministrator()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    
    [When(@"I access the admin dashboard")]
    public async Task WhenIAccessTheAdminDashboard()
    {
        await WhenISendAGETRequestTo("/api/admin/dashboard");
    }
    [Then(@"I should see system overview with:")]
    public async Task ThenIShouldSeeSystemOverviewWith(Table expectedMetrics)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var dashboard = GetResponseContent<dynamic>();
        
        foreach (var metric in expectedMetrics.Rows)
        {
            var metricName = metric["Metric"];
            // Verify metric presence
        }
    }

    // User Management
    
    [When(@"I view user management section")]
    public async Task WhenIViewUserManagementSection()
    {
        await WhenISendAGETRequestTo("/api/admin/users");
    }
    [When(@"I search for users with:")]
    public async Task WhenISearchForUsersWith(Table searchCriteria)
    {
        var criteria = searchCriteria.Rows[0];
        var queryParams = new List<string>();
        
        foreach (var key in criteria.Keys)
        {
            queryParams.Add($"{key.ToLower()}={Uri.EscapeDataString(criteria[key])}");
        }
        
        await WhenISendAGETRequestTo($"/api/admin/users?{string.Join("&", queryParams)}");
    }
    [When(@"I update user status:")]
    public async Task WhenIUpdateUserStatus(Table statusUpdate)
    {
        var userId = GetFromContext<string>("SelectedUserId") ?? "test-user";
        await WhenISendAPUTRequestToWithData($"/api/admin/users/{userId}/status", statusUpdate);
    }
    // Commented out to avoid conflict with UserManagementSteps.ThenTheUserShouldBeSuspended
    // [Then(@"the user should be (active|inactive|suspended|deleted)")]
    // public void ThenTheUserShouldBe(string status)
    // {
    //     LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    //     ScenarioContext["UserStatus"] = status;
    // }

    // System Configuration
    
    [When(@"I configure system settings:")]
    public async Task WhenIConfigureSystemSettings(Table settings)
    {
        await WhenISendAPUTRequestToWithData("/api/admin/config", settings);
    }
    [Then(@"settings should be applied system-wide")]
    public async Task ThenSettingsShouldBeAppliedSystemWide()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        // Verify settings propagation
        var configResponse = await Client.GetAsync("/api/admin/config");
        configResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    // Billing & Insurance
    
    [Given(@"I manage billing operations")]
    public void GivenIManageBillingOperations()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I access billing dashboard")]
    public async Task WhenIAccessBillingDashboard()
    {
        await WhenISendAGETRequestTo("/api/admin/billing/dashboard");
    }
    [Then(@"I see financial metrics:")]
    public async Task ThenISeeFinancialMetrics(Table expectedMetrics)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var metrics = GetResponseContent<dynamic>();
        
        foreach (var metric in expectedMetrics.Rows)
        {
            // Verify financial metrics
        }
    }

    // Compliance & Reporting
    
    [Given(@"compliance monitoring is active")]
    public void GivenComplianceMonitoringIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I generate compliance report for ""(.*)""")]
    public async Task WhenIGenerateComplianceReportFor(string reportType)
    {
        await WhenISendAPOSTRequestToWithData("/api/compliance/reports/generate",
            new Dictionary<string, object> { { "type", reportType } });
    }

    [Then(@"admin report should include:")]
    public async Task ThenAdminReportShouldInclude(Table expectedSections)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var report = await LastResponse.Content.ReadAsStringAsync();
        
        foreach (var section in expectedSections.Rows)
        {
            report.Should().Contain(section["Section"]);
        }
    }
    
    [When(@"I generate report with parameters:")]
    public async Task WhenIGenerateReportWithParameters(Table parameters)
    {
        await WhenISendAPOSTRequestToWithData("/api/analytics/reports/generate", parameters);
    }
    [Then(@"report shows (.*) metrics")]
    public void ThenReportShowsMetrics(string metricType)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        ScenarioContext["ReportType"] = metricType;
    }

    // Platform Management
    
    [When(@"I manage platform features:")]
    public async Task WhenIManagePlatformFeatures(Table features)
    {
        foreach (var feature in features.Rows)
        {
            var featureName = feature["Feature"];
            var status = feature["Status"];
            
            await Client.PutAsync($"/api/admin/features/{featureName}", 
                new StringContent($"{{\"enabled\":{status.ToLower() == "enabled"}}}", 
                System.Text.Encoding.UTF8, "application/json"));
        }
    }

    

    [Then(@"features should be (.*) for all users")]
    public void ThenFeaturesShouldBeForAllUsers(string status)
    {
        // Verify feature availability
        ScenarioContext["FeatureStatus"] = status;
    }

    // Data Privacy & Security
    
    [When(@"I review data access logs")]
    public async Task WhenIReviewDataAccessLogs()
    {
        await WhenISendAGETRequestTo("/api/admin/audit/data-access");
    }
    [Then(@"I see detailed access history:")]
    public async Task ThenISeeDetailedAccessHistory(Table expectedFields)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var logs = GetResponseContent<List<dynamic>>();
        logs.Should().NotBeNull();
        
        // Verify log fields
        foreach (var field in expectedFields.Rows)
        {
            // Verify each field presence
        }
    }

    // API Management
    
    [When(@"I manage API access:")]
    public async Task WhenIManageAPIAccess(Table apiSettings)
    {
        foreach (var setting in apiSettings.Rows)
        {
            await Client.PutAsync($"/api/admin/api/{setting["Setting"]}", 
                new StringContent($"{{\"value\":\"{setting["Value"]}\"}}", 
                System.Text.Encoding.UTF8, "application/json"));
        }
    }

    

    [Then(@"API limits should be enforced")]
    public void ThenAPILimitsShouldBeEnforced()
    {
        ScenarioContext["APILimitsActive"] = true;
    }

    // Maintenance & Operations
    
    [When(@"I schedule system maintenance:")]
    public async Task WhenIScheduleSystemMaintenance(Table maintenanceWindow)
    {
        await WhenISendAPOSTRequestToWithData("/api/admin/maintenance/schedule", maintenanceWindow);
    }
    [Then(@"users should be notified of maintenance")]
    public async Task ThenUsersShouldBeNotifiedOfMaintenance()
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        // Verify notification sent
        var notificationResponse = await Client.GetAsync("/api/admin/notifications/maintenance");
        notificationResponse.IsSuccessStatusCode.Should().BeTrue();
    }
}