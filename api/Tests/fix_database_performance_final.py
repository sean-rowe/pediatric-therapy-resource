#!/usr/bin/env python3

def fix_missing_closing_braces(content):
    """Add missing closing braces to methods"""
    
    lines = content.split('\n')
    fixed_lines = []
    
    # Track method patterns
    method_pattern_indices = []
    
    # Find all method starts
    for i, line in enumerate(lines):
        if (line.strip().startswith('[When(@') or 
            line.strip().startswith('[Then(@') or 
            line.strip().startswith('[Given(@')):
            method_pattern_indices.append(i)
    
    # Process the file
    i = 0
    while i < len(lines):
        line = lines[i]
        
        # Check if we're at a method attribute
        if i in method_pattern_indices:
            # Find the end of this method
            method_idx = method_pattern_indices.index(i)
            next_method_idx = method_pattern_indices[method_idx + 1] if method_idx + 1 < len(method_pattern_indices) else len(lines)
            
            # Copy lines up to the next method
            j = i
            brace_count = 0
            method_body_started = False
            
            while j < next_method_idx:
                curr_line = lines[j]
                fixed_lines.append(curr_line)
                
                # Track braces after method signature
                if '{' in curr_line:
                    method_body_started = True
                
                if method_body_started:
                    brace_count += curr_line.count('{') - curr_line.count('}')
                
                j += 1
            
            # If braces aren't balanced, add closing brace
            if brace_count > 0:
                # Insert closing brace before the next method
                fixed_lines.insert(len(fixed_lines), '    }')
            
            i = j
        else:
            fixed_lines.append(line)
            i += 1
    
    # Ensure class closing brace
    total_braces = 0
    for line in fixed_lines:
        total_braces += line.count('{') - line.count('}')
    
    if total_braces > 0:
        fixed_lines.append('}')
    
    return '\n'.join(fixed_lines)

# Read file
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'r') as f:
    content = f.read()

# Apply fixes
fixed_content = fix_missing_closing_braces(content)

# Write back
with open('/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/DatabasePerformanceSteps.cs', 'w') as f:
    f.write(fixed_content)

print("Fixed missing closing braces")