#!/usr/bin/env python3

def fix_wcag_final():
    """Final comprehensive fix for WcagAccessibilitySteps.cs"""
    
    file_path = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/WcagAccessibilitySteps.cs"
    
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Fix specific issues:
    # 1. Line 230 (index 229) - has extra closing brace with wrong indentation
    if lines[229].strip() == '}':
        lines[229] = '    }\n'
    
    # 2. Line 258 (index 257) needs closing brace for foreach and method
    if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[257]:
        lines[257] = lines[257].rstrip() + '\n        }\n    }\n'
    
    # 3. Line 273 (index 272) needs closing brace
    if 'issueTracking?.ProgressMonitored.Should().BeTrue();' in lines[272]:
        lines[272] = lines[272].rstrip() + '\n    }\n'
    
    # 4. Line 287 (index 286) needs closing brace  
    if 'remediationPlans?.ResourcesAllocated.Should().BeTrue();' in lines[286]:
        lines[286] = lines[286].rstrip() + '\n    }\n'
    
    # 5. Line 325 (index 324) needs closing braces for foreach and method
    if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[324]:
        lines[324] = lines[324].rstrip() + '\n        }\n    }\n'
    
    # 6. Line 345 (index 344) needs closing brace
    if 'feedbackProcessing?.UserSatisfactionImproved.Should().BeTrue();' in lines[344]:
        lines[344] = lines[344].rstrip() + '\n    }\n'
    
    # 7. Line 380 (index 379) needs closing braces for foreach and method
    if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[379]:
        lines[379] = lines[379].rstrip() + '\n        }\n    }\n'
    
    # 8. Line 394 (index 393) needs closing brace
    if 'issuePrioritization?.IssueResolutionTracked.Should().BeTrue();' in lines[393]:
        lines[393] = lines[393].rstrip() + '\n    }\n'
    
    # 9. Line 406 (index 405) needs closing brace
    if 'complianceStatusMaintenance?.StakeholderNotification.Should().BeTrue();' in lines[405]:
        lines[405] = lines[405].rstrip() + '\n    }\n'
    
    # Write back
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    
    print("Applied final fixes to WcagAccessibilitySteps.cs")

if __name__ == "__main__":
    fix_wcag_final()