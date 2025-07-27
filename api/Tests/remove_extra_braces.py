#!/usr/bin/env python3
import os

def remove_extra_closing_braces(file_path):
    """Remove extra consecutive closing braces at the end of files."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Find the last non-brace line
    last_content_line = -1
    for i in range(len(lines) - 1, -1, -1):
        line = lines[i].strip()
        if line and line != '}':
            last_content_line = i
            break
    
    if last_content_line == -1:
        return False
    
    # Count closing braces after last content
    closing_braces = []
    for i in range(last_content_line + 1, len(lines)):
        if lines[i].strip() == '}':
            closing_braces.append(i)
    
    # We should only have 2 closing braces (class and namespace)
    if len(closing_braces) > 2:
        # Keep only the last 2 closing braces
        to_remove = closing_braces[:-2]
        for i in reversed(to_remove):
            lines.pop(i)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(lines)
        return True
    
    return False

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if remove_extra_closing_braces(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files by removing extra closing braces:")
for file in sorted(fixed_files):
    print(f"  - {file}")