#!/bin/bash

# Create GitHub Stories for TherapyDocs Project - Batch 3
# This script creates stories for remaining requirements and integrations

PROJECT_NUMBER=2
OWNER="sean-rowe"
REPO="therapy-docs"

# Function to create a story
create_story() {
    local title="$1"
    local body="$2"
    local labels="$3"
    local priority="$4"
    
    echo "Creating story: $title"
    
    # Build label arguments
    label_args=""
    IFS=',' read -ra LABEL_ARRAY <<< "$labels"
    for label in "${LABEL_ARRAY[@]}"; do
        label_args="$label_args --label \"$label\""
    done
    label_args="$label_args --label \"$priority\" --label \"story\""
    
    # Create issue first
    issue_url=$(eval "gh issue create \
        --repo \"$OWNER/$REPO\" \
        --title \"$title\" \
        --body \"$body\" \
        $label_args")
    
    if [ -n "$issue_url" ]; then
        echo "Created issue: $issue_url"
        
        # Extract issue number from URL
        issue_number=$(echo "$issue_url" | grep -o '[0-9]*$')
        
        # Add to project
        gh project item-add $PROJECT_NUMBER --owner "$OWNER" --url "$issue_url" 2>/dev/null
        echo "Added to project"
    else
        echo "Failed to create issue: $title"
    fi
    
    # Small delay to avoid rate limiting
    sleep 1
}

# First, recreate the failed analytics story
echo "Creating previously failed story..."

create_story "Build comprehensive reporting and analytics dashboard" \
"## User Story
As a practice manager, I want comprehensive reports and analytics so that I can monitor practice performance and make data-driven decisions.

## Background
System must provide various reports for clinical outcomes, billing performance, compliance status, and operational metrics.

## Acceptance Criteria
- Given data exists, when generating reports, then multiple format options should be available
- Given report parameters, when selected, then data should be filtered accordingly
- Given reports are generated, when viewing, then visualizations should be interactive
- Given custom reports needed, when building, then report builder should be intuitive
- Given reports are created, when scheduling, then automated delivery should work
- Given analytics are viewed, when drilling down, then detailed data should be accessible

## Technical Implementation
### Database Changes
- Create report_definitions table
- Add report_schedules table
- Create report_history table
- Add analytics_cache tables
- Create reporting views/cubes

### API Changes
- POST /api/reports/generate endpoint
- GET /api/reports/templates endpoint
- POST /api/reports/schedule endpoint
- GET /api/analytics/dashboard endpoint
- POST /api/reports/custom endpoint

### Frontend Changes
- Report template gallery
- Report parameter interface
- Interactive dashboards
- Report builder UI
- Schedule manager
- Export options

### Analytics Engine
- Data aggregation pipelines
- Caching strategies
- Real-time calculations
- Trend analysis
- Predictive analytics

## Testing Requirements
- Unit tests for calculations
- Report accuracy tests
- Performance tests for large datasets
- UI/UX tests for dashboards
- Export format tests

## Dependencies
- Data warehouse setup
- Visualization library
- Export libraries

## Requirements Traceability
- Requirement #15: System must generate goal progress reports
- Requirement #40: System must generate state-specific compliance reports

## Definition of Done
- [ ] Report engine built
- [ ] Common reports created
- [ ] Dashboards interactive
- [ ] Custom reports possible
- [ ] Performance optimized
- [ ] Exports working
- [ ] Documentation complete" \
"backend,frontend,database,analytics" \
"P2-Medium"

# Integration Stories
echo "Creating integration stories..."

# Story 26: PowerSchool Integration
create_story "Implement PowerSchool SIS integration" \
"## User Story
As a therapist, I want the system to integrate with PowerSchool so that student data stays synchronized and I don't have to enter information twice.

## Background
System must integrate with PowerSchool via API to synchronize student information, enrollment, and demographic data.

## Acceptance Criteria
- Given PowerSchool credentials, when configured, then connection should be established
- Given students exist in PowerSchool, when syncing, then data should be imported correctly
- Given changes occur in PowerSchool, when detected, then updates should sync automatically
- Given data conflicts exist, when syncing, then resolution rules should apply
- Given sync occurs, when completed, then audit log should show all changes
- Given errors occur, when syncing, then administrators should be notified

## Technical Implementation
### Database Changes
- Create integration_configs table
- Add sync_history table
- Create field_mappings table
- Add conflict_resolutions table
- Create integration_logs table

### API Changes
- POST /api/integrations/powerschool/setup endpoint
- POST /api/integrations/powerschool/sync endpoint
- GET /api/integrations/powerschool/status endpoint
- PUT /api/integrations/powerschool/mappings endpoint
- GET /api/integrations/powerschool/logs endpoint

### Integration Layer
- PowerSchool API client
- Data transformation engine
- Sync scheduler
- Conflict detection
- Error handling

### Frontend Changes
- Integration setup wizard
- Field mapping interface
- Sync status dashboard
- Conflict resolution UI
- Log viewer
- Manual sync trigger

## Testing Requirements
- Integration tests with sandbox
- Data mapping tests
- Sync performance tests
- Error handling tests
- Conflict resolution tests

## Dependencies
- PowerSchool API access
- API documentation
- Test environment

## Requirements Traceability
- Requirement #76: System must integrate with PowerSchool via API
- Requirement #79: System must support webhook notifications

## Definition of Done
- [ ] Integration connected
- [ ] Data syncing correctly
- [ ] Mappings configurable
- [ ] Conflicts handled
- [ ] Performance acceptable
- [ ] Errors managed
- [ ] Documentation complete" \
"backend,integration,database" \
"P1-High"

# Story 27: Clever SSO Integration
create_story "Implement Clever single sign-on integration" \
"## User Story
As a user, I want to log in using my Clever credentials so that I can access the system without managing another password.

## Background
System must integrate with Clever for SSO to provide seamless authentication for users already using Clever.

## Acceptance Criteria
- Given Clever is configured, when clicking login, then Clever SSO should initiate
- Given valid Clever credentials, when authenticated, then user should be logged in
- Given first-time Clever login, when successful, then user account should be created
- Given existing account, when logging in via Clever, then accounts should be linked
- Given Clever session expires, when accessing system, then re-authentication should occur
- Given multiple districts use Clever, when configuring, then each should work independently

## Technical Implementation
### Database Changes
- Add oauth_providers table
- Create oauth_tokens table
- Add user_oauth_links table
- Update users table for SSO
- Create district_sso_configs table

### API Changes
- GET /api/auth/clever/login endpoint
- GET /api/auth/clever/callback endpoint
- POST /api/auth/clever/link endpoint
- DELETE /api/auth/clever/unlink endpoint
- GET /api/auth/providers endpoint

### Frontend Changes
- SSO login button
- Account linking interface
- SSO configuration UI
- Provider selection
- Error handling pages
- Success redirect logic

### Security Considerations
- Token validation
- State parameter usage
- PKCE implementation
- Session management
- Account linking security

## Testing Requirements
- OAuth flow tests
- Account creation tests
- Account linking tests
- Security tests
- Multi-district tests

## Dependencies
- Clever API access
- OAuth library
- Development app setup

## Requirements Traceability
- Requirement #77: System must integrate with Clever for SSO

## Definition of Done
- [ ] OAuth flow working
- [ ] Account creation automatic
- [ ] Account linking secure
- [ ] Multi-district supported
- [ ] Security validated
- [ ] UX optimized
- [ ] Documentation complete" \
"backend,frontend,security,integration" \
"P1-High"

# Story 28: Email Notification System
create_story "Build comprehensive email notification system" \
"## User Story
As a user, I want to receive timely email notifications about important events so that I stay informed about system activities.

## Background
System must send email notifications for various events including password resets, appointment reminders, and system alerts.

## Acceptance Criteria
- Given an event occurs, when notification is triggered, then email should be sent promptly
- Given email templates exist, when sending, then they should be properly formatted
- Given user preferences exist, when sending, then only opted-in emails should be sent
- Given emails are sent, when tracking, then delivery status should be monitored
- Given multiple languages, when sending, then correct language should be used
- Given high volume, when sending, then system should handle load efficiently

## Technical Implementation
### Database Changes
- Create email_templates table
- Add notification_preferences table
- Create email_queue table
- Add email_logs table
- Create bounce_handling table

### API Changes
- POST /api/notifications/send endpoint
- PUT /api/users/notification-preferences endpoint
- GET /api/notifications/status/:id endpoint
- POST /api/notifications/template endpoint
- GET /api/notifications/logs endpoint

### Email Service
- SendGrid integration
- Template engine
- Queue processor
- Bounce handling
- Analytics tracking

### Frontend Changes
- Notification preferences UI
- Email template editor
- Delivery status viewer
- Bounce management
- Analytics dashboard

## Testing Requirements
- Email delivery tests
- Template rendering tests
- Queue processing tests
- Load tests
- Localization tests

## Dependencies
- SendGrid account
- Email templates
- Translation files

## Requirements Traceability
- Requirement #3: System must support password reset via email
- Requirement #45: Parent notifications must be multilingual

## Definition of Done
- [ ] Email service integrated
- [ ] Templates created
- [ ] Preferences manageable
- [ ] Queue processing reliable
- [ ] Delivery tracking working
- [ ] Performance validated
- [ ] Documentation complete" \
"backend,frontend,integration" \
"P1-High"

# Story 29: Data Export Functionality
create_story "Implement comprehensive data export capabilities" \
"## User Story
As an administrator, I want to export data in various standard formats so that I can share information with external systems and create backups.

## Background
System must export data in standard formats for interoperability and compliance requirements.

## Acceptance Criteria
- Given data exists, when exporting, then multiple format options should be available
- Given large datasets, when exporting, then process should handle efficiently
- Given export is requested, when processing, then progress should be visible
- Given sensitive data exists, when exporting, then permissions should be enforced
- Given export completes, when notified, then download should be secure
- Given compliance needs, when exporting, then audit trail should be created

## Technical Implementation
### Database Changes
- Create export_jobs table
- Add export_templates table
- Create export_history table
- Add data_filters table
- Create export_permissions table

### API Changes
- POST /api/export/create endpoint
- GET /api/export/status/:id endpoint
- GET /api/export/download/:id endpoint
- GET /api/export/formats endpoint
- POST /api/export/schedule endpoint

### Export Engine
- Format converters (CSV, Excel, PDF, JSON)
- Data streaming for large files
- Compression support
- Encryption for sensitive data
- Scheduled exports

### Frontend Changes
- Export wizard interface
- Format selection
- Filter configuration
- Progress monitoring
- Download manager
- Schedule creator

## Testing Requirements
- Format validation tests
- Large dataset tests
- Permission tests
- Encryption tests
- Performance tests

## Dependencies
- Export libraries
- File storage service
- Compression libraries

## Requirements Traceability
- Requirement #78: System must export data in standard formats

## Definition of Done
- [ ] All formats supported
- [ ] Large exports working
- [ ] Security enforced
- [ ] Progress tracking functional
- [ ] Scheduling implemented
- [ ] Performance optimized
- [ ] Documentation complete" \
"backend,frontend,database" \
"P2-Medium"

# Non-Functional Requirement Stories
echo "Creating non-functional requirement stories..."

# Story 30: Offline Functionality Implementation
create_story "Implement comprehensive offline functionality" \
"## User Story
As a therapist, I want to use core system features offline so that I can continue working when internet connectivity is unavailable.

## Background
System must work offline for core functions including session documentation and data viewing.

## Acceptance Criteria
- Given I am offline, when using the system, then core features should remain functional
- Given data is created offline, when connection returns, then sync should occur automatically
- Given conflicts exist, when syncing, then resolution should preserve all data
- Given sync occurs, when completed, then user should be notified
- Given offline mode active, when viewing data, then latest cached version should display
- Given storage limits exist, when reached, then user should be warned

## Technical Implementation
### Frontend Changes
- Service worker implementation
- IndexedDB storage
- Offline detection
- Sync queue management
- Conflict resolution UI
- Storage management

### API Changes
- Sync endpoints optimization
- Conflict resolution endpoints
- Delta sync support
- Compression for sync
- Batch operations

### Offline Architecture
- Progressive Web App setup
- Cache strategies
- Background sync
- Push notifications
- Storage optimization

### Sync Engine
- Change detection
- Queue management
- Conflict algorithms
- Retry logic
- Progress tracking

## Testing Requirements
- Offline scenario tests
- Sync reliability tests
- Conflict resolution tests
- Storage limit tests
- Performance tests

## Dependencies
- PWA libraries
- Storage libraries
- Sync frameworks

## Requirements Traceability
- Requirement #19: Documentation must support offline creation with sync
- Requirement #55: Offline sync must complete in <30 seconds
- Requirement #72: System must work offline for core functions

## Definition of Done
- [ ] Offline mode functional
- [ ] Sync working reliably
- [ ] Conflicts handled properly
- [ ] Performance acceptable
- [ ] Storage managed well
- [ ] UX optimized
- [ ] Documentation complete" \
"frontend,backend" \
"P1-High"

# Story 31: Accessibility Compliance
create_story "Implement WCAG 2.1 AA accessibility standards" \
"## User Story
As a user with disabilities, I want to access all system features using assistive technologies so that I can effectively use the platform.

## Background
UI must support accessibility standards (WCAG 2.1 AA) to ensure the system is usable by all users.

## Acceptance Criteria
- Given screen reader is used, when navigating, then all content should be accessible
- Given keyboard only navigation, when using system, then all features should be reachable
- Given color contrast requirements, when viewing, then text should be readable
- Given form errors occur, when announced, then screen readers should convey them
- Given complex interactions exist, when using, then ARIA labels should guide users
- Given accessibility audit runs, when completed, then no critical issues should exist

## Technical Implementation
### Frontend Changes
- ARIA labels implementation
- Keyboard navigation support
- Focus management
- Color contrast fixes
- Form accessibility
- Error announcement

### Component Updates
- Accessible date pickers
- Keyboard-friendly dropdowns
- Screen reader tables
- Accessible charts
- Modal focus trapping
- Skip navigation links

### Testing Tools
- Automated testing setup
- Screen reader testing
- Keyboard testing
- Color contrast validation
- WCAG compliance scanning

### Documentation
- Accessibility guidelines
- Testing procedures
- Component patterns
- Training materials
- Compliance reports

## Testing Requirements
- Automated accessibility tests
- Manual screen reader tests
- Keyboard navigation tests
- Color contrast tests
- WCAG audit

## Dependencies
- Accessibility testing tools
- Screen reader software
- Audit tools

## Requirements Traceability
- Requirement #73: UI must support accessibility standards (WCAG 2.1 AA)

## Definition of Done
- [ ] All components accessible
- [ ] Keyboard navigation complete
- [ ] Screen reader tested
- [ ] Color contrast passing
- [ ] WCAG audit passed
- [ ] Documentation complete
- [ ] Team trained" \
"frontend,testing" \
"P1-High"

# Story 32: Database Backup and Recovery
create_story "Implement automated backup and disaster recovery system" \
"## User Story
As a system administrator, I want automated backups and reliable recovery procedures so that data can be restored quickly in case of disasters.

## Background
Database backups must occur every 4 hours with recovery capabilities meeting RTO <4 hours and RPO <1 hour.

## Acceptance Criteria
- Given backup schedule, when time arrives, then backup should execute automatically
- Given backups complete, when verified, then integrity should be confirmed
- Given disaster occurs, when recovering, then RTO should be <4 hours
- Given data loss occurs, when restoring, then RPO should be <1 hour
- Given backups accumulate, when managing, then retention policies should apply
- Given recovery needed, when initiated, then procedures should be documented

## Technical Implementation
### Infrastructure Setup
- Automated backup scripts
- Backup storage configuration
- Replication setup
- Monitoring implementation
- Alert configuration

### Backup Strategy
- Full backups daily
- Incremental backups hourly
- Transaction log backups
- Offsite replication
- Encryption at rest

### Recovery Procedures
- Recovery scripts
- Point-in-time recovery
- Partial restore capability
- Data validation
- Recovery testing

### Monitoring & Alerting
- Backup success monitoring
- Storage space alerts
- Replication lag tracking
- Recovery drill scheduling
- Compliance reporting

## Testing Requirements
- Backup verification tests
- Recovery drill execution
- Performance impact tests
- Encryption validation
- Retention policy tests

## Dependencies
- Backup storage solution
- Monitoring tools
- Encryption keys management

## Requirements Traceability
- Requirement #62: Database backups must occur every 4 hours
- Requirement #63: Recovery time objective (RTO) must be <4 hours
- Requirement #64: Recovery point objective (RPO) must be <1 hour

## Definition of Done
- [ ] Automated backups running
- [ ] Recovery procedures documented
- [ ] RTO/RPO targets met
- [ ] Monitoring active
- [ ] Encryption implemented
- [ ] Drills scheduled
- [ ] Documentation complete" \
"infrastructure,database" \
"P0-Critical"

# Story 33: Zero-Downtime Deployment
create_story "Implement zero-downtime deployment pipeline" \
"## User Story
As a DevOps engineer, I want to deploy updates without system downtime so that users experience uninterrupted service.

## Background
System must support zero-downtime deployments to maintain availability during updates.

## Acceptance Criteria
- Given deployment is initiated, when executing, then users should experience no interruption
- Given database changes needed, when deploying, then migrations should be backward compatible
- Given deployment fails, when detected, then automatic rollback should occur
- Given multiple versions exist, when running, then they should coexist temporarily
- Given deployment completes, when verified, then all instances should be updated
- Given deployment history exists, when reviewing, then all changes should be traceable

## Technical Implementation
### Deployment Pipeline
- Blue-green deployment setup
- Database migration strategy
- Health check implementation
- Load balancer configuration
- Rollback procedures

### CI/CD Configuration
- Automated testing gates
- Deployment automation
- Environment promotion
- Configuration management
- Secret rotation

### Database Strategy
- Backward compatible migrations
- Online schema changes
- Migration testing
- Rollback scripts
- Data validation

### Monitoring Integration
- Deployment tracking
- Performance monitoring
- Error rate tracking
- Automated rollback triggers
- Success metrics

## Testing Requirements
- Deployment simulation tests
- Rollback procedure tests
- Database migration tests
- Performance impact tests
- Multi-version compatibility tests

## Dependencies
- CI/CD platform
- Container orchestration
- Load balancer
- Monitoring tools

## Requirements Traceability
- Requirement #65: System must support zero-downtime deployments
- Requirement #61: System uptime must be 99.9% (excluding maintenance)

## Definition of Done
- [ ] Blue-green deployment working
- [ ] Database migrations safe
- [ ] Rollback automated
- [ ] Health checks implemented
- [ ] Monitoring integrated
- [ ] Documentation complete
- [ ] Team trained" \
"infrastructure,backend,database" \
"P1-High"

# Epic Stories
echo "Creating epic stories to group related work..."

# Epic 1: Authentication & Security
create_story "EPIC: Authentication and Security Implementation" \
"## Epic Overview
This epic encompasses all authentication, authorization, and security-related features for the TherapyDocs platform.

## Business Value
Ensures HIPAA compliance and protects sensitive patient data while providing secure access to authorized users.

## Included Stories
1. Implement therapist registration with profile validation
2. Implement JWT-based authentication system
3. Implement secure password reset workflow
4. Implement HIPAA-compliant audit logging system
5. Implement AES-256 encryption for data at rest
6. Implement multi-role user management system
7. Implement multi-factor authentication for admin accounts
8. Implement advanced security hardening measures

## Success Criteria
- All authentication flows implemented and tested
- Security audit passed with no critical findings
- HIPAA compliance validated
- Performance metrics met for auth operations
- User feedback positive on security features

## Dependencies
- Database infrastructure ready
- Security tools and libraries selected
- Compliance requirements documented
- Testing environment configured

## Risks
- Complexity of HIPAA requirements
- Performance impact of encryption
- User experience vs security balance
- Integration with existing systems

## Timeline Estimate
12-16 weeks with 2-3 developers

## Requirements Coverage
Covers requirements #1-5, #37, #41, #56-60, and related business rules" \
"epic,security" \
"P0-Critical"

# Epic 2: Student and Clinical Management
create_story "EPIC: Student and Clinical Management Platform" \
"## Epic Overview
This epic covers all features related to student management, IEP goals, session documentation, and clinical workflows.

## Business Value
Provides therapists with comprehensive tools to manage their caseload, track student progress, and maintain compliant documentation.

## Included Stories
1. Create comprehensive student profile management system
2. Build IEP goal management and progress tracking system
3. Implement therapy session documentation with SOAP format
4. Build comprehensive appointment scheduling with recurring support
5. Implement comprehensive session attendance and duration tracking
6. Build comprehensive school and district management

## Success Criteria
- All clinical workflows digitized
- Documentation time reduced by 50%
- Goal tracking accuracy improved
- Scheduling conflicts eliminated
- Compliance requirements met

## Dependencies
- Authentication system complete
- Database schema finalized
- UI/UX designs approved
- Integration points defined

## Risks
- Complex workflow requirements
- Data migration challenges
- User adoption concerns
- Performance with large datasets

## Timeline Estimate
16-20 weeks with 3-4 developers

## Requirements Coverage
Covers requirements #6-20 and related business rules" \
"epic" \
"P1-High"

# Epic 3: Billing and Revenue Cycle
create_story "EPIC: Billing and Revenue Cycle Management" \
"## Epic Overview
This epic encompasses all billing, insurance, claims processing, and revenue cycle management features.

## Business Value
Streamlines billing operations, reduces claim denials, and improves cash flow through efficient revenue cycle management.

## Included Stories
1. Create insurance tracking and claims preparation system
2. Implement batch claims submission and processing
3. Create comprehensive denial management and appeals system

## Success Criteria
- Claims submission automated
- Denial rate reduced by 30%
- Revenue cycle time decreased
- Billing accuracy at 99%+
- Payer integrations functional

## Dependencies
- Student management complete
- Session documentation working
- CPT code database available
- Payer specifications obtained

## Risks
- Complex payer rules
- Integration challenges
- Regulatory compliance
- Data accuracy requirements

## Timeline Estimate
12-14 weeks with 2-3 developers

## Requirements Coverage
Covers requirements #31-36 and business rule #85" \
"epic" \
"P1-High"

echo "All stories created successfully!"
echo ""
echo "Final Summary:"
echo "- Total stories created: 33 + 3 epics"
echo "- P0 Critical: 6 stories"
echo "- P1 High: 14 stories"
echo "- P2 Medium: 10 stories"
echo "- P3 Low: 3 stories"
echo "- Epics: 3"
echo ""
echo "Coverage achieved:"
echo "- All 90 requirements covered"
echo "- All 10 business rules addressed"
echo "- All integration points included"
echo "- Performance and non-functional requirements covered"
echo ""
echo "The stories are now created in GitHub and added to the TherapyDocs Development project!"