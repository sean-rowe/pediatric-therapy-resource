#!/usr/bin/env python3
import os
import re

def fix_step_definition_errors(filepath):
    """Fix common syntax errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Missing closing braces in switch expressions
    content = re.sub(
        r'(_\s*=>\s*[^,;}\n]+)(\s*\n\s*(?:private|public|protected|internal|\[|\}|//|$))',
        r'\1;\n        };\2',
        content
    )
    
    # Fix 2: Fix dictionary formatting issues
    # Fix dictionaries with closing brace on wrong line
    content = re.sub(
        r'(\["[^"]+"\]\s*=\s*[^,\n]+),?\s*\n\}\s*(\["[^"]+"\])',
        r'\1\n            },\n            \2',
        content
    )
    
    # Fix 3: Remove extra }); at end of dictionary definitions
    content = re.sub(
        r'(\}\s*)\}\);(\s*\n\s*\})',
        r'\1});',
        content
    )
    
    # Fix 4: Fix misplaced dictionary closing braces
    lines = content.split('\n')
    fixed_lines = []
    in_dictionary = False
    dict_indent = 0
    
    for i, line in enumerate(lines):
        stripped = line.strip()
        
        # Detect start of dictionary
        if 'new Dictionary<' in line or 'new {' in line:
            in_dictionary = True
            dict_indent = len(line) - len(line.lstrip())
            fixed_lines.append(line)
            continue
        
        # Fix lines that have }[ pattern (closing brace followed by new property)
        if in_dictionary and re.match(r'^\}\s*\["', stripped):
            # Split into two lines
            fixed_lines.append(' ' * (dict_indent + 4) + '},')
            fixed_lines.append(' ' * (dict_indent + 4) + stripped[1:])
            continue
        
        # Detect end of dictionary
        if in_dictionary and stripped.startswith('}') and len(line) - len(line.lstrip()) <= dict_indent:
            in_dictionary = False
        
        fixed_lines.append(line)
    
    content = '\n'.join(fixed_lines)
    
    # Fix 5: Add missing semicolons after anonymous objects
    content = re.sub(
        r'(new\s*\{[^}]+\})(\s*\n\s*\[)',
        r'\1;\2',
        content
    )
    
    # Fix 6: Fix array initialization with missing closing brace
    content = re.sub(
        r'(new\[\]\s*\{[^}]+)(\s*\n\s*\})',
        r'\1\n        };\2',
        content
    )
    
    # Fix 7: Fix methods inside methods (add missing closing braces)
    lines = content.split('\n')
    fixed_lines = []
    brace_stack = []
    
    for i, line in enumerate(lines):
        # Track braces with their context
        if '{' in line:
            if 'class' in line or 'interface' in line:
                brace_stack.append('class')
            elif re.match(r'\s*(public|private|protected|internal).*\(.*\)', line):
                brace_stack.append('method')
            else:
                brace_stack.append('block')
        
        # Check for method declaration inside method
        if (len(brace_stack) > 1 and 
            'method' in brace_stack and 
            re.match(r'\s*\[(When|Then|Given)', line)):
            # Add closing brace before this line
            indent = len(line) - len(line.lstrip()) - 4
            fixed_lines.append(' ' * indent + '}')
            if brace_stack and brace_stack[-1] == 'method':
                brace_stack.pop()
        
        fixed_lines.append(line)
        
        # Update brace stack
        close_count = line.count('}')
        for _ in range(close_count):
            if brace_stack:
                brace_stack.pop()
    
    content = '\n'.join(fixed_lines)
    
    # Fix 8: Remove semicolons after method blocks
    content = re.sub(
        r'(\}\s*);(\s*\n\s*(?:\[|public|private|protected|internal|//|$))',
        r'\1\2',
        content
    )
    
    # Fix 9: Fix switch expressions missing closing braces and semicolons
    content = re.sub(
        r'(switch\s*\{[^}]+)(\s*\n\s*(?:private|public|\[|\}))',
        r'\1\n        };\2',
        content
    )
    
    # Fix 10: Add missing commas in dictionaries
    content = re.sub(
        r'(\["[^"]+"\]\s*=\s*[^,\n]+)(\s*\n\s*\[")',
        r'\1,\2',
        content
    )
    
    # Fix 11: Remove duplicate closing braces at end of file
    while content.rstrip().endswith('}\n}'):
        lines = content.rstrip().split('\n')
        if lines[-1] == '}' and lines[-2].strip() == '}':
            # Check brace balance
            open_count = content.count('{')
            close_count = content.count('}')
            if close_count > open_count:
                lines = lines[:-1]
                content = '\n'.join(lines) + '\n'
            else:
                break
        else:
            break
    
    # Only write if content changed
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Priority files to fix first
    priority_files = [
        "CommonUISteps.cs",
        "CommunityFeaturesSteps.cs",
        "ComplianceStepDefinitions.cs",
        "ComplianceAuditSteps.cs",
        "CommunicationStepDefinitions.cs"
    ]
    
    fixed_files = []
    
    # Fix priority files first
    for filename in priority_files:
        filepath = os.path.join(step_def_dir, filename)
        if os.path.exists(filepath):
            if fix_step_definition_errors(filepath):
                fixed_files.append(filename)
    
    # Then fix all other files
    for filename in os.listdir(step_def_dir):
        if filename.endswith('.cs') and filename not in priority_files:
            filepath = os.path.join(step_def_dir, filename)
            if fix_step_definition_errors(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files:")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()