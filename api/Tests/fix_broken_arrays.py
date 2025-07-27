#!/usr/bin/env python3
import os
import re

def fix_broken_arrays(file_path):
    """Fix arrays that are missing closing braces and semicolons."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed_lines = []
        i = 0
        while i < len(lines):
            line = lines[i]
            
            # Check if this line looks like the last element of an array without closing
            if i > 0 and re.match(r'^\s*"[^"]*"\s*$', line.strip()):
                # Look ahead to see if next line is a comment or method definition
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    # If next line is a comment or starts with [ (attribute)
                    if next_line.startswith('//') or next_line.startswith('['):
                        # Check if we're in an unclosed array
                        # Look backwards for array start
                        j = i - 1
                        found_array_start = False
                        brace_count = 0
                        while j >= 0:
                            prev_line = lines[j]
                            if '{' in prev_line:
                                brace_count += prev_line.count('{')
                                brace_count -= prev_line.count('}')
                                if 'new[]' in prev_line or '= new' in prev_line:
                                    found_array_start = True
                                    break
                            if '}' in prev_line:
                                brace_count -= prev_line.count('}')
                                brace_count += prev_line.count('{')
                            j -= 1
                        
                        if found_array_start and brace_count > 0:
                            # Add the missing closing brace and semicolon
                            fixed_lines.append(line.rstrip() + '\n')
                            indent = re.match(r'^(\s*)', line).group(1)
                            # Reduce indent for closing brace
                            if len(indent) >= 4:
                                indent = indent[:-4]
                            fixed_lines.append(indent + '};\n')
                            i += 1
                            continue
                        
            fixed_lines.append(line)
            i += 1
        
        new_content = ''.join(fixed_lines)
        
        if new_content != ''.join(lines):
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_count = 0
    for filename in os.listdir(step_definitions_dir):
        if filename.endswith('.cs'):
            file_path = os.path.join(step_definitions_dir, filename)
            if fix_broken_arrays(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()