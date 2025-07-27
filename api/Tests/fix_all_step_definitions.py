#!/usr/bin/env python3
import os
import re

def fix_step_definition_structure(file_path):
    """Fix common structural issues in step definition files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # 1. Fix missing closing braces for foreach loops
        # Pattern: response.StatusCode.Should().Be(...); at end of foreach without closing brace
        pattern = r'(\s*)(response\.StatusCode\.Should\(\)\.Be\([^)]+\);)\s*\n(\s*)\}'
        def check_and_fix_foreach(match):
            indent1 = match.group(1)
            statement = match.group(2)
            indent2 = match.group(3)
            closing_brace = match.group(0).split('}')[-1]
            
            # If indent2 is less than indent1, we're missing a closing brace
            if len(indent2) < len(indent1):
                return indent1 + statement + '\n' + indent1 + '}}\n' + indent2 + '}'
            return match.group(0)
        
        content = re.sub(pattern, check_and_fix_foreach, content)
        
        # 2. Fix methods missing closing braces
        # Pattern: Method that ends with Should().BeTrue() but next line is a method attribute
        pattern = r'(\s*[^;]+\.Should\(\)\.Be(?:True|False|Null)\(\);)\s*\n(\s*)(\[(?:Given|When|Then)\()'
        content = re.sub(pattern, r'\1\n    }\n\n\2\3', content)
        
        # 3. Fix regions with extra closing braces
        pattern = r'(#endregion.*?)\n\s*\}\s*\n\s*\n'
        content = re.sub(pattern, r'\1\n\n', content)
        
        # 4. Fix orphaned closing braces (line with just })
        pattern = r'\n\s*\}\s*\n(\s*\[(?:Given|When|Then)\()'
        content = re.sub(pattern, r'\n\1', content)
        
        # 5. Fix double closing braces at method end
        pattern = r'(\s*)\}\s*\n\s*\}\s*\n(\s*\[(?:Given|When|Then)\()'
        content = re.sub(pattern, r'\1}\n\n\2', content)
        
        # 6. Remove trailing extra closing braces at end of file
        # Count braces
        open_count = content.count('{')
        close_count = content.count('}')
        
        if close_count > open_count:
            # Remove extra closing braces from the end
            extra = close_count - open_count
            lines = content.split('\n')
            
            # Work backwards removing lines that are just closing braces
            removed = 0
            for i in range(len(lines) - 1, -1, -1):
                if removed >= extra:
                    break
                if lines[i].strip() == '}':
                    lines[i] = ''
                    removed += 1
            
            content = '\n'.join(line for line in lines if line != '')
        
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
            if fix_step_definition_structure(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()