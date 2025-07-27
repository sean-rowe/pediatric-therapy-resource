#!/bin/bash

# Create GitHub Stories for TherapyDocs Project
# This script creates stories for all requirements with proper formatting

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

# P0 Critical Stories - Core Authentication & Security
echo "Creating P0 Critical Stories..."

# Story 1: User Registration
create_story "Implement therapist registration with profile validation" \
"## User Story
As a therapist, I want to register for an account with my professional information so that I can access the system and manage my students.

## Background
Therapists need to register with complete profile information including email, password, name, service type (SLP/OT/PT), and optional license information. This is the entry point to the system and must be secure and user-friendly.

## Acceptance Criteria
- Given I am on the registration page, when I enter my email, password, name, and service type, then my account should be created successfully
- Given I enter an email that already exists, when I submit the form, then I should see an error message
- Given I enter a weak password, when I submit the form, then I should see password requirements
- Given I include my license information, when I register, then it should be stored and validated
- Given I complete registration, when successful, then I should receive a confirmation email
- Given my registration is complete, when I log in, then I should see my dashboard

## Technical Implementation
### Database Changes
- Ensure users table has all required fields
- Add indexes on email for uniqueness
- Add license_info JSON column if not present

### API Changes
- POST /api/auth/register endpoint
- Input validation for all fields
- Email uniqueness check
- Password strength validation
- License format validation

### Frontend Changes
- Registration form with all fields
- Real-time validation
- Password strength indicator
- Service type dropdown
- Optional license fields

### Security Considerations
- Hash passwords with bcrypt
- Validate email format
- Prevent SQL injection
- Rate limit registration attempts

## Testing Requirements
- Unit tests for registration logic
- Integration tests for API endpoint
- UI tests for form validation
- Security tests for password handling
- Load tests for concurrent registrations

## Dependencies
- Email service for confirmations
- Database schema must be complete

## Requirements Traceability
- Requirement #1: Therapists must register with email, password, name, service type, and optional license information
- Business Rule #81: Email addresses must be unique across all users

## Definition of Done
- [ ] Database schema updated
- [ ] API endpoint implemented and tested
- [ ] Frontend form complete with validation
- [ ] Email confirmation working
- [ ] All tests passing
- [ ] Security review completed
- [ ] Documentation updated" \
"backend,frontend,database,security" \
"P0-Critical"

# Story 2: JWT Authentication
create_story "Implement JWT-based authentication system" \
"## User Story
As a registered user, I want to securely log in with my credentials so that I can access my personalized workspace and maintain session security.

## Background
Users must authenticate with email and password to receive JWT tokens. This is critical for system security and must implement industry best practices for token management.

## Acceptance Criteria
- Given valid credentials, when I log in, then I should receive a JWT token
- Given invalid credentials, when I attempt login, then I should see an appropriate error
- Given a valid token, when I make API requests, then they should be authenticated
- Given an expired token, when I make requests, then I should be prompted to re-authenticate
- Given I'm logged in, when I log out, then my token should be invalidated
- Given multiple failed attempts, when I try to log in, then my account should be temporarily locked

## Technical Implementation
### Database Changes
- Add refresh_tokens table
- Add login_attempts tracking
- Add account lockout fields

### API Changes
- POST /api/auth/login endpoint
- POST /api/auth/refresh endpoint
- POST /api/auth/logout endpoint
- JWT middleware for protected routes
- Rate limiting on auth endpoints

### Frontend Changes
- Login form with remember me option
- Token storage in secure storage
- Automatic token refresh
- Logout functionality
- Session timeout warnings

### Security Considerations
- Use secure token storage
- Implement refresh token rotation
- Set appropriate token expiration
- Prevent brute force attacks
- Log authentication events

## Testing Requirements
- Unit tests for JWT generation/validation
- Integration tests for auth flow
- Security tests for token handling
- Performance tests for auth middleware
- E2E tests for login/logout flow

## Dependencies
- JWT library implementation
- Secure storage solution

## Requirements Traceability
- Requirement #2: Users must authenticate with email and password to receive JWT tokens
- Requirement #4: User sessions must expire after 30 minutes of inactivity

## Definition of Done
- [ ] JWT implementation complete
- [ ] Login/logout endpoints working
- [ ] Token refresh mechanism in place
- [ ] Session timeout implemented
- [ ] All tests passing
- [ ] Security audit completed
- [ ] Performance benchmarks met" \
"backend,frontend,security" \
"P0-Critical"

# Story 3: Password Reset
create_story "Implement secure password reset workflow" \
"## User Story
As a user who forgot my password, I want to reset it securely via email so that I can regain access to my account without compromising security.

## Background
System must support password reset via email with secure tokens. This is a critical security feature that must prevent unauthorized access while being user-friendly.

## Acceptance Criteria
- Given I forgot my password, when I request a reset, then I should receive an email with a secure link
- Given a valid reset token, when I set a new password, then it should update successfully
- Given an expired token, when I try to use it, then I should see an error message
- Given I reset my password, when complete, then I should receive a confirmation email
- Given a used token, when I try to reuse it, then it should be rejected
- Given I request multiple resets, when checking email, then only the latest should be valid

## Technical Implementation
### Database Changes
- Create password_reset_tokens table
- Add token expiration tracking
- Add token usage tracking

### API Changes
- POST /api/auth/forgot-password endpoint
- POST /api/auth/reset-password endpoint
- GET /api/auth/verify-reset-token endpoint
- Token generation and validation logic

### Frontend Changes
- Forgot password form
- Reset password form
- Token validation on page load
- Password strength requirements
- Success/error messaging

### Security Considerations
- Use cryptographically secure tokens
- Set 1-hour expiration
- One-time use tokens only
- Rate limit reset requests
- Log all reset attempts

## Testing Requirements
- Unit tests for token generation
- Integration tests for reset flow
- Security tests for token validation
- Email delivery tests
- UI tests for reset forms

## Dependencies
- Email service configuration
- Secure token generation

## Requirements Traceability
- Requirement #3: System must support password reset via email with secure tokens
- Business Rule #86: Passwords expire every 90 days

## Definition of Done
- [ ] Database tables created
- [ ] API endpoints implemented
- [ ] Email templates created
- [ ] Frontend forms complete
- [ ] Token security validated
- [ ] All tests passing
- [ ] Documentation updated" \
"backend,frontend,database,security" \
"P0-Critical"

# Story 4: HIPAA Audit Logging
create_story "Implement HIPAA-compliant audit logging system" \
"## User Story
As a compliance officer, I want comprehensive audit logs of all system activities so that we can maintain HIPAA compliance and investigate security incidents.

## Background
System must maintain HIPAA-compliant audit logs for all data access and modifications. This is legally required and critical for security compliance.

## Acceptance Criteria
- Given any data access, when it occurs, then it should be logged with user, timestamp, and action
- Given a data modification, when saved, then the before/after states should be logged
- Given a security event, when it happens, then it should be immediately logged and flagged
- Given audit logs, when reviewed, then they should be immutable and tamper-evident
- Given a compliance audit, when requested, then logs should be exportable in standard format
- Given log retention policy, when logs age out, then they should be archived securely

## Technical Implementation
### Database Changes
- Create audit_logs table with appropriate indexes
- Implement triggers for data changes
- Add audit_log_archive table
- Create audit reporting views

### API Changes
- Audit middleware for all endpoints
- Audit log query endpoints
- Log export functionality
- Real-time alerting for security events

### Frontend Changes
- Audit log viewer for admins
- Search and filter capabilities
- Export functionality
- Real-time alerts dashboard

### Security Considerations
- Logs must be write-only
- Implement log integrity checks
- Encrypt sensitive data in logs
- Secure log storage and backup
- Access control for log viewing

## Testing Requirements
- Unit tests for audit triggers
- Integration tests for logging flow
- Performance tests for high-volume logging
- Security tests for log tampering
- Compliance validation tests

## Dependencies
- Database trigger support
- Secure storage solution
- Alerting infrastructure

## Requirements Traceability
- Requirement #37: System must maintain HIPAA-compliant audit logs
- Requirement #41: Security incidents must be tracked and reported

## Definition of Done
- [ ] Audit tables created
- [ ] Logging triggers implemented
- [ ] API middleware in place
- [ ] Log viewer functional
- [ ] Performance validated
- [ ] Security review completed
- [ ] HIPAA compliance verified" \
"backend,database,security,compliance" \
"P0-Critical"

# Story 5: Data Encryption at Rest
create_story "Implement AES-256 encryption for data at rest" \
"## User Story
As a security administrator, I want all sensitive data encrypted at rest so that we protect patient information even if physical security is compromised.

## Background
All data must be encrypted at rest using AES-256. This is a fundamental security requirement for healthcare applications.

## Acceptance Criteria
- Given sensitive data is stored, when written to disk, then it should be encrypted with AES-256
- Given encrypted data, when accessed by authorized users, then it should be transparently decrypted
- Given database backups, when created, then they should be encrypted
- Given file uploads, when stored, then they should be encrypted
- Given encryption keys, when managed, then they should follow key rotation policies
- Given a security audit, when reviewed, then all PHI should be verified as encrypted

## Technical Implementation
### Database Changes
- Enable Transparent Data Encryption (TDE)
- Implement column-level encryption for sensitive fields
- Create key management tables
- Set up encrypted backups

### Infrastructure Changes
- Configure encrypted storage volumes
- Implement key management service
- Set up backup encryption
- Configure monitoring for encryption status

### API Changes
- Add encryption/decryption utilities
- Implement field-level encryption for sensitive data
- Add key rotation endpoints
- Create encryption status monitoring

### Security Considerations
- Use hardware security modules (HSM) if available
- Implement key rotation every 90 days
- Separate encryption keys from data
- Monitor encryption performance
- Audit all key access

## Testing Requirements
- Unit tests for encryption utilities
- Integration tests for data flow
- Performance tests for encryption overhead
- Security tests for key management
- Disaster recovery tests

## Dependencies
- Encryption library selection
- Key management infrastructure
- HSM or KMS availability

## Requirements Traceability
- Requirement #56: All data must be encrypted at rest (AES-256)
- Requirement #64: Recovery point objective (RPO) must be <1 hour

## Definition of Done
- [ ] TDE enabled on database
- [ ] Column encryption implemented
- [ ] File storage encrypted
- [ ] Key management in place
- [ ] Performance impact measured
- [ ] Security audit passed
- [ ] Documentation complete" \
"backend,database,infrastructure,security" \
"P0-Critical"

echo "P0 Critical stories created. Continuing with P1 High priority stories..."

# P1 High Priority Stories - Core Functionality

# Story 6: Student Profile Management
create_story "Create comprehensive student profile management system" \
"## User Story
As a therapist, I want to create and manage detailed student profiles so that I can track all relevant information for providing effective therapy services.

## Background
Therapists must create student profiles with demographics, IEP information, medical alerts, and behavioral notes. This is core functionality for the system.

## Acceptance Criteria
- Given I have a new student, when I create their profile, then all required fields should be saved
- Given a student profile exists, when I update it, then changes should be tracked in audit log
- Given medical alerts exist, when viewing a student, then they should be prominently displayed
- Given behavioral notes are added, when saved, then they should be timestamped and attributed
- Given I search for a student, when using various criteria, then relevant results should appear
- Given a student is inactive, when viewing lists, then they should be filtered by default

## Technical Implementation
### Database Changes
- Ensure students table has all fields
- Add medical_alerts table
- Add behavioral_notes table
- Create search indexes
- Add soft delete support

### API Changes
- POST /api/students endpoint
- PUT /api/students/:id endpoint
- GET /api/students search endpoint
- POST /api/students/:id/alerts endpoint
- POST /api/students/:id/notes endpoint

### Frontend Changes
- Student creation form
- Student profile editor
- Medical alerts manager
- Behavioral notes interface
- Advanced search filters
- Quick student finder

### Security Considerations
- Role-based access control
- Audit all student data access
- Encrypt sensitive fields
- Validate data ownership
- Implement field-level permissions

## Testing Requirements
- Unit tests for CRUD operations
- Integration tests for search
- UI tests for forms
- Performance tests for search
- Security tests for access control

## Dependencies
- School management must exist
- User authentication required
- Audit logging must be active

## Requirements Traceability
- Requirement #6: Therapists must create student profiles with demographics and IEP information
- Requirement #7: System must track student medical alerts and behavioral notes
- Requirement #9: System must support student search by name, ID, or school
- Requirement #10: Student records must include parent/guardian contact information

## Definition of Done
- [ ] Database schema complete
- [ ] All API endpoints working
- [ ] Frontend forms functional
- [ ] Search performing well
- [ ] Security validated
- [ ] Tests passing
- [ ] Documentation updated" \
"backend,frontend,database" \
"P1-High"

# Story 7: IEP Goal Creation and Tracking
create_story "Build IEP goal management and progress tracking system" \
"## User Story
As a therapist, I want to create and track measurable IEP goals so that I can monitor student progress and demonstrate outcomes to stakeholders.

## Background
Therapists must create measurable IEP goals with baselines, targets, and measurement methods. Progress tracking is essential for compliance and outcomes reporting.

## Acceptance Criteria
- Given I create a goal, when I save it, then all measurement criteria should be captured
- Given a goal exists, when I record progress, then it should be dated and measured
- Given progress entries exist, when viewing trends, then a graph should show progress over time
- Given a goal is met, when marking complete, then achievement date should be recorded
- Given goals exist, when generating reports, then progress summaries should be accurate
- Given multiple goals, when viewing, then they should be organized by status and priority

## Technical Implementation
### Database Changes
- Create goals table with all fields
- Create goal_progress table
- Add goal_benchmarks table
- Create progress calculation views
- Add archival support

### API Changes
- POST /api/goals endpoint
- PUT /api/goals/:id endpoint
- POST /api/goals/:id/progress endpoint
- GET /api/goals/report endpoint
- PUT /api/goals/:id/status endpoint

### Frontend Changes
- Goal creation wizard
- Progress entry form
- Progress visualization charts
- Goal status manager
- Report generator
- Bulk progress entry

### Business Logic
- Progress calculation algorithms
- Goal completion logic
- Benchmark validation
- Measurement validation
- Report generation logic

## Testing Requirements
- Unit tests for calculations
- Integration tests for workflows
- UI tests for visualizations
- Data validation tests
- Report accuracy tests

## Dependencies
- Student profiles must exist
- Report template system
- Charting library

## Requirements Traceability
- Requirement #11: Therapists must create measurable IEP goals for students
- Requirement #12: Goals must include measurement methods, baselines, and target dates
- Requirement #13: System must track goal progress with dated entries
- Requirement #14: Goals must support status tracking
- Requirement #15: System must generate goal progress reports
- Business Rule #83: Goals must have measurable criteria

## Definition of Done
- [ ] Goal management complete
- [ ] Progress tracking working
- [ ] Visualizations implemented
- [ ] Reports generating correctly
- [ ] All validations in place
- [ ] Tests passing
- [ ] Performance optimized" \
"backend,frontend,database" \
"P1-High"

# Story 8: Session Documentation with SOAP Notes
create_story "Implement therapy session documentation with SOAP format" \
"## User Story
As a therapist, I want to document my therapy sessions using SOAP notes so that I can maintain professional documentation standards and track treatment effectiveness.

## Background
Therapists must document sessions using SOAP (Subjective, Objective, Assessment, Plan) format with support for offline creation and synchronization.

## Acceptance Criteria
- Given I complete a session, when documenting, then SOAP format should be enforced
- Given I'm offline, when creating notes, then they should save locally and sync later
- Given notes are synced, when conflicts exist, then resolution should preserve all data
- Given a session is documented, when viewing history, then all versions should be available
- Given documentation exists, when generating reports, then SOAP notes should be included
- Given 7 days have passed, when attempting to document, then the system should warn/prevent

## Technical Implementation
### Database Changes
- Create session_notes table
- Add SOAP format columns
- Create note_versions table
- Add sync_status tracking
- Create conflict_resolution table

### API Changes
- POST /api/sessions/:id/notes endpoint
- PUT /api/sessions/:id/notes endpoint
- POST /api/sync/notes endpoint
- GET /api/sessions/:id/notes/versions
- POST /api/notes/resolve-conflict

### Frontend Changes
- SOAP note editor
- Offline storage capability
- Sync status indicator
- Conflict resolution UI
- Version history viewer
- Auto-save functionality

### Offline Support
- Local storage implementation
- Sync queue management
- Conflict detection
- Merge strategies
- Retry mechanisms

## Testing Requirements
- Unit tests for SOAP validation
- Integration tests for sync
- Offline functionality tests
- Conflict resolution tests
- Performance tests for sync
- UI tests for editor

## Dependencies
- Session scheduling complete
- Offline storage library
- Sync infrastructure

## Requirements Traceability
- Requirement #18: Therapists must document sessions using SOAP format
- Requirement #19: Documentation must support offline creation with sync
- Business Rule #84: Sessions cannot be documented after 7 days

## Definition of Done
- [ ] SOAP editor complete
- [ ] Offline support working
- [ ] Sync mechanism reliable
- [ ] Conflict resolution tested
- [ ] Version history functional
- [ ] 7-day rule enforced
- [ ] All tests passing" \
"backend,frontend,database" \
"P1-High"

# Story 9: Appointment Scheduling System
create_story "Build comprehensive appointment scheduling with recurring support" \
"## User Story
As a therapist, I want to schedule and manage therapy appointments including recurring sessions so that I can organize my caseload efficiently.

## Background
Therapists must schedule therapy sessions with support for recurring appointments. This is essential for managing regular therapy schedules.

## Acceptance Criteria
- Given I need to schedule a session, when I create it, then it should appear in my calendar
- Given I set up recurring appointments, when saved, then all instances should be created
- Given a conflict exists, when scheduling, then I should be warned
- Given appointments exist, when viewing calendar, then they should be clearly displayed
- Given I need to cancel, when I do so, then notifications should be sent
- Given recurring appointments exist, when modifying, then I should choose scope of changes

## Technical Implementation
### Database Changes
- Create appointments table
- Add recurring_patterns table
- Create appointment_series table
- Add appointment_conflicts view
- Create cancellation_log table

### API Changes
- POST /api/appointments endpoint
- PUT /api/appointments/:id endpoint
- DELETE /api/appointments/:id endpoint
- POST /api/appointments/recurring endpoint
- GET /api/appointments/conflicts endpoint
- GET /api/calendar endpoint

### Frontend Changes
- Calendar interface
- Appointment creation form
- Recurring pattern builder
- Conflict resolution UI
- Bulk scheduling tool
- Cancellation workflow

### Business Logic
- Recurrence pattern engine
- Conflict detection algorithm
- Time zone handling
- Notification triggers
- Calendar generation

## Testing Requirements
- Unit tests for recurrence logic
- Integration tests for scheduling
- UI tests for calendar
- Performance tests for bulk operations
- Time zone tests
- Conflict detection tests

## Dependencies
- Student profiles required
- Notification system
- Calendar library

## Requirements Traceability
- Requirement #16: Therapists must schedule therapy sessions with students
- Requirement #17: System must support recurring appointment scheduling
- Requirement #20: System must track session duration and attendance

## Definition of Done
- [ ] Basic scheduling working
- [ ] Recurring appointments functional
- [ ] Calendar view complete
- [ ] Conflict detection active
- [ ] Notifications configured
- [ ] Performance optimized
- [ ] All tests passing" \
"backend,frontend,database" \
"P1-High"

# Story 10: Insurance Information Management
create_story "Create insurance tracking and claims preparation system" \
"## User Story
As a billing coordinator, I want to track student insurance information and prepare claims so that services can be properly billed and reimbursed.

## Background
System must track insurance information for students and enable therapists to create claims for services rendered. This is critical for practice revenue.

## Acceptance Criteria
- Given a student has insurance, when I enter details, then all required fields should be validated
- Given services are provided, when creating claims, then CPT codes should be validated
- Given claims are created, when reviewing, then all required information should be present
- Given multiple insurances exist, when billing, then coordination of benefits should apply
- Given insurance changes, when updated, then history should be maintained
- Given claims are ready, when exporting, then format should match payer requirements

## Technical Implementation
### Database Changes
- Create insurance_info table
- Add claims table
- Create claim_lines table
- Add payer_rules table
- Create eligibility_checks table

### API Changes
- POST /api/students/:id/insurance endpoint
- POST /api/claims endpoint
- PUT /api/claims/:id endpoint
- POST /api/claims/validate endpoint
- POST /api/insurance/eligibility endpoint
- GET /api/claims/export endpoint

### Frontend Changes
- Insurance entry form
- Claims creation wizard
- CPT code selector
- Validation feedback
- Claims review screen
- Export interface

### Integration Requirements
- CPT code database
- Payer-specific rules
- EDI formatting
- Eligibility checking
- Prior auth tracking

## Testing Requirements
- Unit tests for validation rules
- Integration tests for claims flow
- CPT code validation tests
- Export format tests
- UI tests for forms
- Payer rule tests

## Dependencies
- CPT code database
- EDI libraries
- Payer configurations

## Requirements Traceability
- Requirement #31: System must track insurance information for students
- Requirement #32: Therapists must create claims for services rendered
- Requirement #33: System must validate CPT codes and modifiers
- Business Rule #85: Claims must be submitted within timely filing limits

## Definition of Done
- [ ] Insurance tracking complete
- [ ] Claims creation working
- [ ] Validation rules active
- [ ] Export formats correct
- [ ] All payers configured
- [ ] Tests passing
- [ ] Documentation complete" \
"backend,frontend,database" \
"P1-High"

echo "P1 High priority stories created. Continuing with P2 Medium priority stories..."

# P2 Medium Priority Stories - Enhanced Features

# Story 11: AI-Powered Content Generation
create_story "Implement AI-powered therapy material generation" \
"## User Story
As a therapist, I want to generate customized therapy materials using AI so that I can quickly create age-appropriate activities for my students.

## Background
System must generate therapy materials using AI including mazes, worksheets, and activities that are age and skill appropriate.

## Acceptance Criteria
- Given student age and skill level, when requesting materials, then appropriate content should be generated
- Given generated content, when reviewing, then it should match therapeutic goals
- Given content is generated, when saved, then it should be added to the library
- Given I use content, when rating effectiveness, then ratings should inform future generations
- Given content exists, when searching, then I should find relevant materials quickly
- Given AI generates content, when reviewing, then it should be safe and appropriate

## Technical Implementation
### Database Changes
- Create content_library table
- Add content_ratings table
- Create content_metadata table
- Add generation_parameters table
- Create content_categories table

### API Changes
- POST /api/content/generate endpoint
- GET /api/content/search endpoint
- POST /api/content/:id/rate endpoint
- GET /api/content/recommendations endpoint
- POST /api/content/save endpoint

### Frontend Changes
- Content generation wizard
- Parameter selection UI
- Content preview/editor
- Rating interface
- Library browser
- Search and filters

### AI Integration
- OpenAI API integration
- Prompt engineering
- Content validation
- Safety filters
- Quality scoring

## Testing Requirements
- Unit tests for generation logic
- Integration tests with AI service
- Content quality tests
- Safety validation tests
- Performance tests
- UI/UX tests

## Dependencies
- OpenAI API access
- Content storage solution
- PDF generation library

## Requirements Traceability
- Requirement #21: System must generate therapy materials using AI
- Requirement #22: Generated content must be age and skill appropriate
- Requirement #23: Users must rate content effectiveness
- Requirement #24: System must recommend content based on student needs
- Requirement #25: Content library must support search and filtering

## Definition of Done
- [ ] AI integration complete
- [ ] Generation working reliably
- [ ] Content quality validated
- [ ] Rating system functional
- [ ] Search performing well
- [ ] Safety measures in place
- [ ] All tests passing" \
"backend,frontend,database,ai" \
"P2-Medium"

# Story 12: Digital Evaluation Platform
create_story "Build digital standardized evaluation system with auto-scoring" \
"## User Story
As a therapist, I want to conduct standardized evaluations digitally with automatic scoring so that I can efficiently assess students and generate comprehensive reports.

## Background
Therapists must conduct standardized evaluations digitally with auto-scoring capabilities and comprehensive report generation.

## Acceptance Criteria
- Given I start an evaluation, when administering items, then responses should be captured accurately
- Given responses are entered, when completed, then scores should be calculated automatically
- Given evaluation is complete, when generating report, then it should include all required sections
- Given historical evaluations exist, when comparing, then progress should be visualized
- Given an evaluation tool, when selected, then age-appropriate norms should be applied
- Given results exist, when interpreting, then recommendations should be evidence-based

## Technical Implementation
### Database Changes
- Create evaluations table
- Add evaluation_items table
- Create scoring_rules table
- Add norm_tables table
- Create evaluation_reports table

### API Changes
- POST /api/evaluations endpoint
- PUT /api/evaluations/:id/items endpoint
- POST /api/evaluations/:id/score endpoint
- POST /api/evaluations/:id/report endpoint
- GET /api/evaluations/history endpoint

### Frontend Changes
- Evaluation selector
- Item administration UI
- Response capture forms
- Auto-scoring display
- Report generator
- Progress comparisons

### Scoring Engine
- Rule-based scoring
- Norm table lookups
- Standard score calculations
- Percentile rankings
- Confidence intervals

## Testing Requirements
- Unit tests for scoring algorithms
- Integration tests for workflows
- Accuracy tests against manual scoring
- Report generation tests
- UI/UX testing
- Performance tests

## Dependencies
- Evaluation tool licenses
- Norm table data
- Report templates
- PDF generation

## Requirements Traceability
- Requirement #26: Therapists must conduct standardized evaluations digitally
- Requirement #27: System must auto-score evaluation items
- Requirement #28: Evaluations must generate comprehensive reports
- Requirement #29: System must track evaluation history
- Requirement #30: Reports must include interpretation and recommendations

## Definition of Done
- [ ] Evaluation platform built
- [ ] Auto-scoring accurate
- [ ] Reports generating properly
- [ ] History tracking working
- [ ] All tools configured
- [ ] Accuracy validated
- [ ] Tests passing" \
"backend,frontend,database" \
"P2-Medium"

# Story 13: Parent Portal Implementation
create_story "Create secure parent portal for progress access and communication" \
"## User Story
As a parent, I want to access my child's therapy progress and communicate with therapists so that I can be involved in their treatment and support their development.

## Background
Parents must access student progress reports and communicate with therapists securely. The portal must be multilingual and respect consent status.

## Acceptance Criteria
- Given I am a parent, when I log in, then I should only see my authorized children
- Given progress reports exist, when viewing, then I should see current status and trends
- Given I have questions, when messaging therapist, then communication should be secure
- Given documents are shared, when accessing, then they should be downloadable
- Given I prefer Spanish, when using portal, then all content should be translated
- Given consent is revoked, when accessing, then I should see appropriate restrictions

## Technical Implementation
### Database Changes
- Create parent_accounts table
- Add parent_student_access table
- Create parent_messages table
- Add document_sharing table
- Create consent_tracking table

### API Changes
- POST /api/parents/register endpoint
- GET /api/parents/students endpoint
- GET /api/parents/progress/:studentId endpoint
- POST /api/parents/messages endpoint
- GET /api/parents/documents endpoint
- PUT /api/parents/preferences endpoint

### Frontend Changes
- Parent registration flow
- Student progress dashboard
- Messaging interface
- Document library
- Language selector
- Consent status display

### Security & Localization
- Parent authentication separate from therapist
- Role-based access control
- Message encryption
- Multi-language support
- Consent enforcement

## Testing Requirements
- Unit tests for access control
- Integration tests for messaging
- Localization testing
- Security penetration tests
- UI/UX tests in multiple languages
- Consent workflow tests

## Dependencies
- Translation service
- Document storage
- Messaging infrastructure
- Authentication system

## Requirements Traceability
- Requirement #42: Parents must access student progress reports
- Requirement #43: Parents must communicate with therapists securely
- Requirement #44: System must support document sharing with parents
- Requirement #45: Parent notifications must be multilingual
- Requirement #46: Access must be controlled by consent status
- Business Rule #88: Consent must be obtained before sharing data

## Definition of Done
- [ ] Parent portal functional
- [ ] Progress viewing working
- [ ] Messaging implemented
- [ ] Documents shareable
- [ ] Translations complete
- [ ] Consent enforced
- [ ] Security validated" \
"backend,frontend,database,security" \
"P2-Medium"

# Story 14: Teletherapy Support Features
create_story "Add teletherapy session tracking and resource management" \
"## User Story
As a therapist providing virtual services, I want to track teletherapy sessions and manage digital resources so that I can deliver effective remote therapy.

## Background
System must track virtual session details, manage digital resources, record quality metrics, and support platform integrations.

## Acceptance Criteria
- Given a virtual session, when documenting, then platform and quality metrics should be captured
- Given digital resources exist, when searching, then I should find teletherapy-specific materials
- Given quality issues occur, when recorded, then patterns should be identifiable
- Given different platforms are used, when integrating, then session data should sync
- Given virtual sessions occur, when reporting, then they should be distinguished from in-person
- Given resources are used, when tracking, then effectiveness should be measurable

## Technical Implementation
### Database Changes
- Add teletherapy_sessions table
- Create digital_resources table
- Add quality_metrics table
- Create platform_integrations table
- Add resource_usage_tracking table

### API Changes
- POST /api/teletherapy/sessions endpoint
- POST /api/teletherapy/quality endpoint
- GET /api/resources/digital endpoint
- POST /api/integrations/sync endpoint
- GET /api/teletherapy/analytics endpoint

### Frontend Changes
- Teletherapy session form
- Quality metrics capture
- Digital resource library
- Platform integration settings
- Analytics dashboard
- Resource effectiveness tracking

### Integration Points
- Zoom API integration
- Teams integration
- Google Meet support
- Resource CDN
- Analytics engine

## Testing Requirements
- Integration tests with platforms
- Quality metric validation
- Resource search performance
- Analytics accuracy tests
- UI/UX tests
- Load tests for resources

## Dependencies
- Video platform APIs
- CDN for resources
- Analytics infrastructure

## Requirements Traceability
- Requirement #47: System must track virtual session details
- Requirement #48: Virtual resources must be categorized and searchable
- Requirement #49: Session quality metrics must be recorded
- Requirement #50: Platform integrations must be supported

## Definition of Done
- [ ] Teletherapy tracking complete
- [ ] Resource library functional
- [ ] Quality metrics capturing
- [ ] Platform integrations working
- [ ] Analytics providing insights
- [ ] Performance optimized
- [ ] All tests passing" \
"backend,frontend,database,integration" \
"P2-Medium"

# Story 15: Compliance Reporting Suite
create_story "Build comprehensive compliance and regulatory reporting system" \
"## User Story
As a compliance officer, I want to generate state-specific compliance reports and track regulatory requirements so that our practice maintains all necessary certifications.

## Background
System must generate state-specific compliance reports, track FERPA consents, and maintain comprehensive audit trails for regulatory purposes.

## Acceptance Criteria
- Given state requirements, when generating reports, then all required elements should be included
- Given FERPA consents, when tracking, then expiration and scope should be monitored
- Given data is shared, when logged, then justification and authorization should be recorded
- Given an audit occurs, when providing documentation, then all required reports should be available
- Given regulations change, when updating, then historical compliance should be maintained
- Given multiple states, when reporting, then state-specific rules should apply

## Technical Implementation
### Database Changes
- Create compliance_reports table
- Add state_requirements table
- Create ferpa_consents table
- Add data_sharing_log table
- Create regulatory_updates table

### API Changes
- POST /api/compliance/reports endpoint
- GET /api/compliance/requirements/:state endpoint
- POST /api/compliance/consents endpoint
- GET /api/compliance/audit-trail endpoint
- PUT /api/compliance/regulations endpoint

### Frontend Changes
- Report generation wizard
- Consent management interface
- Data sharing authorization
- Audit trail viewer
- Regulatory update tracker
- Multi-state configuration

### Compliance Engine
- State-specific rule engine
- Report template system
- Consent expiration monitoring
- Automated compliance checks
- Regulatory change tracking

## Testing Requirements
- Unit tests for rule engine
- Report accuracy tests
- Consent workflow tests
- Multi-state scenarios
- Audit trail completeness
- Performance tests

## Dependencies
- State regulatory databases
- Report template engine
- Scheduling system

## Requirements Traceability
- Requirement #38: FERPA consents must be tracked and enforced
- Requirement #39: Data sharing must be logged with justification
- Requirement #40: System must generate state-specific compliance reports
- Business Rule #88: Consent must be obtained before sharing data
- Business Rule #89: Evaluations require physician orders in some states

## Definition of Done
- [ ] Report generation working
- [ ] State rules implemented
- [ ] Consent tracking active
- [ ] Audit trails complete
- [ ] Multi-state support tested
- [ ] Compliance validated
- [ ] Documentation updated" \
"backend,frontend,database,compliance" \
"P2-Medium"

echo "All stories created successfully!"
echo "Summary:"
echo "- P0 Critical: 5 stories (Core security and authentication)"
echo "- P1 High: 5 stories (Core functionality)"
echo "- P2 Medium: 5 stories (Enhanced features)"
echo ""
echo "Total: 15 stories covering critical requirements"
echo ""
echo "Note: Additional stories for remaining requirements, integrations, and non-functional requirements should be created in subsequent phases."