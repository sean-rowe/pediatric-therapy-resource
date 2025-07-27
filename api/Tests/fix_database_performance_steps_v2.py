#!/usr/bin/env python3
import re

def fix_file_comprehensively(content):
    """Comprehensive fix for all compilation errors in DatabasePerformanceSteps.cs"""
    
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Fix pattern 1: Anonymous objects missing semicolons
        # Check for closing brace that belongs to an anonymous object
        if line.strip() == '}' and i > 0:
            # Look back to see if this is part of an anonymous object
            # Check previous lines for "= new" pattern
            found_new = False
            for j in range(max(0, i-10), i):
                if '= new' in lines[j] and '{' in lines[j]:
                    found_new = True
                    break
            
            if found_new:
                # This might be an anonymous object closing brace
                # Check if the next non-empty line starts a method call or is a closing brace
                next_line_idx = i + 1
                while next_line_idx < len(lines) and not lines[next_line_idx].strip():
                    next_line_idx += 1
                
                if next_line_idx < len(lines):
                    next_line = lines[next_line_idx].strip()
                    # If the next line is not a closing brace or method attribute, add semicolon
                    if not next_line.startswith('}') and not next_line.startswith('[') and not next_line.startswith('public') and not next_line.startswith('private'):
                        line = line.rstrip() + ';'
        
        # Fix pattern 2: Missing closing braces for methods
        # Look for [When or [Then attributes followed by method signature
        if line.strip().startswith('[When(@') or line.strip().startswith('[Then(@'):
            # This is a method attribute, check if the method is missing closing brace
            fixed_lines.append(line)
            i += 1
            
            # Get the method signature line
            if i < len(lines):
                fixed_lines.append(lines[i])
                i += 1
                
                # Track braces for this method
                method_braces = 0
                method_start = i
                
                while i < len(lines):
                    curr_line = lines[i]
                    
                    # Count braces
                    method_braces += curr_line.count('{') - curr_line.count('}')
                    
                    # Check if we've found the next method or end of class
                    if (curr_line.strip().startswith('[When(@') or 
                        curr_line.strip().startswith('[Then(@') or
                        curr_line.strip().startswith('[Given(@')):
                        # We've hit the next method without closing current one
                        if method_braces > 0:
                            # Add missing closing brace
                            fixed_lines.append('    }')
                        break
                    
                    fixed_lines.append(curr_line)
                    
                    # If braces are balanced, method is complete
                    if method_braces == 0 and i > method_start:
                        i += 1
                        break
                    
                    i += 1
                
                continue
        
        fixed_lines.append(line)
        i += 1
    
    # Fix missing closing brace at end of class if needed
    class_braces = 0
    for line in fixed_lines:
        class_braces += line.count('{') - line.count('}')
    
    if class_braces > 0:
        fixed_lines.append('}')
    
    return '\n'.join(fixed_lines)

# Read the current file
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'r') as f:
    content = f.read()

# Apply comprehensive fixes
fixed_content = fix_file_comprehensively(content)

# Write the fixed content
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'w') as f:
    f.write(fixed_content)

print("Applied comprehensive fixes to DatabasePerformanceSteps.cs")