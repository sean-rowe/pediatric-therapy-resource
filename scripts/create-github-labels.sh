#!/bin/bash

# Create GitHub labels for the TherapyDocs project

OWNER="sean-rowe"
REPO="therapy-docs"

echo "Creating GitHub labels..."

# Component labels
gh label create backend --color "0e8a16" --description "Backend/API work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'backend' already exists"
gh label create frontend --color "006b75" --description "Frontend/UI work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'frontend' already exists"
gh label create database --color "fbca04" --description "Database work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'database' already exists"
gh label create security --color "ee0701" --description "Security-related work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'security' already exists"
gh label create testing --color "0052cc" --description "Testing work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'testing' already exists"
gh label create infrastructure --color "5319e7" --description "Infrastructure/DevOps work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'infrastructure' already exists"
gh label create compliance --color "d93f0b" --description "Compliance/regulatory work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'compliance' already exists"
gh label create integration --color "1d76db" --description "Third-party integration work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'integration' already exists"
gh label create ai --color "f9d0c4" --description "AI/ML related work" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'ai' already exists"

# Priority labels
gh label create P0-Critical --color "b60205" --description "Critical priority - must be done immediately" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'P0-Critical' already exists"
gh label create P1-High --color "ff9900" --description "High priority" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'P1-High' already exists"
gh label create P2-Medium --color "fbca04" --description "Medium priority" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'P2-Medium' already exists"
gh label create P3-Low --color "0e8a16" --description "Low priority" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'P3-Low' already exists"

# Type labels
gh label create story --color "1d76db" --description "User story" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'story' already exists"
gh label create bug --color "d73a4a" --description "Something isn't working" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'bug' already exists"
gh label create epic --color "5319e7" --description "Epic - contains multiple stories" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'epic' already exists"
gh label create task --color "0075ca" --description "Technical task" --repo "$OWNER/$REPO" 2>/dev/null || echo "Label 'task' already exists"

echo "Labels created successfully!"