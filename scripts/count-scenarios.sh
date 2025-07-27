#!/bin/bash

echo "Counting Gherkin scenarios in feature files..."
echo "============================================"

total_scenarios=0
total_files=0

# Find all .feature files and count scenarios
for file in $(find features -name "*.feature" | sort); do
    # Count lines that start with "Scenario:" or "Scenario Outline:"
    scenarios=$(grep -c "^\s*Scenario\( Outline\)\?:" "$file")
    total_scenarios=$((total_scenarios + scenarios))
    total_files=$((total_files + 1))
    
    # Print file and scenario count
    printf "%-60s %3d scenarios\n" "$file" "$scenarios"
done

echo "============================================"
echo "Total feature files: $total_files"
echo "Total scenarios: $total_scenarios"
echo ""

# Count endpoints marked as not-implemented
not_implemented=$(grep -r "@not-implemented" features --include="*.feature" | wc -l)
echo "Endpoints marked @not-implemented: $not_implemented"

# Count unique endpoint paths
echo ""
echo "Counting unique API endpoint paths..."
endpoints=$(grep -r "I send a .* request to" features --include="*.feature" | \
    sed -E 's/.*"(\/api[^"]+)".*/\1/' | \
    sed -E 's/\{[^}]+\}/\{id\}/g' | \
    sort | uniq | wc -l)
echo "Unique API endpoint patterns: $endpoints"