-- =====================================================
-- ADDITIONAL TABLES TO MATCH INDUSTRY STANDARDS
-- =====================================================

USE TherapyDocs;
GO

-- =====================================================
-- BILLING & INSURANCE TABLES
-- =====================================================

-- Insurance Payers
CREATE TABLE insurance_payers (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    payer_name NVARCHAR(255) NOT NULL,
    payer_type NVARCHAR(50) CHECK (payer_type IN ('medicaid', 'private', 'self_pay', 'district')),
    payer_id NVARCHAR(100),
    
    -- Contact Information
    billing_address NVARCHAR(500),
    billing_phone NVARCHAR(20),
    billing_email NVARCHAR(255),
    
    -- Medicaid Specific
    is_medicaid BIT DEFAULT 0,
    medicaid_provider_number NVARCHAR(100),
    state_specific_requirements NVARCHAR(MAX),
    
    -- Rates and Rules
    default_rate_per_unit DECIMAL(10,2),
    billing_unit_minutes INT DEFAULT 15,
    requires_authorization BIT DEFAULT 0,
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Student Insurance Information
CREATE TABLE student_insurance (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    payer_id UNIQUEIDENTIFIER REFERENCES insurance_payers(id),
    
    -- Insurance Details
    policy_number NVARCHAR(100),
    group_number NVARCHAR(100),
    subscriber_name NVARCHAR(255),
    subscriber_relationship NVARCHAR(50),
    
    -- Coverage
    effective_date DATE,
    termination_date DATE,
    authorization_number NVARCHAR(100),
    authorized_visits INT,
    visits_used INT DEFAULT 0,
    
    -- Priority
    coverage_priority INT DEFAULT 1, -- 1=Primary, 2=Secondary, etc.
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Billing Claims
CREATE TABLE billing_claims (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Claim Details
    claim_number NVARCHAR(100) UNIQUE,
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    payer_id UNIQUEIDENTIFIER REFERENCES insurance_payers(id),
    
    -- Service Information
    service_date_from DATE,
    service_date_to DATE,
    total_units INT,
    total_amount DECIMAL(10,2),
    
    -- Status Tracking
    claim_status NVARCHAR(50) DEFAULT 'draft' CHECK (claim_status IN ('draft', 'submitted', 'accepted', 'rejected', 'paid', 'appealed')),
    submission_date DATETIME2,
    payment_date DATETIME2,
    payment_amount DECIMAL(10,2),
    
    -- Medicaid Specific
    prior_authorization_number NVARCHAR(100),
    medicaid_id NVARCHAR(100),
    
    -- Rejection/Denial
    rejection_reason NVARCHAR(MAX),
    appeal_notes NVARCHAR(MAX),
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Claim Line Items
CREATE TABLE claim_line_items (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    claim_id UNIQUEIDENTIFIER REFERENCES billing_claims(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    
    -- Service Details
    service_date DATE,
    cpt_code NVARCHAR(10),
    modifier_codes NVARCHAR(50),
    units INT,
    rate_per_unit DECIMAL(10,2),
    line_total DECIMAL(10,2),
    
    -- Status
    is_billable BIT DEFAULT 1,
    denial_reason NVARCHAR(255),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- CONSENT & COMPLIANCE TABLES
-- =====================================================

-- FERPA Consents
CREATE TABLE ferpa_consents (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Consent Details
    consent_type NVARCHAR(100), -- 'general_release', 'evaluation_report', 'progress_sharing'
    consenting_party NVARCHAR(255),
    relationship_to_student NVARCHAR(100),
    
    -- What's Consented
    authorized_parties NVARCHAR(MAX), -- JSON array of authorized recipients
    information_types NVARCHAR(MAX), -- JSON array of allowed info types
    purpose NVARCHAR(500),
    
    -- Validity
    consent_date DATE,
    expiration_date DATE,
    
    -- Signature
    signature_method NVARCHAR(50), -- 'electronic', 'paper', 'verbal'
    signature_data NVARCHAR(MAX), -- Base64 encoded signature or file reference
    witness_name NVARCHAR(255),
    
    -- Revocation
    is_revoked BIT DEFAULT 0,
    revocation_date DATE,
    revocation_reason NVARCHAR(500),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Data Sharing Log (FERPA Compliance)
CREATE TABLE data_sharing_log (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- What was shared
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    shared_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- With whom
    recipient_name NVARCHAR(255),
    recipient_organization NVARCHAR(255),
    recipient_email NVARCHAR(255),
    
    -- What data
    data_types_shared NVARCHAR(MAX), -- JSON array
    specific_records NVARCHAR(MAX), -- JSON array of record IDs
    purpose NVARCHAR(500),
    
    -- Consent basis
    consent_id UNIQUEIDENTIFIER REFERENCES ferpa_consents(id),
    legal_basis NVARCHAR(255), -- 'parent_consent', 'eligible_student', 'education_official', 'emergency'
    
    -- Method
    sharing_method NVARCHAR(50), -- 'email', 'fax', 'portal', 'print'
    
    shared_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- TELETHERAPY & VIRTUAL SESSION TABLES
-- =====================================================

-- Virtual Session Details
CREATE TABLE virtual_sessions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    
    -- Platform Details
    platform NVARCHAR(50), -- 'zoom', 'teams', 'custom'
    meeting_id NVARCHAR(255),
    meeting_url NVARCHAR(500),
    
    -- Session Metrics
    actual_start_time DATETIME2,
    actual_end_time DATETIME2,
    connection_quality NVARCHAR(50), -- 'excellent', 'good', 'fair', 'poor'
    
    -- Participation
    student_joined_at DATETIME2,
    student_left_at DATETIME2,
    parent_present BIT DEFAULT 0,
    technical_issues NVARCHAR(MAX),
    
    -- Recording
    was_recorded BIT DEFAULT 0,
    recording_url NVARCHAR(500),
    recording_consent BIT DEFAULT 0,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Virtual Therapy Resources
CREATE TABLE virtual_resources (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Resource Details
    resource_name NVARCHAR(255),
    resource_type NVARCHAR(50), -- 'interactive_game', 'assessment', 'worksheet', 'video'
    resource_url NVARCHAR(500),
    
    -- Categorization
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    skill_areas NVARCHAR(MAX), -- JSON array
    age_range_min INT,
    age_range_max INT,
    
    -- Platform
    platform_compatible NVARCHAR(MAX), -- JSON array of compatible platforms
    requires_student_account BIT DEFAULT 0,
    
    -- Usage
    times_used INT DEFAULT 0,
    average_rating DECIMAL(3,2),
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- PARENT/GUARDIAN PORTAL TABLES
-- =====================================================

-- Parent Accounts
CREATE TABLE parent_accounts (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Account Info
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100),
    last_name NVARCHAR(100),
    
    -- Contact
    phone NVARCHAR(20),
    preferred_language NVARCHAR(50) DEFAULT 'en',
    
    -- Preferences
    notification_preferences NVARCHAR(MAX), -- JSON
    timezone NVARCHAR(50),
    
    -- Status
    is_verified BIT DEFAULT 0,
    verification_token NVARCHAR(255),
    last_login DATETIME2,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Parent-Student Relationships
CREATE TABLE parent_student_access (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    parent_id UNIQUEIDENTIFIER REFERENCES parent_accounts(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Relationship
    relationship NVARCHAR(50), -- 'mother', 'father', 'guardian', 'other'
    is_primary_contact BIT DEFAULT 0,
    
    -- Access Rights
    can_view_progress BIT DEFAULT 1,
    can_view_documents BIT DEFAULT 1,
    can_communicate_therapist BIT DEFAULT 1,
    can_update_info BIT DEFAULT 0,
    
    -- Legal
    has_educational_rights BIT DEFAULT 1,
    custody_notes NVARCHAR(MAX),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Parent Communications
CREATE TABLE parent_communications (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Participants
    parent_id UNIQUEIDENTIFIER REFERENCES parent_accounts(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Message Details
    subject NVARCHAR(255),
    message_body NVARCHAR(MAX),
    message_type NVARCHAR(50), -- 'progress_update', 'scheduling', 'homework', 'concern'
    
    -- Threading
    parent_message_id UNIQUEIDENTIFIER REFERENCES parent_communications(id),
    
    -- Status
    is_read BIT DEFAULT 0,
    read_at DATETIME2,
    requires_response BIT DEFAULT 0,
    
    -- Direction
    sent_by_parent BIT,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- CASELOAD MANAGEMENT TABLES
-- =====================================================

-- Therapist Caseloads
CREATE TABLE therapist_caseloads (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Capacity Settings
    max_caseload_size INT DEFAULT 50,
    current_caseload_size INT DEFAULT 0,
    
    -- Workload Metrics
    total_weekly_minutes INT DEFAULT 0,
    max_weekly_minutes INT DEFAULT 1800, -- 30 hours
    
    -- School Assignments
    primary_schools NVARCHAR(MAX), -- JSON array of school IDs
    coverage_schools NVARCHAR(MAX), -- JSON array for substitute coverage
    
    -- Scheduling Preferences
    available_days NVARCHAR(50), -- 'MTWRF' format
    blackout_dates NVARCHAR(MAX), -- JSON array of dates
    
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Caseload Analytics
CREATE TABLE caseload_analytics (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Time Period
    analytics_month DATE,
    
    -- Productivity Metrics
    total_sessions_scheduled INT,
    total_sessions_completed INT,
    total_cancellations INT,
    total_no_shows INT,
    
    -- Documentation Metrics
    documentation_compliance_rate DECIMAL(5,2), -- Percentage
    average_note_completion_time INT, -- Minutes
    notes_completed_same_day INT,
    
    -- Outcome Metrics
    goals_mastered INT,
    average_goal_progress DECIMAL(5,2), -- Percentage
    
    -- Billing Metrics
    billable_units INT,
    non_billable_units INT,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- INTEGRATION & SYNC TABLES
-- =====================================================

-- External System Mappings
CREATE TABLE integration_mappings (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- System Info
    external_system NVARCHAR(100), -- 'PowerSchool', 'Infinite Campus', 'Skyward'
    external_system_version NVARCHAR(50),
    
    -- Mapping Details
    local_entity_type NVARCHAR(50), -- 'student', 'school', 'user'
    local_entity_id UNIQUEIDENTIFIER,
    external_entity_id NVARCHAR(255),
    
    -- Sync Status
    last_sync_at DATETIME2,
    sync_status NVARCHAR(50),
    sync_errors NVARCHAR(MAX),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- API Access Logs
CREATE TABLE api_access_logs (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Access Details
    api_key_id NVARCHAR(255),
    endpoint NVARCHAR(500),
    method NVARCHAR(10),
    
    -- Request/Response
    request_body NVARCHAR(MAX),
    response_status INT,
    response_time_ms INT,
    
    -- Context
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- PREDICTIVE ANALYTICS TABLES
-- =====================================================

-- Goal Prediction Models
CREATE TABLE goal_predictions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    goal_id UNIQUEIDENTIFIER REFERENCES iep_goals(id),
    
    -- Predictions
    predicted_mastery_date DATE,
    confidence_score DECIMAL(5,2), -- 0-100%
    risk_level NVARCHAR(20), -- 'low', 'medium', 'high'
    
    -- Factors
    contributing_factors NVARCHAR(MAX), -- JSON of factors
    recommended_interventions NVARCHAR(MAX), -- JSON of suggestions
    
    -- Model Info
    model_version NVARCHAR(20),
    calculated_at DATETIME2 DEFAULT GETDATE()
);

-- Student Risk Indicators
CREATE TABLE student_risk_indicators (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Risk Categories
    attendance_risk NVARCHAR(20), -- 'low', 'medium', 'high'
    progress_risk NVARCHAR(20),
    engagement_risk NVARCHAR(20),
    
    -- Metrics
    missed_session_rate DECIMAL(5,2),
    goal_progress_rate DECIMAL(5,2),
    parent_engagement_score DECIMAL(5,2),
    
    -- Recommendations
    recommended_actions NVARCHAR(MAX), -- JSON array
    
    calculated_at DATETIME2 DEFAULT GETDATE()
);

GO

-- Create indexes for new tables
CREATE INDEX idx_insurance_medicaid ON insurance_payers(is_medicaid);
CREATE INDEX idx_student_insurance_active ON student_insurance(student_id, is_active);
CREATE INDEX idx_claims_status ON billing_claims(claim_status, submission_date);
CREATE INDEX idx_ferpa_student ON ferpa_consents(student_id, is_revoked);
CREATE INDEX idx_virtual_appointment ON virtual_sessions(appointment_id);
CREATE INDEX idx_parent_student ON parent_student_access(parent_id, student_id);
CREATE INDEX idx_communications_unread ON parent_communications(parent_id, is_read);
CREATE INDEX idx_caseload_therapist ON therapist_caseloads(therapist_id);
CREATE INDEX idx_predictions_goal ON goal_predictions(goal_id, predicted_mastery_date);

GO