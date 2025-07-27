#!/bin/bash
# TherapyDocs Project Items Standardization Script
# This script standardizes naming and publishes all 315 project items

set -e

PROJECT_NUMBER=2
OWNER="sean-rowe"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}❌${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

echo "🏥 TherapyDocs Project Items Standardization"
echo "============================================="
echo ""

# Function to get item details
get_item_id() {
    local item_line="$1"
    echo "$item_line" | awk -F'\t' '{print $5}'
}

get_item_title() {
    local item_line="$1"
    echo "$item_line" | awk -F'\t' '{print $2}'
}

get_item_number() {
    local item_line="$1"
    echo "$item_line" | awk -F'\t' '{print $3}'
}

is_draft() {
    local item_line="$1"
    echo "$item_line" | grep -q "^DraftIssue"
}

# Export all current items
echo "📋 Exporting all project items..."
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/all_therapy_items.txt
TOTAL_ITEMS=$(wc -l < /tmp/all_therapy_items.txt)
print_status "Found $TOTAL_ITEMS items"

# Count items by type
EPICS=$(awk -F'\t' '{print $2}' /tmp/all_therapy_items.txt | grep -c "^Epic:" || echo 0)
STORIES=$(awk -F'\t' '{print $2}' /tmp/all_therapy_items.txt | grep -c "^Story:" || echo 0)
ENHANCEMENTS=$(awk -F'\t' '{print $2}' /tmp/all_therapy_items.txt | grep -c "^ENHANCE:" || echo 0)
DRAFTS=$(grep -c "^DraftIssue" /tmp/all_therapy_items.txt || echo 0)
INCONSISTENT=$(awk -F'\t' '{print $2}' /tmp/all_therapy_items.txt | grep -vcE "^Epic:|^Story:|^ENHANCE:|^Project:" || echo 0)

echo ""
echo "📊 Current State:"
echo "  - Epics: $EPICS"
echo "  - Stories: $STORIES" 
echo "  - Enhancements: $ENHANCEMENTS"
echo "  - Inconsistent naming: $INCONSISTENT"
echo "  - Draft items: $DRAFTS"
echo ""

# Define Epic to Feature mappings based on TherapyDocs requirements
declare -A EPIC_TO_FEATURES=(
    ["Epic: User Authentication System"]="Authentication"
    ["Epic: Student Management System"]="Student-Management"
    ["Epic: IEP Goal Tracking System"]="IEP-Goals"
    ["Epic: Session Documentation System"]="Documentation"
    ["Epic: Billing and Insurance Management"]="Billing"
    ["Epic: Teletherapy Platform"]="Teletherapy"
    ["Epic: Parent Portal System"]="Parent-Portal"
    ["Epic: AI Content Generation System"]="AI-Content"
    ["Epic: Digital Evaluations System"]="Evaluations"
    ["Epic: Compliance and Reporting System"]="Compliance"
    ["Epic: Caseload Management System"]="Caseload"
    ["Epic: Reporting and Analytics Platform"]="Analytics"
    ["Epic: System Administration Platform"]="Administration"
)

# Function to standardize naming based on content analysis
standardize_name() {
    local title="$1"
    local epic_context="$2"
    
    # Remove existing prefixes
    title=$(echo "$title" | sed -E 's/^(Story:|Feature:|Epic:|ENHANCE:)\s*//')
    
    # Determine the type and create standardized name
    case "$title" in
        "User Authentication System"|"Session Documentation System"|"Billing and Insurance Management"|"AI Content Generation System"|"Digital Evaluations System"|"Parent Portal System"|"Teletherapy Platform"|"Compliance and Reporting System"|"Caseload Management System"|"Reporting and Analytics Platform"|"System Administration Platform"|"Student Management System"|"IEP Goal Tracking System")
            echo "Epic: $title"
            ;;
        *"Authentication and Security Implementation"|*"Student and Clinical Management Platform"|*"Billing and Revenue Cycle Management")
            echo "Epic: $title"
            ;;
        *"Add Backup Authentication Methods"*|*"Automate Encryption Key Rotation"*|*"Offline EVV Synchronization"*|*"Supervisor Workflow"*|*"Bulk Prior Authorization"*|*"Time-Bound Substitute"*)
            echo "Enhancement: $title"
            ;;
        *"Setup"*|*"Configure"*|*"Implement"*|*"Build"*|*"Create"*|*"Establish"*)
            # These are typically features or large stories
            if [[ ${#title} -gt 80 ]]; then
                echo "Feature: $title"
            else
                echo "Story: $title"
            fi
            ;;
        *)
            # Default to Story for user stories and small features
            echo "Story: $title"
            ;;
    esac
}

# Process all items and create rename commands
echo "🔄 Processing items for standardization..."
processed=0
rename_commands=()
publish_commands=()

while IFS= read -r line; do
    if [[ -z "$line" ]] || [[ "$line" == *"Project: TherapyDocs Development"* ]]; then
        continue
    fi
    
    item_id=$(get_item_id "$line")
    current_title=$(get_item_title "$line")
    item_number=$(get_item_number "$line")
    
    # Skip if no item ID
    if [[ -z "$item_id" ]]; then
        continue
    fi
    
    # Standardize the name
    new_title=$(standardize_name "$current_title")
    
    # Check if rename is needed
    if [[ "$current_title" != "$new_title" ]]; then
        rename_commands+=("gh issue edit $item_number --repo $OWNER/therapy-docs --title \"$new_title\"")
        print_info "Will rename: '$current_title' → '$new_title'"
    fi
    
    # Check if publishing is needed
    if is_draft "$line"; then
        # Convert draft to published issue
        publish_commands+=("gh project item-edit --project-id $PROJECT_NUMBER --id $item_id --field-id Status --text \"Ready\"")
        print_info "Will publish draft: $new_title"
    fi
    
    ((processed++))
    if ((processed % 50 == 0)); then
        print_status "Processed $processed items..."
    fi
    
done < /tmp/all_therapy_items.txt

echo ""
print_status "Analysis complete. Found ${#rename_commands[@]} items to rename and ${#publish_commands[@]} items to publish."

# Ask for confirmation
echo ""
echo "⚠️  This will:"
echo "   - Rename ${#rename_commands[@]} items for consistency"
echo "   - Publish ${#publish_commands[@]} draft items"
echo ""
read -p "Do you want to proceed? (y/N): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Operation cancelled."
    exit 0
fi

# Execute rename commands
if [[ ${#rename_commands[@]} -gt 0 ]]; then
    echo ""
    echo "🏷️  Renaming items..."
    for cmd in "${rename_commands[@]}"; do
        if eval "$cmd"; then
            print_status "Renamed item"
        else
            print_error "Failed to rename item"
        fi
        sleep 0.5  # Rate limiting
    done
fi

# Execute publish commands  
if [[ ${#publish_commands[@]} -gt 0 ]]; then
    echo ""
    echo "📤 Publishing draft items..."
    for cmd in "${publish_commands[@]}"; do
        if eval "$cmd"; then
            print_status "Published item"
        else
            print_error "Failed to publish item"
        fi
        sleep 0.5  # Rate limiting
    done
fi

echo ""
echo "🎉 Standardization complete!"
echo ""

# Generate final report
echo "📊 Final Report:"
gh project item-list $PROJECT_NUMBER --owner $OWNER --limit 315 > /tmp/final_therapy_items.txt

FINAL_EPICS=$(awk -F'\t' '{print $2}' /tmp/final_therapy_items.txt | grep -c "^Epic:" || echo 0)
FINAL_FEATURES=$(awk -F'\t' '{print $2}' /tmp/final_therapy_items.txt | grep -c "^Feature:" || echo 0)
FINAL_STORIES=$(awk -F'\t' '{print $2}' /tmp/final_therapy_items.txt | grep -c "^Story:" || echo 0)
FINAL_ENHANCEMENTS=$(awk -F'\t' '{print $2}' /tmp/final_therapy_items.txt | grep -c "^Enhancement:" || echo 0)
FINAL_DRAFTS=$(grep -c "^DraftIssue" /tmp/final_therapy_items.txt || echo 0)

echo "  ✅ Epics: $FINAL_EPICS"
echo "  ✅ Features: $FINAL_FEATURES"
echo "  ✅ Stories: $FINAL_STORIES"
echo "  ✅ Enhancements: $FINAL_ENHANCEMENTS"
echo "  ✅ Draft items remaining: $FINAL_DRAFTS"
echo ""

if [[ $FINAL_DRAFTS -eq 0 ]]; then
    print_status "All items are now published and ready for development!"
else
    print_warning "$FINAL_DRAFTS items still in draft status (may need manual review)"
fi

echo ""
echo "🚀 TherapyDocs project is now standardized with Epic > Feature > Story hierarchy"
echo "   Ready for sprint planning and development execution!"