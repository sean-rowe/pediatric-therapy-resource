#!/usr/bin/env python3
import os
import re

def fix_syntax_issues(file_path):
    """Fix remaining syntax issues like missing semicolons and braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Fix common issues
        # 1. Add missing semicolons after closing braces in anonymous objects
        content = re.sub(r'(\}\s*)([\r\n]+\s*ScenarioContext)', r'};\2', content)
        
        # 2. Fix foreach loops missing closing brace
        content = re.sub(r'(\}\s*)([\r\n]+\s*\[(?:Given|When|Then)\()', r'}\n        }\2', content)
        
        # 3. Count and balance braces
        lines = content.split('\n')
        open_count = sum(line.count('{') for line in lines)
        close_count = sum(line.count('}') for line in lines)
        
        # Remove excess closing braces at the end
        while lines and lines[-1].strip() == '}' and close_count > open_count:
            lines.pop()
            close_count -= 1
        
        # Add missing closing braces
        if open_count > close_count:
            missing = open_count - close_count
            if lines and lines[-1].strip() != '':
                lines.append('')
            for _ in range(missing):
                lines.append('}')
        
        new_content = '\n'.join(lines)
        
        if new_content != content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Focus on files with remaining errors
    problem_files = [
        "ParentPortalSteps.cs",
        "OutcomeMeasurementSteps.cs",
        "AdultTherapySteps.cs",
        "SoxComplianceSteps.cs",
        "FerpaComplianceSteps.cs",
        "WcagAccessibilitySteps.cs",
        "PECSImplementationSteps.cs",
        "StudentManagementSteps.cs",
        "IEPGoalTrackingSteps.cs"
    ]
    
    # Also process all files
    all_files = []
    for root, dirs, files in os.walk(step_definitions_dir):
        for file in files:
            if file.endswith('.cs'):
                all_files.append(os.path.join(root, file))
    
    print(f"Processing {len(all_files)} files...")
    
    fixed_count = 0
    for file_path in all_files:
        if fix_syntax_issues(file_path):
            fixed_count += 1
            print(f"Fixed: {os.path.basename(file_path)}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()