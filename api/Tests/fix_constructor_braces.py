#!/usr/bin/env python3
import os
import re

def fix_constructor_missing_brace(file_path):
    """Fix constructors that are missing their closing brace."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern to find constructors that don't have closing braces
        # Match: constructor declaration with opening brace followed directly by a [Given/When/Then attribute
        pattern = r'(public\s+\w+Steps\s*\([^)]+\)\s*:\s*base\s*\([^)]+\)\s*\n\s*\{)\s*\n(\s*\[(?:Given|When|Then)\()'
        
        def fix_constructor(match):
            constructor = match.group(1)
            attribute = match.group(2)
            # Add closing brace for constructor
            return constructor + '\n    }\n\n' + attribute
        
        content = re.sub(pattern, fix_constructor, content, flags=re.MULTILINE)
        
        # Also fix methods that are missing closing braces
        # Pattern: method that ends with throw statement but next line is an attribute
        pattern2 = r'(throw new NotImplementedException\("Feature not yet implemented - this is expected in BDD"\);)\s*\n(\s*\[(?:Given|When|Then)\()'
        
        def fix_method(match):
            throw_statement = match.group(1)
            attribute = match.group(2)
            return throw_statement + '\n    }\n' + attribute
        
        content = re.sub(pattern2, fix_method, content, flags=re.MULTILINE)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    fixed_count = 0
    for filename in os.listdir(step_definitions_dir):
        if filename.endswith('.cs') and filename != 'BaseStepDefinitions.cs':
            file_path = os.path.join(step_definitions_dir, filename)
            if fix_constructor_missing_brace(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()