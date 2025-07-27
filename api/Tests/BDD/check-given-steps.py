#!/usr/bin/env python3
import os
import re

def check_given_steps(file_path):
    """Check Given steps in a file"""
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Find all Given step methods
    given_pattern = r'(\[Given\(@[^\]]+\)\]\s*\n\s*public\s+(?:async\s+)?(?:Task\s+)?(?:void\s+)?([^\(]+)\([^\)]*\)\s*\n\s*\{)([^}]*)\}'
    
    given_steps = []
    for match in re.finditer(given_pattern, content, re.MULTILINE | re.DOTALL):
        attribute = match.group(1).strip()
        method_name = match.group(2).strip()
        method_body = match.group(3).strip()
        
        has_not_implemented = 'throw new NotImplementedException' in method_body and 'this is expected in BDD' in method_body
        
        given_steps.append({
            'method': method_name,
            'has_not_implemented': has_not_implemented,
            'body_preview': method_body[:100] + '...' if len(method_body) > 100 else method_body
        })
    
    return given_steps

def main():
    step_defs_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
    
    total_given = 0
    total_with_not_implemented = 0
    files_with_issues = []
    
    for filename in sorted(os.listdir(step_defs_dir)):
        if filename.endswith('.cs') and filename != 'BaseStepDefinitions.cs':
            file_path = os.path.join(step_defs_dir, filename)
            given_steps = check_given_steps(file_path)
            
            if given_steps:
                file_given = len(given_steps)
                file_not_implemented = sum(1 for s in given_steps if s['has_not_implemented'])
                total_given += file_given
                total_with_not_implemented += file_not_implemented
                
                # Check for issues
                steps_without_not_implemented = [s for s in given_steps if not s['has_not_implemented']]
                if steps_without_not_implemented:
                    files_with_issues.append((filename, steps_without_not_implemented))
    
    print(f"Total Given steps found: {total_given}")
    print(f"Given steps with NotImplementedException: {total_with_not_implemented}")
    print(f"Given steps WITHOUT NotImplementedException: {total_given - total_with_not_implemented}")
    
    if files_with_issues:
        print(f"\nFiles with Given steps that don't throw NotImplementedException:")
        for filename, steps in files_with_issues:
            print(f"\n{filename}:")
            for step in steps:
                print(f"  - {step['method']}")
                print(f"    Body: {step['body_preview']}")

if __name__ == "__main__":
    main()