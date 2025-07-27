#!/usr/bin/env python3
import re

def fix_anonymous_objects_and_dictionaries(content):
    """Fix anonymous object semicolons and dictionary initialization patterns."""
    
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Pattern 1: Fix anonymous objects missing semicolons
        # Look for closing brace of anonymous object (indented) followed by empty lines
        if i > 0 and line.strip() == '}' and i + 1 < len(lines):
            # Check if this is part of an anonymous object
            prev_lines = []
            j = i - 1
            while j >= 0 and lines[j].strip():
                prev_lines.append(lines[j])
                j -= 1
            
            # Check if we have an anonymous object pattern
            if any('= new' in pl for pl in prev_lines[-5:] if pl):
                # Look ahead to see if there's a dictionary initialization
                k = i + 1
                while k < len(lines) and not lines[k].strip():
                    k += 1
                
                if k < len(lines) and 'Add(' in lines[k]:
                    # This is the closing brace of anonymous object before Add
                    fixed_lines.append(line + ';')
                    i += 1
                    continue
        
        # Pattern 2: Fix dictionary initialization with content outside braces
        # Look for pattern: new Dictionary<string, object>\n{\n}\n}\n}\n["key"] = value
        if 'new Dictionary<string, object>' in line:
            dict_start_idx = i
            fixed_lines.append(line)
            i += 1
            
            # Skip to opening brace
            while i < len(lines) and '{' not in lines[i]:
                fixed_lines.append(lines[i])
                i += 1
            
            if i < len(lines):
                fixed_lines.append(lines[i])  # Add the '{'
                i += 1
                
                # Collect all dictionary content that might be misplaced
                dict_content = []
                brace_count = 1
                extra_closing_braces = []
                
                while i < len(lines) and brace_count > 0:
                    curr_line = lines[i]
                    
                    if curr_line.strip() == '}':
                        brace_count -= 1
                        if brace_count == 0:
                            # This is the closing brace of the dictionary
                            # Look ahead for misplaced content
                            j = i + 1
                            while j < len(lines):
                                next_line = lines[j]
                                if next_line.strip().startswith('["'):
                                    # Found dictionary content outside braces
                                    dict_content.append('                ' + next_line.strip())
                                    j += 1
                                elif next_line.strip() == '}':
                                    extra_closing_braces.append(j)
                                    j += 1
                                elif next_line.strip() == '});':
                                    # Found the end of the method call
                                    break
                                elif next_line.strip():
                                    break
                                else:
                                    j += 1
                            
                            # Add collected dictionary content before closing brace
                            for content_line in dict_content:
                                fixed_lines.append(content_line)
                            
                            fixed_lines.append('            }')
                            fixed_lines.append('        });')
                            
                            # Skip all the lines we've processed
                            i = j + 1
                            continue
                    
                    fixed_lines.append(curr_line)
                    i += 1
                
                continue
        
        # Pattern 3: Fix missing closing braces at end of methods
        if i == len(lines) - 1 and line.strip() == '}':
            # Check if we need another closing brace
            open_braces = 0
            for fl in fixed_lines:
                open_braces += fl.count('{') - fl.count('}')
            
            fixed_lines.append(line)
            if open_braces > 1:  # Account for class brace
                fixed_lines.append('}')
            i += 1
            continue
        
        fixed_lines.append(line)
        i += 1
    
    return '\n'.join(fixed_lines)

# Read the file
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'r') as f:
    content = f.read()

# Apply fixes
fixed_content = fix_anonymous_objects_and_dictionaries(content)

# Write the fixed content
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'w') as f:
    f.write(fixed_content)

print("Fixed DatabasePerformanceSteps.cs")