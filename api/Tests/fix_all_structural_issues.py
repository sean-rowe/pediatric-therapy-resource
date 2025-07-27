#!/usr/bin/env python3
import os
import re

def count_braces(content):
    """Count opening and closing braces in content."""
    open_count = content.count('{')
    close_count = content.count('}')
    return open_count, close_count

def fix_file_structure(file_path):
    """Fix all structural issues in a file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        
        # Remove all trailing braces from the end of the file
        while lines and lines[-1].strip() in ['}', '}}', '}}}', '}}}}', '}}}}}', '}}}}}}']:
            lines.pop()
        
        # Count braces in the remaining content
        remaining_content = '\n'.join(lines)
        open_count, close_count = count_braces(remaining_content)
        
        # Find where the namespace starts
        namespace_line = -1
        for i, line in enumerate(lines):
            if line.startswith('namespace '):
                namespace_line = i
                break
        
        # Count braces up to the end of the last class/method
        last_class_end = len(lines) - 1
        for i in range(len(lines) - 1, -1, -1):
            if '#endregion' in lines[i] or (lines[i].strip() == '}' and i > 0 and 'class' in lines[i-1]):
                last_class_end = i
                break
        
        # Calculate how many closing braces we need
        # Typically: 1 for namespace, 1 for class, maybe 1-2 for nested structures
        content_up_to_end = '\n'.join(lines[:last_class_end+1])
        open_up_to_end, close_up_to_end = count_braces(content_up_to_end)
        
        missing_braces = open_up_to_end - close_up_to_end
        
        # Add the missing closing braces
        if missing_braces > 0:
            # Add empty line before closing braces if not present
            if lines and lines[-1].strip() != '':
                lines.append('')
            
            # Add closing braces
            for _ in range(missing_braces):
                lines.append('}')
        
        # Write back
        new_content = '\n'.join(lines)
        if new_content != content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Get all .cs files
    cs_files = []
    for root, dirs, files in os.walk(step_definitions_dir):
        for file in files:
            if file.endswith('.cs'):
                cs_files.append(os.path.join(root, file))
    
    print(f"Found {len(cs_files)} C# files to check")
    
    fixed_count = 0
    for file_path in sorted(cs_files):
        if fix_file_structure(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()