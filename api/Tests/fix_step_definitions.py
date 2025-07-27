#!/usr/bin/env python3
import os
import re

def fix_step_definition_errors(filepath):
    """Fix common syntax errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Pattern 1: Fix semicolons after anonymous objects in method calls
    # Example: new { Name = "Test" }; should be new { Name = "Test" }
    content = re.sub(
        r'(new\s*\{[^}]+\})\s*;(\s*\))',
        r'\1\2',
        content
    )
    
    # Pattern 2: Fix semicolons after method blocks
    # Example: public void Method() { }; should be public void Method() { }
    content = re.sub(
        r'(\}\s*)\};(\s*(?:public|private|protected|internal|static|\[|//|/\*|\n|$))',
        r'\1}\2',
        content
    )
    
    # Pattern 3: Fix malformed dictionary/array syntax
    # Fix missing commas in dictionaries
    content = re.sub(
        r'(\{[^}]*"[^"]+"\s*:\s*"[^"]+")(\s*"[^"]+"\s*:)',
        r'\1,\2',
        content
    )
    
    # Pattern 4: Fix methods declared inside other methods (missing closing braces)
    lines = content.split('\n')
    fixed_lines = []
    brace_count = 0
    class_brace_count = 0
    in_class = False
    method_pattern = re.compile(r'^\s*(public|private|protected|internal)\s+.*\s+\w+\s*\([^)]*\)\s*$')
    class_pattern = re.compile(r'^\s*(public|private|protected|internal)?\s*(class|interface)\s+\w+')
    
    for i, line in enumerate(lines):
        # Track if we're inside a class
        if class_pattern.match(line):
            in_class = True
            class_brace_count = 0
        
        # Count braces
        open_braces = line.count('{')
        close_braces = line.count('}')
        
        if in_class:
            class_brace_count += open_braces - close_braces
            if class_brace_count == 0:
                in_class = False
        
        brace_count += open_braces - close_braces
        
        # Check if this looks like a method declaration
        if method_pattern.match(line) and in_class and brace_count > class_brace_count:
            # We're inside a method trying to declare another method
            # Add closing brace before this line
            fixed_lines.append('    }')
            brace_count -= 1
        
        fixed_lines.append(line)
    
    content = '\n'.join(fixed_lines)
    
    # Pattern 5: Fix missing semicolons after statements
    # Add semicolon after variable assignments without semicolon
    content = re.sub(
        r'(\w+\s*=\s*(?:new\s+\w+(?:<[^>]+>)?\([^)]*\)|"[^"]*"|\'[^\']*\'|\d+|true|false|null))(\s*\n)',
        r'\1;\2',
        content
    )
    
    # Pattern 6: Fix semicolons in the middle of method calls
    content = re.sub(
        r'(\([^)]*);([^)]*\))',
        r'\1,\2',
        content
    )
    
    # Pattern 7: Fix extra closing braces at end of file
    while content.rstrip().endswith('}}'):
        # Check if we have unmatched closing braces
        open_count = content.count('{')
        close_count = content.count('}')
        if close_count > open_count:
            content = content.rstrip()[:-1]
        else:
            break
    
    # Pattern 8: Fix public modifiers inside methods
    # This is more complex - need context-aware fixing
    lines = content.split('\n')
    fixed_lines = []
    method_depth = 0
    
    for line in lines:
        # Track method depth
        if '{' in line:
            method_depth += line.count('{')
        if '}' in line:
            method_depth -= line.count('}')
        
        # If we're inside a method (depth > 1) and see a public method declaration
        if method_depth > 1 and re.match(r'\s*public\s+.*\s+\w+\s*\([^)]*\)', line):
            # This is likely an error - remove the line or comment it out
            fixed_lines.append('        // ' + line.strip())
        else:
            fixed_lines.append(line)
    
    content = '\n'.join(fixed_lines)
    
    # Pattern 9: Fix Table syntax in method calls
    content = re.sub(
        r'(Table\s*\([^)]*\))\s*;(\s*\))',
        r'\1\2',
        content
    )
    
    # Only write if content changed
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_files = []
    
    for filename in os.listdir(step_def_dir):
        if filename.endswith('.cs'):
            filepath = os.path.join(step_def_dir, filename)
            if fix_step_definition_errors(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files:")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()