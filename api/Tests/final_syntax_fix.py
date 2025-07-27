#!/usr/bin/env python3
import os

def count_braces(content):
    """Count opening and closing braces."""
    open_count = 0
    close_count = 0
    in_string = False
    escape_next = False
    
    for i, char in enumerate(content):
        if escape_next:
            escape_next = False
            continue
            
        if char == '\\':
            escape_next = True
            continue
            
        if char == '"' and (i == 0 or content[i-1] != '@'):
            in_string = not in_string
            
        if not in_string:
            if char == '{':
                open_count += 1
            elif char == '}':
                close_count += 1
                
    return open_count, close_count

def fix_file_structure(file_path):
    """Fix various structural issues in the file."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Count braces
    open_count, close_count = count_braces(content)
    
    # If we have more opening braces than closing, add closing braces at the end
    if open_count > close_count:
        # Find the last non-empty line
        lines = content.split('\n')
        last_content_line = -1
        for i in range(len(lines) - 1, -1, -1):
            if lines[i].strip():
                last_content_line = i
                break
        
        # Add the missing closing braces
        missing = open_count - close_count
        for _ in range(missing):
            lines.insert(last_content_line + 1, '}')
        
        content = '\n'.join(lines)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
        
    return False

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_file_structure(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files with structural issues:")
for file in sorted(fixed_files):
    print(f"  - {file}")