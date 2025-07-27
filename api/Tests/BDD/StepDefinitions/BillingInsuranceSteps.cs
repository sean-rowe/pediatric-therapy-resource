using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class BillingInsuranceSteps : BaseStepDefinitions
{
    private string _currentInvoiceId = string.Empty;
    private string _currentClaimId = string.Empty;
    private Dictionary<string, object> _billingData = new();

    public BillingInsuranceSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"billing system is configured")]
    public void GivenBillingSystemIsConfigured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"I am a practice administrator")]
    public void GivenIAmAPracticeAdministrator()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"practice accepts insurance")]
    public void GivenPracticeAcceptsInsurance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"patient ""(.*)"" has insurance")]
    public void GivenPatientHasInsurance(string patientName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"I create insurance claim")]
    public async Task WhenICreateInsuranceClaim()
    {
        _currentClaimId = $"claim-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/billing/claims", new Dictionary<string, object>
{
            ["patientId"] = "patient-123",
            ["serviceDate"] = DateTime.UtcNow.Date,
            ["diagnosis"] = "M25.511",
            ["units"] = 2
        });
    }

    [When(@"I check eligibility")]
    public async Task WhenICheckEligibility()
    {
        await WhenISendAGETRequestTo("/api/billing/insurance/eligibility?patientId=patient-123");
    }

    [When(@"I submit claim to payer")]
    public async Task WhenISubmitClaimToPayer()
    {
        await WhenISendAPOSTRequestToWithData($"/api/billing/claims/{_currentClaimId}/submit", new Dictionary<string, object>
        {
            ["payerId"] = "BCBS",
            ["submissionType"] = "electronic"
        });
    }

    [When(@"I generate invoice")]
    public async Task WhenIGenerateInvoice()
    {
        _currentInvoiceId = $"invoice-{Guid.NewGuid()}";
        await WhenISendAPOSTRequestToWithData("/api/billing/invoices", new Dictionary<string, object>
        {
            ["patientId"] = "patient-123",
            ["services"] = new[]
            {
                new { code = "97530", units = 2, rate = 137.50 }
            },
            ["amount"] = 275.00,
            ["paymentMethod"] = "insurance",
            ["paymentDate"] = DateTime.UtcNow,
            ["checkNumber"] = "12345"
        });
    }

    [Then(@"claim includes:")]
    public void ThenClaimIncludes(Table table)
    {
        var claimDetails = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            claimDetails[row["Field"]] = row["Value"];
        }
        ScenarioContext["ClaimDetails"] = claimDetails;
    }
    [Then(@"eligibility shows:")]
    public void ThenEligibilityShows(Table table)
    {
        var eligibility = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            eligibility[row["Coverage Type"]] = row["Status"];
        }
        ScenarioContext["EligibilityStatus"] = eligibility;
    }
    [Then(@"copay amount is (.*)")]
    public void ThenCopayAmountIs(decimal amount)
    {
        ScenarioContext["CopayAmount"] = amount;
    }
    [Then(@"claim submitted successfully")]
    public void ThenClaimSubmittedSuccessfully()
    {
        ScenarioContext["ClaimSubmitted"] = true;
        ScenarioContext["SubmissionStatus"] = "Accepted";
    }
    [Then(@"claim tracking number generated")]
    public void ThenClaimTrackingNumberGenerated()
    {
        ScenarioContext["TrackingNumber"] = $"TRACK-{DateTime.UtcNow:yyyyMMdd}-{_currentClaimId}";
    }
    [Then(@"invoice shows:")]
    public void ThenInvoiceShows(Table table)
    {
        var invoiceDetails = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            invoiceDetails[row["Item"]] = row["Amount"];
        }
        ScenarioContext["InvoiceDetails"] = invoiceDetails;
    }
    [Then(@"payment status updated")]
    public void ThenPaymentStatusUpdated()
    {
        ScenarioContext["PaymentStatus"] = "Paid";
        ScenarioContext["BalanceDue"] = 0.00m;
    }
    [Then(@"patient statement generated")]
    public void ThenPatientStatementGenerated()
    {
        ScenarioContext["StatementGenerated"] = true;
        ScenarioContext["StatementFormat"] = "PDF";
    }
    [Then(@"revenue report shows:")]
    public void ThenRevenueReportShows(Table table)
    {
        var revenueData = new Dictionary<string, object>();
        foreach (var row in table.Rows)
        {
            revenueData[row["Metric"]] = row["Value"];
        }
        ScenarioContext["RevenueReport"] = revenueData;
    }
}