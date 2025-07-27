#!/usr/bin/env python3
import os
import re

def fix_specific_semicolons(file_path):
    """Fix specific semicolon issues in step definition files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Fix pattern: closing brace of anonymous object without semicolon
        # Match: }  (with possible whitespace) followed by newline and then var/await/response
        pattern = r'(\}\s*)\n(\s*)(var\s+|await\s+|response\.|_)'
        
        def replacement(match):
            brace_part = match.group(1).rstrip()  # Remove trailing whitespace
            indent = match.group(2)
            next_line = match.group(3)
            return f'{brace_part};\n{indent}{next_line}'
        
        new_content = re.sub(pattern, replacement, content, flags=re.MULTILINE)
        
        if new_content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Focus on files with known issues
    problem_files = [
        "WcagAccessibilitySteps.cs",
        "ZeroTrustSteps.cs",
        "SoxComplianceSteps.cs",
        "AuditTrailSteps.cs"
    ]
    
    fixed_count = 0
    for filename in problem_files:
        file_path = os.path.join(step_definitions_dir, filename)
        if os.path.exists(file_path):
            if fix_specific_semicolons(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()