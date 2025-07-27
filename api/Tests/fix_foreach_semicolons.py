#!/usr/bin/env python3
import os
import re

def fix_foreach_closing_braces():
    """Fix foreach loops that have ); instead of just }."""
    
    step_def_dir = '/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions'
    fixed_count = 0
    
    for filename in os.listdir(step_def_dir):
        if filename.endswith('.cs'):
            file_path = os.path.join(step_def_dir, filename)
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Replace }; at the end of foreach loops with just }
            # Pattern: find lines that have just }; and are likely at the end of a foreach
            original_content = content
            content = re.sub(r'(\s+)\};(\s*\n)', r'\1}\2', content)
            
            # Also fix the pattern where foreach closing brace is followed by semicolon
            content = re.sub(r'(\s+)\}(\s*);(\s*\n)', r'\1}\3', content)
            
            if content != original_content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                fixed_count += 1
                print(f"Fixed {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

fix_foreach_closing_braces()