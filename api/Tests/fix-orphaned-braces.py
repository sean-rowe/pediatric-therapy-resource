#!/usr/bin/env python3

import os
import re
import glob

def fix_file(filepath):
    """Fix a single file by removing orphaned }; } patterns"""
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    # Look for pattern of orphaned braces
    fixed_lines = []
    i = 0
    while i < len(lines):
        # Check if current line is just whitespace + };
        if re.match(r'^\s*};\s*$', lines[i]):
            # Check if next line exists and is just whitespace + }
            if i + 1 < len(lines) and re.match(r'^\s*}\s*$', lines[i + 1]):
                # Skip both lines - they are orphaned
                print(f"  Removing orphaned braces at line {i+1}")
                i += 2
                continue
        
        fixed_lines.append(lines[i])
        i += 1
    
    # Write back the fixed content
    with open(filepath, 'w') as f:
        f.writelines(fixed_lines)
    
    return len(lines) - len(fixed_lines)

def main():
    # Find all step definition files
    pattern = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/*.cs"
    files = glob.glob(pattern)
    
    print(f"Found {len(files)} step definition files to process")
    
    total_lines_removed = 0
    for filepath in files:
        print(f"Processing: {os.path.basename(filepath)}")
        lines_removed = fix_file(filepath)
        if lines_removed > 0:
            print(f"  Removed {lines_removed} lines")
            total_lines_removed += lines_removed
    
    print(f"\nCompleted! Total lines removed: {total_lines_removed}")

if __name__ == "__main__":
    main()