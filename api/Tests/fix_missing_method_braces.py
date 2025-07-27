#!/usr/bin/env python3
import os
import re

def fix_missing_method_closing_braces(file_path):
    """Fix methods that are missing their closing braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed = False
        i = 0
        while i < len(lines):
            # Check if this line ends with }); and the next line is a method attribute
            if i + 1 < len(lines):
                current_line = lines[i].rstrip()
                next_line = lines[i + 1].strip()
                
                # Pattern 1: Line ends with }); and next line is [Given/When/Then
                if (current_line.endswith('});') and 
                    next_line.startswith('[Given(') or next_line.startswith('[When(') or next_line.startswith('[Then(')):
                    # Add closing brace
                    lines[i] = current_line + '\n    }\n'
                    fixed = True
                
                # Pattern 2: Line ends with ); (method call) and next is attribute
                elif (current_line.endswith(');') and not current_line.strip().startswith('throw') and
                      (next_line.startswith('[Given(') or next_line.startswith('[When(') or next_line.startswith('[Then('))):
                    # Check if this is inside a method (by indentation)
                    if len(current_line) - len(current_line.lstrip()) >= 8:
                        lines[i] = current_line + '\n    }\n'
                        fixed = True
                
                # Pattern 3: Line ends with } (object/array initializer) and next is attribute  
                elif (current_line.endswith('}') and not current_line.strip() == '}' and
                      (next_line.startswith('[Given(') or next_line.startswith('[When(') or next_line.startswith('[Then('))):
                    # Check indentation to see if we need a method closing brace
                    current_indent = len(current_line) - len(current_line.lstrip())
                    if current_indent >= 8:  # Inside a method
                        lines[i] = current_line + '\n    }\n'
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
    
    # Files with many CS0106 errors
    problem_files = [
        "ABAToolsSteps.cs",
        "AACComprehensiveSteps.cs", 
        "AuthorizationRbacSteps.cs",
        "AIQualityAssuranceSteps.cs",
        "DataEncryptionSteps.cs",
        "DocumentationHelpersSteps.cs",
        "AIServiceIntegrationSteps.cs",
        "AIGenerationSteps.cs",
        "CaseloadManagementSteps.cs",
        "OutcomeMeasurementSteps.cs"
    ]
    
    fixed_count = 0
    for filename in problem_files:
        file_path = os.path.join(step_definitions_dir, filename)
        if os.path.exists(file_path):
            if fix_missing_method_closing_braces(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()