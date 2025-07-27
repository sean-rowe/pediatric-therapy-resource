#!/usr/bin/env python3
import re

file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/ComplianceAuditSteps.cs'

with open(file_path, 'r') as f:
    content = f.read()

# Find the ComplianceViolation class and fix it
# First add the missing closing brace after Timestamp property
content = re.sub(
    r'(public DateTime Timestamp { get; set; })\s*\n\s*#endregion',
    r'\1\n    }\n\n    #endregion',
    content
)

# Remove all the extra closing braces at the end
# Keep only one closing brace for the namespace
content = re.sub(
    r'(#endregion)\s*\n}\s*\n}\s*(?:\n}\s*)*$',
    r'\1\n}',
    content
)

with open(file_path, 'w') as f:
    f.write(content)

print("Fixed ComplianceAuditSteps.cs")