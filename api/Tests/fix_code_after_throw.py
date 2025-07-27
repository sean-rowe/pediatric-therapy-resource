#!/usr/bin/env python3
import os
import re

def fix_code_after_throw(file_path):
    """Remove unreachable code after throw statements."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Pattern to find throw statements followed by code (not just a closing brace)
    # This will find throw statements and capture everything until the next method or closing brace
    pattern = r'(throw new NotImplementedException\([^)]+\);\s*\n\s*})(\s*\n)([^}#\[])'
    
    modified_content = content
    while True:
        # Replace pattern - keep the throw and closing brace, but remove code after
        new_content = re.sub(pattern, r'\1\2    #endregion', modified_content, count=1)
        if new_content == modified_content:
            break
        modified_content = new_content
    
    # Also handle cases where there's code directly after throw without closing brace
    pattern2 = r'(throw new NotImplementedException\([^)]+\);\s*\n)(\s*)([^}].*?)(\n\s*(?:\[|public|private|protected|#endregion))'
    modified_content = re.sub(pattern2, r'\1\2}\n\n\4', modified_content, flags=re.DOTALL)
    
    if modified_content != content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(modified_content)
        return True
    
    return False

# Process specific files that have this issue
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
files_to_fix = ['WcagAccessibilitySteps.cs', 'SoxComplianceSteps.cs', 'AuditTrailSteps.cs', 'ZeroTrustSteps.cs']
fixed_count = 0

for filename in files_to_fix:
    file_path = os.path.join(step_def_dir, filename)
    if os.path.exists(file_path):
        if fix_code_after_throw(file_path):
            fixed_count += 1
            print(f"Fixed: {filename}")

print(f"\nTotal files fixed: {fixed_count}")