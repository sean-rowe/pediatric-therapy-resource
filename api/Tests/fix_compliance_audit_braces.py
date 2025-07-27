#!/usr/bin/env python3

file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/ComplianceAuditSteps.cs'

with open(file_path, 'r') as f:
    content = f.read()

# Add missing closing braces for classes
fixes = [
    ('        public bool TransmissionSecurity { get; set; }\n\n    public class SafeguardImplementationResult',
     '        public bool TransmissionSecurity { get; set; }\n    }\n\n    public class SafeguardImplementationResult'),
    
    ('        public bool EscalationProcedures { get; set; }\n\n    public class ComplianceVerificationReport',
     '        public bool EscalationProcedures { get; set; }\n    }\n\n    public class ComplianceVerificationReport'),
    
    ('        public bool CertificationValid { get; set; }\n\n    public class AdministrativeSafeguardsStatus',
     '        public bool CertificationValid { get; set; }\n    }\n\n    public class AdministrativeSafeguardsStatus'),
    
    ('        public bool WorkforceTrainingActive { get; set; }\n\n    public class WorkforceTrainingStatus',
     '        public bool WorkforceTrainingActive { get; set; }\n    }\n\n    public class WorkforceTrainingStatus'),
    
    ('        public int ComplianceAwarenessScore { get; set; }\n\n    public class AdministrativeSafeguardsCheck',
     '        public int ComplianceAwarenessScore { get; set; }\n    }\n\n    public class AdministrativeSafeguardsCheck'),
    
    ('        public bool RecertificationTracked { get; set; }\n\n    public class EnforcementConsistencyReport',
     '        public bool RecertificationTracked { get; set; }\n    }\n\n    public class EnforcementConsistencyReport'),
    
    ('        public DateTime IdentifiedDate { get; set; }\n\n    public class RemediationPlans',
     '        public DateTime IdentifiedDate { get; set; }\n    }\n\n    public class RemediationPlans'),
    
    ('        public int PciDssScore { get; set; }\n\n    public class ComplianceAuditRecord',
     '        public int PciDssScore { get; set; }\n    }\n\n    public class ComplianceAuditRecord'),
]

for old, new in fixes:
    content = content.replace(old, new)

# Add newline at end of file if missing
if not content.endswith('\n'):
    content += '\n'

with open(file_path, 'w') as f:
    f.write(content)

print("Fixed missing closing braces in ComplianceAuditSteps.cs")