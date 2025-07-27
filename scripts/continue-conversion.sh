#!/bin/bash
# Continue converting draft items in smaller batches

PROJECT_NUMBER=2
OWNER="sean-rowe"
REPO="therapy-docs"

echo "🔄 Continuing draft conversion..."

# Process remaining drafts in batches of 20
batch_size=20
converted=0

while [[ $converted -lt 50 ]]; do
    echo "Processing batch starting at item $((converted + 1))..."
    
    # Get next batch of drafts
    gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 | grep "^DraftIssue" | head -$batch_size > /tmp/current_batch.txt
    
    batch_count=$(wc -l < /tmp/current_batch.txt)
    if [[ $batch_count -eq 0 ]]; then
        echo "✅ No more drafts to convert!"
        break
    fi
    
    while IFS= read -r line; do
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
        
        if issue_url=$(gh issue create --repo $OWNER/$REPO --title "$standardized_title" --body "Converted from draft project item" 2>/dev/null); then
            echo "✅ Created: $issue_url"
            gh project item-delete $PROJECT_NUMBER --owner $OWNER --id "$item_id" 2>/dev/null || true
            ((converted++))
        else
            echo "❌ Failed: $title"
        fi
        
        sleep 0.3
    done < /tmp/current_batch.txt
    
    echo "Batch complete. Total converted: $converted"
    sleep 1
done

echo "🎉 Conversion batch complete! Converted $converted items."