#!/usr/bin/env python3
import os
import re

def fix_incomplete_arrays(file_path):
    """Fix arrays that have missing closing braces and method braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern to match arrays that end with a string literal without closing braces
        # This will match cases where array ends but method/array closing braces are missing
        # Match: "some string"\n\n    // comment or [attribute]
        pattern = r'(".*?")\s*\n\s*\n(\s*)(//.*?|\s*\[)'
        
        # Replace with the string, then proper closing braces
        def replacement(match):
            string_part = match.group(1)
            indent = match.group(2)
            rest = match.group(3)
            # Reduce indent for closing braces
            close_indent = indent[4:] if len(indent) >= 4 else ""
            return f'{string_part}\n{close_indent}        }};\n{close_indent}    }}\n\n{indent}{rest}'
        
        new_content = re.sub(pattern, replacement, content, flags=re.MULTILINE)
        
        # Another pattern: fix where we have just empty lines between array end and next method
        pattern2 = r'(".*?")\s*\n\s*\n\s*\n(\s*)(\[(?:Given|When|Then))'
        def replacement2(match):
            string_part = match.group(1)
            indent = match.group(2)
            attribute = match.group(3)
            return f'{string_part}\n        }};\n    }}\n\n{indent}{attribute}'
        
        new_content = re.sub(pattern2, replacement2, new_content, flags=re.MULTILINE)
        
        if new_content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_count = 0
    for filename in os.listdir(step_definitions_dir):
        if filename.endswith('.cs'):
            file_path = os.path.join(step_definitions_dir, filename)
            if fix_incomplete_arrays(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()