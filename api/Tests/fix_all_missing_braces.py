#!/usr/bin/env python3
import os
import re

def fix_missing_method_closing_braces(file_path):
    """Fix all missing closing braces in methods."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    fixed = False
    i = 0
    while i < len(lines):
        line = lines[i].strip()
        
        # Check if this line has NotImplementedException without closing brace
        if 'throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");' in line:
            # Check if next non-empty line is a comment or attribute or another method
            j = i + 1
            while j < len(lines) and lines[j].strip() == '':
                j += 1
            
            if j < len(lines):
                next_line = lines[j].strip()
                # If next line is a comment, attribute, or method declaration, we need a closing brace
                if (next_line.startswith('//') or 
                    next_line.startswith('[') or 
                    next_line.startswith('public') or
                    next_line.startswith('private') or
                    next_line.startswith('protected')):
                    # Insert closing brace after the throw statement
                    lines.insert(i + 1, '    }\n')
                    fixed = True
                    i += 1  # Skip the inserted line
        i += 1
    
    if fixed:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(lines)
    
    return fixed

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_missing_method_closing_braces(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files with missing method closing braces:")
for file in sorted(fixed_files):
    print(f"  - {file}")