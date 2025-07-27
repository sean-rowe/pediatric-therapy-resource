#!/bin/bash
# Convert draft project items to published issues
# This handles the 255 draft items in the TherapyDocs project

PROJECT_NUMBER=2
OWNER="sean-rowe"
REPO="therapy-docs"

echo "🔄 Converting draft items to published issues..."

# Get all draft items
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 | grep "^DraftIssue" > /tmp/draft_items.txt

draft_count=$(wc -l < /tmp/draft_items.txt)
echo "📋 Found $draft_count draft items to convert"

converted=0
failed=0

while IFS= read -r line; do
    if [[ -z "$line" ]]; then
        continue
    fi
    
    # Extract title and item ID
    title=$(echo "$line" | awk -F'\t' '{print $2}')
    item_id=$(echo "$line" | awk -F'\t' '{print $5}')
    
    # Skip empty titles or project header
    if [[ -z "$title" ]] || [[ "$title" == "Project: TherapyDocs Development" ]]; then
        continue
    fi
    
    # Standardize the title
    standardized_title="$title"
    case "$title" in
        "User Management"|"System Configuration"|"Backup and Recovery"|"Real-time Authorization Unit Tracking"|"EDI 837 Claim Submission"|"EDI 835 Payment Processing")
            standardized_title="Feature: $title"
            ;;
        "Tech:"*)
            standardized_title=$(echo "$title" | sed 's/^Tech: /Story: /')
            ;;
        "Database:"*|"API:"*|"UI:"*)
            standardized_title="Story: $(echo "$title" | sed 's/^[^:]*: //')"
            ;;
        "ENHANCE:"*)
            standardized_title="Enhancement: $(echo "$title" | sed 's/^ENHANCE: //')"
            ;;
        *)
            # If no prefix, default to Story unless it's clearly a large feature
            if [[ ! "$title" =~ ^(Epic|Story|Feature|Enhancement): ]] && [[ ${#title} -lt 80 ]]; then
                if [[ "$title" =~ ^(Build|Implement|Setup|Configure|Create) ]] && [[ ${#title} -gt 50 ]]; then
                    standardized_title="Feature: $title"
                else
                    standardized_title="Story: $title"
                fi
            fi
            ;;
    esac
    
    echo "Converting: '$title' → '$standardized_title'"
    
    # Create a new issue with the standardized title
    if issue_url=$(gh issue create --repo $OWNER/$REPO --title "$standardized_title" --body "Converted from draft project item: $item_id" --label "converted-draft" 2>/dev/null); then
        echo "✅ Created: $issue_url"
        
        # Try to remove the draft item from project (optional - may fail)
        gh project item-delete $PROJECT_NUMBER --owner $OWNER --id "$item_id" 2>/dev/null || true
        
        ((converted++))
    else
        echo "❌ Failed to create issue for: $title"
        ((failed++))
    fi
    
    # Rate limiting
    sleep 0.5
    
done < /tmp/draft_items.txt

echo ""
echo "📊 Conversion Summary:"
echo "  ✅ Converted: $converted"
echo "  ❌ Failed: $failed"
echo "  📋 Total processed: $((converted + failed))"
echo ""
echo "🎉 Draft conversion complete!"