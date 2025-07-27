#!/usr/bin/env python3
import os
import re

def fix_array_initializer_semicolons(file_path):
    """Add missing semicolons after array initializers and fix foreach semicolons."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern 1: Fix array initializers without semicolons
        # Match patterns like:
        # } on a line by itself after array elements, not followed by semicolon
        pattern1 = r'(\n\s*"[^"]*"(?:,\s*\n\s*"[^"]*")*\s*\n\s*)}(\s*\n)'
        new_content = re.sub(pattern1, r'\1};\2', content, flags=re.MULTILINE)
        
        # Pattern 2: Fix new[] array initializers
        # Match new[] { ... } without semicolon
        pattern2 = r'(new\[\]\s*\{[^}]*\})\s*(?=\n)'
        new_content = re.sub(pattern2, r'\1;', new_content, flags=re.MULTILINE | re.DOTALL)
        
        # Pattern 3: Fix semicolons after closing brace in foreach loops
        # Remove semicolons after } inside foreach/for/while loops
        pattern3 = r'(foreach\s*\([^)]+\)\s*\{[^}]*\});\s*(?=\n)'
        new_content = re.sub(pattern3, r'\1', new_content, flags=re.MULTILINE | re.DOTALL)
        
        # Pattern 4: Fix trailing semicolons after adding to collections in loops
        # Match patterns like: trackingMetrics[row["Metric"]] = row["Purpose"]; };
        pattern4 = r'(\[[^\]]+\]\s*=\s*[^;]+);\s*};'
        new_content = re.sub(pattern4, r'\1;\n        }', new_content)
        
        # Pattern 5: Fix closing braces after property assignment without semicolon
        # Match patterns ending with string/int/bool literals followed by } without semicolon  
        pattern5 = r'(=\s*(?:"[^"]*"|true|false|\d+))\s*}(\s*(?:\n|$))'
        new_content = re.sub(pattern5, r'\1\n        };\2', new_content, flags=re.MULTILINE)
        
        # Pattern 6: Fix standalone closing braces after array content
        # Match a line with just whitespace and }
        lines = new_content.split('\n')
        fixed_lines = []
        i = 0
        while i < len(lines):
            line = lines[i]
            # Check if this line is just whitespace + }
            if re.match(r'^\s*}\s*$', line):
                # Look at previous non-empty line
                j = i - 1
                while j >= 0 and lines[j].strip() == '':
                    j -= 1
                if j >= 0:
                    prev_line = lines[j].strip()
                    # If previous line ends with a quote (array element) and no comma
                    if prev_line.endswith('"') and not prev_line.endswith('",'):
                        fixed_lines.append(line + ';')
                    else:
                        fixed_lines.append(line)
                else:
                    fixed_lines.append(line)
            else:
                fixed_lines.append(line)
            i += 1
        
        new_content = '\n'.join(fixed_lines)
        
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
            if fix_array_initializer_semicolons(file_path):
                fixed_count += 1
                print(f"Fixed: {filename}")
    
    print(f"\nTotal files fixed: {fixed_count}")

if __name__ == "__main__":
    main()