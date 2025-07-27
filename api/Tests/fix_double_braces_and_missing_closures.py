#!/usr/bin/env python3
import os
import re

def fix_double_braces_and_missing_closures(file_path):
    """Fix double closing braces and missing method closures."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed = False
        i = 0
        while i < len(lines):
            # Fix double closing braces (e.g., "}}")
            if '}}' in lines[i] and not '{{' in lines[i]:
                # Replace }} with just }
                lines[i] = lines[i].replace('}}', '}')
                fixed = True
            
            # Check for missing closing braces on Then/When/Given methods
            if i + 1 < len(lines):
                current_line = lines[i].rstrip()
                next_line = lines[i + 1].strip() if i + 1 < len(lines) else ''
                
                # Pattern: Line ends with .BeTrue(); or similar and next line is [Then/When/Given
                if (current_line.endswith(');') and 
                    'Should()' in current_line and
                    (next_line.startswith('[Then(') or 
                     next_line.startswith('[When(') or 
                     next_line.startswith('[Given('))):
                    # Insert closing brace
                    lines[i] = lines[i].rstrip() + '\n    }\n'
                    fixed = True
            
            i += 1
        
        if fixed:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(lines)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Focus on files with compilation errors
    problem_files = [
        "FerpaComplianceSteps.cs",
        "AdultTherapySteps.cs",
        "SoxComplianceSteps.cs",
        "WcagAccessibilitySteps.cs"
    ]
    
    fixed_count = 0
    for filename in problem_files:
        file_path = os.path.join(step_definitions_dir, filename)
        if os.path.exists(file_path):
            if fix_double_braces_and_missing_closures(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()