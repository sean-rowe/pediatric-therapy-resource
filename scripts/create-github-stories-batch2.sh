#!/bin/bash

# Create GitHub Stories for TherapyDocs Project - Batch 2
# This script creates stories for remaining requirements

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

# More P1 High Priority Stories - Business Critical Features
echo "Creating additional P1 High priority stories..."

# Story 16: User Role Management
create_story "Implement multi-role user management system" \
"## User Story
As an administrator, I want to manage user roles and permissions so that I can control access to system features based on user responsibilities.

## Background
System must support multiple user roles (therapist, admin) with appropriate permissions and access controls.

## Acceptance Criteria
- Given I am an admin, when I create users, then I can assign appropriate roles
- Given roles exist, when assigned, then permissions should be enforced throughout the system
- Given a user's role changes, when updated, then access should reflect immediately
- Given multiple roles exist, when viewing users, then roles should be clearly displayed
- Given permissions are set, when accessing features, then unauthorized access should be blocked
- Given an audit occurs, when reviewing, then all role changes should be logged

## Technical Implementation
### Database Changes
- Create roles table
- Add permissions table
- Create role_permissions junction table
- Add user_roles table
- Create permission_checks view

### API Changes
- POST /api/admin/roles endpoint
- PUT /api/admin/users/:id/roles endpoint
- GET /api/admin/permissions endpoint
- Middleware for permission checking
- Role-based route protection

### Frontend Changes
- Role management interface
- User role assignment
- Permission matrix display
- Access denied pages
- Role-based UI elements

### Security Considerations
- Principle of least privilege
- Role hierarchy implementation
- Permission inheritance
- Audit all role changes
- Prevent privilege escalation

## Testing Requirements
- Unit tests for permission logic
- Integration tests for role assignment
- Security tests for access control
- UI tests for role management
- Audit trail verification

## Dependencies
- Authentication system complete
- Audit logging active

## Requirements Traceability
- Requirement #5: System must support multiple user roles (therapist, admin)
- Business Rule #87: Inactive accounts are disabled after 180 days

## Definition of Done
- [ ] Role system implemented
- [ ] Permission checking active
- [ ] UI reflects permissions
- [ ] Audit logging complete
- [ ] Security validated
- [ ] Tests passing
- [ ] Documentation updated" \
"backend,frontend,database,security" \
"P1-High"

# Story 17: School Management System
create_story "Build comprehensive school and district management" \
"## User Story
As a therapist, I want to manage school and district information so that I can properly associate students and track service locations.

## Background
Students must be associated with schools and schools must be organized by districts for proper reporting and organization.

## Acceptance Criteria
- Given school information, when entered, then all required fields should be validated
- Given schools exist, when assigning students, then the association should be tracked
- Given districts exist, when viewing schools, then they should be properly organized
- Given a school search, when performed, then results should include district information
- Given school changes, when updated, then student associations should remain intact
- Given reporting needs, when generating, then school/district grouping should be available

## Technical Implementation
### Database Changes
- Enhance schools table
- Create districts table
- Add school_contacts table
- Create school_services table
- Add district_requirements table

### API Changes
- POST /api/schools endpoint
- PUT /api/schools/:id endpoint
- GET /api/schools/search endpoint
- POST /api/districts endpoint
- GET /api/schools/:id/students endpoint

### Frontend Changes
- School management interface
- District hierarchy view
- School search functionality
- Student assignment UI
- Contact management
- Service tracking

### Business Logic
- District hierarchy rules
- School validation logic
- Service area mapping
- Contact role management
- Reporting aggregation

## Testing Requirements
- Unit tests for hierarchy logic
- Integration tests for associations
- UI tests for management interface
- Performance tests for searches
- Data integrity tests

## Dependencies
- Geographic data for districts
- State education databases

## Requirements Traceability
- Requirement #8: Students must be associated with schools and assigned therapists
- Requirement #9: System must support student search by name, ID, or school

## Definition of Done
- [ ] School management complete
- [ ] District hierarchy working
- [ ] Student associations functional
- [ ] Search performing well
- [ ] Reports grouping correctly
- [ ] All tests passing
- [ ] Documentation updated" \
"backend,frontend,database" \
"P1-High"

# Story 18: Session Attendance Tracking
create_story "Implement comprehensive session attendance and duration tracking" \
"## User Story
As a therapist, I want to track session attendance and actual duration so that I can maintain accurate service records for billing and compliance.

## Background
System must track session duration and attendance status for all appointments, supporting various attendance scenarios.

## Acceptance Criteria
- Given a scheduled session, when marking attendance, then status options should include present/absent/partial
- Given session occurs, when recording duration, then actual vs scheduled time should be tracked
- Given absence occurs, when recording, then reason codes should be available
- Given partial attendance, when documenting, then actual minutes should be recorded
- Given attendance is tracked, when reporting, then patterns should be identifiable
- Given billing occurs, when calculating, then only attended time should be billed

## Technical Implementation
### Database Changes
- Add attendance_status to appointments
- Create absence_reasons table
- Add actual_duration column
- Create attendance_tracking view
- Add make_up_sessions table

### API Changes
- PUT /api/appointments/:id/attendance endpoint
- POST /api/appointments/:id/duration endpoint
- GET /api/students/:id/attendance-report endpoint
- POST /api/appointments/make-up endpoint

### Frontend Changes
- Attendance marking interface
- Duration tracker/timer
- Absence reason selector
- Attendance report viewer
- Make-up session scheduler
- Pattern analysis dashboard

### Business Logic
- Attendance percentage calculations
- Chronic absenteeism alerts
- Make-up session rules
- Billing adjustments
- Compliance reporting

## Testing Requirements
- Unit tests for calculations
- Integration tests for workflows
- Timer accuracy tests
- Report generation tests
- Billing calculation tests

## Dependencies
- Appointment system complete
- Billing rules defined

## Requirements Traceability
- Requirement #20: System must track session duration and attendance
- Business Rule #90: Service authorizations cannot be exceeded

## Definition of Done
- [ ] Attendance tracking functional
- [ ] Duration recording accurate
- [ ] Reports generating correctly
- [ ] Billing integration complete
- [ ] Alerts configured
- [ ] Tests passing
- [ ] Documentation complete" \
"backend,frontend,database" \
"P1-High"

# P2 Medium Priority Stories - Continued
echo "Creating additional P2 Medium priority stories..."

# Story 19: Advanced Reporting Analytics
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

# Story 20: Batch Claims Processing
create_story "Implement batch claims submission and processing" \
"## User Story
As a billing specialist, I want to submit claims in batches so that I can efficiently process multiple claims and track their status.

## Background
Claims must support batch submission with validation, tracking, and error handling for efficient billing operations.

## Acceptance Criteria
- Given multiple claims exist, when selecting for batch, then validation should occur
- Given a batch is created, when submitting, then all claims should be processed together
- Given submission occurs, when errors happen, then individual claim status should be tracked
- Given batches are submitted, when checking status, then real-time updates should be available
- Given errors occur, when reviewing, then correction workflow should be clear
- Given successful submission, when confirmed, then acknowledgments should be recorded

## Technical Implementation
### Database Changes
- Create claim_batches table
- Add batch_status tracking
- Create batch_errors table
- Add submission_history table
- Create reconciliation tables

### API Changes
- POST /api/claims/batch endpoint
- PUT /api/claims/batch/:id/submit endpoint
- GET /api/claims/batch/:id/status endpoint
- POST /api/claims/batch/:id/correct endpoint
- GET /api/claims/batch/history endpoint

### Frontend Changes
- Batch creation interface
- Claim selection tools
- Validation feedback
- Status dashboard
- Error correction workflow
- Submission history

### Integration Features
- EDI file generation
- Clearinghouse APIs
- Status polling
- Error parsing
- Acknowledgment processing

## Testing Requirements
- Unit tests for batch logic
- Integration tests with clearinghouses
- Error handling tests
- Performance tests for large batches
- EDI format validation

## Dependencies
- Claims system complete
- EDI libraries
- Clearinghouse credentials

## Requirements Traceability
- Requirement #34: Claims must support batch submission
- Business Rule #85: Claims must be submitted within timely filing limits

## Definition of Done
- [ ] Batch processing working
- [ ] EDI generation correct
- [ ] Status tracking real-time
- [ ] Error handling robust
- [ ] Performance acceptable
- [ ] Integration tested
- [ ] Documentation complete" \
"backend,frontend,database,integration" \
"P2-Medium"

# Story 21: Denial Management Workflow
create_story "Create comprehensive denial management and appeals system" \
"## User Story
As a billing specialist, I want to manage claim denials and appeals so that I can maximize reimbursement and track denial patterns.

## Background
System must track payments and denials with a complete appeals workflow to manage the revenue cycle effectively.

## Acceptance Criteria
- Given a denial is received, when entered, then reason codes should be categorized
- Given denials exist, when reviewing, then patterns should be identifiable
- Given an appeal is needed, when creating, then workflow should guide through requirements
- Given appeals are submitted, when tracking, then deadlines should be monitored
- Given denial patterns exist, when analyzing, then preventive actions should be suggested
- Given appeals succeed, when updated, then payment adjustments should be automatic

## Technical Implementation
### Database Changes
- Create denials table
- Add denial_reasons table
- Create appeals table
- Add appeal_documents table
- Create denial_analytics views

### API Changes
- POST /api/claims/:id/denial endpoint
- POST /api/denials/:id/appeal endpoint
- GET /api/denials/analytics endpoint
- PUT /api/appeals/:id/status endpoint
- GET /api/denials/patterns endpoint

### Frontend Changes
- Denial entry interface
- Reason code selector
- Appeals workflow wizard
- Document upload system
- Analytics dashboard
- Deadline tracker

### Business Logic
- Reason code mapping
- Appeal deadline calculations
- Pattern detection algorithms
- Success rate tracking
- Preventive suggestions

## Testing Requirements
- Unit tests for workflows
- Deadline calculation tests
- Pattern detection tests
- Document handling tests
- Analytics accuracy tests

## Dependencies
- Claims system complete
- Document storage
- Analytics engine

## Requirements Traceability
- Requirement #35: System must track payments and denials
- Requirement #36: Denial management must include appeals workflow

## Definition of Done
- [ ] Denial tracking complete
- [ ] Appeals workflow functional
- [ ] Analytics providing insights
- [ ] Deadlines monitored
- [ ] Documents manageable
- [ ] Patterns detectable
- [ ] All tests passing" \
"backend,frontend,database" \
"P2-Medium"

# Story 22: Multi-Factor Authentication
create_story "Implement multi-factor authentication for admin accounts" \
"## User Story
As a security-conscious admin, I want to use multi-factor authentication so that my account has additional protection against unauthorized access.

## Background
System must support MFA for admin accounts to enhance security for privileged users.

## Acceptance Criteria
- Given I am an admin, when logging in, then MFA should be required
- Given MFA is enabled, when setting up, then multiple methods should be available
- Given I lose my device, when recovering, then backup codes should work
- Given MFA is active, when authenticating, then time-based codes should be validated
- Given multiple devices, when managing, then I should see all registered devices
- Given security concerns, when needed, then MFA can be required for all users

## Technical Implementation
### Database Changes
- Create mfa_devices table
- Add backup_codes table
- Create mfa_settings table
- Add mfa_audit_log table
- Update users table

### API Changes
- POST /api/auth/mfa/setup endpoint
- POST /api/auth/mfa/verify endpoint
- GET /api/auth/mfa/backup-codes endpoint
- DELETE /api/auth/mfa/device/:id endpoint
- PUT /api/admin/mfa/require endpoint

### Frontend Changes
- MFA setup wizard
- QR code generator
- Verification interface
- Device management
- Backup code display
- Recovery flow

### Security Implementation
- TOTP implementation
- SMS backup option
- Backup code generation
- Device fingerprinting
- Rate limiting

## Testing Requirements
- Unit tests for TOTP
- Integration tests for auth flow
- Security tests for bypass attempts
- UI tests for setup
- Recovery flow tests

## Dependencies
- QR code library
- TOTP library
- SMS service (optional)

## Requirements Traceability
- Requirement #59: System must support MFA for admin accounts

## Definition of Done
- [ ] MFA implementation complete
- [ ] Multiple methods supported
- [ ] Backup codes working
- [ ] Device management functional
- [ ] Security validated
- [ ] UX optimized
- [ ] Documentation complete" \
"backend,frontend,security" \
"P2-Medium"

# P3 Low Priority Stories - Performance and Optimization
echo "Creating P3 Low priority stories..."

# Story 23: Performance Monitoring and Optimization
create_story "Implement comprehensive performance monitoring system" \
"## User Story
As a system administrator, I want to monitor system performance metrics so that I can ensure optimal user experience and identify bottlenecks.

## Background
System must meet specific performance requirements including page load times, API response times, and query performance.

## Acceptance Criteria
- Given the system is running, when monitoring, then all key metrics should be tracked
- Given performance issues occur, when detected, then alerts should be triggered
- Given metrics are collected, when analyzing, then trends should be visible
- Given optimization is needed, when implementing, then improvements should be measurable
- Given different conditions, when testing, then performance should remain consistent
- Given reports are needed, when generating, then performance history should be available

## Technical Implementation
### Database Changes
- Create performance_metrics table
- Add query_performance_log table
- Create slow_query_log table
- Add index_usage_stats table
- Optimize existing indexes

### Infrastructure Changes
- APM tool integration
- Metrics collection agents
- Log aggregation setup
- Alert configuration
- Dashboard creation

### API Changes
- Performance middleware
- Metrics endpoints
- Health check endpoints
- Cache warming endpoints
- Optimization endpoints

### Frontend Changes
- Performance dashboard
- Real-time metrics display
- Alert management UI
- Optimization recommendations
- Historical trends viewer

## Testing Requirements
- Load testing scenarios
- Stress testing plans
- Performance regression tests
- Monitoring accuracy tests
- Alert reliability tests

## Dependencies
- APM tool selection
- Metrics storage solution
- Alerting infrastructure

## Requirements Traceability
- Requirement #51: Page load time must be <2 seconds on 4G networks
- Requirement #52: API response time must be <200ms for read operations
- Requirement #54: Database queries must complete in <100ms

## Definition of Done
- [ ] Monitoring infrastructure deployed
- [ ] All metrics being collected
- [ ] Dashboards configured
- [ ] Alerts functional
- [ ] Performance targets met
- [ ] Documentation complete
- [ ] Runbooks created" \
"backend,infrastructure,testing" \
"P3-Low"

# Story 24: Horizontal Scaling Implementation
create_story "Configure horizontal scaling for API layer" \
"## User Story
As a DevOps engineer, I want to implement horizontal scaling for the API layer so that the system can handle increased load automatically.

## Background
System must scale horizontally for the API layer to support 10,000 concurrent users and handle growth.

## Acceptance Criteria
- Given increased load, when detected, then new instances should be automatically provisioned
- Given load decreases, when sustained, then instances should be decommissioned
- Given multiple instances, when running, then load should be balanced effectively
- Given instance failure, when detected, then traffic should be rerouted automatically
- Given scaling occurs, when monitoring, then all instances should be healthy
- Given different times, when analyzing, then scaling patterns should be optimized

## Technical Implementation
### Infrastructure Changes
- Container orchestration setup
- Load balancer configuration
- Auto-scaling policies
- Health check implementation
- Service discovery setup

### API Changes
- Stateless service design
- Session management updates
- Cache distribution
- Queue implementation
- Circuit breakers

### Monitoring Setup
- Instance metrics
- Scaling events tracking
- Performance monitoring
- Cost optimization
- Capacity planning

### Configuration Management
- Environment variables
- Configuration service
- Secret management
- Feature flags
- Deployment automation

## Testing Requirements
- Load testing at scale
- Failover testing
- Auto-scaling validation
- Performance benchmarks
- Chaos engineering tests

## Dependencies
- Container platform
- Load balancer solution
- Monitoring tools
- CI/CD pipeline

## Requirements Traceability
- Requirement #53: System must support 10,000 concurrent users
- Requirement #66: System must scale horizontally for API layer
- Requirement #69: System must handle 100% YoY growth

## Definition of Done
- [ ] Auto-scaling configured
- [ ] Load balancing working
- [ ] Health checks active
- [ ] Monitoring complete
- [ ] Performance validated
- [ ] Cost optimized
- [ ] Documentation updated" \
"infrastructure,backend,testing" \
"P3-Low"

# Story 25: Advanced Security Hardening
create_story "Implement advanced security hardening measures" \
"## User Story
As a security engineer, I want to implement comprehensive security hardening so that the system is protected against common and advanced threats.

## Background
Beyond basic security requirements, the system needs advanced hardening measures for healthcare data protection.

## Acceptance Criteria
- Given security scans run, when completed, then no critical vulnerabilities should exist
- Given attacks are attempted, when detected, then they should be blocked and logged
- Given security policies, when enforced, then compliance should be verifiable
- Given incidents occur, when detected, then response procedures should be triggered
- Given configurations exist, when audited, then they should meet security baselines
- Given updates are available, when assessed, then patches should be applied timely

## Technical Implementation
### Security Measures
- WAF implementation
- DDoS protection
- Intrusion detection system
- Security scanning automation
- Vulnerability management

### API Security
- API gateway hardening
- Rate limiting enhancement
- Input validation library
- Output encoding
- CORS configuration

### Database Security
- Database firewall
- Query parameterization
- Encryption key rotation
- Access logging
- Privilege minimization

### Application Security
- Security headers
- CSP implementation
- HSTS configuration
- Certificate pinning
- Session security

## Testing Requirements
- Penetration testing
- Security scanning
- Compliance validation
- Incident response drills
- Security training verification

## Dependencies
- Security tools procurement
- Security team training
- Incident response plan
- Compliance frameworks

## Requirements Traceability
- Requirement #57: All communications must use TLS 1.3+
- Requirement #58: Passwords must meet complexity requirements
- Requirement #60: API must implement rate limiting (100 req/min)

## Definition of Done
- [ ] All hardening measures implemented
- [ ] Security scans passing
- [ ] Penetration test completed
- [ ] Compliance validated
- [ ] Monitoring active
- [ ] Response plans tested
- [ ] Team trained" \
"security,backend,infrastructure" \
"P3-Low"

echo "All stories created successfully!"
echo ""
echo "Total stories created: 25"
echo "Coverage includes:"
echo "- Core authentication and security (P0)"
echo "- Essential features (P1)"
echo "- Enhanced functionality (P2)"
echo "- Performance and optimization (P3)"
echo ""
echo "Next steps:"
echo "1. Create stories for remaining functional requirements"
echo "2. Create stories for integration requirements"
echo "3. Create stories for remaining non-functional requirements"
echo "4. Create epic stories to group related work"