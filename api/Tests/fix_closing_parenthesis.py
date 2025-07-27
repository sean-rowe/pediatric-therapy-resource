#!/usr/bin/env python3
import os
import re

def fix_closing_parenthesis(filepath):
    """Fix extra closing parenthesis issues in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix 1: Remove extra closing parenthesis from array initializations
    # Pattern: new[] { ... } });
    # Should be: new[] { ... };
    content = re.sub(
        r'(\s*}\s*)\s*}\s*\);\s*\n(\s*}\s*\n)',
        r'\1;\n\2',
        content,
        flags=re.MULTILINE
    )
    
    # Fix 2: Fix dictionary closing with extra parenthesis
    # Pattern: } });
    # Should be: });
    # But only when it's part of a method call
    lines = content.split('\n')
    fixed_lines = []
    
    for i, line in enumerate(lines):
        if line.strip() == '}            });' or line.strip() == '}        });':
            # Check if this is inside a method call by looking at previous lines
            # If we see "new Dictionary" or similar pattern, keep the );
            found_dictionary_start = False
            for j in range(max(0, i-10), i):
                if 'new Dictionary' in lines[j] or 'await WhenISend' in lines[j]:
                    found_dictionary_start = True
                    break
            
            if found_dictionary_start:
                # This is correct, keep it
                fixed_lines.append(line)
            else:
                # This is incorrect, fix it
                fixed_lines.append(line.replace('});', '}'))
        else:
            fixed_lines.append(line)
    
    content = '\n'.join(fixed_lines)
    
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
            if fix_closing_parenthesis(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()