#!/usr/bin/env python3
import re

def fix_wcag_complete():
    """Completely fix the WcagAccessibilitySteps.cs file."""
    
    file_path = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/WcagAccessibilitySteps.cs"
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove the duplicate closing brace after #region WCAG Level AA Compliance Steps
    content = re.sub(r'(#region WCAG Level AA Compliance Steps\s*\n)\s*\}\s*\n', r'\1', content)
    
    # Fix missing closing braces after foreach loops
    # Pattern: Find foreach loops that end with response.StatusCode.Should().Be... but no closing brace
    def fix_foreach_closing(match):
        indent = match.group(1)
        statement = match.group(2)
        return f'{indent}{statement}\n{indent}    }}'
    
    # Fix foreach loops missing closing braces
    content = re.sub(
        r'^(\s*)(response\.StatusCode\.Should\(\)\.Be\(HttpStatusCode\.Created\);)$(?!\n\s*\})',
        fix_foreach_closing,
        content,
        flags=re.MULTILINE
    )
    
    # Remove orphaned code between methods
    # Remove the orphaned userTesting code
    content = re.sub(
        r'\n\s*\}\s*\n\s*\};\s*\n\s*var json = JsonSerializer\.Serialize\(userTesting\);[\s\S]*?response\.StatusCode\.Should\(\)\.Be\(HttpStatusCode\.Created\);\s*\n',
        '\n    }\n',
        content
    )
    
    # Fix missing closing braces for Then methods
    # Pattern: Methods that end without proper closing brace
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        fixed_lines.append(line)
        
        # Check if this is the end of a Then method that needs a closing brace
        if i > 0 and 'Should().BeTrue();' in line and i + 1 < len(lines):
            next_line = lines[i + 1] if i + 1 < len(lines) else ''
            # If next line is a Then attribute or region, we need a closing brace
            if '[Then(@' in next_line or '#endregion' in next_line:
                indent = len(line) - len(line.lstrip())
                fixed_lines.append(' ' * (indent - 4) + '}')
        
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # Ensure the file ends with proper closing braces for namespace and class
    if not content.rstrip().endswith('}'):
        # Count open and close braces
        open_count = content.count('{')
        close_count = content.count('}')
        missing = open_count - close_count
        
        if missing > 0:
            content = content.rstrip() + '\n' + '}'.join([''] * (missing + 1))
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print("Fixed WcagAccessibilitySteps.cs completely")

if __name__ == "__main__":
    fix_wcag_complete()