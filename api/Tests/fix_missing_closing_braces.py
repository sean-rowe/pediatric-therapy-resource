#!/usr/bin/env python3
import os
import re

def fix_missing_closing_braces(file_path):
    """Fix missing closing braces after throw statements in step definition methods."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Pattern to find methods with throw statements missing closing brace
    # Look for throw statement followed by a new line and then a method attribute or another method
    pattern = r'(throw new NotImplementedException\("Feature not yet implemented - this is expected in BDD"\);)\s*\n\s*(\[(?:Given|When|Then)\(|public\s+(?:void|async Task)|private\s+|protected\s+)'
    
    # Replace with throw statement followed by closing brace
    content = re.sub(pattern, r'\1\n    }\n\n    \2', content)
    
    # Also check for cases where there's no newline between throw and next method
    pattern2 = r'(throw new NotImplementedException\("Feature not yet implemented - this is expected in BDD"\);)\s*(\[(?:Given|When|Then)\()'
    content = re.sub(pattern2, r'\1\n    }\n\n    \2', content)
    
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

# Process all step definition files
step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
fixed_count = 0
fixed_files = []

for filename in os.listdir(step_def_dir):
    if filename.endswith('.cs'):
        file_path = os.path.join(step_def_dir, filename)
        if fix_missing_closing_braces(file_path):
            fixed_count += 1
            fixed_files.append(filename)

print(f"Fixed {fixed_count} files:")
for file in sorted(fixed_files):
    print(f"  - {file}")