#!/usr/bin/env python3
import os
import re

def remove_unreachable_code(file_path):
    """Remove code after throw statements that's unreachable."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    modified = False
    i = 0
    new_lines = []
    
    while i < len(lines):
        line = lines[i]
        new_lines.append(line)
        
        # Check if this line contains throw new NotImplementedException
        if 'throw new NotImplementedException' in line and line.strip().endswith(';'):
            # Look for the next line
            i += 1
            if i < len(lines):
                next_line = lines[i].strip()
                # If next line is a closing brace, keep it
                if next_line == '}':
                    new_lines.append(lines[i])
                    i += 1
                    
                    # Skip any code until we find a method attribute or #endregion
                    while i < len(lines):
                        current_line = lines[i].strip()
                        if (current_line.startswith('[') or 
                            current_line.startswith('public') or 
                            current_line.startswith('private') or
                            current_line.startswith('protected') or
                            current_line.startswith('#endregion') or
                            current_line == '}'):
                            # We've found the next section, stop skipping
                            break
                        else:
                            # Skip this line (it's unreachable code)
                            modified = True
                            i += 1
                    continue
                else:
                    # No closing brace after throw, add one
                    indent = len(line) - len(line.lstrip())
                    new_lines.append(' ' * indent + '}\n')
                    modified = True
                    
                    # Skip any code until we find a method attribute or #endregion
                    while i < len(lines):
                        current_line = lines[i].strip()
                        if (current_line.startswith('[') or 
                            current_line.startswith('public') or 
                            current_line.startswith('private') or
                            current_line.startswith('protected') or
                            current_line.startswith('#endregion') or
                            current_line == '}'):
                            # We've found the next section, stop skipping
                            break
                        else:
                            # Skip this line (it's unreachable code)
                            i += 1
                    continue
        else:
            i += 1
    
    if modified:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
    
    return modified

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if remove_unreachable_code(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files by removing unreachable code after throw statements:")
for file in sorted(fixed_files):
    print(f"  - {file}")