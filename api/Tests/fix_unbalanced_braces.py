#!/usr/bin/env python3
import os
import re

def count_braces(content):
    """Count opening and closing braces."""
    open_count = content.count('{')
    close_count = content.count('}')
    return open_count, close_count

def fix_unbalanced_braces(file_path):
    """Fix files with unbalanced braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        open_count, close_count = count_braces(content)
        
        if open_count > close_count:
            missing = open_count - close_count
            # Add missing closing braces at end of file
            if not content.rstrip().endswith('}'):
                content = content.rstrip() + '\n' + '}' * missing + '\n'
            else:
                # Find the last } and add more after it
                content = content.rstrip() + '}' * (missing - 1) + '\n'
            
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True, missing
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False, 0

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_count = 0
    for filename in os.listdir(step_definitions_dir):
        if filename.endswith('.cs'):
            file_path = os.path.join(step_definitions_dir, filename)
            fixed, missing = fix_unbalanced_braces(file_path)
            if fixed:
                fixed_count += 1
                print(f"Fixed: {filename} (added {missing} closing braces)")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()