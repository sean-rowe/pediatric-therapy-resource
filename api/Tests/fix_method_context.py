#!/usr/bin/env python3
import os
import re

def fix_missing_closing_braces_for_methods(file_path):
    """Fix methods that are missing closing braces causing CS0106 errors."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        fixed = False
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            
            # Check if this is a Then method declaration
            if line.startswith('[Then(@') and i + 1 < len(lines):
                # Find the method body
                j = i + 1
                while j < len(lines) and not lines[j].strip().startswith('{'):
                    j += 1
                
                if j < len(lines):
                    # Now find where this method should end
                    brace_count = 0
                    k = j
                    method_start = j
                    
                    while k < len(lines):
                        for char in lines[k]:
                            if char == '{':
                                brace_count += 1
                            elif char == '}':
                                brace_count -= 1
                                
                        if brace_count == 0:
                            # Method should end here
                            break
                        k += 1
                    
                    # Check if the next line after method end is another attribute or region
                    if k + 1 < len(lines):
                        next_line = lines[k + 1].strip()
                        if (next_line.startswith('[') or next_line.startswith('#endregion') or 
                            next_line.startswith('public') or next_line == ''):
                            # We might be missing a closing brace
                            if brace_count > 0:
                                # Insert closing brace
                                lines[k] = lines[k].rstrip() + '\n    }\n'
                                fixed = True
            
            i += 1
        
        if fixed:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(lines)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def fix_orphaned_methods(file_path):
    """Fix methods that appear outside of class context."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Pattern to find methods after #endregion but before the class closing brace
        # These methods should be inside the class
        pattern = r'(#endregion\s*\n)\s*(\[(?:Given|When|Then)\(.*?\]\s*\n\s*public.*?(?:\n.*?)*?\})'
        
        matches = list(re.finditer(pattern, content, re.MULTILINE | re.DOTALL))
        
        if matches:
            # Work backwards to avoid offset issues
            for match in reversed(matches):
                endregion = match.group(1)
                method = match.group(2)
                
                # Find the proper location (before the class closing brace)
                # Look for the helper classes region or the final closing braces
                helper_match = re.search(r'(#region Helper Classes.*?\n)', content[match.end():])
                
                if helper_match:
                    # Insert the method before the helper classes region
                    insert_pos = match.end() + helper_match.start()
                    content = content[:match.start()] + endregion + content[match.end():insert_pos] + '\n' + method + '\n' + content[insert_pos:]
                else:
                    # Find the class closing brace and insert before it
                    # Count braces from the beginning
                    brace_count = 0
                    class_end = -1
                    
                    for i, char in enumerate(content):
                        if char == '{':
                            brace_count += 1
                        elif char == '}':
                            brace_count -= 1
                            if brace_count == 1:  # This is likely the class closing brace
                                class_end = i
                    
                    if class_end > 0:
                        content = content[:match.start()] + endregion + content[match.end():class_end] + '\n' + method + '\n' + content[class_end:]
            
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
    
    return False

def main():
    step_definitions_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions"
    
    # Focus on files with CS0106 errors
    problem_files = ["WcagAccessibilitySteps.cs"]
    
    for filename in problem_files:
        file_path = os.path.join(step_definitions_dir, filename)
        if os.path.exists(file_path):
            if fix_missing_closing_braces_for_methods(file_path):
                print(f"Fixed closing braces in {filename}")
            if fix_orphaned_methods(file_path):
                print(f"Fixed orphaned methods in {filename}")

if __name__ == "__main__":
    main()