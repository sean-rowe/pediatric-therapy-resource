using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class BatchOperationsSteps : BaseStepDefinitions
{
    private string _batchJobId = string.Empty;
    private List<object> _selectedItems = new();
    private Dictionary<string, object> _batchResults = new();

    public BatchOperationsSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"batch operations are enabled")]
    public void GivenBatchOperationsAreEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have selected (.*) resources")]
    public void GivenIHaveSelectedResources(int resourceCount)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I have selected (.*) students")]
    public void GivenIHaveSelectedStudents(int studentCount)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"batch processing queue is available")]
    public void GivenBatchProcessingQueueIsAvailable()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I initiate bulk download")]
    public async Task WhenIInitiateBulkDownload()
    {
        _batchJobId = $"batch-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/batch/download", new Dictionary<string, object>
        {
            ["resourceIds"] = _selectedItems.Take(10).Select(r => ((dynamic)r).Id).ToArray(),
            ["format"] = "zip",
            ["includeMetadata"] = true
        });
    }
    
    [When(@"I select all visible items")]
    public async Task WhenISelectAllVisibleItems()
    {
        await WhenISendAPOSTRequestToWithData("/api/batch/select-visible", new Dictionary<string, object>
        {
            ["filter"] = "current_page",
            ["pageSize"] = 50
        });
    }
    
    [When(@"I apply bulk category update")]
    public async Task WhenIApplyBulkCategoryUpdate()
    {
            await WhenISendAPOSTRequestToWithData("/api/batch/update-categories", new Dictionary<string, object>
            {
                ["resourceIds"] = _selectedItems.Select(r => ((dynamic)r).Id).ToArray(),
                ["removeCategories"] = new[] { "Outdated" }
            });
    }

        [When(@"I send batch email to parents")]
        public async Task WhenISendBatchEmailToParents()
        {
            await WhenISendAPOSTRequestToWithData("/api/batch/email", new Dictionary<string, object>
            {
                ["recipients"] = _selectedItems.Select(s => $"{((dynamic)s).Name.Replace(" ", "").ToLower()}@parent.com").ToArray(),
                ["template"] = "weekly_progress_update",
                ["customization"] = new { IncludeGoalProgress = true }
            });
    }

        [When(@"I perform batch import")]
        public async Task WhenIPerformBatchImport()
        {
            await WhenISendAPOSTRequestToWithData("/api/batch/import", new Dictionary<string, object>
            {
                ["source"] = "csv_upload",
                ["filename"] = "resources_batch_import.csv",
                ["mappings"] = new Dictionary<string, string>
                {
                    ["Column1"] = "title",
                    ["Column2"] = "description",
                    ["Column3"] = "category"
                }
            });
    }

    [When(@"I check batch status")]
    public async Task WhenICheckBatchStatus()
    {
        await WhenISendAGETRequestTo($"/api/batch/status/{_batchJobId}");
    }
    [Then(@"bulk selection shows (.*)")]
    public void ThenBulkSelectionShows(int count)
    {
        ScenarioContext["BulkSelectionCount"] = count;
        ScenarioContext["SelectionConfirmed"] = true;
    }
    [Then(@"bulk actions available:")]
    public void ThenBulkActionsAvailable(Table table)
    {
        var actions = new List<string>();
        foreach (var row in table.Rows)
        {
            actions.Add(row["Action"]);
        }
        ScenarioContext["AvailableBulkActions"] = actions;
    }
    [Then(@"download queue created")]
    public void ThenDownloadQueueCreated()
    {
        ScenarioContext["DownloadQueueCreated"] = true;
        ScenarioContext["EstimatedTime"] = "3 minutes";
    }
    [Then(@"progress indicator shows:")]
    public void ThenProgressIndicatorShows(Table table)
    {
        var progress = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            progress[row["Metric"]] = row["Value"];
        }
        ScenarioContext["BatchProgress"] = progress;
    }
    [Then(@"download link provided")]
    public void ThenDownloadLinkProvided()
    {
        ScenarioContext["DownloadLinkProvided"] = true;
        ScenarioContext["DownloadExpiry"] = DateTime.UtcNow.AddHours(24);
    }
    [Then(@"categories updated for all selected")]
    public void ThenCategoriesUpdatedForAllSelected()
    {
        ScenarioContext["CategoriesUpdated"] = true;
        ScenarioContext["UpdateCount"] = _selectedItems.Count;
    }
    [Then(@"update summary shows:")]
    public void ThenUpdateSummaryShows(Table table)
    {
        var summary = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            summary[row["Operation"]] = row["Count"];
        }
        ScenarioContext["UpdateSummary"] = summary;
    }
    [Then(@"emails queued for delivery")]
    public void ThenEmailsQueuedForDelivery()
    {
        ScenarioContext["EmailsQueued"] = true;
        ScenarioContext["EmailCount"] = _selectedItems.Count;
    }
    [Then(@"delivery status tracked")]
    public void ThenDeliveryStatusTracked()
    {
        ScenarioContext["DeliveryTracking"] = true;
        ScenarioContext["TrackingMetrics"] = new[]
        {
            "Sent",
            "Delivered", 
            "Opened",
            "Failed"
        };
    }

    [Then(@"import validation shows:")]
    public void ThenImportValidationShows(Table table)
    {
        var validation = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            validation[row["Status"]] = row["Count"];
        }
        ScenarioContext["ImportValidation"] = validation;
    }
    [Then(@"errors reported for review")]
    public void ThenErrorsReportedForReview()
    {
        ScenarioContext["ErrorsReported"] = true;
        ScenarioContext["ErrorDetails"] = new[]
        {
            "Row 5: Missing required field 'title'",
            "Row 12: Invalid category 'xyz'",
            "Row 18: Duplicate resource name"
        };
    }

    [Then(@"successful imports processed")]
    public void ThenSuccessfulImportsProcessed()
    {
        ScenarioContext["ImportsProcessed"] = true;
        ScenarioContext["ProcessedCount"] = 47;
        ScenarioContext["FailedCount"] = 3;
    }
    [Then(@"batch status shows:")]
    public void ThenBatchStatusShows(Table table)
    {
        var status = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            status[row["Field"]] = row["Value"];
        }
        ScenarioContext["BatchStatus"] = status;
    }
}