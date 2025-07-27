#!/usr/bin/env python3
import os
import re

def fix_specific_errors(filepath):
    """Fix specific syntax errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Remove extra closing parenthesis from dictionary entries
    # Pattern: new[] { "value" });
    # Should be: new[] { "value" }
    content = re.sub(
        r'(new\[\]\s*\{\s*[^}]+)\)\s*\}',
        r'\1}',
        content
    )
    
    # Fix 2: Add missing semicolon and closing brace
    # Pattern: new[] { "value" }
    # Next line: [Given/When/Then
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Check if this line ends with } without semicolon
        if i + 1 < len(lines):
            next_line = lines[i + 1]
            
            # If current line ends with } and has array initialization
            if 'new[]' in line and line.strip().endswith('}') and not line.strip().endswith('};'):
                # If next line starts with step attribute
                if re.match(r'\s*\[(?:Given|When|Then)', next_line):
                    line = line.rstrip() + ';'
                    # Add closing brace before next line
                    fixed_lines.append(line)
                    fixed_lines.append('    }')
                    i += 1
                    continue
            
            # Fix missing closing brace and semicolon in anonymous object
            if 'ScenarioContext[' in line and line.strip().endswith('}') and not line.strip().endswith('};'):
                if re.match(r'\s*\[(?:Given|When|Then)', next_line):
                    line = line.rstrip() + ';'
                    fixed_lines.append(line)
                    fixed_lines.append('    }')
                    i += 1
                    continue
        
        fixed_lines.append(line)
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # Fix 3: Remove extra closing braces at end of file
    content = re.sub(r'(\s*}\s*\n)\s*}\s*$', r'\1', content)
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Target specific files with known issues
    target_files = [
        "AIGenerationSteps.cs",
        "AACComprehensiveSteps.cs",
        "ABAToolsSteps.cs"
    ]
    
    fixed_files = []
    
    for filename in target_files:
        filepath = os.path.join(step_def_dir, filename)
        if os.path.exists(filepath):
            if fix_specific_errors(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()