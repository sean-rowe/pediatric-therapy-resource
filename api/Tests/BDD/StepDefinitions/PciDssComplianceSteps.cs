using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace UPTRMS.Api.Tests.BDD.StepDefinitions;

/// <summary>
/// Step definitions for comprehensive PCI DSS compliance scenarios
/// These tests will FAIL initially (RED phase) until PCI DSS compliance services are implemented
/// </summary>
[Binding]
public class PciDssComplianceSteps : BaseStepDefinitions
{
    private readonly Dictionary<string, object> _pciDssContext = new();
    private HttpResponseMessage? _lastResponse;
    private List<PaymentCardData> _paymentCardData = new();
    private List<SecurityIncident> _securityIncidents = new();
    private string _merchantId = string.Empty;
    private string _transactionId = string.Empty;

    public PciDssComplianceSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext) 
        : base(factory, scenarioContext)
    {
    }

    #region Background Steps
    
    [Given(@"PCI DSS compliance systems are operational")]
    public async Task GivenPciDssComplianceSystemsAreOperational()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [Given(@"payment card data security measures are implemented")]
    public async Task GivenPaymentCardDataSecurityMeasuresAreImplemented()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [Given(@"secure network infrastructure is configured")]
    public async Task GivenSecureNetworkInfrastructureIsConfigured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [Given(@"vulnerability management programs are active")]
    public async Task GivenVulnerabilityManagementProgramsAreActive()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [Given(@"access control measures are enforced")]
    public async Task GivenAccessControlMeasuresAreEnforced()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    #endregion

    #region Payment Card Data Protection Steps

    [Given(@"PCI DSS requires protection of payment card data")]
    public async Task GivenPciDssRequiresProtectionOfPaymentCardData()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [Given(@"cardholder data environment must be secured")]
    public async Task GivenCardholderDataEnvironmentMustBeSecured()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [When(@"implementing payment card data security:")]
    public async Task WhenImplementingPaymentCardDataSecurity(Table table)
    {
        // This will FAIL initially - no payment card data security implementation service implemented yet
        foreach (var row in table.Rows)
        {
            var securityControl = row["Security Control"];
            var implementationMethod = row["Implementation Method"];
            var encryptionStandard = row["Encryption Standard"];
            var accessRestrictions = row["Access Restrictions"];
            var auditRequirements = row["Audit Requirements"];
            var complianceValidation = row["Compliance Validation"];

            var cardDataSecurity = new
            {
                SecurityControl = securityControl,
                ImplementationMethod = implementationMethod,
                EncryptionStandard = encryptionStandard,
                AccessRestrictions = accessRestrictions,
                AuditRequirements = auditRequirements,
                ComplianceValidation = complianceValidation,
                ImplementationTimestamp = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(cardDataSecurity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/compliance/pci-dss/card-data-security/implement", content);
            
            // This will fail because card data security implementation doesn't exist yet
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Then(@"payment card data should be encrypted")]
    public async Task ThenPaymentCardDataShouldBeEncrypted()
    {
        // This will FAIL initially - no payment card data encryption validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/card-data-security/encryption-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var encryptionValidation = JsonSerializer.Deserialize<PaymentCardDataEncryptionValidation>(content);
        encryptionValidation?.DataEncrypted.Should().BeTrue();
        encryptionValidation?.EncryptionStandard.Should().Be("AES-256");
        encryptionValidation?.KeyManagementSecure.Should().BeTrue();
    }

    [Then(@"access should be restricted to authorized personnel")]
    public async Task ThenAccessShouldBeRestrictedToAuthorizedPersonnel()
    {
        // This will FAIL initially - no access restriction validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/access-controls/restriction-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var accessRestriction = JsonSerializer.Deserialize<PaymentCardDataAccessRestriction>(content);
        accessRestriction?.AccessRestricted.Should().BeTrue();
        accessRestriction?.OnlyAuthorizedPersonnel.Should().BeTrue();
        accessRestriction?.BusinessJustificationRequired.Should().BeTrue();
    }

    [Then(@"cardholder data environment should be protected")]
    public async Task ThenCardholderDataEnvironmentShouldBeProtected()
    {
        // This will FAIL initially - no CDE protection validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/cardholder-data-environment/protection-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var cdeProtection = JsonSerializer.Deserialize<CardholderDataEnvironmentProtection>(content);
        cdeProtection?.EnvironmentProtected.Should().BeTrue();
        cdeProtection?.SecurityControlsActive.Should().BeTrue();
        cdeProtection?.NetworkIsolation.Should().BeTrue();
    }

    [Then(@"security monitoring should be continuous")]
    public async Task ThenSecurityMonitoringShouldBeContinuous()
    {
        // This will FAIL initially - no continuous security monitoring validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/security-monitoring/continuous-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var continuousMonitoring = JsonSerializer.Deserialize<ContinuousSecurityMonitoring>(content);
        continuousMonitoring?.MonitoringContinuous.Should().BeTrue();
        continuousMonitoring?.RealTimeAlerts.Should().BeTrue();
        continuousMonitoring?.MonitoringCoverage.Should().Be(1.0m); // 100% coverage
    }

    #endregion

    #region Network Security Steps

    [Given(@"secure network and systems must be maintained")]
    public async Task GivenSecureNetworkAndSystemsMustBeMaintained()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [When(@"configuring network security controls:")]
    public async Task WhenConfiguringNetworkSecurityControls(Table table)
    {
        // This will FAIL initially - no network security controls configuration service implemented yet
        foreach (var row in table.Rows)
        {
            var securityControl = row["Security Control"];
            var configurationMethod = row["Configuration Method"];
            var protectionLevel = row["Protection Level"];
            var monitoringCapability = row["Monitoring Capability"];
            var maintenanceSchedule = row["Maintenance Schedule"];

            var networkSecurity = new
            {
                SecurityControl = securityControl,
                ConfigurationMethod = configurationMethod,
                ProtectionLevel = protectionLevel,
                MonitoringCapability = monitoringCapability,
                MaintenanceSchedule = maintenanceSchedule,
                ConfigurationTimestamp = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(networkSecurity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/compliance/pci-dss/network-security/configure", content);
            
            // This will fail because network security configuration doesn't exist yet
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Then(@"network security should meet PCI DSS requirements")]
    public async Task ThenNetworkSecurityShouldMeetPciDssRequirements()
    {
        // This will FAIL initially - no PCI DSS network security requirements validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/network-security/requirements-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var requirementsValidation = JsonSerializer.Deserialize<PciDssNetworkSecurityRequirementsValidation>(content);
        requirementsValidation?.RequirementsMet.Should().BeTrue();
        requirementsValidation?.FirewallsConfigured.Should().BeTrue();
        requirementsValidation?.NetworkSegmented.Should().BeTrue();
    }

    #endregion

    #region Vulnerability Management Steps
    
    [Given(@"vulnerability management programs are required")]
    public async Task GivenVulnerabilityManagementProgramsAreRequired()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [When(@"implementing vulnerability management:")]
    public async Task WhenImplementingVulnerabilityManagement(Table table)
    {
        // This will FAIL initially - no vulnerability management implementation service implemented yet
        foreach (var row in table.Rows)
        {
            var managementComponent = row["Management Component"];
            var implementationMethod = row["Implementation Method"];
            var frequency = row["Frequency"];
            var remediationProcess = row["Remediation Process"];
            var reportingMechanism = row["Reporting Mechanism"];

            var vulnerabilityManagement = new
            {
                ManagementComponent = managementComponent,
                ImplementationMethod = implementationMethod,
                Frequency = frequency,
                RemediationProcess = remediationProcess,
                ReportingMechanism = reportingMechanism,
                ImplementationTimestamp = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(vulnerabilityManagement);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/compliance/pci-dss/vulnerability-management/implement", content);
            
            // This will fail because vulnerability management implementation doesn't exist yet
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Then(@"vulnerability scans should be performed regularly")]
    public async Task ThenVulnerabilityScansShouldBePerformedRegularly()
    {
        // This will FAIL initially - no vulnerability scan frequency validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/vulnerability-management/scan-frequency-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var scanFrequency = JsonSerializer.Deserialize<VulnerabilityScanFrequency>(content);
        scanFrequency?.ScansPerformedRegularly.Should().BeTrue();
        scanFrequency?.QuarterlyScanningActive.Should().BeTrue();
        scanFrequency?.AfterSignificantChanges.Should().BeTrue();
    }

    [Then(@"antivirus software should be deployed")]
    public async Task ThenAntivirusSoftwareShouldBeDeployed()
    {
        // This will FAIL initially - no antivirus deployment validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/vulnerability-management/antivirus-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var antivirusValidation = JsonSerializer.Deserialize<AntivirusDeploymentValidation>(content);
        antivirusValidation?.AntivirusDeployed.Should().BeTrue();
        antivirusValidation?.DefinitionsUpToDate.Should().BeTrue();
        antivirusValidation?.RealTimeProtection.Should().BeTrue();
    }

    #endregion

    #region Security Incident Response Steps

    [Given(@"a security incident affecting payment systems is detected")]
    public async Task GivenASecurityIncidentAffectingPaymentSystemsIsDetected()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [When(@"PCI DSS incident response is initiated:")]
    public async Task WhenPciDssIncidentResponseIsInitiated(Table table)
    {
        // This will FAIL initially - no PCI DSS incident response service implemented yet
        foreach (var row in table.Rows)
        {
            var responseAction = row["Response Action"];
            var timeline = row["Timeline"];
            var responsibleParty = row["Responsible Party"];
            var notificationRequirement = row["Notification Requirement"];
            var containmentMeasure = row["Containment Measure"];

            var incidentResponse = new
            {
                ResponseAction = responseAction,
                Timeline = timeline,
                ResponsibleParty = responsibleParty,
                NotificationRequirement = notificationRequirement,
                ContainmentMeasure = containmentMeasure,
                ResponseInitiatedAt = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(incidentResponse);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/compliance/pci-dss/security-incidents/response", content);
            
            // This will fail because incident response service doesn't exist yet
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Then(@"incident should be contained immediately")]
    public async Task ThenIncidentShouldBeContainedImmediately()
    {
        // This will FAIL initially - no incident containment validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/security-incidents/containment-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var containmentValidation = JsonSerializer.Deserialize<IncidentContainmentValidation>(content);
        containmentValidation?.IncidentContained.Should().BeTrue();
        containmentValidation?.ContainmentImmediate.Should().BeTrue();
        containmentValidation?.SpreadPrevented.Should().BeTrue();
    }

    [Then(@"forensic investigation should be conducted")]
    public async Task ThenForensicInvestigationShouldBeConducted()
    {
        // This will FAIL initially - no forensic investigation validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/security-incidents/forensic-investigation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var forensicInvestigation = JsonSerializer.Deserialize<ForensicInvestigationValidation>(content);
        forensicInvestigation?.InvestigationConducted.Should().BeTrue();
        forensicInvestigation?.EvidencePreserved.Should().BeTrue();
        forensicInvestigation?.RootCauseIdentified.Should().BeTrue();
    }

    [Then(@"payment brands should be notified if required")]
    public async Task ThenPaymentBrandsShouldBeNotifiedIfRequired()
    {
        // This will FAIL initially - no payment brand notification validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/security-incidents/payment-brand-notification");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var brandNotification = JsonSerializer.Deserialize<PaymentBrandNotificationValidation>(content);
        brandNotification?.NotificationRequired.Should().BeTrue();
        brandNotification?.NotificationTimely.Should().BeTrue();
        brandNotification?.RequiredInformationProvided.Should().BeTrue();
    }

    #endregion

    #region Compliance Assessment Steps

    [Given(@"PCI DSS compliance assessment is required")]
    public async Task GivenPciDssComplianceAssessmentIsRequired()
    {
        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");
    }

    [When(@"conducting PCI DSS compliance assessment:")]
    public async Task WhenConductingPciDssComplianceAssessment(Table table)
    {
        // This will FAIL initially - no PCI DSS compliance assessment service implemented yet
        foreach (var row in table.Rows)
        {
            var assessmentComponent = row["Assessment Component"];
            var complianceStatus = row["Compliance Status"];
            var evidenceRequired = row["Evidence Required"];
            var testingMethod = row["Testing Method"];
            var validationProcess = row["Validation Process"];

            var complianceAssessment = new
            {
                AssessmentComponent = assessmentComponent,
                ComplianceStatus = complianceStatus,
                EvidenceRequired = evidenceRequired,
                TestingMethod = testingMethod,
                ValidationProcess = validationProcess,
                AssessmentTimestamp = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(complianceAssessment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/compliance/pci-dss/assessment/conduct", content);
            
            // This will fail because compliance assessment doesn't exist yet
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Then(@"all PCI DSS requirements should be met")]
    public async Task ThenAllPciDssRequirementsShouldBeMet()
    {
        // This will FAIL initially - no PCI DSS requirements compliance validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/assessment/requirements-compliance");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var requirementsCompliance = JsonSerializer.Deserialize<PciDssRequirementsCompliance>(content);
        requirementsCompliance?.AllRequirementsMet.Should().BeTrue();
        requirementsCompliance?.ComplianceScore.Should().Be(1.0m); // 100% compliance
        requirementsCompliance?.NoComplianceGaps.Should().BeTrue();
    }

    [Then(@"compliance certificate should be valid")]
    public async Task ThenComplianceCertificateShouldBeValid()
    {
        // This will FAIL initially - no compliance certificate validation service implemented yet
        var response = await Client.GetAsync("/api/compliance/pci-dss/assessment/certificate-validation");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var certificateValidation = JsonSerializer.Deserialize<ComplianceCertificateValidation>(content);
        certificateValidation?.CertificateValid.Should().BeTrue();
        certificateValidation?.CertificateExpiry.Should().BeAfter(DateTime.UtcNow.AddDays(30));
        certificateValidation?.AuditCompleted.Should().BeTrue();
    }

    #endregion

    #region Helper Classes (These represent the expected API models that don't exist yet)

    public class PciDssSystemStatus
    {
        public bool SystemsOperational { get; set; }
        public bool ComplianceActive { get; set; }
        public bool SecurityControlsActive { get; set; }
        public bool AuditingEnabled { get; set; }
        public DateTime LastStatusCheck { get; set; }
    }

    public class PaymentCardDataSecurity
    {
        public bool SecurityMeasuresImplemented { get; set; }
        public bool DataEncrypted { get; set; }
        public bool AccessControlsActive { get; set; }
        public bool NetworkSecurityActive { get; set; }
        public string EncryptionStandard { get; set; } = string.Empty;
    }

    public class PciDssNetworkSecurity
    {
        public bool SecureNetworkConfigured { get; set; }
        public bool FirewallsActive { get; set; }
        public bool NetworkSegmentationImplemented { get; set; }
        public bool SecureTransmissionProtocols { get; set; }
    }

    public class VulnerabilityManagementStatus
    {
        public bool ProgramsActive { get; set; }
        public bool RegularScanning { get; set; }
        public bool AntivirusDeployed { get; set; }
        public bool PatchManagementActive { get; set; }
        public DateTime LastScanDate { get; set; }
    }

    public class PciDssAccessControls
    {
        public bool AccessControlsEnforced { get; set; }
        public bool NeedToKnowBasis { get; set; }
        public bool UniqueUserIds { get; set; }
        public bool RestrictedPhysicalAccess { get; set; }
    }

    public class PaymentCardDataProtectionRequirements
    {
        public bool ProtectionRequired { get; set; }
        public bool DataMinimizationRequired { get; set; }
        public bool EncryptionMandatory { get; set; }
        public bool SecureStorageRequired { get; set; }
        public string[] RequiredControls { get; set; } = Array.Empty<string>();
    }

    public class CardholderDataEnvironmentSecurity
    {
        public bool EnvironmentSecured { get; set; }
        public bool NetworkSegmentation { get; set; }
        public bool AccessRestrictions { get; set; }
        public bool MonitoringActive { get; set; }
        public string[] SecurityMeasures { get; set; } = Array.Empty<string>();
    }

    public class PaymentCardDataEncryptionValidation
    {
        public bool DataEncrypted { get; set; }
        public string EncryptionStandard { get; set; } = string.Empty;
        public bool KeyManagementSecure { get; set; }
        public DateTime LastEncryptionCheck { get; set; }
    }

    public class PaymentCardDataAccessRestriction
    {
        public bool AccessRestricted { get; set; }
        public bool OnlyAuthorizedPersonnel { get; set; }
        public bool BusinessJustificationRequired { get; set; }
        public int AuthorizedPersonnelCount { get; set; }
    }

    public class CardholderDataEnvironmentProtection
    {
        public bool EnvironmentProtected { get; set; }
        public bool SecurityControlsActive { get; set; }
        public bool NetworkIsolation { get; set; }
        public decimal ProtectionLevel { get; set; }
    }

    public class ContinuousSecurityMonitoring
    {
        public bool MonitoringContinuous { get; set; }
        public bool RealTimeAlerts { get; set; }
        public decimal MonitoringCoverage { get; set; }
        public int AlertsToday { get; set; }
    }

    public class SecureNetworkMaintenanceRequirements
    {
        public bool MaintenanceRequired { get; set; }
        public bool FirewallConfiguration { get; set; }
        public bool DefaultSecurityParameters { get; set; }
        public bool NetworkSegmentation { get; set; }
    }

    public class PciDssNetworkSecurityRequirementsValidation
    {
        public bool RequirementsMet { get; set; }
        public bool FirewallsConfigured { get; set; }
        public bool NetworkSegmented { get; set; }
        public decimal ComplianceScore { get; set; }
    }

    public class VulnerabilityManagementRequirements
    {
        public bool ProgramsRequired { get; set; }
        public bool RegularScanningRequired { get; set; }
        public bool AntivirusRequired { get; set; }
        public bool SecureSystemsRequired { get; set; }
    }

    public class VulnerabilityScanFrequency
    {
        public bool ScansPerformedRegularly { get; set; }
        public bool QuarterlyScanningActive { get; set; }
        public bool AfterSignificantChanges { get; set; }
        public DateTime LastScanDate { get; set; }
    }

    public class AntivirusDeploymentValidation
    {
        public bool AntivirusDeployed { get; set; }
        public bool DefinitionsUpToDate { get; set; }
        public bool RealTimeProtection { get; set; }
        public DateTime LastDefinitionUpdate { get; set; }
    }

    public class IncidentContainmentValidation
    {
        public bool IncidentContained { get; set; }
        public bool ContainmentImmediate { get; set; }
        public bool SpreadPrevented { get; set; }
        public TimeSpan ContainmentTime { get; set; }
    }

    public class ForensicInvestigationValidation
    {
        public bool InvestigationConducted { get; set; }
        public bool EvidencePreserved { get; set; }
        public bool RootCauseIdentified { get; set; }
        public string InvestigationStatus { get; set; } = string.Empty;
    }

    public class PaymentBrandNotificationValidation
    {
        public bool NotificationRequired { get; set; }
        public bool NotificationTimely { get; set; }
        public bool RequiredInformationProvided { get; set; }
        public DateTime NotificationDate { get; set; }
    }

    public class PciDssRequirementsCompliance
    {
        public bool AllRequirementsMet { get; set; }
        public decimal ComplianceScore { get; set; }
        public bool NoComplianceGaps { get; set; }
        public string[] ComplianceEvidence { get; set; } = Array.Empty<string>();
    }

    public class ComplianceCertificateValidation
    {
        public bool CertificateValid { get; set; }
        public DateTime CertificateExpiry { get; set; }
        public bool AuditCompleted { get; set; }
        public string CertificateType { get; set; } = string.Empty;
    }

    public class PaymentCardData
    {
        public string CardId { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public DateTime ProcessingDate { get; set; }
        public bool EncryptionStatus { get; set; }
    }

    public class SecurityIncident
    {
        public string IncidentId { get; set; } = string.Empty;
        public string IncidentType { get; set; } = string.Empty;
        public DateTime DetectionTime { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string[] AffectedSystems { get; set; } = Array.Empty<string>();
    }

    #endregion
}