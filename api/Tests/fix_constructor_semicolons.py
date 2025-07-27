#!/usr/bin/env python3
import os
import re

def fix_constructor_semicolons(file_path):
    """Remove semicolons after constructor closing braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern to match constructor with semicolon after closing brace
        # Matches: } followed by optional whitespace and semicolon on the same line or next line
        pattern = r'(public\s+\w+\s*\([^)]*\)\s*:\s*base\s*\([^)]*\)\s*\{[^}]*\})\s*;'
        
        # Replace with just the constructor without the semicolon
        new_content = re.sub(pattern, r'\1', content, flags=re.MULTILINE | re.DOTALL)
        
        if new_content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_count = 0
    for filename in os.listdir(step_definitions_dir):
        if filename.endswith('.cs'):
            file_path = os.path.join(step_definitions_dir, filename)
            if fix_constructor_semicolons(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()