#!/usr/bin/env python3
import os
import re

def fix_final_structural_issues(file_path):
    """Final comprehensive fix for all remaining structural issues."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed_lines = []
        i = 0
        in_array = False
        array_indent = 0
        
        while i < len(lines):
            line = lines[i]
            stripped = line.strip()
            
            # Check if we're entering an array initialization
            if 'new[]' in line or ('{' in line and i > 0 and 'new' in lines[i-1]):
                in_array = True
                array_indent = len(line) - len(line.lstrip())
            
            # Check if this looks like the last item in an array (no comma at end)
            if in_array and stripped and not stripped.endswith(',') and not stripped.endswith('{'):
                # Check if next line is not continuing the array
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    # If next line is closing brace at lower indent, we need closing braces
                    if next_line.startswith('}') or (next_line and not next_line.startswith('"')):
                        # Check current indent vs array indent
                        current_indent = len(line) - len(line.lstrip())
                        if current_indent > array_indent:
                            # Add closing braces and semicolon
                            fixed_lines.append(line)
                            fixed_lines.append(' ' * array_indent + '        };\n')
                            in_array = False
                            i += 1
                            continue
            
            # Handle lines that are just closing quotes without proper array closure
            if stripped == '"' or (stripped.endswith('"') and not stripped.endswith('",') and in_array):
                # Check if we need to close array
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    if next_line == '}' or next_line.startswith('[') or next_line.startswith('public'):
                        fixed_lines.append(line)
                        # Calculate proper indent
                        current_indent = len(line) - len(line.lstrip())
                        if current_indent > 8:
                            fixed_lines.append(' ' * (current_indent - 4) + '};\n')
                        in_array = False
                        i += 1
                        continue
            
            # Fix missing semicolons after closing braces in specific contexts
            if stripped == '}' and i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                # Check if this closing brace needs a semicolon
                if next_line.startswith('[') or next_line.startswith('public') or next_line.startswith('private'):
                    # Look back to see if this is closing an anonymous object or array
                    j = i - 1
                    needs_semicolon = False
                    while j >= 0 and j > i - 20:
                        prev_line = lines[j].strip()
                        if 'new' in prev_line and '{' in prev_line:
                            needs_semicolon = True
                            break
                        elif prev_line.endswith('};') or prev_line.endswith('{'):
                            break
                        j -= 1
                    
                    if needs_semicolon:
                        fixed_lines.append(line.rstrip() + ';\n')
                        i += 1
                        continue
            
            # Fix specific pattern in AACComprehensiveSteps around line 183
            if stripped == '"Home button always visible"' and i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                if next_line == '}':
                    # This is missing array closing
                    fixed_lines.append(line)
                    fixed_lines.append(' ' * 8 + '};\n')
                    i += 2  # Skip the standalone }
                    continue
            
            fixed_lines.append(line)
            i += 1
        
        # Clean up the file by removing extra closing braces at the end
        # Count braces to ensure balance
        content = ''.join(fixed_lines)
        open_count = content.count('{')
        close_count = content.count('}')
        
        # If we have too many closing braces, remove from the end
        while close_count > open_count and fixed_lines:
            if fixed_lines[-1].strip() == '}':
                fixed_lines.pop()
                close_count -= 1
            else:
                break
        
        # Write the fixed content
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(fixed_lines)
        return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Process all files
    cs_files = []
    for root, dirs, files in os.walk(step_definitions_dir):
        for file in files:
            if file.endswith('.cs'):
                cs_files.append(os.path.join(root, file))
    
    print(f"Found {len(cs_files)} C# files to process")
    
    fixed_count = 0
    for file_path in sorted(cs_files):
        if fix_final_structural_issues(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files processed: {fixed_count}")

if __name__ == "__main__":
    main()