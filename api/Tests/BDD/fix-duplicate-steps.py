#!/usr/bin/env python3
import os
import re

# Define common step patterns that should only exist in CommonStepDefinitions
common_patterns = [
    r'\[Given\(@"the API is available"\)\]',
    r'\[Given\(@"I am authenticated as ""(.*)"""\)\]',
    r'\[When\(@"I send a GET request to ""(.*)"""\)\]',
    r'\[When\(@"I send a POST request to ""(.*)"" with:"\)\]',
    r'\[When\(@"I send a PUT request to ""(.*)"" with:"\)\]',
    r'\[When\(@"I send a DELETE request to ""(.*)"""\)\]',
    r'\[Then\(@"the response status should be (.*)"\)\]',
    r'\[Then\(@"the response should contain ""(.*)"""\)\]',
    r'\[Then\(@"the response should contain:"\)\]',
]

# Directory containing step definitions
step_defs_dir = "/home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions/"

# Files to exclude from cleanup (these should keep the common steps)
exclude_files = ["CommonStepDefinitions.cs", "BaseStepDefinitions.cs", "AuthenticationSteps.cs"]

def remove_duplicate_steps(filepath):
    """Remove duplicate common step definitions from a file."""
    with open(filepath, 'r') as f:
        content = f.read()
    
    original_content = content
    
    # For each common pattern, find and remove the entire method
    for pattern in common_patterns:
        # Find all occurrences
        matches = list(re.finditer(pattern, content))
        
        # Process matches in reverse order to maintain indices
        for match in reversed(matches):
            # Find the start of the method (look backwards for method signature)
            start = match.start()
            
            # Look backwards to find the start of the attribute
            while start > 0 and content[start-1] != '\n':
                start -= 1
            
            # Skip whitespace before attribute
            while start > 0 and content[start-1] in ' \t':
                start -= 1
            if start > 0 and content[start-1] == '\n':
                start -= 1
            
            # Find the end of the method (count braces)
            end = match.end()
            brace_count = 0
            in_method = False
            
            while end < len(content):
                if content[end] == '{':
                    in_method = True
                    brace_count += 1
                elif content[end] == '}' and in_method:
                    brace_count -= 1
                    if brace_count == 0:
                        end += 1
                        # Skip any trailing whitespace/newlines
                        while end < len(content) and content[end] in ' \t\n':
                            end += 1
                        break
                end += 1
            
            # Remove the method
            content = content[:start] + content[end:]
    
    # Clean up any duplicate empty lines
    content = re.sub(r'\n\s*\n\s*\n', '\n\n', content)
    
    # Only write if changes were made
    if content != original_content:
        with open(filepath, 'w') as f:
            f.write(content)
        return True
    return False

# Process all step definition files
changes_made = 0
for filename in os.listdir(step_defs_dir):
    if filename.endswith("Steps.cs") and filename not in exclude_files:
        filepath = os.path.join(step_defs_dir, filename)
        if remove_duplicate_steps(filepath):
            print(f"Cleaned duplicate steps from: {filename}")
            changes_made += 1

print(f"\nTotal files cleaned: {changes_made}")