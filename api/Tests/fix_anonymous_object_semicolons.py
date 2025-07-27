#!/usr/bin/env python3
import os
import re

def fix_anonymous_object_semicolons(file_path):
    """Fix missing semicolons after anonymous object initializers."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Pattern to find anonymous object closing braces that need semicolons
        # Look for patterns like:
        # SomeProperty = value
        # }
        # var json = 
        pattern = r'(\n\s*[A-Za-z_][A-Za-z0-9_]*\s*=\s*[^,\n]+\n\s*\})\n(\s*var\s+\w+\s*=)'
        
        # Replace with semicolon after closing brace
        fixed_content = re.sub(pattern, r'\1;\n\2', content)
        
        # Also fix cases where anonymous object is followed by method calls
        # Pattern for:
        # Property = value
        # }
        # .SomeMethod() or response.StatusCode
        pattern2 = r'(\n\s*[A-Za-z_][A-Za-z0-9_]*\s*=\s*[^,\n]+\n\s*\})\n(\s*[a-zA-Z_][\w\.]*\s*[\(\.=])'
        fixed_content = re.sub(pattern2, r'\1;\n\2', fixed_content)
        
        # Fix cases where the closing brace is on its own line but missing semicolon
        # and followed by variable declaration or other code
        lines = fixed_content.split('\n')
        fixed_lines = []
        
        i = 0
        while i < len(lines):
            line = lines[i]
            stripped = line.strip()
            
            # If this line is just a closing brace
            if stripped == '}':
                # Check if this is likely the end of an anonymous object
                # by looking at previous lines for property assignments
                looks_like_anonymous_object = False
                j = i - 1
                while j >= 0 and j > i - 10:  # Look back up to 10 lines
                    prev_line = lines[j].strip()
                    if re.match(r'^[A-Za-z_][A-Za-z0-9_]*\s*=\s*.+', prev_line):
                        # Found a property assignment
                        looks_like_anonymous_object = True
                        break
                    elif prev_line == '' or prev_line.startswith('//'):
                        # Empty line or comment, keep looking
                        j -= 1
                        continue
                    elif prev_line == '{' or 'new' in prev_line:
                        # Found the opening, this is likely anonymous object
                        looks_like_anonymous_object = True
                        break
                    else:
                        break
                    j -= 1
                
                # Check next line to see if semicolon is needed
                if looks_like_anonymous_object and i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    # If next line starts with var, response, or looks like code continuation
                    if (next_line.startswith('var ') or 
                        next_line.startswith('response') or
                        next_line.startswith('content') or
                        re.match(r'^[a-zA-Z_][\w\.]*[\s\(=]', next_line) or
                        next_line == '}'):
                        fixed_lines.append(line + ';')
                    else:
                        fixed_lines.append(line)
                else:
                    fixed_lines.append(line)
            else:
                fixed_lines.append(line)
            i += 1
        
        fixed_content = '\n'.join(fixed_lines)
        
        if content != fixed_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(fixed_content)
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
        if fix_anonymous_object_semicolons(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()