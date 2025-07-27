#!/usr/bin/env python3
import os
import re
import sys

def update_given_steps(file_path):
    """Update all Given steps in a file to throw NotImplementedException"""
    
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Pattern to match Given steps with their complete method body
    # This handles various patterns including async methods
    pattern = r'(\s*\[Given\(@[^\]]+\)\]\s*\n\s*public\s+(?:async\s+)?(?:Task\s+)?(?:void\s+)?[^\{]+\{)[^}]*(\})'
    
    def replacer(match):
        method_header = match.group(1)
        method_end = match.group(2)
        
        # Check if it already throws NotImplementedException
        method_body = match.group(0)
        if 'throw new NotImplementedException' in method_body:
            return match.group(0)  # Already updated, return as-is
        
        # Replace with NotImplementedException
        return f'{method_header}\n        throw new NotImplementedException("Feature not yet implemented - this is expected in BDD");\n    {method_end}'
    
    # Count Given steps before update
    given_count_before = len(re.findall(r'\[Given\(@', content))
    
    # Update content
    updated_content = re.sub(pattern, replacer, content, flags=re.MULTILINE | re.DOTALL)
    
    # Count how many were updated
    given_count_after = len(re.findall(r'throw new NotImplementedException.*this is expected in BDD', updated_content))
    
    # Write back if changes were made
    if content != updated_content:
        with open(file_path, 'w') as f:
            f.write(updated_content)
        return given_count_before, given_count_after
    
    return given_count_before, given_count_after

def main():
    step_defs_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
    
    total_files = 0
    total_given_before = 0
    total_given_updated = 0
    files_updated = []
    
    for filename in os.listdir(step_defs_dir):
        if filename.endswith('.cs') and filename != 'BaseStepDefinitions.cs':
            file_path = os.path.join(step_defs_dir, filename)
            total_files += 1
            
            given_before, given_after = update_given_steps(file_path)
            total_given_before += given_before
            total_given_updated += given_after
            
            if given_after > 0:
                files_updated.append((filename, given_before, given_after))
    
    print(f"Total files processed: {total_files}")
    print(f"Total Given steps found: {total_given_before}")
    print(f"Total Given steps with NotImplementedException: {total_given_updated}")
    print(f"\nFiles updated:")
    for filename, before, after in sorted(files_updated):
        print(f"  {filename}: {before} Given steps, {after} with NotImplementedException")

if __name__ == "__main__":
    main()