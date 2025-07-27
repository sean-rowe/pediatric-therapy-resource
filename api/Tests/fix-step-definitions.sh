#!/bin/bash

# Script to fix step definition files with orphaned closing braces

echo "Fixing step definition files..."

# Find all step definition files
find /home/srowe/workspace/pediatric-therapy-resource/api/Tests/BDD/StepDefinitions -name "*.cs" | while read file; do
    echo "Processing: $file"
    
    # Create a temporary file
    temp_file="${file}.tmp"
    
    # Process the file to remove orphaned }; patterns
    # This removes lines that have only whitespace followed by }; and then a closing }
    awk '
    {
        # Store current line
        lines[NR] = $0
    }
    END {
        skip_next = 0
        for (i = 1; i <= NR; i++) {
            if (skip_next) {
                skip_next = 0
                continue
            }
            
            # Check if current line is just whitespace + };
            if (lines[i] ~ /^[[:space:]]*};[[:space:]]*$/) {
                # Check if next line is just whitespace + }
                if (i+1 <= NR && lines[i+1] ~ /^[[:space:]]*}[[:space:]]*$/) {
                    # Skip both lines - they are orphaned
                    skip_next = 1
                    continue
                }
            }
            
            # Print the line
            print lines[i]
        }
    }
    ' "$file" > "$temp_file"
    
    # Replace the original file
    mv "$temp_file" "$file"
done

echo "Completed fixing step definition files"