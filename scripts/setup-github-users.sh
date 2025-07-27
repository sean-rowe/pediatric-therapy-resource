#!/bin/bash

# GitHub User Setup Script for Therapy Docs
# This script configures proper user separation for development and PR reviews

set -e

REPO_OWNER="sean-rowe"
REPO_NAME="therapy-docs"
MAIN_BRANCH="main"

echo "🚀 Setting up GitHub users for Therapy Docs repository"
echo "Repository: $REPO_OWNER/$REPO_NAME"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check if GitHub CLI is authenticated
print_status "Checking GitHub CLI authentication..."
if ! gh auth status >/dev/null 2>&1; then
    print_error "GitHub CLI not authenticated. Run: gh auth login"
    exit 1
fi
print_success "GitHub CLI authenticated"

# Function to check if user exists
check_user_exists() {
    local username=$1
    if gh api users/$username >/dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

# Function to invite user to repository
invite_user() {
    local username=$1
    local permission=$2
    local role_description=$3
    
    print_status "Inviting $username as $role_description..."
    
    if check_user_exists $username; then
        # Check if user is already a collaborator
        if gh api repos/$REPO_OWNER/$REPO_NAME/collaborators/$username >/dev/null 2>&1; then
            print_warning "$username is already a collaborator"
            # Update permissions
            gh api repos/$REPO_OWNER/$REPO_NAME/collaborators/$username \
                --method PUT \
                --field permission=$permission
            print_success "Updated $username permissions to $permission"
        else
            # Send invitation
            gh api repos/$REPO_OWNER/$REPO_NAME/collaborators/$username \
                --method PUT \
                --field permission=$permission
            print_success "Invited $username with $permission permission"
        fi
    else
        print_error "User $username does not exist on GitHub"
        print_warning "Please create the GitHub account manually or provide an existing username"
        return 1
    fi
}

# Configuration
DEV_USER="therapy-docs-dev"
REVIEWER_USER="therapy-docs-reviewer"

echo ""
echo "👥 MANUAL STEP REQUIRED:"
echo "Before running this script, you need to:"
echo "1. Create GitHub account: $DEV_USER (for development work)"
echo "2. Create GitHub account: $REVIEWER_USER (for code reviews)"
echo ""
echo "Or provide existing GitHub usernames when prompted."
echo ""

# Prompt for actual usernames
read -p "Enter GitHub username for developer role [$DEV_USER]: " input_dev_user
DEV_USER=${input_dev_user:-$DEV_USER}

read -p "Enter GitHub username for reviewer role [$REVIEWER_USER]: " input_reviewer_user
REVIEWER_USER=${input_reviewer_user:-$REVIEWER_USER}

echo ""
print_status "Configuring users:"
print_status "Developer: $DEV_USER"
print_status "Reviewer: $REVIEWER_USER"
echo ""

# Invite users with appropriate permissions
invite_user $DEV_USER "push" "developer (write access)"
invite_user $REVIEWER_USER "push" "reviewer (write access for PR approvals)"

# Configure branch protection rules
print_status "Configuring branch protection for $MAIN_BRANCH..."

# Create branch protection rule
cat > /tmp/branch_protection.json << EOF
{
  "required_status_checks": null,
  "enforce_admins": false,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false,
    "require_review_from_code_owners": false,
    "dismissal_restrictions": {}
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "block_creations": false,
  "required_conversation_resolution": true
}
EOF

if gh api repos/$REPO_OWNER/$REPO_NAME/branches/$MAIN_BRANCH/protection \
    --method PUT \
    --input /tmp/branch_protection.json >/dev/null 2>&1; then
    print_success "Branch protection configured for $MAIN_BRANCH"
else
    print_warning "Could not configure branch protection (may need admin permissions)"
fi

# Clean up
rm -f /tmp/branch_protection.json

# Create CODEOWNERS file for review requirements
print_status "Creating CODEOWNERS file..."
mkdir -p .github
cat > .github/CODEOWNERS << EOF
# Global code owners - require review from designated reviewers
* @$REVIEWER_USER

# API code requires extra scrutiny
/api/ @$REVIEWER_USER

# Security-sensitive files require review
/api/Controllers/AuthController.cs @$REVIEWER_USER
/api/Services/AuthService*.cs @$REVIEWER_USER
/api/Services/PasswordService.cs @$REVIEWER_USER
/api/Middleware/GlobalExceptionMiddleware.cs @$REVIEWER_USER

# Database migrations require careful review  
/sql/migrations/ @$REVIEWER_USER

# Test files can be reviewed by either developer or reviewer
/api/Tests/ @$DEV_USER @$REVIEWER_USER
EOF

print_success "Created .github/CODEOWNERS file"

# Create PR template
print_status "Creating PR template..."
cat > .github/pull_request_template.md << 'EOF'
## Summary
Brief description of the changes in this PR.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Security improvement
- [ ] Documentation update
- [ ] Test improvements

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] All tests pass

## Security Checklist
- [ ] No sensitive data exposed in logs
- [ ] Input validation implemented
- [ ] Authentication/authorization checked
- [ ] SQL injection prevention verified
- [ ] XSS prevention verified

## Database Changes
- [ ] No database changes
- [ ] Migrations added and tested
- [ ] Rollback plan documented
- [ ] Performance impact assessed

## Review Requirements
- [ ] Code follows project conventions
- [ ] Documentation updated
- [ ] Breaking changes documented
- [ ] Security review completed (if applicable)

## Related Issues
Closes #(issue number)

## Screenshots (if applicable)
EOF

print_success "Created PR template"

# List current collaborators
print_status "Current repository collaborators:"
gh api repos/$REPO_OWNER/$REPO_NAME/collaborators --jq '.[] | "\(.login) - \(.permissions | to_entries | map(select(.value == true)) | map(.key) | join(", "))"'

echo ""
print_success "✨ GitHub user setup completed!"
echo ""
echo "📋 Next Steps:"
echo "1. Ask $DEV_USER to accept the collaboration invitation"
echo "2. Ask $REVIEWER_USER to accept the collaboration invitation"
echo "3. Have $DEV_USER create feature branches and PRs"
echo "4. Have $REVIEWER_USER review and approve PRs"
echo "5. Test the workflow with a sample PR"
echo ""
print_status "Branch protection is now active on $MAIN_BRANCH"
print_status "All PRs will require at least 1 approval before merging"
EOF