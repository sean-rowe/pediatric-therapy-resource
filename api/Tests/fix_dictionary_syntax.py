#!/usr/bin/env python3
import os
import re

def fix_dictionary_and_array_syntax(file_path):
    """Fix missing closing braces and semicolons in dictionaries and arrays."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed = False
        i = 0
        
        while i < len(lines):
            line = lines[i]
            
            # Check if this line looks like the last item in a dictionary/array initialization
            # and the next line is a method attribute
            if i + 1 < len(lines):
                current_line = line.rstrip()
                next_line = lines[i + 1].strip()
                
                # Pattern: Line contains "] = " or ends with a value, and next line is a method
                if ('] = ' in current_line or 
                    (current_line and not current_line.endswith('{') and 
                     not current_line.endswith('}') and 
                     not current_line.endswith(';') and
                     not current_line.endswith(','))):
                    
                    if (next_line.startswith('[Given(') or 
                        next_line.startswith('[When(') or 
                        next_line.startswith('[Then(') or
                        next_line.startswith('public ')):
                        
                        # Check indentation to see if we need closing braces
                        indent = len(line) - len(line.lstrip())
                        
                        # Add closing brace and semicolon
                        lines[i] = current_line + '\n' + ' ' * indent + '};\n'
                        fixed = True
            
            i += 1
        
        if fixed:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(lines)
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
        if fix_dictionary_and_array_syntax(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()