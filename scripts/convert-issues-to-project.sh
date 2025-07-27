#!/bin/bash

# Convert repository issues to project draft items
# Usage: ./convert-issues-to-project.sh START_ISSUE END_ISSUE

START=${1:-14}
END=${2:-30}
PROJECT_NUMBER=2  # TherapyDocs Development project

echo "Converting issues #$START to #$END to project items..."
echo "Target project: TherapyDocs Development (ID: $PROJECT_NUMBER)"
echo ""

# Track success/failure
SUCCESS_COUNT=0
FAIL_COUNT=0

for i in $(seq $START $END); do
    echo "Processing issue #$i..."
    
    # Get issue details
    ISSUE_JSON=$(gh issue view $i --json number,title,body,state 2>/dev/null)
    
    if [ -z "$ISSUE_JSON" ]; then
        echo "  ❌ Issue #$i not found"
        ((FAIL_COUNT++))
        continue
    fi
    
    # Extract fields
    TITLE=$(echo "$ISSUE_JSON" | jq -r '.title')
    BODY=$(echo "$ISSUE_JSON" | jq -r '.body')
    STATE=$(echo "$ISSUE_JSON" | jq -r '.state')
    
    if [ "$STATE" != "OPEN" ]; then
        echo "  ⚠️  Issue #$i is already $STATE, skipping..."
        continue
    fi
    
    echo "  📋 Title: $TITLE"
    
    # Create project item
    echo "  🔄 Creating project item..."
    PROJECT_ITEM=$(gh project item-create $PROJECT_NUMBER \
        --owner sean-rowe \
        --title "$TITLE" \
        --body "$BODY" 2>&1)
    
    if [ $? -eq 0 ]; then
        echo "  ✅ Project item created successfully"
        
        # Close the issue
        echo "  🔄 Closing issue #$i..."
        gh issue close $i --comment "Converted to project item in TherapyDocs Development project" 2>&1
        
        if [ $? -eq 0 ]; then
            echo "  ✅ Issue #$i closed successfully"
            ((SUCCESS_COUNT++))
        else
            echo "  ❌ Failed to close issue #$i"
            ((FAIL_COUNT++))
        fi
    else
        echo "  ❌ Failed to create project item: $PROJECT_ITEM"
        ((FAIL_COUNT++))
    fi
    
    echo ""
    
    # Add a small delay to avoid rate limiting
    sleep 1
done

echo "========================================="
echo "Conversion complete!"
echo "✅ Successfully converted: $SUCCESS_COUNT issues"
echo "❌ Failed: $FAIL_COUNT issues"
echo "========================================="