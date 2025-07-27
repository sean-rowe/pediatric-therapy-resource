#!/usr/bin/env python3
import os
import re

def fix_final_syntax_errors(filepath):
    """Fix remaining syntax errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Missing closing parenthesis in method calls
    # Pattern: Dictionary<string, object> { { "code", code } }
    # Should be: Dictionary<string, object> { { "code", code } })
    content = re.sub(
        r'(new Dictionary<string, object>\s*\{[^}]+\}\s*)(\n\s*\})',
        r'\1);\2',
        content
    )
    
    # Fix 2: Fix dictionary syntax with missing closing parenthesis
    content = re.sub(
        r'(\}\s*)\n(\s*\})(\s*\[(?:When|Then|Given))',
        r'\1);\n\2\n\3',
        content
    )
    
    # Fix 3: Fix malformed dictionary continuation
    # Pattern where dictionary continues on next line without proper syntax
    content = re.sub(
        r'(\["[^"]+"\]\s*=\s*[^,\n]+),?\s*\n\s*\},\s*\n\s*(\[")',
        r'\1\n            },\n            \2',
        content
    )
    
    # Fix 4: Fix switch expressions missing semicolon
    content = re.sub(
        r'(_ => "[^"]+")(\s*\n\s*\}\s*\n\s*\})',
        r'\1\n        };\n    }',
        content
    )
    
    # Fix 5: Fix method calls with missing closing parenthesis
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Check for patterns like:
        # new Dictionary<string, object> { { "code", code } }
        # without closing parenthesis
        if 'new Dictionary<string, object>' in line and line.strip().endswith('}'):
            if not line.strip().endswith('});') and not line.strip().endswith('})'):
                line = line.rstrip() + ');'
        
        # Check for incomplete method calls
        if i + 1 < len(lines):
            next_line = lines[i + 1]
            # If current line ends with } and next line starts with [When/Then/Given
            if line.strip().endswith('}') and re.match(r'\s*\[(?:When|Then|Given)', next_line):
                # Check if this looks like end of a method call
                if 'await' in lines[max(0, i-5):i] or 'WhenISend' in lines[max(0, i-5):i]:
                    if not line.strip().endswith('});'):
                        line = line.rstrip()[:-1] + '});'
        
        fixed_lines.append(line)
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # Fix 6: Remove extra closing braces at end of classes
    content = re.sub(r'(\}\s*\n)\s*\}\s*\n\s*\}\s*$', r'\1}', content)
    
    # Fix 7: Ensure files end with proper newline
    content = content.rstrip() + '\n'
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Target specific files with known issues
    target_files = [
        "CommunityFeaturesSteps.cs",
        "CommunicationStepDefinitions.cs",
        "ComplianceStepDefinitions.cs",
        "CommonUISteps.cs",
        "ComplianceAuditSteps.cs"
    ]
    
    fixed_files = []
    
    for filename in target_files:
        filepath = os.path.join(step_def_dir, filename)
        if os.path.exists(filepath):
            if fix_final_syntax_errors(filepath):
                fixed_files.append(filename)
    
    # Then fix all other files
    for filename in os.listdir(step_def_dir):
        if filename.endswith('.cs') and filename not in target_files:
            filepath = os.path.join(step_def_dir, filename)
            if fix_final_syntax_errors(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()