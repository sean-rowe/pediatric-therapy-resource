#!/usr/bin/env python3
import os
import re

def fix_excess_closing_braces(file_path):
    """Remove excess closing braces at the end of files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        
        # Remove trailing empty lines and excessive closing braces
        while lines and (lines[-1].strip() == '' or lines[-1].strip() == '}'):
            lines.pop()
        
        # Count braces in the file
        remaining_content = '\n'.join(lines)
        open_count = remaining_content.count('{')
        close_count = remaining_content.count('}')
        
        # Add back the correct number of closing braces
        # Typically we need: 1 for class, 1 for namespace
        missing_braces = open_count - close_count
        
        # Add a blank line before closing braces
        if lines and lines[-1].strip() != '':
            lines.append('')
        
        # Add the correct number of closing braces
        for i in range(missing_braces):
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
    
    print(f"Found {len(cs_files)} C# files to process")
    
    fixed_count = 0
    for file_path in sorted(cs_files):
        if fix_excess_closing_braces(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()