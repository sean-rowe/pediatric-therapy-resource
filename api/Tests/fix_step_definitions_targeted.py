#!/usr/bin/env python3
import os
import re

def fix_file_specific_issues(filepath):
    """Fix specific issues in certain files"""
    filename = os.path.basename(filepath)
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix CommonUISteps.cs specific issues
    if filename == "CommonUISteps.cs":
        # Fix missing closing braces in switch expressions
        content = re.sub(
            r'(_ => [^}\n]+)(\s*\n\s*private string)',
            r'\1\n        };\2',
            content
        )
        
        # Fix method declarations inside other methods
        content = re.sub(
            r'(\s*ScenarioContext\["LastClickedElement"\] = buttonOrLinkText;\s*\n)(\s*\[When)',
            r'\1    }\n\2',
            content
        )
        
        # Fix the end of file issues
        content = re.sub(
            r'(\s*_[^}]+)(\s*\n\s*#endregion)',
            r'\1\n        };\n    }\2',
            content
        )
        
        # Fix multiple closing braces at end
        content = re.sub(r'(\}\s*){4,}$', '}\n}', content.strip()) + '\n'
    
    # Fix CommunityFeaturesSteps.cs specific issues
    elif filename == "CommunityFeaturesSteps.cs":
        # Fix malformed dictionaries
        lines = content.split('\n')
        fixed_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i]
            
            # Fix pattern where dictionary item is split across lines
            if re.match(r'\s*\["title"\]\s*=.*".*"$', line) and i + 1 < len(lines):
                next_line = lines[i + 1]
                if re.match(r'\s*\},', next_line):
                    fixed_lines.append(line + ',')
                    i += 1
                    continue
            
            # Fix closing brace followed by property on same line
            if re.match(r'^\}\s*\["', line.strip()):
                indent = len(line) - len(line.lstrip()) + 4
                fixed_lines.append(' ' * indent + '},')
                fixed_lines.append(' ' * indent + line.strip()[1:])
                i += 1
                continue
            
            # Fix method declaration missing closing brace
            if re.match(r'\s*await.*\);$', line) and i + 1 < len(lines):
                next_line = lines[i + 1]
                if re.match(r'\s*\[(?:When|Then|Given)', next_line):
                    fixed_lines.append(line)
                    indent = len(line) - len(line.lstrip()) - 4
                    fixed_lines.append(' ' * indent + '}')
                    i += 1
                    continue
            
            # Fix stray }); in method body
            if line.strip() == '});' and i + 1 < len(lines):
                next_line = lines[i + 1]
                if re.match(r'\s*ScenarioContext\[', next_line):
                    # Skip this line, it's an error
                    i += 1
                    continue
            
            fixed_lines.append(line)
            i += 1
        
        content = '\n'.join(fixed_lines)
        
        # Fix array initialization with missing closing brace
        content = re.sub(
            r'(new\[\]\s*\{[^}]+)(\s*\n\s*};)',
            r'\1\n        }\2',
            content
        )
    
    # Fix ComplianceAuditSteps.cs specific issues
    elif filename == "ComplianceAuditSteps.cs":
        # Fix missing closing braces for nested classes
        content = re.sub(
            r'(public\s+class\s+\w+\s*\{[^}]+)(\s*\n\s*public\s+class)',
            r'\1\n    }\n\2',
            content
        )
        
        # Fix missing semicolons after properties
        content = re.sub(
            r'(public\s+\w+(?:<[^>]+>)?\s+\w+\s*\{\s*get;\s*set;\s*\}\s*=\s*[^;]+)(\s*\n)',
            r'\1;\2',
            content
        )
    
    # Fix CommunicationStepDefinitions.cs specific issues
    elif filename == "CommunicationStepDefinitions.cs":
        # Fix method declarations inside constructors
        content = re.sub(
            r'(\s*\{)\s*\n\s*(\s*//[^\n]+)\s*\n\s*(\s*\[(?:Given|When|Then))',
            r'\1\n    }\n\n\2\n\3',
            content
        )
        
        # Fix double semicolons after method calls
        content = re.sub(r'\)\s*;;\s*(\n|$)', ');\n', content)
        
        # Fix switch expression in helper method
        content = re.sub(
            r'(_ => "[^"]+")(\s*};\s*};\s*\n)',
            r'\1\n        };\n    }\n',
            content
        )
    
    # Fix ComplianceStepDefinitions.cs specific issues
    elif filename == "ComplianceStepDefinitions.cs":
        # Fix missing closing braces for method calls
        content = re.sub(
            r'(\s*new Dictionary<string, object>[^}]+\})\s*\)\s*;;',
            r'\1\n            });\n    }',
            content
        )
    
    # General fixes for all files
    
    # Fix missing closing brace at end of method before next method
    content = re.sub(
        r'(\s*})(\s*\n\s*\[(?:When|Then|Given))',
        r'\1\n    }\2',
        content
    )
    
    # Remove extra closing braces at end of file
    while content.rstrip().endswith('}\n}\n}'):
        open_count = content.count('{')
        close_count = content.count('}')
        if close_count > open_count:
            lines = content.rstrip().split('\n')
            lines = lines[:-1]
            content = '\n'.join(lines) + '\n'
        else:
            break
    
    if content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    step_def_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Priority files with specific issues
    priority_files = [
        "CommonUISteps.cs",
        "CommunityFeaturesSteps.cs", 
        "ComplianceAuditSteps.cs",
        "CommunicationStepDefinitions.cs",
        "ComplianceStepDefinitions.cs"
    ]
    
    fixed_files = []
    
    for filename in priority_files:
        filepath = os.path.join(step_def_dir, filename)
        if os.path.exists(filepath):
            if fix_file_specific_issues(filepath):
                fixed_files.append(filename)
    
    print(f"Fixed {len(fixed_files)} files:")
    for f in sorted(fixed_files):
        print(f"  - {f}")

if __name__ == "__main__":
    main()