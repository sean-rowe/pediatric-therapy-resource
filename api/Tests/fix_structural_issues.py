#!/usr/bin/env python3
import os

def fix_specific_files():
    """Fix specific structural issues in problematic files."""
    
    # Fix ResourceCreationSteps.cs - Extra closing braces at end
    file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/ResourceCreationSteps.cs'
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove the extra }; at the end and replace with just }
    content = content.replace('\n};\n}', '\n}')
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_path}")
    
    # Fix SessionDocumentationSteps.cs - Missing closing braces
    file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/SessionDocumentationSteps.cs'
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Find lines that need closing braces
    new_lines = []
    for i, line in enumerate(lines):
        new_lines.append(line)
        # Add missing closing braces after ScenarioContext assignments
        if 'ScenarioContext[' in line and line.strip().endswith(';') and not lines[i+1].strip().startswith('}'):
            # Check if we're at the end of a method
            if i + 2 < len(lines) and (lines[i+2].strip().startswith('[') or lines[i+2].strip().startswith('}')):
                indent = len(line) - len(line.lstrip()) - 4
                new_lines.append(' ' * indent + '}\n')
    
    # Fix the ending
    if new_lines[-1].strip() == '};':
        new_lines[-1] = '}'
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
    print(f"Fixed {file_path}")
    
    # Fix ReportingAnalyticsSteps.cs - Similar issue
    file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/ReportingAnalyticsSteps.cs'
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Ensure it ends with just one }
    if content.rstrip().endswith('};'):
        content = content.rstrip()[:-2] + '}'
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Fixed {file_path}")
    
    # Fix ResearchEvidenceSteps.cs
    file_path = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/ResearchEvidenceSteps.cs'
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Add missing closing braces
    new_lines = []
    for i, line in enumerate(lines):
        new_lines.append(line)
        if 'ScenarioContext["UpdateFrequency"] = "Monthly";' in line:
            # Add missing closing brace for the method
            indent = len(line) - len(line.lstrip()) - 4
            new_lines.append(' ' * indent + '}\n')
    
    # Ensure proper ending
    while new_lines and new_lines[-1].strip() == '':
        new_lines.pop()
    
    if not new_lines[-1].strip() == '}':
        new_lines.append('\n}')
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
    print(f"Fixed {file_path}")

fix_specific_files()