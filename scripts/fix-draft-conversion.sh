#!/bin/bash
# Convert remaining draft items properly using project item-create

PROJECT_NUMBER=2
OWNER="sean-rowe"

echo "🔧 Converting drafts to proper project items..."

# Get remaining drafts
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 | grep "^DraftIssue" > /tmp/remaining_drafts.txt

remaining_count=$(wc -l < /tmp/remaining_drafts.txt)
echo "📋 Found $remaining_count remaining draft items"

converted=0
batch_size=10

while IFS= read -r line && [[ $converted -lt $batch_size ]]; do
    if [[ -z "$line" ]]; then continue; fi
    
    title=$(echo "$line" | awk -F'\t' '{print $2}')
    item_id=$(echo "$line" | awk -F'\t' '{print $5}')
    
    # Skip empty titles
    if [[ -z "$title" ]]; then continue; fi
    
    # Standardize title
    standardized_title="$title"
    case "$title" in
        "Tech:"*)
            standardized_title=$(echo "$title" | sed 's/^Tech: /Story: /')
            ;;
        "Database:"*|"API:"*|"UI:"*)
            standardized_title="Story: $(echo "$title" | sed 's/^[^:]*: //')"
            ;;
        "User Management"|"System Configuration"|"Backup and Recovery")
            standardized_title="Feature: $title"
            ;;
        "ENHANCE:"*)
            standardized_title="Enhancement: $(echo "$title" | sed 's/^ENHANCE: //')"
            ;;
        *)
            if [[ ! "$title" =~ ^(Epic|Story|Feature|Enhancement): ]]; then
                if [[ "$title" =~ ^(Build|Implement|Setup|Configure|Create) ]] && [[ ${#title} -gt 50 ]]; then
                    standardized_title="Feature: $title"
                else
                    standardized_title="Story: $title"
                fi
            fi
            ;;
    esac
    
    echo "📝 Converting: '$title' → '$standardized_title'"
    
    # Create new project item with standardized title
    if new_item=$(gh project item-create $PROJECT_NUMBER --owner $OWNER --title "$standardized_title" --body "Standardized from draft item" 2>/dev/null); then
        echo "✅ Created project item: $standardized_title"
        
        # Remove old draft item
        gh project item-delete $PROJECT_NUMBER --owner $OWNER --id "$item_id" 2>/dev/null || true
        
        ((converted++))
    else
        echo "❌ Failed to create: $standardized_title"
    fi
    
    sleep 0.5
done < /tmp/remaining_drafts.txt

echo ""
echo "✅ Converted $converted draft items to properly named project items"
echo "📋 This batch complete - run again to continue if needed"