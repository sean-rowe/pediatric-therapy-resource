#!/usr/bin/env python3
import os
import re

def fix_wcag_structure(file_path):
    """Fix specific structural issues in WcagAccessibilitySteps.cs."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        # Fix specific issues:
        # 1. Line 111 (index 110) - Remove extra closing brace
        if lines[110].strip() == '}':
            lines[110] = ''
        
        # 2. Add closing brace after line 127 (index 126)
        if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[126]:
            lines[126] = lines[126].rstrip() + '\n        }\n    }\n'
        
        # 3. Add closing braces after lines with missing ones
        # Line 180 (index 179)
        if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[179] and not lines[180].strip().startswith('}'):
            lines[179] = lines[179].rstrip() + '\n        }\n    }\n'
        
        # Line 207 (index 206) 
        if 'remediationPlans?.ResourcesAllocated.Should().BeTrue();' in lines[207] and not lines[208].strip().startswith('}'):
            lines[207] = lines[207].rstrip() + '\n    }\n'
        
        # Fix the orphaned code at lines 218-225 by making it part of a method
        # Find line that starts with '        }'
        for i in range(217, 226):
            if i < len(lines) and lines[i].strip() == '}':
                # Remove this orphaned closing brace
                lines[i] = ''
            elif i < len(lines) and lines[i].strip() == '};':
                # This is orphaned code - remove it
                lines[i] = ''
            elif i < len(lines) and 'var json = JsonSerializer.Serialize(userTesting);' in lines[i]:
                # This whole block is orphaned - remove it
                for j in range(i-1, min(i+5, len(lines))):
                    if j >= 0:
                        lines[j] = ''
        
        # Line 254 (index 253)
        if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[253] and not lines[254].strip().startswith('}'):
            lines[253] = lines[253].rstrip() + '\n        }\n    }\n'
        
        # Line 307 (index 306)
        if 'response.StatusCode.Should().Be(HttpStatusCode.Created);' in lines[306] and not lines[307].strip().startswith('}'):
            lines[306] = lines[306].rstrip() + '\n        }\n    }\n'
        
        # Line 321 (index 320)
        if 'issuePrioritization?.IssueResolutionTracked.Should().BeTrue();' in lines[320] and not lines[321].strip().startswith('}'):
            lines[320] = lines[320].rstrip() + '\n    }\n'
        
        # Write the fixed content
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(lines)
        
        return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    file_path = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/WcagAccessibilitySteps.cs"
    
    if fix_wcag_structure(file_path):
        print("Fixed WcagAccessibilitySteps.cs structure")
    else:
        print("Failed to fix WcagAccessibilitySteps.cs")

if __name__ == "__main__":
    main()