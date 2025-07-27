#!/usr/bin/env python3
import os
import re

def fix_missing_braces_after_throw(file_path):
    """Add missing closing braces after throw statements."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    modified = False
    i = 0
    while i < len(lines):
        line = lines[i].strip()
        # Check if line contains throw new NotImplementedException
        if 'throw new NotImplementedException' in line and line.endswith(';'):
            # Check if next line is not a closing brace
            next_line_idx = i + 1
            if next_line_idx < len(lines):
                next_line = lines[next_line_idx].strip()
                if next_line != '}':
                    # Insert a closing brace
                    indent = len(lines[i]) - len(lines[i].lstrip())
                    lines.insert(next_line_idx, ' ' * indent + '}\n')
                    modified = True
                    i += 1  # Skip the inserted line
        i += 1
    
    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(lines)
    
    return modified

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_missing_braces_after_throw(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files by adding missing closing braces after throw statements:")
for file in sorted(fixed_files):
    print(f"  - {file}")