#!/usr/bin/env python3
import os

def fix_file_endings():
    """Fix files that have incorrect endings."""
    
    step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
    
    # Fix SessionDocumentationSteps.cs - Remove the }; at line 157
    file_path = os.path.join(step_def_dir, 'SessionDocumentationSteps.cs')
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Find and fix the }; line
    for i in range(len(lines)):
        if lines[i].strip() == '};':
            lines[i] = '}\n'
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    print(f"Fixed {file_path}")
    
    # Fix ResourceCreationSteps.cs - Ensure proper ending
    file_path = os.path.join(step_def_dir, 'ResourceCreationSteps.cs')
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Ensure it ends with a single }
    while lines and lines[-1].strip() == '':
        lines.pop()
    
    if not lines[-1].strip() == '}':
        lines.append('}\n')
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    print(f"Fixed {file_path}")
    
    # Fix ResearchEvidenceSteps.cs - Fix the broken structure
    file_path = os.path.join(step_def_dir, 'ResearchEvidenceSteps.cs')
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Fix the misplaced code after line 203
    content = content.replace(
        '        ScenarioContext["UpdateFrequency"] = "Monthly";\n    }\n        ScenarioContext["UpdateTypes"] = new[]',
        '        ScenarioContext["UpdateFrequency"] = "Monthly";\n        ScenarioContext["UpdateTypes"] = new[]'
    )
    
    # Ensure the file ends properly
    content = content.rstrip()
    if not content.endswith('}'):
        content += '\n}'
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_path}")
    
    # Fix SoxComplianceSteps.cs - Fix anonymous object semicolons
    file_path = os.path.join(step_def_dir, 'SoxComplianceSteps.cs')
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Fix missing semicolons after anonymous object closing braces
    content = content.replace('}\n\n            var json', '};\n\n            var json')
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_path}")

fix_file_endings()