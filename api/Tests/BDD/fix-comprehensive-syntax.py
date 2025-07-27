#!/usr/bin/env python3
import os
import re

def fix_file_comprehensive(filepath):
    """Fix all syntax errors in step definition files."""
    with open(filepath, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix pattern where closing brace is missing before attribute
    # Look for patterns like: "string literal";[Given or "string literal";[When or "string literal";[Then
    content = re.sub(r'(ScenarioContext\[.*?\]\s*=\s*[^;]+;)(\[(?:Given|When|Then))', r'\1\n    }\n    \2', content)
    
    # Fix pattern where attribute appears on same line after statement
    content = re.sub(r'([;\}])(\[(?:Given|When|Then))', r'\1\n    \2', content)
    
    # Fix incomplete method bodies - ensure throw statements have closing braces
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        fixed_lines.append(line)
        
        # Check if this line has a throw statement
        if 'throw new NotImplementedException' in line and line.strip().endswith(';'):
            # Check if next line exists and has an attribute without a closing brace
            if i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                if next_line and '[' in next_line and (next_line.startswith('[Given') or next_line.startswith('[When') or next_line.startswith('[Then')):
                    # Insert closing brace
                    fixed_lines.append('    }')
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # Clean up any duplicate empty lines
    content = re.sub(r'\n\s*\n\s*\n', '\n\n', content)
    
    # Only write if changes were made
    if content != original_content:
        with open(filepath, 'w') as f:
            f.write(content)
        return True
    return False

# Fix the problematic files
files_to_fix = [
    "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/APIManagementSteps.cs",
    "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/AdvocacyLegalSteps.cs"
]

for filepath in files_to_fix:
    if os.path.exists(filepath):
        if fix_file_comprehensive(filepath):
            print(f"Fixed syntax errors in: {os.path.basename(filepath)}")
        else:
            print(f"No changes needed for: {os.path.basename(filepath)}")