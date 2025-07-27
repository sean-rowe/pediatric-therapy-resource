#!/usr/bin/env python3

import os
import re
import glob

def fix_missing_closing_braces(filepath):
    """Fix missing closing braces and semicolons in dictionaries and arrays"""
    with open(filepath, 'r') as f:
        content = f.read()
    
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    fixes_made = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Check if this line has an opening brace/bracket for dictionary or array initialization
        if re.search(r'=\s*new\s*(Dictionary<[^>]+>|List<[^>]+>|\[\]|object\[\])\s*$', line):
            # This is the start of a collection initialization
            fixed_lines.append(line)
            i += 1
            
            # Look for the opening brace
            if i < len(lines) and re.match(r'^\s*{\s*$', lines[i]):
                fixed_lines.append(lines[i])
                i += 1
                
                # Now look for the content and check if closing braces are missing
                brace_count = 1
                content_lines = []
                
                while i < len(lines) and brace_count > 0:
                    if '{' in lines[i]:
                        brace_count += lines[i].count('{')
                    if '}' in lines[i]:
                        brace_count -= lines[i].count('}')
                    
                    content_lines.append(lines[i])
                    i += 1
                    
                    if brace_count == 0:
                        break
                
                # Check if we ended without proper closing
                if brace_count > 0 and i < len(lines):
                    # We're missing closing braces
                    # Check if the next line is a method declaration
                    if i < len(lines) and re.match(r'^\s*\[(Given|When|Then)', lines[i]):
                        # Add the missing closing braces
                        fixed_lines.extend(content_lines[:-1])  # Add all but last
                        if content_lines:
                            last_content = content_lines[-1]
                            # Check if it needs a semicolon
                            if not last_content.rstrip().endswith((';', '}', '{')):
                                fixed_lines.append(last_content)
                                fixed_lines.append('        };')
                                fixed_lines.append('    }')
                                print(f"  Fixed missing closing braces at line {i}")
                                fixes_made += 1
                            else:
                                fixed_lines.append(last_content)
                        continue
                    else:
                        fixed_lines.extend(content_lines)
                else:
                    fixed_lines.extend(content_lines)
            else:
                continue
        
        # Check for incomplete method bodies (dictionary initialization without closing)
        elif re.search(r'ScenarioContext\[.+\]\s*=\s*new\s*(Dictionary<[^>]+>|object\[\]|string\[\]|List<[^>]+>)\s*$', line):
            fixed_lines.append(line)
            i += 1
            
            # Check if next line starts the initialization block
            if i < len(lines) and re.match(r'^\s*{\s*$', lines[i]):
                fixed_lines.append(lines[i])
                i += 1
                
                # Collect the content
                init_content = []
                while i < len(lines) and not re.match(r'^\s*\[(Given|When|Then)', lines[i]):
                    init_content.append(lines[i])
                    i += 1
                
                # Check if the last line has proper closing
                if init_content:
                    last_line = init_content[-1].strip()
                    if last_line and not last_line.endswith((';', '}')):
                        # Missing closing
                        fixed_lines.extend(init_content)
                        fixed_lines.append('        };')
                        print(f"  Fixed missing semicolon at line {i}")
                        fixes_made += 1
                    else:
                        fixed_lines.extend(init_content)
                continue
        
        fixed_lines.append(line)
        i += 1
    
    # Write back
    with open(filepath, 'w') as f:
        f.write('\n'.join(fixed_lines))
    
    return fixes_made

def main():
    # Find all step definition files
    pattern = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/*.cs"
    files = glob.glob(pattern)
    
    print(f"Found {len(files)} step definition files to check")
    
    total_fixes = 0
    for filepath in files:
        print(f"Checking: {os.path.basename(filepath)}")
        fixes = fix_missing_closing_braces(filepath)
        if fixes > 0:
            print(f"  Made {fixes} fixes")
            total_fixes += fixes
    
    print(f"\nCompleted! Total fixes made: {total_fixes}")

if __name__ == "__main__":
    main()