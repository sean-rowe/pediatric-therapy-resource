#!/usr/bin/env python3
import os
import re

def fix_missing_method_closing_braces(file_path):
    """Fix methods that are missing their closing braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed = False
        i = 0
        new_lines = []
        
        while i < len(lines):
            line = lines[i]
            new_lines.append(line)
            
            # Check if next line starts with an attribute without proper closure
            if i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                current_line = line.rstrip()
                
                # Pattern: Current line doesn't end with } and next line is an attribute
                if (not current_line.endswith('}') and 
                    not current_line.strip().startswith('[') and
                    not current_line.strip().startswith('#') and
                    not current_line.strip().startswith('//') and
                    current_line.strip() != '' and
                    (next_line.startswith('[Given(') or 
                     next_line.startswith('[When(') or 
                     next_line.startswith('[Then('))):
                    
                    # Check if we're inside a method (by looking for common method endings)
                    if ('await ' in current_line or 
                        current_line.endswith(';') or
                        current_line.endswith('};') or
                        current_line.endswith('}') or
                        'ScenarioContext[' in current_line or
                        'throw new NotImplementedException' in current_line):
                        # Add closing brace
                        new_lines.append('    }\n')
                        fixed = True
            
            i += 1
        
        if fixed:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(new_lines)
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
        if fix_missing_method_closing_braces(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()