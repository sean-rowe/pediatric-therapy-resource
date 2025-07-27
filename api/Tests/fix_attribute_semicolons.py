#!/usr/bin/env python3
import os
import re

def fix_attribute_semicolons(file_path):
    """Remove incorrect semicolons after attributes and fix structure."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i]
            
            # Remove }; after class declaration
            if line.strip() == '};' and i > 0:
                prev_line = lines[i-1].strip()
                if (prev_line.startswith('[Binding]') or 
                    prev_line.startswith('public class') or
                    prev_line.startswith('[Given(') or
                    prev_line.startswith('[When(') or
                    prev_line.startswith('[Then(')):
                    # Skip this line entirely
                    i += 1
                    continue
            
            # Remove }; pattern but keep the line if it has other content
            if '};' in line and not line.strip().endswith('});'):
                line = line.replace('};', '')
            
            fixed_lines.append(line)
            i += 1
        
        # Write back
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(fixed_lines)
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
        if fix_attribute_semicolons(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()