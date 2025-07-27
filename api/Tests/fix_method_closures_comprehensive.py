#!/usr/bin/env python3
import os
import re

def fix_method_closures(file_path):
    """Fix methods missing closing braces by analyzing method structure."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        fixed_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i]
            fixed_lines.append(line)
            
            # Check if this line is a method attribute
            if i + 1 < len(lines) and (line.strip().startswith('[Given(') or 
                                       line.strip().startswith('[When(') or 
                                       line.strip().startswith('[Then(')):
                # Look for the method signature on the next line
                method_line_idx = i + 1
                if method_line_idx < len(lines):
                    method_line = lines[method_line_idx]
                    
                    # If it's a method signature (public async Task or public void)
                    if ('public async Task' in method_line or 'public void' in method_line) and '{' in method_line:
                        # Find the opening brace
                        fixed_lines.append(method_line)
                        i = method_line_idx + 1
                        
                        # Track brace depth
                        brace_depth = 1
                        method_body_lines = []
                        
                        # Collect method body
                        while i < len(lines) and brace_depth > 0:
                            current_line = lines[i]
                            
                            # Check if next line is another method attribute
                            is_next_method = False
                            if i + 1 < len(lines):
                                next_line = lines[i + 1].strip()
                                if (next_line.startswith('[Given(') or 
                                    next_line.startswith('[When(') or 
                                    next_line.startswith('[Then(') or
                                    next_line.startswith('#endregion') or
                                    next_line.startswith('public class') or
                                    next_line.startswith('}') and i + 2 < len(lines) and lines[i + 2].strip() == ''):
                                    is_next_method = True
                            
                            # Count braces in current line
                            open_braces = current_line.count('{')
                            close_braces = current_line.count('}')
                            brace_depth += open_braces - close_braces
                            
                            method_body_lines.append(current_line)
                            
                            # If we're at depth 0 or about to hit another method, we're done
                            if brace_depth == 0 or is_next_method:
                                break
                            
                            i += 1
                        
                        # If brace_depth > 0, we need to add closing braces
                        if brace_depth > 0:
                            # Add the collected method body
                            fixed_lines.extend(method_body_lines[:-1] if is_next_method else method_body_lines)
                            
                            # Add missing closing braces
                            for _ in range(brace_depth):
                                fixed_lines.append('    }')
                            
                            # Continue from where we left off
                            if is_next_method:
                                i -= 1  # Back up one line to process the next method
                        else:
                            # Add all the method body lines
                            fixed_lines.extend(method_body_lines)
                        
                        continue
            
            i += 1
        
        # Check if file ends properly
        # Count total braces in the file
        total_open = sum(line.count('{') for line in fixed_lines)
        total_close = sum(line.count('}') for line in fixed_lines)
        
        if total_open > total_close:
            # Add closing braces at the end
            for _ in range(total_open - total_close):
                fixed_lines.append('}')
        
        # Write back
        new_content = '\n'.join(fixed_lines)
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
        if fix_method_closures(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()