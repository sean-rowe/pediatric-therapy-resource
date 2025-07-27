#\!/usr/bin/env python3
import os
import re

def fix_comprehensive_structure():
    """Fix comprehensive structural issues in step definition files"""
    step_def_dir = 'BDD/StepDefinitions'
    cs_files = [f for f in os.listdir(step_def_dir) if f.endswith('.cs') and not f.endswith('.feature.cs')]
    
    total_fixed = 0
    
    for file in cs_files:
        if file in ['BaseStepDefinitions.cs', 'TestBase.cs']:
            continue
            
        file_path = os.path.join(step_def_dir, file)
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original_content = content
            
            # Fix 1: Dictionary elements outside braces
            # Pattern: })\n[property] = value
            content = re.sub(
                r'}\s*\)\s*\n(\s*)\[("[^"]+" < /dev/null | \'[^\']+\')\]\s*=\s*([^,\n]+),?',
                r'    \1[\2] = \3\n\1});',
                content
            )
            
            # Fix 2: Fix methods with missing closing braces
            # Look for methods that have dictionary initialization but missing closing brace
            lines = content.split('\n')
            fixed_lines = []
            i = 0
            
            while i < len(lines):
                line = lines[i]
                fixed_lines.append(line)
                
                # Check if this is a dictionary closing that needs the method closing
                if (line.strip() == '});' and 
                    i + 1 < len(lines) and 
                    lines[i + 1].strip() == '}' and
                    i + 2 < len(lines) and
                    (lines[i + 2].strip().startswith('[') or lines[i + 2].strip() == '')):
                    # This looks correct, keep going
                    pass
                elif (line.strip() == '});' and 
                      i + 1 < len(lines) and 
                      lines[i + 1].strip().startswith('[')):
                    # Missing method closing brace
                    fixed_lines.append('    }')
                    
                i += 1
            
            content = '\n'.join(fixed_lines)
            
            # Fix 3: Remove excessive closing braces at end of file
            # Count the number of class/namespace opens vs closes
            open_braces = content.count('{')
            close_braces = content.count('}')
            
            if close_braces > open_braces:
                # Remove extra closing braces from the end
                lines = content.split('\n')
                while lines and close_braces > open_braces:
                    if lines[-1].strip() == '}':
                        lines.pop()
                        close_braces -= 1
                    elif lines[-1].strip() == '':
                        lines.pop()
                    else:
                        break
                content = '\n'.join(lines)
            
            # Fix 4: Ensure proper file ending
            content = content.rstrip()
            if not content.endswith('}'):
                content += '\n}'
            else:
                # Ensure exactly one newline before final brace
                content = content.rstrip() + '\n}'
            
            # Fix 5: Fix specific patterns in AIQualityAssuranceSteps
            if file == 'AIQualityAssuranceSteps.cs':
                # Fix the specific pattern where dictionary elements are outside
                content = re.sub(
                    r'{\s*}\s*}\s*}\s*\[("[^"]+")\]\s*=\s*([^;]+);',
                    r'{\n                [\1] = \2\n            });',
                    content
                )
                
                # Fix the pattern where methods have wrong structure
                content = re.sub(
                    r'var validations = new Dictionary<string, object>\(\);\s*}\);\s*foreach',
                    r'var validations = new Dictionary<string, object>();\n        foreach',
                    content
                )
            
            # Fix 6: Fix specific patterns in ZeroTrustSteps
            if file == 'ZeroTrustSteps.cs':
                # Remove the excessive closing braces and #endregion duplicates
                lines = content.split('\n')
                fixed_lines = []
                seen_final_endregion = False
                
                for line in lines:
                    if line.strip() == '#endregion' and seen_final_endregion:
                        continue  # Skip duplicate #endregion
                    if line.strip() == '}' and seen_final_endregion:
                        continue  # Skip extra closing braces after final #endregion
                    if 'public class DeviceCertificateValidation' in line:
                        seen_final_endregion = True
                    fixed_lines.append(line)
                
                content = '\n'.join(fixed_lines)
            
            if content \!= original_content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"Fixed {file}")
                total_fixed += 1
                
        except Exception as e:
            print(f"Error processing {file}: {e}")
    
    print(f"\nTotal files fixed: {total_fixed}")
    return total_fixed

if __name__ == "__main__":
    fix_comprehensive_structure()
