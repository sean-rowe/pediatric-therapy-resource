#!/usr/bin/env python3
import os
import re

def fix_targeted_errors(filepath):
    """Fix specific targeted errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Fix pattern new Dictionary<string, object> { { "key", value } ;
    # Should be: new Dictionary<string, object> { { "key", value } });
    content = re.sub(
        r'(new Dictionary<string, object>\s*\{\s*\{\s*"[^"]+",\s*[^}]+\s*\}\s*);(\s*\n\s*\})',
        r'\1});',
        content
    )
    
    # Fix 2: Fix broken array initializations that end with }            ;
    # Should be: };
    content = re.sub(
        r'(\s*)\}\s+;',
        r'\1};',
        content
    )
    
    # Fix 3: Add missing closing brace at end of file if needed
    # Count opening and closing braces
    open_braces = content.count('{')
    close_braces = content.count('}')
    
    if open_braces > close_braces:
        # Add missing closing braces at the end
        content = content.rstrip()
        for _ in range(open_braces - close_braces):
            content += '\n}'
        content += '\n'
    
    # Fix 4: Fix incomplete methods (missing closing brace before next method)
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        fixed_lines.append(line)
        
        # Check if we have a method declaration after what looks like an incomplete method
        if i + 1 < len(lines):
            next_line = lines[i + 1]
            # Pattern: current line doesn't end with { or } and next line is a method attribute
            if (not line.strip().endswith('{') and 
                not line.strip().endswith('}') and 
                not line.strip().endswith(';') and
                line.strip() != '' and
                re.match(r'\s*\[(?:Given|When|Then|Before|After)', next_line)):
                # Check if we're missing a closing brace
                # Look backwards to see if we have an unclosed method
                unclosed = False
                brace_count = 0
                for j in range(i, max(0, i-20), -1):
                    if '{' in lines[j]:
                        brace_count += lines[j].count('{')
                    if '}' in lines[j]:
                        brace_count -= lines[j].count('}')
                if brace_count > 0:
                    fixed_lines.append('    }')
        
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Priority files to fix first
    priority_files = [
        "AdminStepDefinitions.cs",
        "CommunicationStepDefinitions.cs",
        "ComplianceStepDefinitions.cs",
        "ContentManagementSteps.cs",
        "CommunityFeaturesSteps.cs",
        "CreationToolsSteps.cs",
        "ContinuingEducationSteps.cs"
    ]
    
    fixed_files = []
    
    # Fix priority files first
    for filename in priority_files:
        filepath = os.path.join(step_def_dir, filename)
        if os.path.exists(filepath):
            if fix_targeted_errors(filepath):
                fixed_files.append(filename)
    
    # Then fix all other files
    for filename in os.listdir(step_def_dir):
        if filename.endswith('.cs') and filename not in priority_files:
            filepath = os.path.join(step_def_dir, filename)
            if fix_targeted_errors(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()