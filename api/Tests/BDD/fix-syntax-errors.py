#!/usr/bin/env python3
import os
import re

def fix_file(filepath):
    """Fix syntax errors in step definition files."""
    with open(filepath, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # Fix pattern: }[Given or }[When or }[Then
    # This happens when a method closing brace is directly followed by an attribute
    content = re.sub(r'\}(\[(?:Given|When|Then))', r'}\n    \1', content)
    
    # Fix pattern where method signature appears without proper closure
    # Look for patterns like: "...);[Given" or "...);[When" or "...);[Then"
    content = re.sub(r'\)\;(\[(?:Given|When|Then))', r');\n    }\n    \1', content)
    
    # Fix pattern where we have incomplete method bodies
    # Look for throw statements without closing brace
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        fixed_lines.append(line)
        
        # Check if this line has a throw statement
        if 'throw new NotImplementedException' in line and not line.strip().endswith('}'):
            # Check if next line exists and doesn't have a closing brace
            if i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                if next_line and not next_line.startswith('}') and '[' in next_line:
                    # Insert closing brace
                    fixed_lines.append('    }')
        i += 1
    
    content = '\n'.join(fixed_lines)
    
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
        if fix_file(filepath):
            print(f"Fixed syntax errors in: {os.path.basename(filepath)}")
        else:
            print(f"No changes needed for: {os.path.basename(filepath)}")