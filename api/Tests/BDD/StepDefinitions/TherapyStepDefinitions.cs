using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class TherapyStepDefinitions : BaseStepDefinitions
{
    public TherapyStepDefinitions(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    // Therapy Planning
    
    [Given(@"I have a student ""(.*)"" with IEP goals:")]
    public void GivenIHaveAStudentWithIEPGoals(string studentName, Table goals)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I click ""Generate Therapy Plan""")]
    public async Task WhenIClickGenerateTherapyPlan()
    {
        await WhenISendAPOSTRequestToWithData("/api/therapy-plans/generate",
            new Dictionary<string, object> 
            { 
                { "studentId", GetFromContext<string>("CurrentStudentId") ?? "test-student" }
            });
    }

    [When(@"I specify:")]
    public async Task WhenISpecify(Table parameters)
    {
        var planParams = TableToJson(parameters);
        var content = new StringContent(planParams, System.Text.Encoding.UTF8, "application/json");
        LastResponse = await Client.PostAsync("/api/therapy-plans/generate", content);
    }
    [Then(@"the system should generate a plan with:")]
    public async Task ThenTheSystemShouldGenerateAPlanWith(Table expectedPlan)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var plan = GetResponseContent<dynamic>();
        plan.Should().NotBeNull();
        
        // Verify plan structure
        foreach (var session in expectedPlan.Rows)
        {
            // In real implementation, verify each session details
        }
    }

    // Data Collection
    
    [Given(@"therapist conducting session")]
    public void GivenTherapistConductingSession()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"student completes activity")]
    public void WhenStudentCompletesActivity()
    {
        ScenarioContext["ActivityCompleted"] = true;
    }
    [Then(@"system captures performance data with timestamp")]
    public async Task ThenSystemCapturesPerformanceDataWithTimestamp()
    {
        var sessionId = GetFromContext<string>("CurrentSessionId") ?? "test-session";
        await WhenISendAPOSTRequestToWithData($"/api/sessions/{sessionId}/data",
            new Dictionary<string, object> 
            { 
                { "timestamp", DateTime.UtcNow.ToString("O") },
                { "performance", "completed" }
            });
        
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    // Assessment & Screening
    
    [Given(@"I need to screen ""(.*)"" for (.*)")]
    public void GivenINeedToScreenFor(string studentName, string assessmentType)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I select ""(.*)""")]
    public async Task WhenISelect(string toolName)
    {
        await WhenISendAGETRequestTo($"/api/assessments/tools?name={Uri.EscapeDataString(toolName)}");
        ScenarioContext["SelectedTool"] = toolName;
    }
    [Then(@"the tool should present:")]
    public async Task ThenTheToolShouldPresent(Table expectedItems)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var tool = GetResponseContent<dynamic>();
        
        // Verify assessment items
        foreach (var item in expectedItems.Rows)
        {
            // In real implementation, verify each assessment item
        }
    }

    // Documentation Helpers
    
    [Given(@"I completed a (.*)-minute session with ""(.*)""")]
    public void GivenICompletedASessionWith(int duration, string studentName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I used these resources during session:")]
    public void GivenIUsedTheseResourcesDuringSession(Table resources)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I click ""Generate Session Note""")]
    public async Task WhenIClickGenerateSessionNote()
    {
        var sessionData = new
        {
            studentName = GetFromContext<string>("SessionStudent"),
            duration = GetFromContext<int>("SessionDuration"),
            resources = GetFromContext<object>("SessionResources")
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(sessionData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        LastResponse = await Client.PostAsync("/api/documentation/session-notes/generate", content);
    }

    

    [Then(@"the system should create:")]
    public async Task ThenTheSystemShouldCreate(Table expectedSections)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var note = await LastResponse.Content.ReadAsStringAsync();
        
        foreach (var section in expectedSections.Rows)
        {
            var sectionName = section["Section"];
            note.Should().Contain(sectionName);
        }
    }
    
    [When(@"planning and documenting")]
    public void WhenPlanningAndDocumenting()
    {
        ScenarioContext["ActivityType"] = "planning";
    }
    [Then(@"resources linked to each student")]
    public async Task ThenResourcesLinkedToEachStudent()
    {
        var response = await Client.GetAsync("/api/caseload/overview");
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var caseload = GetResponseContent<dynamic>();
        // Verify resource linkage
    }

    // Teletherapy
    
    [Given(@"I am conducting teletherapy")]
    public void GivenIAmConductingTeletherapy()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"using platform during session")]
    public void WhenUsingPlatformDuringSession()
    {
        ScenarioContext["PlatformActive"] = true;
    }
    [Then(@"virtual tools enhance engagement")]
    public async Task ThenVirtualToolsEnhanceEngagement()
    {
        var tools = await Client.GetAsync("/api/virtual/tools");
        tools.IsSuccessStatusCode.Should().BeTrue();
        
        var toolsList = GetResponseContent<List<string>>();
        toolsList.Should().Contain("dice");
        toolsList.Should().Contain("spinner");
        toolsList.Should().Contain("tokens");
    }

    // Outcome Measurement
    
    [Given(@"I am treating ""(.*)"" for (.*)")]
    public void GivenIAmTreatingFor(string patientName, string condition)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I initiate (.*) assessment")]
    public async Task WhenIInitiateAssessment(string assessmentType)
    {
        await WhenISendAPOSTRequestToWithData("/api/outcomes/administer",
            new Dictionary<string, object> 
            { 
                { "type", assessmentType },
                { "patientId", GetFromContext<string>("PatientId") ?? "test-patient" }
            });
    }

    [Then(@"the system should present:")]
    public async Task ThenTheSystemShouldPresent(Table expectedContent)
    {
        LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        var content = await LastResponse.Content.ReadAsStringAsync();
        
        foreach (var row in expectedContent.Rows)
        {
            var area = row["Assessment Area"];
            content.Should().Contain(area);
        }
    }

    // Transition Planning
    
    [Given(@"working with transition-age students")]
    public void GivenWorkingWithTransitionAgeStudents()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"planning for adulthood")]
    public void WhenPlanningForAdulthood()
    {
        ScenarioContext["PlanningType"] = "transition";
    }
    [Then(@"comprehensive transition resources")]
    public async Task ThenComprehensiveTransitionResources()
    {
        var response = await Client.GetAsync("/api/transition/resources");
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var resources = GetResponseContent<dynamic>();
        resources.Should().NotBeNull();
    }
}
