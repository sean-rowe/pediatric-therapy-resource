#!/usr/bin/env python3
import os
import re

def fix_missing_semicolons(file_path):
    """Fix missing semicolons after closing braces in various contexts."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        fixed = False
        
        # Pattern 1: Missing semicolon after array/object initializer closing brace
        # Matches patterns like: } (newline or space) (not followed by semicolon, comma, or another closing brace)
        pattern1 = r'(\}\s*\n\s*)(?![;,\}])'
        
        # Check if the next line doesn't start with a method/property declaration or closing brace
        lines = content.split('\n')
        new_lines = []
        
        for i, line in enumerate(lines):
            new_lines.append(line)
            
            # Check if this line ends with a closing brace and is likely an initializer
            if line.strip().endswith('}') and not line.strip().endswith('};'):
                # Look at the next non-empty line
                next_line_idx = i + 1
                while next_line_idx < len(lines) and not lines[next_line_idx].strip():
                    next_line_idx += 1
                
                if next_line_idx < len(lines):
                    next_line = lines[next_line_idx].strip()
                    
                    # Check if this looks like the end of an initializer
                    # (next line doesn't start with typical continuation patterns)
                    if (not next_line.startswith(('public', 'private', 'protected', 'internal', 
                                                 '[', '}', 'else', 'catch', 'finally')) and
                        not next_line.startswith('//') and
                        next_line != '' and
                        # Check if the current line is inside an initializer context
                        any(keyword in lines[max(0, i-10):i+1].__str__() for keyword in 
                            ['= new', '= {', 'new[]', 'new Dictionary', 'new List'])):
                        
                        # Add semicolon
                        new_lines[-1] = line.rstrip() + ';'
                        fixed = True
        
        if fixed:
            new_content = '\n'.join(new_lines)
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
            if fix_missing_semicolons(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()