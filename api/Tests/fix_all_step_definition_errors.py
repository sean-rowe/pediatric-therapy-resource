#!/usr/bin/env python3
import os
import re

def fix_step_definition_syntax(filepath):
    """Fix all syntax errors in step definition files"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Step 1: Remove semicolons after method blocks
    # Pattern: }; at the end of a method followed by } or [
    content = re.sub(
        r'(\}\s*);(\s*\}|\s*\[)',
        r'\1\2',
        content
    )
    
    # Step 2: Remove duplicate closing braces after methods
    # When we see patterns like:
    # }
    # }
    # [When/Then/Given
    content = re.sub(
        r'(\}\s*\n\s*)\}(\s*\n\s*\[(?:When|Then|Given))',
        r'\1\2',
        content
    )
    
    # Step 3: Fix methods that are missing closing braces
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    brace_depth = 0
    method_start_depth = 0
    in_method = False
    
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()
        
        # Count braces
        open_braces = line.count('{')
        close_braces = line.count('}')
        
        # Check if this is a method declaration with attribute
        if re.match(r'\s*\[(?:When|Then|Given)', stripped):
            # If we're inside a method, we need to close it first
            if in_method and brace_depth > method_start_depth:
                # Add closing braces to match the depth
                while brace_depth > method_start_depth:
                    indent = '    ' * (brace_depth - 1)
                    fixed_lines.append(indent + '}')
                    brace_depth -= 1
                in_method = False
        
        # Update brace depth before processing the line
        brace_depth += open_braces - close_braces
        
        # Check if next line is a method declaration
        if i + 1 < len(lines) and re.match(r'\s*(public|private|protected|internal)', lines[i + 1]):
            if '{' in lines[i + 1]:
                in_method = True
                method_start_depth = brace_depth
        
        fixed_lines.append(line)
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # Step 4: Fix dictionaries with malformed syntax
    # Fix pattern where closing brace is on wrong line
    content = re.sub(
        r'(\["[^"]+"\]\s*=\s*[^,\n]+)\n\s*\},',
        r'\1,',
        content
    )
    
    # Step 5: Fix array initialization 
    content = re.sub(
        r'(new\[\]\s*\{[^}]+)\n\s*\}(\s*\n\s*\};)',
        r'\1\n        }\2',
        content
    )
    
    # Step 6: Remove extra semicolons after closing braces
    content = re.sub(
        r'(\}\s*);(\s*\n\s*\})',
        r'\1\2',
        content
    )
    
    # Step 7: Fix missing closing braces in switch expressions
    content = re.sub(
        r'(_ => [^,;}\n]+)(\s*\n\s*(?:private|public|protected|internal|\[|#))',
        r'\1\n        };\2',
        content
    )
    
    # Step 8: Fix double semicolons
    content = re.sub(r';;', ';', content)
    
    # Step 9: Remove trailing }); patterns that shouldn't be there
    content = re.sub(
        r'(\s*\})\s*\);\s*(\n\s*\[(?:When|Then|Given))',
        r'\1\n    }\2',
        content
    )
    
    # Step 10: Clean up end of file
    while content.rstrip().endswith('}\n}\n}\n}'):
        content = content.rstrip()[:-2] + '\n'
    
    # Ensure file ends with single newline
    content = content.rstrip() + '\n'
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_files = []
    error_files = []
    
    for filename in sorted(os.listdir(step_def_dir)):
        if filename.endswith('.cs'):
            filepath = os.path.join(step_def_dir, filename)
            try:
                if fix_step_definition_syntax(filepath):
                    fixed_files.append(filename)
            except Exception as e:
                error_files.append((filename, str(e)))
    
    print(f"Fixed {len(fixed_files)} files")
    
    if error_files:
        print(f"\nErrors in {len(error_files)} files:")
        for filename, error in error_files:
            print(f"  - {filename}: {error}")

if __name__ == "__main__":
    main()