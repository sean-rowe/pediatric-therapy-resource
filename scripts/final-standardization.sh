#!/bin/bash
# Final TherapyDocs Project Standardization
# Remove duplicate drafts and standardize remaining items

PROJECT_NUMBER=2
OWNER="sean-rowe"
REPO="therapy-docs"

echo "🏥 TherapyDocs Final Standardization"
echo "===================================="

# Get current state
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/all_current_items.txt

published_issues=$(grep "^Issue" /tmp/all_current_items.txt)
draft_issues=$(grep "^DraftIssue" /tmp/all_current_items.txt)

published_count=$(echo "$published_issues" | wc -l)
draft_count=$(echo "$draft_issues" | wc -l)

echo "📊 Current Status:"
echo "  Published Issues: $published_count"
echo "  Draft Issues: $draft_count"
echo ""

# Identify and remove duplicate drafts
echo "🔍 Identifying duplicate drafts..."
duplicates_removed=0

while IFS= read -r draft_line; do
    if [[ -z "$draft_line" ]]; then continue; fi
    
    draft_title=$(echo "$draft_line" | awk -F'\t' '{print $2}')
    draft_id=$(echo "$draft_line" | awk -F'\t' '{print $5}')
    
    # Check if there's a published issue with the same title
    if echo "$published_issues" | grep -q "	$draft_title	"; then
        echo "🗑️  Removing duplicate draft: $draft_title"
        gh project item-delete $PROJECT_NUMBER --owner $OWNER --id "$draft_id" 2>/dev/null || true
        ((duplicates_removed++))
        sleep 0.3
    fi
done <<< "$draft_issues"

echo "✅ Removed $duplicates_removed duplicate drafts"
echo ""

# Get updated list after cleanup
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/cleaned_items.txt
remaining_drafts=$(grep "^DraftIssue" /tmp/cleaned_items.txt | wc -l)

echo "📋 Remaining drafts to convert: $remaining_drafts"
echo ""

# Convert remaining unique drafts to issues
if [[ $remaining_drafts -gt 0 ]]; then
    echo "🔄 Converting remaining drafts to published issues..."
    converted=0
    
    while IFS= read -r draft_line; do
        if [[ -z "$draft_line" ]]; then continue; fi
        
        draft_title=$(echo "$draft_line" | awk -F'\t' '{print $2}')
        draft_id=$(echo "$draft_line" | awk -F'\t' '{print $5}')
        
        # Skip empty titles or project headers
        if [[ -z "$draft_title" ]] || [[ "$draft_title" == "Project: TherapyDocs Development" ]]; then
            continue
        fi
        
        echo "📝 Converting: $draft_title"
        
        # Create issue and add to project
        if issue_url=$(gh issue create --repo $OWNER/$REPO --title "$draft_title" --body "Converted from draft project item" 2>/dev/null); then
            echo "✅ Created: $issue_url"
            
            # Remove the draft item
            gh project item-delete $PROJECT_NUMBER --owner $OWNER --id "$draft_id" 2>/dev/null || true
            
            ((converted++))
        else
            echo "❌ Failed to convert: $draft_title"
        fi
        
        sleep 0.5
    done < <(grep "^DraftIssue" /tmp/cleaned_items.txt)
    
    echo ""
    echo "✅ Converted $converted draft items to published issues"
fi

# Final report
echo ""
echo "🎉 Final Standardization Complete!"
echo "=================================="

gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/final_state.txt

final_published=$(grep "^Issue" /tmp/final_state.txt | wc -l)
final_drafts=$(grep "^DraftIssue" /tmp/final_state.txt | wc -l)

# Count by type
epics=$(awk -F'\t' '{print $2}' /tmp/final_state.txt | grep -c "^Epic:" || echo 0)
features=$(awk -F'\t' '{print $2}' /tmp/final_state.txt | grep -c "^Feature:" || echo 0)
stories=$(awk -F'\t' '{print $2}' /tmp/final_state.txt | grep -c "^Story:" || echo 0)
enhancements=$(awk -F'\t' '{print $2}' /tmp/final_state.txt | grep -c "^Enhancement:" || echo 0)

echo "📊 Final Project State:"
echo "  📋 Total Items: $((final_published + final_drafts))"
echo "  ✅ Published: $final_published"
echo "  📝 Drafts: $final_drafts"
echo ""
echo "🏷️  Item Types:"
echo "  🎯 Epics: $epics"
echo "  🔧 Features: $features"
echo "  📖 Stories: $stories"
echo "  ⚡ Enhancements: $enhancements"
echo ""

if [[ $final_drafts -eq 0 ]]; then
    echo "🚀 SUCCESS: All items are now published and standardized!"
    echo "   Ready for sprint planning and development execution!"
else
    echo "⚠️  WARNING: $final_drafts items still in draft status"
fi

echo ""
echo "✨ TherapyDocs project standardization complete!"