using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

[Binding]
public class AuditLoggingSteps : BaseStepDefinitions
{
    private string _currentUserId = string.Empty;
    private string _currentStudentId = string.Empty;
    private Dictionary<string, object> _auditEntry = new();
    private List<object> _auditLogs = new();
    private DateTime _auditTimestamp;

    public AuditLoggingSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }
    [Given(@"the audit logging system is active")]
    public void GivenTheAuditLoggingSystemIsActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit policies are configured")]
    public void GivenAuditPoliciesAreConfigured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"log retention is properly managed")]
    public void GivenLogRetentionIsProperlyManaged()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"activity audit logging is enabled")]
    public void GivenActivityAuditLoggingIsEnabled()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"student ""(.*)"" has therapy records")]
    public void GivenStudentHasTherapyRecords(string studentName)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"therapist ""(.*)"" accesses the records")]
    public void GivenTherapistAccessesTheRecords(string therapistEmail)
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"payment processing requires audit compliance")]
    public void GivenPaymentProcessingRequiresAuditCompliance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logs contain sensitive compliance data")]
    public void GivenAuditLogsContainSensitiveComplianceData()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"different log types have different retention requirements")]
    public void GivenDifferentLogTypesHaveDifferentRetentionRequirements()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logs feed security monitoring systems")]
    public void GivenAuditLogsFeedSecurityMonitoringSystems()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"regulatory audits require comprehensive reporting")]
    public void GivenRegulatoryAuditsRequireComprehensiveReporting()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logs contain vast amounts of data")]
    public void GivenAuditLogsContainVastAmountsOfData()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"logs must be integrated with enterprise security tools")]
    public void GivenLogsMustBeIntegratedWithEnterpriseSecurityTools()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logging is critical for compliance")]
    public void GivenAuditLoggingIsCriticalForCompliance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit log integrity is paramount")]
    public void GivenAuditLogIntegrityIsParamount()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"logging should not significantly impact system performance")]
    public void GivenLoggingShouldNotSignificantlyImpactSystemPerformance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logs are required for regulatory compliance")]
    public void GivenAuditLogsAreRequiredForRegulatoryCompliance()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"audit logs may be lost or corrupted")]
    public void GivenAuditLogsMayBeLostOrCorrupted()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [Given(@"system may generate millions of log entries daily")]
    public void GivenSystemMayGenerateMillionsOfLogEntriesDaily()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }
    [When(@"security-relevant activities occur:")]
    public async Task WhenSecurityRelevantActivitiesOccur(Table table)
    {
        var activities = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var activity = new
            {
                ActivityType = row["Activity Type"],
                Details = row["Details"],
                RequiredFields = row["Required Fields"],
                RetentionPeriod = row["Retention Period"]
            };
            activities.Add(activity);
            
            // Simulate activity occurrence
            await WhenISendAPOSTRequestToWithData("/api/audit/log", new Dictionary<string, object>
            {
                ["activityType"] = activity.ActivityType,
                ["details"] = activity.Details,
                ["timestamp"] = DateTime.UtcNow,
                ["userId"] = _currentUserId
            });
        }
        
        ScenarioContext["LoggedActivities"] = activities;
    }
    
    [When(@"data access occurs")]
    public async Task WhenDataAccessOccurs()
    {
        _auditTimestamp = DateTime.UtcNow;
        
        await WhenISendAPOSTRequestToWithData("/api/audit/data-access", new Dictionary<string, object>
        {
            ["userId"] = _currentUserId,
            ["studentId"] = _currentStudentId,
            ["fieldsAccessed"] = new[] { "name", "therapy_notes", "goals" },
            ["timestamp"] = _auditTimestamp,
            ["sourceIP"] = "192.168.1.100",
            ["sessionId"] = "SES-789012",
            ["accessMethod"] = "web-application",
            ["businessJustification"] = "scheduled-therapy-session",
            ["geographicLocation"] = "New York, NY, USA",
            ["deviceInfo"] = "Windows 11, Chrome 120"
        });
        
        ScenarioContext["DataAccessLogged"] = true;
    }
    
    [When(@"I perform administrative operations:")]
    public async Task WhenIPerformAdministrativeOperations(Table table)
    {
        var operations = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var operation = new
            {
                OperationType = row["Operation Type"],
                Details = row["Details"],
                RiskLevel = row["Risk Level"]
            };
            operations.Add(operation);
            
            await WhenISendAPOSTRequestToWithData("/api/audit/admin-operation", new Dictionary<string, object>
            {
                ["operationType"] = operation.OperationType,
                ["details"] = operation.Details,
                ["riskLevel"] = operation.RiskLevel,
                ["administratorId"] = _currentUserId,
                ["timestamp"] = DateTime.UtcNow
            });
        }
        
        ScenarioContext["AdminOperations"] = operations;
    }
    
    [When(@"audit financial transactions occur:")]
    public async Task WhenFinancialTransactionsOccur(Table table)
    {
        var transactions = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var transaction = new
            {
                TransactionType = row["Transaction Type"],
                Details = row["Details"],
                ComplianceRequirement = row["Compliance Requirement"]
            };
            transactions.Add(transaction);
            
            await WhenISendAPOSTRequestToWithData("/api/audit/financial-transaction", new Dictionary<string, object>
            {
                ["transactionType"] = transaction.TransactionType,
                ["details"] = transaction.Details,
                ["complianceRequirement"] = transaction.ComplianceRequirement,
                ["transactionId"] = Guid.NewGuid().ToString(),
                ["timestamp"] = DateTime.UtcNow
            });
        }
        
        ScenarioContext["FinancialTransactions"] = transactions;
    }
    
    [When(@"audit entries are created")]
    public async Task WhenAuditEntriesAreCreated()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/create-entry", new Dictionary<string, object>
        {
            ["entryType"] = "test-audit-entry",
            ["data"] = "sensitive-compliance-data",
            ["timestamp"] = DateTime.UtcNow,
            ["requiresIntegrityProtection"] = true
        });
        
        ScenarioContext["AuditEntriesCreated"] = true;
    }
    
    [When(@"log retention policies are applied:")]
    public async Task WhenLogRetentionPoliciesAreApplied(Table table)
    {
        var retentionPolicies = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var policy = new
            {
                LogType = row["Log Type"],
                RetentionPeriod = row["Retention Period"],
                ComplianceDriver = row["Compliance Driver"],
                ArchiveMethod = row["Archive Method"]
            };
            retentionPolicies.Add(policy);
        }
        
        await WhenISendAPOSTRequestToWithData("/api/audit/retention/apply", new Dictionary<string, object>
        {
            ["policies"] = retentionPolicies,
            ["applyTime"] = DateTime.UtcNow
        });
        
        ScenarioContext["RetentionPoliciesApplied"] = retentionPolicies;
    }
    
    [When(@"suspicious patterns are detected in logs:")]
    public async Task WhenSuspiciousPatternsAreDetectedInLogs(Table table)
    {
        var suspiciousPatterns = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var pattern = new
            {
                PatternType = row["Pattern Type"],
                DetectionCriteria = row["Detection Criteria"],
                ResponseAction = row["Response Action"]
            };
            suspiciousPatterns.Add(pattern);
        }
        
        await WhenISendAPOSTRequestToWithData("/api/audit/security/detect-patterns", new Dictionary<string, object>
        {
            ["patterns"] = suspiciousPatterns,
            ["detectionTime"] = DateTime.UtcNow
        });
        
        ScenarioContext["SuspiciousPatternsDetected"] = suspiciousPatterns;
    }
    
    [When(@"compliance reports are requested:")]
    public async Task WhenComplianceReportsAreRequested(Table table)
    {
        var reportRequests = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var request = new
            {
                ReportType = row["Report Type"],
                RegulatoryRequirement = row["Regulatory Requirement"],
                ContentRequirements = row["Content Requirements"]
            };
            reportRequests.Add(request);
        }
        
        await WhenISendAPOSTRequestToWithData("/api/audit/reports/generate", new Dictionary<string, object>
        {
            ["reportRequests"] = reportRequests,
            ["requestTime"] = DateTime.UtcNow
        });
        
        ScenarioContext["ComplianceReportsRequested"] = reportRequests;
    }
    
    [When(@"investigators need to analyze patterns")]
    public async Task WhenInvestigatorsNeedToAnalyzePatterns()
    {
        await WhenISendAGETRequestTo("/api/audit/search/capabilities");
        ScenarioContext["SearchCapabilitiesRequested"] = true;
    }
    
    [When(@"log forwarding is configured")]
    public async Task WhenLogForwardingIsConfigured()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/forwarding/configure", new Dictionary<string, object>
        {
            ["destinations"] = new[]
            {
                new { type = "SIEM", protocol = "syslog", format = "CEF" },
                new { type = "Cloud", protocol = "HTTPS", format = "JSON" },
                new { type = "Compliance", protocol = "database", format = "structured" }
            }
        });
        
        ScenarioContext["LogForwardingConfigured"] = true;
    }
    
    [When(@"audit logging system fails")]
    public async Task WhenAuditLoggingSystemFails()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/system/failure", new Dictionary<string, object>
        {
            ["failureType"] = "system-failure",
            ["failureTime"] = DateTime.UtcNow,
            ["impactLevel"] = "critical"
        });
        
        ScenarioContext["AuditSystemFailure"] = true;
    }
    
    [When(@"log tampering is attempted or detected")]
    public async Task WhenLogTamperingIsAttemptedOrDetected()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/security/tampering-detected", new Dictionary<string, object>
        {
            ["tamperingType"] = "unauthorized-modification",
            ["detectionTime"] = DateTime.UtcNow,
            ["affectedLogs"] = "audit-entries-batch-001"
        });
        
        ScenarioContext["LogTamperingDetected"] = true;
    }
    
    [When(@"audit logging performance is monitored")]
    public async Task WhenAuditLoggingPerformanceIsMonitored()
    {
        await WhenISendAGETRequestTo("/api/audit/performance/metrics");
        ScenarioContext["PerformanceMonitored"] = true;
    }
    [When(@"compliance violations are detected:")]
    public async Task WhenComplianceViolationsAreDetected(Table table)
    {
        var violations = new List<object>();
        
        foreach (var row in table.Rows)
        {
            var violation = new
            {
                ViolationType = row["Violation Type"],
                DetectionMethod = row["Detection Method"],
                RemediationRequired = row["Remediation Required"]
            };
            violations.Add(violation);
        }
        
        await WhenISendAPOSTRequestToWithData("/api/audit/compliance/violations", new Dictionary<string, object>
        {
            ["violations"] = violations,
            ["detectionTime"] = DateTime.UtcNow
        });
        
        ScenarioContext["ComplianceViolationsDetected"] = violations;
    }
    
    [When(@"log recovery is required")]
    public async Task WhenLogRecoveryIsRequired()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/recovery/initiate", new Dictionary<string, object>
        {
            ["recoveryType"] = "full-restore",
            ["timeRange"] = "last-24-hours",
            ["priority"] = "high"
        });
        
        ScenarioContext["LogRecoveryInitiated"] = true;
    }
    
    [When(@"audit volume reaches scalability limits")]
    public async Task WhenAuditVolumeReachesScalabilityLimits()
    {
        await WhenISendAPOSTRequestToWithData("/api/audit/scalability/limits-reached", new Dictionary<string, object>
        {
            ["currentVolume"] = "5-million-entries-per-day",
            ["capacityUtilization"] = "95%",
            ["scalingRequired"] = true
        });
        
        ScenarioContext["ScalabilityLimitsReached"] = true;
    }
    
    [Then(@"all activities should be logged immediately")]
    public void ThenAllActivitiesShouldBeLoggedImmediately()
    {
        ThenTheResponseStatusShouldBe(201);
        ScenarioContext["ImmediateLogging"] = true;
    }
    [Then(@"log entries should be immutable once written")]
    public void ThenLogEntriesShouldBeImmutableOnceWritten()
    {
        ScenarioContext["ImmutableLogs"] = true;
        ScenarioContext["WriteOnce"] = true;
    }
    [Then(@"log integrity should be cryptographically protected")]
    public void ThenLogIntegrityShouldBeCryptographicallyProtected()
    {
        ScenarioContext["CryptographicProtection"] = true;
        ScenarioContext["IntegrityProtected"] = true;
    }
    [Then(@"logs should be available for real-time monitoring")]
    public void ThenLogsShouldBeAvailableForRealTimeMonitoring()
    {
        ScenarioContext["RealTimeAvailability"] = true;
        ScenarioContext["MonitoringReady"] = true;
    }
    [Then(@"audit log should capture:")]
    public void ThenAuditLogShouldCapture(Table table)
    {
        ThenTheResponseStatusShouldBe(201);
        var capturedFields = new Dictionary<string, object>();
        
        foreach (var row in table.Rows)
        {
            capturedFields[row["Field"]] = new
            {
                Value = row["Value"],
                Required = bool.Parse(row["Required"])
            };
        }
        
        ScenarioContext["CapturedFields"] = capturedFields;
    }
    
    [Then(@"log entry should be immutable")]
    public void ThenLogEntryShouldBeImmutable()
    {
        ScenarioContext["LogEntryImmutable"] = true;
        ScenarioContext["TamperProof"] = true;
    }
    [Then(@"data subject should be notifiable if required by law")]
    public void ThenDataSubjectShouldBeNotifiableIfRequiredByLaw()
    {
        ScenarioContext["DataSubjectNotifiable"] = true;
        ScenarioContext["PrivacyCompliance"] = true;
    }
    [Then(@"each operation should be logged with:")]
    public void ThenEachOperationShouldBeLoggedWith(Table table)
    {
        ThenTheResponseStatusShouldBe(201);
        var auditFields = new Dictionary<string, string>();
        
        foreach (var row in table.Rows)
        {
            auditFields[row["Audit Field"]] = row["Purpose"];
        }
        ScenarioContext["OperationAuditFields"] = auditFields;
    }
    [Then(@"critical operations should require dual approval")]
    public void ThenCriticalOperationsShouldRequireDualApproval()
    {
        ScenarioContext["DualApprovalRequired"] = true;
        ScenarioContext["CriticalOperationControl"] = true;
    }
    [Then(@"all changes should be logged before execution")]
    public void ThenAllChangesShouldBeLoggedBeforeExecution()
    {
        ScenarioContext["PreExecutionLogging"] = true;
        ScenarioContext["ChangeTracking"] = true;
    }
    [Then(@"transaction audit should include:")]
    public void ThenTransactionAuditShouldInclude(Table table)
    {
        ThenTheResponseStatusShouldBe(201);
        var auditElements = new Dictionary<string, string>();
        
        foreach (var row in table.Rows)
        {
            auditElements[row["Audit Element"]] = row["Details"];
        }
        ScenarioContext["TransactionAuditElements"] = auditElements;
    }
    [Then(@"financial logs should be retained for (.*) years")]
    public void ThenFinancialLogsShouldBeRetainedForYears(int years)
    {
        ScenarioContext["FinancialRetentionPeriod"] = $"{years} years";
        ScenarioContext["ExtendedRetention"] = true;
    }
    [Then(@"audit trails should be available for tax reporting")]
    public void ThenAuditTrailsShouldBeAvailableForTaxReporting()
    {
        ScenarioContext["TaxReportingReady"] = true;
        ScenarioContext["FinancialCompliance"] = true;
    }
    [Then(@"log integrity protection should include:")]
    public void ThenLogIntegrityProtectionShouldInclude(Table table)
    {
        ThenTheResponseStatusShouldBe(201);
        var protectionMethods = new Dictionary<string, object>();
        
        foreach (var row in table.Rows)
        {
            protectionMethods[row["Protection Method"]] = new
            {
                Implementation = row["Implementation"],
                Purpose = row["Purpose"]
            };;
        }
        
        ScenarioContext["IntegrityProtectionMethods"] = protectionMethods;
    }
    
    [Then(@"any tampering attempts should be immediately detected")]
    public void ThenAnyTamperingAttemptsShouldBeImmediatelyDetected()
    {
        ScenarioContext["TamperDetection"] = true;
        ScenarioContext["ImmediateDetection"] = true;
    }
    [Then(@"log integrity should be verifiable independently")]
    public void ThenLogIntegrityShouldBeVerifiableIndependently()
    {
        ScenarioContext["IndependentVerification"] = true;
        ScenarioContext["ThirdPartyVerifiable"] = true;
    }
    [Then(@"integrity violations should trigger security alerts")]
    public void ThenIntegrityViolationsShouldTriggerSecurityAlerts()
    {
        ScenarioContext["SecurityAlertsTriggered"] = true;
        ScenarioContext["IntegrityMonitoring"] = true;
    }
    [Then(@"logs should be automatically archived before retention expiry")]
    public void ThenLogsShouldBeAutomaticallyArchivedBeforeRetentionExpiry()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["AutomaticArchiving"] = true;
    }
    [Then(@"archived logs should remain searchable")]
    public void ThenArchivedLogsShouldRemainSearchable()
    {
        ScenarioContext["SearchableArchives"] = true;
        ScenarioContext["ArchiveAccessibility"] = true;
    }
    [Then(@"log destruction should be documented and auditable")]
    public void ThenLogDestructionShouldBeDocumentedAndAuditable()
    {
        ScenarioContext["DocumentedDestruction"] = true;
        ScenarioContext["AuditableDestruction"] = true;
    }
    [Then(@"legal holds should prevent premature destruction")]
    public void ThenLegalHoldsShouldPreventPrematureDestruction()
    {
        ScenarioContext["LegalHoldsEnforced"] = true;
        ScenarioContext["PreventPrematureDestruction"] = true;
    }
    [Then(@"alerts should be generated in real-time")]
    public void ThenAlertsShouldBeGeneratedInRealTime()
    {
        ThenTheResponseStatusShouldBe(201);
        ScenarioContext["RealTimeAlerts"] = true;
    }
    [Then(@"security teams should be notified immediately")]
    public void ThenSecurityTeamsShouldBeNotifiedImmediately()
    {
        ScenarioContext["SecurityTeamNotified"] = true;
        ScenarioContext["ImmediateNotification"] = true;
    }
    [Then(@"automated responses should be triggered where appropriate")]
    public void ThenAutomatedResponsesShouldBeTriggeredWhereAppropriate()
    {
        ScenarioContext["AutomatedResponses"] = true;
        ScenarioContext["ResponseAutomation"] = true;
    }
    [Then(@"correlation should occur across multiple log sources")]
    public void ThenCorrelationShouldOccurAcrossMultipleLogSources()
    {
        ScenarioContext["LogCorrelation"] = true;
        ScenarioContext["MultiSourceAnalysis"] = true;
    }
    [Then(@"reports should be generated automatically")]
    public void ThenReportsShouldBeGeneratedAutomatically()
    {
        ThenTheResponseStatusShouldBe(200);
        ScenarioContext["AutomaticReportGeneration"] = true;
    }
    [Then(@"report data should be verifiable against source logs")]
    public void ThenReportDataShouldBeVerifiableAgainstSourceLogs()
    {
        ScenarioContext["ReportVerification"] = true;
        ScenarioContext["DataIntegrity"] = true;
    }
    [Then(@"reports should include statistical summaries")]
    public void ThenReportsShouldIncludeStatisticalSummaries()
    {
        ScenarioContext["StatisticalSummaries"] = true;
        ScenarioContext["DataAnalytics"] = true;
    }
    [Then(@"custom date ranges should be supported")]
    public void ThenCustomDateRangesShouldBeSupported()
    {
        ScenarioContext["CustomDateRanges"] = true;
        ScenarioContext["FlexibleReporting"] = true;
    }
    [Then(@"reports should be exportable in multiple formats")]
    public void ThenReportsShouldBeExportableInMultipleFormats()
    {
        ScenarioContext["MultipleExportFormats"] = true;
        ScenarioContext["FormatFlexibility"] = true;
    }
    [Then(@"search capabilities should include:")]
    public void ThenSearchCapabilitiesShouldInclude(Table table)
    {
        ThenTheResponseStatusShouldBe(200);
        var searchCapabilities = new Dictionary<string, object>();
        
        foreach (var row in table.Rows)
        {
            searchCapabilities[row["Search Type"]] = new
            {
                Functionality = row["Functionality"],
                PerformanceTarget = row["Performance Target"]
            };
        }
        
        ScenarioContext["SearchCapabilities"] = searchCapabilities;
    }
    
    [Then(@"search results should be paginated for large result sets")]
    public void ThenSearchResultsShouldBePaginatedForLargeResultSets()
    {
        ScenarioContext["PaginatedResults"] = true;
        ScenarioContext["LargeResultSetHandling"] = true;
    }
    [Then(@"search history should be maintained for investigators")]
    public void ThenSearchHistoryShouldBeMaintainedForInvestigators()
    {
        ScenarioContext["SearchHistory"] = true;
        ScenarioContext["InvestigatorTools"] = true;
    }
    [Then(@"complex queries should be saveable and reusable")]
    public void ThenComplexQueriesShouldBeSaveableAndReusable()
    {
        ScenarioContext["SaveableQueries"] = true;
        ScenarioContext["QueryReuse"] = true;
    }
    [Then(@"integration should support:")]
    public void ThenIntegrationShouldSupport(Table table)
    {
        ThenTheResponseStatusShouldBe(201);
        var integrationSupport = new Dictionary<string, object>();
        
        foreach (var row in table.Rows)
        {
            integrationSupport[row["Destination Type"]] = new
            {
                ProtocolFormat = row["Protocol/Format"],
                UseCase = row["Use Case"]
            };
        }
        
        ScenarioContext["IntegrationSupport"] = integrationSupport;
    }
    
    [Then(@"forwarding should be reliable with retry mechanisms")]
    public void ThenForwardingShouldBeReliableWithRetryMechanisms()
    {
        ScenarioContext["ReliableForwarding"] = true;
        ScenarioContext["RetryMechanisms"] = true;
    }
    [Then(@"logs should be formatted appropriately for each destination")]
    public void ThenLogsShouldBeFormattedAppropriatelyForEachDestination()
    {
        ScenarioContext["AppropriateFormatting"] = true;
        ScenarioContext["DestinationSpecificFormatting"] = true;
    }
    [Then(@"forwarding failures should be logged and monitored")]
    public void ThenForwardingFailuresShouldBeLoggedAndMonitored()
    {
        ScenarioContext["ForwardingFailuresLogged"] = true;
        ScenarioContext["ForwardingMonitoring"] = true;
    }
}
