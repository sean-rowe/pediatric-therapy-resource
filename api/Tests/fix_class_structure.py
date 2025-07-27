#!/usr/bin/env python3
import os
import re

def count_braces(content):
    """Count opening and closing braces in content."""
    open_count = content.count('{')
    close_count = content.count('}')
    return open_count, close_count

def fix_class_structure(file_path):
    """Ensure all methods are inside the class definition."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find the last closing brace that should be the class closing brace
    lines = content.split('\n')
    
    # Find where the class starts
    class_start = -1
    for i, line in enumerate(lines):
        if 'public class' in line and ': BaseStepDefinitions' in line:
            class_start = i
            break
    
    if class_start == -1:
        return False
    
    # Count braces to find where class should end
    brace_count = 0
    class_end = -1
    in_class = False
    
    for i in range(class_start, len(lines)):
        line = lines[i]
        for char in line:
            if char == '{':
                brace_count += 1
                in_class = True
            elif char == '}':
                brace_count -= 1
                if in_class and brace_count == 0:
                    class_end = i
                    break
        if class_end != -1:
            break
    
    # Check if there are methods after the class closing brace
    if class_end != -1 and class_end < len(lines) - 1:
        # Look for methods after class end
        has_methods_after = False
        for i in range(class_end + 1, len(lines)):
            if re.search(r'public\s+(void|async Task)', lines[i]):
                has_methods_after = True
                break
        
        if has_methods_after:
            # Move the class closing brace to the end
            # Remove the current closing brace
            if lines[class_end].strip() == '}':
                lines.pop(class_end)
            
            # Add closing brace at the end (before the namespace closing brace)
            # Find the namespace closing brace
            namespace_close = -1
            for i in range(len(lines) - 1, -1, -1):
                if lines[i].strip() == '}':
                    namespace_close = i
                    break
            
            if namespace_close != -1:
                lines.insert(namespace_close, '}')
            
            # Write back
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write('\n'.join(lines))
            return True
    
    return False

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_class_structure(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files:")
for file in sorted(fixed_files):
    print(f"  - {file}")