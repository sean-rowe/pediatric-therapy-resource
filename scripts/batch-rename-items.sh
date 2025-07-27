#!/bin/bash
# Batch rename and publish TherapyDocs project items

PROJECT_NUMBER=2
OWNER="sean-rowe"

# Get all items that need renaming (non-standard naming)
echo "Finding items that need renaming..."

# Export current items
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/current_items.txt

# Items to rename (Issue number, current title, new title)
declare -a RENAME_ITEMS=(
    # User stories to Story format
    "28|As a therapist, I want to register with license validation so that I can access the system securely|Story: Therapist Registration with License Validation"
    
    # Generic titles to Feature format
    "66|User Management|Feature: User Management System"
    "67|System Configuration|Feature: System Configuration Management"
    
    # Technical stories that need Story prefix
    # Add more as we identify them...
)

# Find items with "Tech:" prefix that should be "Story:"
while IFS= read -r line; do
    if [[ "$line" =~ ^Issue.*Tech: ]]; then
        issue_num=$(echo "$line" | awk -F'\t' '{print $3}')
        current_title=$(echo "$line" | awk -F'\t' '{print $2}')
        new_title=$(echo "$current_title" | sed 's/^Tech: /Story: /')
        
        echo "Renaming #$issue_num: '$current_title' → '$new_title'"
        gh issue edit $issue_num --repo $OWNER/therapy-docs --title "$new_title"
        sleep 0.5
    fi
done < /tmp/current_items.txt

# Find items that are just descriptive and should have "Story:" prefix
while IFS= read -r line; do
    if [[ "$line" =~ ^Issue ]]; then
        issue_num=$(echo "$line" | awk -F'\t' '{print $3}')
        current_title=$(echo "$line" | awk -F'\t' '{print $2}')
        
        # Skip if already has prefix
        if [[ "$current_title" =~ ^(Epic|Story|Feature|Enhancement): ]]; then
            continue
        fi
        
        # Skip the project header
        if [[ "$current_title" == "Project: TherapyDocs Development" ]]; then
            continue
        fi
        
        # Determine appropriate prefix based on content
        new_title=""
        case "$current_title" in
            *"Real-time Authorization Unit Tracking"|*"EDI 837 Claim Submission"|*"EDI 835 Payment Processing"|*"Backup and Recovery")
                new_title="Feature: $current_title"
                ;;
            *"Database Procedures"|*"API Endpoints"|*"UI Components"|*)
                new_title="Story: $current_title"
                ;;
        esac
        
        if [[ -n "$new_title" && "$current_title" != "$new_title" ]]; then
            echo "Renaming #$issue_num: '$current_title' → '$new_title'"
            gh issue edit $issue_num --repo $OWNER/therapy-docs --title "$new_title"
            sleep 0.5
        fi
    fi
done < /tmp/current_items.txt

echo "Batch renaming complete!"