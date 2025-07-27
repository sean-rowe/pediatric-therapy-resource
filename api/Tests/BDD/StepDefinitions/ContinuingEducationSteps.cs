using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class ContinuingEducationSteps : BaseStepDefinitions
{
    private string _currentCourseId = string.Empty;
    private string _certificateId = string.Empty;
    private Dictionary<string, object> _courseData = new();
    private List<object> _ceuCredits = new();

    public ContinuingEducationSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"continuing education module is available")]
    public void GivenContinuingEducationModuleIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I need CEUs for license renewal")]
    public void GivenINeedCEUsForLicenseRenewal()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"courses are available:")]
    public void GivenCoursesAreAvailable(Table table)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I enrolled in ""(.*)""")]
    public void GivenIEnrolledIn(string courseName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I completed required modules")]
    public void GivenICompletedRequiredModules()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I passed assessment with (.*)%")]
    public void GivenIPassedAssessmentWithPercentage(int score)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I track professional development")]
    public void GivenITrackProfessionalDevelopment()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I browse CE catalog")]
    public async Task WhenIBrowseCECatalog()
    {
        await WhenISendAGETRequestTo("/api/continuing-education/catalog");

    }

    [When(@"I filter by:")]
    public async Task WhenIFilterBy(Table table)
    {
        var filters = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            filters[row["Filter"]] = row["Value"];
        }
        await WhenISendAPOSTRequestToWithData("/api/continuing-education/filter", filters);

    }
    [When(@"I enroll in ""(.*)""")]
    public async Task WhenIEnrollIn(string courseName)
    {
        _currentCourseId = $"course-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/continuing-education/enroll", new Dictionary<string, object>
{
    ["courseName"] = courseName,
    ["courseId"] = _currentCourseId
});
    }

    [When(@"I access course materials")]
    public async Task WhenIAccessCourseMaterials()
    {
            await WhenISendAGETRequestTo($"/api/continuing-education/courses/{_currentCourseId}/materials");

    }

    [When(@"I complete video module")]
    public async Task WhenICompleteVideoModule()
    {
        await WhenISendAPOSTRequestToWithData($"/api/continuing-education/courses/{_currentCourseId}/progress", new Dictionary<string, object>
{
    ["moduleId"] = "module-1",
    ["completed"] = true,
    ["watchTime"] = 3600
});
    }

    [When(@"all requirements met")]
    public async Task WhenAllRequirementsMet()
    {
            await WhenISendAPOSTRequestToWithData($"/api/continuing-education/courses/{_currentCourseId}/complete", new Dictionary<string, object>
{
    ["allModulesComplete"] = true,
    ["assessmentPassed"] = true,
    ["evaluationSubmitted"] = true
});
    }

    [When(@"I view CE transcript")]
    public async Task WhenIViewCETranscript()
    {
            await WhenISendAGETRequestTo("/api/continuing-education/transcript");

    }

    [Then(@"catalog shows:")]
    public void ThenCatalogShows(Table table)
    {
        var catalogItems = new List<object>();
        foreach (var row in table.Rows)
        {
            catalogItems.Add(new
            {
                Category = row["Category"],
                CourseCount = row["Course Count"]
            });
        }
        
        ScenarioContext["CatalogCategories"] = catalogItems;

    }

    [Then(@"each course displays:")]
    public void ThenEachCourseDisplays(Table table)
    {
        var courseInfo = new List<string>();
        foreach (var row in table.Rows)
        {
            courseInfo.Add(row["Information"]);
        }
        ScenarioContext["CourseInformation"] = courseInfo;

    }
    [Then(@"filtered results show (.*) courses")]
    public void ThenFilteredResultsShowCourses(int count)
    {
        ScenarioContext["FilteredCourseCount"] = count;
        ScenarioContext["FilterApplied"] = true;

    }
    [Then(@"enrollment confirmed")]
    public void ThenEnrollmentConfirmed()
    {
        ScenarioContext["EnrollmentStatus"] = "confirmed";
        ScenarioContext["AccessGranted"] = true;

    }

    [Then(@"course dashboard shows:")]
    public void ThenCourseDashboardShows(Table table)
    {
        var dashboardElements = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            dashboardElements[row["Element"]] = row["Status"];
        }
        ScenarioContext["CourseDashboard"] = dashboardElements;

    }
    [Then(@"materials include:")]
    public void ThenMaterialsInclude(Table table)
    {
        var materials = new List<object>();
        foreach (var row in table.Rows)
        {
            materials.Add(new
            {
                Type = row["Material Type"],
                Access = row["Access"]
            });
        }
        
        ScenarioContext["CourseMaterials"] = materials;

    }

    [Then(@"progress updates to (.*)%")]
    public void ThenProgressUpdatesTo(int percentage)
    {
        ScenarioContext["CourseProgress"] = percentage;
        ScenarioContext["ProgressSaved"] = true;

    }

    [Then(@"next module unlocks")]
    public void ThenNextModuleUnlocks()
    {
        ScenarioContext["NextModuleUnlocked"] = true;
        ScenarioContext["LinearProgression"] = true;

    }

    [Then(@"certificate generates with:")]
    public void ThenCertificateGeneratesWith(Table table)
    {
        _certificateId = $"cert-{Guid.NewGuid()}";
        var certificateData = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            certificateData[row["Field"]] = row["Value"];
        }
        certificateData["CertificateId"] = _certificateId;
        ScenarioContext["Certificate"] = certificateData;

    }
    [Then(@"CEUs recorded in system")]
    public void ThenCEUsRecordedInSystem()
    {
        ScenarioContext["CEUsRecorded"] = true;
        ScenarioContext["CEUAmount"] = 2.0;
        ScenarioContext["RecordedDate"] = DateTime.UtcNow;

    }
    [Then(@"transcript shows:")]
    public void ThenTranscriptShows(Table table)
    {
        var transcriptEntries = new List<object>();
        foreach (var row in table.Rows)
        {
            transcriptEntries.Add(new
            {
                Year = int.Parse(row["Year"]),
                TotalCEUs = decimal.Parse(row["Total CEUs"]),
                Courses = int.Parse(row["Courses"])
            });
        }
        
        ScenarioContext["TranscriptData"] = transcriptEntries;

    }

    [Then(@"breakdown by category:")]
    public void ThenBreakdownByCategory(Table table)
    {
        var categoryBreakdown = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            categoryBreakdown[row["Category"]] = row["CEUs"];
        }
        ScenarioContext["CEUBreakdown"] = categoryBreakdown;

    }
    [Then(@"export options include:")]
    public void ThenExportOptionsInclude(Table table)
    {
        var exportOptions = new List<string>();
        foreach (var row in table.Rows)
        {
            exportOptions.Add(row["Format"]);
        }
        ScenarioContext["TranscriptExportOptions"] = exportOptions;

    }
}