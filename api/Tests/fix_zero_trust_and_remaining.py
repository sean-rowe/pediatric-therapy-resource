#!/usr/bin/env python3
import os
import re

def fix_remaining_issues(file_path):
    """Fix remaining structural issues in step definition files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i]
            stripped = line.strip()
            
            # Check if this looks like the end of an anonymous object that's missing closing brace
            if i > 0 and re.match(r'^[A-Za-z_][A-Za-z0-9_]*\s*=\s*["\'].*["\']$', stripped):
                # Look ahead to see if next line is "var" without closing brace
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    if next_line.startswith('var ') and i + 2 < len(lines):
                        # Check if we're missing a closing brace
                        look_back = 5
                        found_opening = False
                        j = i - 1
                        while j >= 0 and j > i - look_back:
                            if 'new' in lines[j] and '{' in lines[j]:
                                found_opening = True
                                break
                            j -= 1
                        
                        if found_opening:
                            # Add the current line
                            fixed_lines.append(line)
                            # Add closing brace with semicolon
                            fixed_lines.append('            };\n')
                            i += 1
                            continue
            
            # Fix the specific pattern where we have extra closing braces at end of file
            if stripped == '}' and i == len(lines) - 1:
                # Count braces in the entire file
                content = ''.join(fixed_lines + [line])
                open_count = content.count('{')
                close_count = content.count('}')
                
                if close_count >= open_count:
                    # Skip this extra closing brace
                    i += 1
                    continue
            
            # Fix lines that have }; where it should just be }
            if stripped == '};' and i > 0:
                prev_line = lines[i-1].strip() if i > 0 else ''
                # Check if this is NOT after an anonymous object
                if not re.match(r'^[A-Za-z_][A-Za-z0-9_]*\s*=\s*.+', prev_line):
                    # This is likely end of a method or class, should just be }
                    fixed_lines.append(line.replace('};', '}'))
                    i += 1
                    continue
            
            fixed_lines.append(line)
            i += 1
        
        # Remove trailing empty lines and extra closing braces
        while fixed_lines and (fixed_lines[-1].strip() == '' or fixed_lines[-1].strip() == '}'):
            if fixed_lines[-1].strip() == '}':
                # Check if this closing brace is needed
                temp_content = ''.join(fixed_lines[:-1])
                open_count = temp_content.count('{')
                close_count = temp_content.count('}')
                if close_count >= open_count:
                    fixed_lines.pop()
                else:
                    break
            else:
                fixed_lines.pop()
        
        # Ensure file ends with single closing brace for namespace
        if fixed_lines and not fixed_lines[-1].strip() == '}':
            fixed_lines.append('}\n')
        
        content = ''.join(fixed_lines)
        
        # Fix specific patterns
        # Pattern: Property = "value"\n\nvar (missing closing brace)
        pattern = r'(\n\s+[A-Za-z_][A-Za-z0-9_]*\s*=\s*["\'][^"\']+["\']\s*\n)\s*\n(\s*var\s+)'
        content = re.sub(pattern, r'\1            };\n\n\2', content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Process specific problem files first
    problem_files = [
        "ZeroTrustSteps.cs",
        "SoxComplianceSteps.cs",
        "WcagAccessibilitySteps.cs",
        "AdultTherapySteps.cs",
        "ParentPortalSteps.cs",
        "OutcomeMeasurementSteps.cs",
        "FerpaComplianceSteps.cs"
    ]
    
    print("Processing specific problem files...")
    for filename in problem_files:
        file_path = os.path.join(step_definitions_dir, filename)
        if os.path.exists(file_path):
            if fix_remaining_issues(file_path):
                print(f"Fixed: {filename}")
    
    print("\nProcessing all files...")
    # Then process all files
    cs_files = []
    for root, dirs, files in os.walk(step_definitions_dir):
        for file in files:
            if file.endswith('.cs'):
                cs_files.append(os.path.join(root, file))
    
    fixed_count = 0
    for file_path in sorted(cs_files):
        if fix_remaining_issues(file_path):
            fixed_count += 1
    
    print(f"\nTotal files processed: {fixed_count}")

if __name__ == "__main__":
    main()