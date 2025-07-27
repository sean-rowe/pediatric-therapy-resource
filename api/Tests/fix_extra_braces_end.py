#!/usr/bin/env python3
import os

def fix_extra_closing_braces_at_end(file_path):
    """Remove extra closing braces at the end of files."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Count consecutive closing braces at the end
    closing_braces_count = 0
    for i in range(len(lines) - 1, -1, -1):
        line = lines[i].strip()
        if line == '}':
            closing_braces_count += 1
        elif line == '':
            continue
        else:
            break
    
    # We should only have 2 closing braces at the end (one for class, one for namespace)
    if closing_braces_count > 2:
        # Remove extra braces
        removed = 0
        new_lines = []
        for i in range(len(lines)):
            if i >= len(lines) - closing_braces_count:
                if lines[i].strip() == '}' and removed < (closing_braces_count - 2):
                    removed += 1
                    continue
            new_lines.append(lines[i])
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        return True
    return False

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_extra_closing_braces_at_end(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files with extra closing braces:")
for file in sorted(fixed_files):
    print(f"  - {file}")