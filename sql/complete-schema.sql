-- =====================================================
-- THERAPYDOCS COMPLETE SCHEMA - ALL 32 TABLES
-- =====================================================
-- Run this script to create the complete database schema
-- with all industry-standard features
-- =====================================================

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TherapyDocs')
BEGIN
    CREATE DATABASE TherapyDocs;
END
GO

USE TherapyDocs;
GO

-- =====================================================
-- CORE TABLES (15 TABLES)
-- =====================================================

-- TABLE 1: USERS (Therapists)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type in (N'U'))
CREATE TABLE users (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) DEFAULT 'therapist' CHECK (role IN ('therapist', 'admin')),
    license_number NVARCHAR(50),
    license_state NVARCHAR(2),
    service_type NVARCHAR(50) NOT NULL CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    subscription_tier NVARCHAR(50) DEFAULT 'basic',
    subscription_expires DATE,
    monthly_content_generated INT DEFAULT 0,
    content_generation_limit INT DEFAULT 50,
    timezone NVARCHAR(50) DEFAULT 'America/New_York',
    preferred_note_template NVARCHAR(50) DEFAULT 'soap',
    auto_save_notes BIT DEFAULT 1,
    offline_sync_enabled BIT DEFAULT 1,
    push_notifications BIT DEFAULT 1,
    is_active BIT DEFAULT 1,
    last_login DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 2: SCHOOLS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[schools]') AND type in (N'U'))
CREATE TABLE schools (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(255) NOT NULL,
    district NVARCHAR(255),
    address NVARCHAR(500),
    phone NVARCHAR(20),
    principal_name NVARCHAR(255),
    timezone NVARCHAR(50) DEFAULT 'America/New_York',
    typical_schedule NVARCHAR(MAX) DEFAULT '{"start": "08:00", "end": "15:30"}',
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 3: STUDENTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[students]') AND type in (N'U'))
CREATE TABLE students (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    grade_level NVARCHAR(10),
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    student_id NVARCHAR(50),
    case_manager NVARCHAR(255),
    has_iep BIT DEFAULT 0,
    iep_start_date DATE,
    iep_end_date DATE,
    primary_disability NVARCHAR(100),
    medical_alerts NVARCHAR(MAX),
    behavioral_notes NVARCHAR(MAX),
    communication_method NVARCHAR(100),
    last_evaluation_date DATE,
    next_evaluation_due DATE,
    parent_name NVARCHAR(255),
    parent_phone NVARCHAR(20),
    parent_email NVARCHAR(255),
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 4: IEP GOALS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[iep_goals]') AND type in (N'U'))
CREATE TABLE iep_goals (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    goal_number NVARCHAR(20),
    goal_area NVARCHAR(100),
    goal_text NVARCHAR(MAX) NOT NULL,
    measurement_method NVARCHAR(255),
    baseline NVARCHAR(MAX),
    target_date DATE,
    status NVARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'met', 'discontinued', 'not_met')),
    current_performance NVARCHAR(MAX),
    responsible_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 5: SERVICES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[services]') AND type in (N'U'))
CREATE TABLE services (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    service_type NVARCHAR(50) NOT NULL CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    minutes_per_week INT DEFAULT 30,
    frequency_per_week INT DEFAULT 1,
    is_iep_service BIT DEFAULT 1,
    service_location NVARCHAR(100),
    start_date DATE NOT NULL,
    end_date DATE,
    service_notes NVARCHAR(MAX),
    status NVARCHAR(50) DEFAULT 'active',
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 6: APPOINTMENTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[appointments]') AND type in (N'U'))
CREATE TABLE appointments (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    service_id UNIQUEIDENTIFIER REFERENCES services(id),
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    scheduled_date DATE NOT NULL,
    scheduled_start_time TIME NOT NULL,
    scheduled_end_time TIME NOT NULL,
    actual_start_time TIME,
    actual_end_time TIME,
    session_duration_minutes INT,
    location NVARCHAR(100),
    session_type NVARCHAR(50) DEFAULT 'individual',
    subjective NVARCHAR(MAX),
    objective NVARCHAR(MAX),
    assessment NVARCHAR(MAX),
    [plan] NVARCHAR(MAX),
    goals_addressed NVARCHAR(MAX) DEFAULT '[]',
    goal_progress_notes NVARCHAR(MAX),
    activities_completed NVARCHAR(MAX),
    materials_used NVARCHAR(MAX),
    student_behavior NVARCHAR(MAX),
    generated_content_used NVARCHAR(MAX) DEFAULT '[]',
    content_effectiveness_rating INT,
    next_session_focus NVARCHAR(MAX),
    homework_assigned NVARCHAR(MAX),
    student_engagement INT,
    session_productivity INT,
    status NVARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled', 'completed', 'cancelled', 'no_show')),
    synced_to_server BIT DEFAULT 1,
    offline_created BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 7: CONTENT LIBRARY
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[content_library]') AND type in (N'U'))
CREATE TABLE content_library (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    content_type NVARCHAR(50) NOT NULL CHECK (content_type IN ('coloring_sheet', 'maze', 'puzzle', 'game', 'worksheet', 'exercise_guide')),
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    skill_areas NVARCHAR(MAX) DEFAULT '[]',
    age_range_min INT,
    age_range_max INT,
    difficulty_level INT,
    file_url NVARCHAR(1000),
    file_type NVARCHAR(20),
    thumbnail_url NVARCHAR(1000),
    is_ai_generated BIT DEFAULT 0,
    generation_prompt NVARCHAR(MAX),
    generated_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    generation_date DATETIME2,
    download_count INT DEFAULT 0,
    average_rating DECIMAL(3,2),
    rating_count INT DEFAULT 0,
    is_approved BIT DEFAULT 1,
    approved_by NVARCHAR(100),
    quality_score DECIMAL(3,2),
    tags NVARCHAR(MAX) DEFAULT '[]',
    search_keywords NVARCHAR(MAX),
    is_active BIT DEFAULT 1,
    is_featured BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 8: CONTENT GENERATION REQUESTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[content_requests]') AND type in (N'U'))
CREATE TABLE content_requests (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    requested_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    content_type NVARCHAR(50) NOT NULL CHECK (content_type IN ('coloring_sheet', 'maze', 'puzzle', 'game', 'worksheet', 'exercise_guide')),
    skill_focus NVARCHAR(MAX) NOT NULL,
    target_age INT,
    difficulty_level INT,
    specific_requirements NVARCHAR(MAX),
    generation_prompt NVARCHAR(MAX) NOT NULL,
    request_status NVARCHAR(50) DEFAULT 'pending',
    started_at DATETIME2,
    completed_at DATETIME2,
    processing_time_seconds INT,
    generated_content_id UNIQUEIDENTIFIER REFERENCES content_library(id),
    generation_error NVARCHAR(MAX),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 9: EVALUATION TEMPLATES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[evaluation_templates]') AND type in (N'U'))
CREATE TABLE evaluation_templates (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    assessment_name NVARCHAR(255) NOT NULL,
    assessment_type NVARCHAR(100),
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    age_range_min INT,
    age_range_max INT,
    estimated_time_minutes INT,
    sections NVARCHAR(MAX) NOT NULL,
    scoring_criteria NVARCHAR(MAX) NOT NULL,
    interpretation_guidelines NVARCHAR(MAX) DEFAULT '{}',
    normative_data NVARCHAR(MAX) DEFAULT '{}',
    cut_off_scores NVARCHAR(MAX) DEFAULT '{}',
    is_public BIT DEFAULT 1,
    usage_count INT DEFAULT 0,
    version NVARCHAR(20) DEFAULT '1.0',
    previous_version_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    created_by UNIQUEIDENTIFIER REFERENCES users(id)
);

-- TABLE 10: EVALUATIONS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[evaluations]') AND type in (N'U'))
CREATE TABLE evaluations (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    template_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    evaluation_type NVARCHAR(100),
    evaluation_date DATE NOT NULL,
    start_time DATETIME2,
    end_time DATETIME2,
    total_time_minutes INT,
    testing_location NVARCHAR(255),
    environmental_factors NVARCHAR(MAX),
    student_cooperation NVARCHAR(100),
    accommodations_used NVARCHAR(MAX) DEFAULT '[]',
    breaks_taken INT DEFAULT 0,
    raw_responses NVARCHAR(MAX) DEFAULT '{}',
    section_scores NVARCHAR(MAX) DEFAULT '{}',
    standard_scores NVARCHAR(MAX) DEFAULT '{}',
    percentile_ranks NVARCHAR(MAX) DEFAULT '{}',
    age_equivalents NVARCHAR(MAX) DEFAULT '{}',
    overall_interpretation NVARCHAR(MAX),
    strengths NVARCHAR(MAX),
    areas_of_concern NVARCHAR(MAX),
    clinical_observations NVARCHAR(MAX),
    therapy_recommendations NVARCHAR(MAX),
    educational_recommendations NVARCHAR(MAX),
    home_recommendations NVARCHAR(MAX),
    results_valid BIT DEFAULT 1,
    validity_concerns NVARCHAR(MAX),
    report_generated BIT DEFAULT 0,
    report_url NVARCHAR(1000),
    report_sent_to_team BIT DEFAULT 0,
    status NVARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled', 'in_progress', 'completed', 'cancelled')),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 11: EVALUATION ITEMS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[evaluation_items]') AND type in (N'U'))
CREATE TABLE evaluation_items (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    evaluation_id UNIQUEIDENTIFIER REFERENCES evaluations(id) ON DELETE CASCADE,
    template_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    section_name NVARCHAR(255),
    item_number NVARCHAR(20),
    item_description NVARCHAR(MAX),
    response_value NVARCHAR(500),
    response_type NVARCHAR(50),
    raw_score DECIMAL(8,2),
    weighted_score DECIMAL(8,2),
    max_possible_score DECIMAL(8,2),
    trials_attempted INT,
    trials_successful INT,
    assistance_level NVARCHAR(100),
    item_notes NVARCHAR(MAX),
    behavioral_observations NVARCHAR(MAX),
    item_start_time DATETIME2,
    item_end_time DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 12: NOTE TEMPLATES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[note_templates]') AND type in (N'U'))
CREATE TABLE note_templates (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(255) NOT NULL,
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    template_type NVARCHAR(50),
    subjective_template NVARCHAR(MAX),
    objective_template NVARCHAR(MAX),
    assessment_template NVARCHAR(MAX),
    plan_template NVARCHAR(MAX),
    suggest_activities BIT DEFAULT 0,
    suggest_materials BIT DEFAULT 0,
    auto_generate_content BIT DEFAULT 0,
    common_activities NVARCHAR(MAX) DEFAULT '[]',
    common_materials NVARCHAR(MAX) DEFAULT '[]',
    preferred_content_types NVARCHAR(MAX) DEFAULT '[]',
    created_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    is_public BIT DEFAULT 0,
    usage_count INT DEFAULT 0,
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 13: CONTENT RATINGS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[content_ratings]') AND type in (N'U'))
CREATE TABLE content_ratings (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    content_id UNIQUEIDENTIFIER REFERENCES content_library(id) ON DELETE CASCADE,
    rated_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    overall_rating INT NOT NULL,
    age_appropriateness INT,
    skill_targeting INT,
    student_engagement INT,
    difficulty_level INT,
    written_feedback NVARCHAR(MAX),
    improvement_suggestions NVARCHAR(MAX),
    student_age INT,
    skill_areas_targeted NVARCHAR(MAX) DEFAULT '[]',
    session_outcome NVARCHAR(100),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 14: GOAL PROGRESS ENTRIES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[goal_progress_entries]') AND type in (N'U'))
CREATE TABLE goal_progress_entries (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    goal_id UNIQUEIDENTIFIER REFERENCES iep_goals(id) ON DELETE CASCADE,
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    date_recorded DATE NOT NULL,
    progress_rating INT,
    trials_attempted INT,
    trials_successful INT,
    independence_level NVARCHAR(50),
    progress_notes NVARCHAR(MAX),
    next_steps NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 15: AUDIT LOG
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[audit_log]') AND type in (N'U'))
CREATE TABLE audit_log (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER REFERENCES users(id),
    action NVARCHAR(100) NOT NULL,
    timestamp DATETIME2 DEFAULT GETDATE(),
    entity_type NVARCHAR(50),
    entity_id UNIQUEIDENTIFIER,
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    description NVARCHAR(MAX),
    ip_address NVARCHAR(45),
    device_type NVARCHAR(50),
    offline_action BIT DEFAULT 0
);

-- =====================================================
-- BILLING & INSURANCE TABLES (4 NEW TABLES)
-- =====================================================

-- TABLE 16: INSURANCE PAYERS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insurance_payers]') AND type in (N'U'))
CREATE TABLE insurance_payers (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    payer_name NVARCHAR(255) NOT NULL,
    payer_type NVARCHAR(50) CHECK (payer_type IN ('medicaid', 'private', 'self_pay', 'district')),
    payer_id NVARCHAR(100),
    billing_address NVARCHAR(500),
    billing_phone NVARCHAR(20),
    billing_email NVARCHAR(255),
    is_medicaid BIT DEFAULT 0,
    medicaid_provider_number NVARCHAR(100),
    state_specific_requirements NVARCHAR(MAX),
    default_rate_per_unit DECIMAL(10,2),
    billing_unit_minutes INT DEFAULT 15,
    requires_authorization BIT DEFAULT 0,
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 17: STUDENT INSURANCE
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[student_insurance]') AND type in (N'U'))
CREATE TABLE student_insurance (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    payer_id UNIQUEIDENTIFIER REFERENCES insurance_payers(id),
    policy_number NVARCHAR(100),
    group_number NVARCHAR(100),
    subscriber_name NVARCHAR(255),
    subscriber_relationship NVARCHAR(50),
    effective_date DATE,
    termination_date DATE,
    authorization_number NVARCHAR(100),
    authorized_visits INT,
    visits_used INT DEFAULT 0,
    coverage_priority INT DEFAULT 1,
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 18: BILLING CLAIMS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[billing_claims]') AND type in (N'U'))
CREATE TABLE billing_claims (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    claim_number NVARCHAR(100) UNIQUE,
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    payer_id UNIQUEIDENTIFIER REFERENCES insurance_payers(id),
    service_date_from DATE,
    service_date_to DATE,
    total_units INT,
    total_amount DECIMAL(10,2),
    claim_status NVARCHAR(50) DEFAULT 'draft' CHECK (claim_status IN ('draft', 'submitted', 'accepted', 'rejected', 'paid', 'appealed')),
    submission_date DATETIME2,
    payment_date DATETIME2,
    payment_amount DECIMAL(10,2),
    prior_authorization_number NVARCHAR(100),
    medicaid_id NVARCHAR(100),
    rejection_reason NVARCHAR(MAX),
    appeal_notes NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 19: CLAIM LINE ITEMS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[claim_line_items]') AND type in (N'U'))
CREATE TABLE claim_line_items (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    claim_id UNIQUEIDENTIFIER REFERENCES billing_claims(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    service_date DATE,
    cpt_code NVARCHAR(10),
    modifier_codes NVARCHAR(50),
    units INT,
    rate_per_unit DECIMAL(10,2),
    line_total DECIMAL(10,2),
    is_billable BIT DEFAULT 1,
    denial_reason NVARCHAR(255),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- CONSENT & COMPLIANCE TABLES (2 NEW TABLES)
-- =====================================================

-- TABLE 20: FERPA CONSENTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ferpa_consents]') AND type in (N'U'))
CREATE TABLE ferpa_consents (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    consent_type NVARCHAR(100),
    consenting_party NVARCHAR(255),
    relationship_to_student NVARCHAR(100),
    authorized_parties NVARCHAR(MAX),
    information_types NVARCHAR(MAX),
    purpose NVARCHAR(500),
    consent_date DATE,
    expiration_date DATE,
    signature_method NVARCHAR(50),
    signature_data NVARCHAR(MAX),
    witness_name NVARCHAR(255),
    is_revoked BIT DEFAULT 0,
    revocation_date DATE,
    revocation_reason NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 21: DATA SHARING LOG
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[data_sharing_log]') AND type in (N'U'))
CREATE TABLE data_sharing_log (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    shared_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    recipient_name NVARCHAR(255),
    recipient_organization NVARCHAR(255),
    recipient_email NVARCHAR(255),
    data_types_shared NVARCHAR(MAX),
    specific_records NVARCHAR(MAX),
    purpose NVARCHAR(500),
    consent_id UNIQUEIDENTIFIER REFERENCES ferpa_consents(id),
    legal_basis NVARCHAR(255),
    sharing_method NVARCHAR(50),
    shared_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- TELETHERAPY & VIRTUAL SESSION TABLES (2 NEW TABLES)
-- =====================================================

-- TABLE 22: VIRTUAL SESSIONS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[virtual_sessions]') AND type in (N'U'))
CREATE TABLE virtual_sessions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    platform NVARCHAR(50),
    meeting_id NVARCHAR(255),
    meeting_url NVARCHAR(500),
    actual_start_time DATETIME2,
    actual_end_time DATETIME2,
    connection_quality NVARCHAR(50),
    student_joined_at DATETIME2,
    student_left_at DATETIME2,
    parent_present BIT DEFAULT 0,
    technical_issues NVARCHAR(MAX),
    was_recorded BIT DEFAULT 0,
    recording_url NVARCHAR(500),
    recording_consent BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 23: VIRTUAL RESOURCES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[virtual_resources]') AND type in (N'U'))
CREATE TABLE virtual_resources (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    resource_name NVARCHAR(255),
    resource_type NVARCHAR(50),
    resource_url NVARCHAR(500),
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    skill_areas NVARCHAR(MAX),
    age_range_min INT,
    age_range_max INT,
    platform_compatible NVARCHAR(MAX),
    requires_student_account BIT DEFAULT 0,
    times_used INT DEFAULT 0,
    average_rating DECIMAL(3,2),
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- PARENT/GUARDIAN PORTAL TABLES (3 NEW TABLES)
-- =====================================================

-- TABLE 24: PARENT ACCOUNTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[parent_accounts]') AND type in (N'U'))
CREATE TABLE parent_accounts (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100),
    last_name NVARCHAR(100),
    phone NVARCHAR(20),
    preferred_language NVARCHAR(50) DEFAULT 'en',
    notification_preferences NVARCHAR(MAX),
    timezone NVARCHAR(50),
    is_verified BIT DEFAULT 0,
    verification_token NVARCHAR(255),
    last_login DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 25: PARENT-STUDENT ACCESS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[parent_student_access]') AND type in (N'U'))
CREATE TABLE parent_student_access (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    parent_id UNIQUEIDENTIFIER REFERENCES parent_accounts(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    relationship NVARCHAR(50),
    is_primary_contact BIT DEFAULT 0,
    can_view_progress BIT DEFAULT 1,
    can_view_documents BIT DEFAULT 1,
    can_communicate_therapist BIT DEFAULT 1,
    can_update_info BIT DEFAULT 0,
    has_educational_rights BIT DEFAULT 1,
    custody_notes NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 26: PARENT COMMUNICATIONS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[parent_communications]') AND type in (N'U'))
CREATE TABLE parent_communications (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    parent_id UNIQUEIDENTIFIER REFERENCES parent_accounts(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    subject NVARCHAR(255),
    message_body NVARCHAR(MAX),
    message_type NVARCHAR(50),
    parent_message_id UNIQUEIDENTIFIER REFERENCES parent_communications(id),
    is_read BIT DEFAULT 0,
    read_at DATETIME2,
    requires_response BIT DEFAULT 0,
    sent_by_parent BIT,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- CASELOAD MANAGEMENT TABLES (2 NEW TABLES)
-- =====================================================

-- TABLE 27: THERAPIST CASELOADS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[therapist_caseloads]') AND type in (N'U'))
CREATE TABLE therapist_caseloads (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    max_caseload_size INT DEFAULT 50,
    current_caseload_size INT DEFAULT 0,
    total_weekly_minutes INT DEFAULT 0,
    max_weekly_minutes INT DEFAULT 1800,
    primary_schools NVARCHAR(MAX),
    coverage_schools NVARCHAR(MAX),
    available_days NVARCHAR(50),
    blackout_dates NVARCHAR(MAX),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 28: CASELOAD ANALYTICS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[caseload_analytics]') AND type in (N'U'))
CREATE TABLE caseload_analytics (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    analytics_month DATE,
    total_sessions_scheduled INT,
    total_sessions_completed INT,
    total_cancellations INT,
    total_no_shows INT,
    documentation_compliance_rate DECIMAL(5,2),
    average_note_completion_time INT,
    notes_completed_same_day INT,
    goals_mastered INT,
    average_goal_progress DECIMAL(5,2),
    billable_units INT,
    non_billable_units INT,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- INTEGRATION & SYNC TABLES (2 NEW TABLES)
-- =====================================================

-- TABLE 29: INTEGRATION MAPPINGS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[integration_mappings]') AND type in (N'U'))
CREATE TABLE integration_mappings (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    external_system NVARCHAR(100),
    external_system_version NVARCHAR(50),
    local_entity_type NVARCHAR(50),
    local_entity_id UNIQUEIDENTIFIER,
    external_entity_id NVARCHAR(255),
    last_sync_at DATETIME2,
    sync_status NVARCHAR(50),
    sync_errors NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 30: API ACCESS LOGS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_access_logs]') AND type in (N'U'))
CREATE TABLE api_access_logs (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    api_key_id NVARCHAR(255),
    endpoint NVARCHAR(500),
    method NVARCHAR(10),
    request_body NVARCHAR(MAX),
    response_status INT,
    response_time_ms INT,
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- PREDICTIVE ANALYTICS TABLES (2 NEW TABLES)
-- =====================================================

-- TABLE 31: GOAL PREDICTIONS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[goal_predictions]') AND type in (N'U'))
CREATE TABLE goal_predictions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    goal_id UNIQUEIDENTIFIER REFERENCES iep_goals(id),
    predicted_mastery_date DATE,
    confidence_score DECIMAL(5,2),
    risk_level NVARCHAR(20),
    contributing_factors NVARCHAR(MAX),
    recommended_interventions NVARCHAR(MAX),
    model_version NVARCHAR(20),
    calculated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 32: STUDENT RISK INDICATORS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[student_risk_indicators]') AND type in (N'U'))
CREATE TABLE student_risk_indicators (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    attendance_risk NVARCHAR(20),
    progress_risk NVARCHAR(20),
    engagement_risk NVARCHAR(20),
    missed_session_rate DECIMAL(5,2),
    goal_progress_rate DECIMAL(5,2),
    parent_engagement_score DECIMAL(5,2),
    recommended_actions NVARCHAR(MAX),
    calculated_at DATETIME2 DEFAULT GETDATE()
);

GO

-- =====================================================
-- CREATE ALL INDEXES
-- =====================================================

-- Core table indexes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_service_type ON users(service_type);
CREATE INDEX idx_students_school ON students(school_id);
CREATE INDEX idx_students_name ON students(last_name, first_name);
CREATE INDEX idx_students_active ON students(is_active);
CREATE INDEX idx_services_therapist ON services(therapist_id, status);
CREATE INDEX idx_services_student ON services(student_id, status);
CREATE INDEX idx_appointments_therapist_date ON appointments(therapist_id, scheduled_date);
CREATE INDEX idx_appointments_student ON appointments(student_id);
CREATE INDEX idx_appointments_status ON appointments(status);
CREATE INDEX idx_appointments_sync ON appointments(synced_to_server);
CREATE INDEX idx_goals_student ON iep_goals(student_id, status);
CREATE INDEX idx_goals_therapist ON iep_goals(responsible_therapist_id);
CREATE INDEX idx_goal_progress_goal ON goal_progress_entries(goal_id, date_recorded);
CREATE INDEX idx_content_library_type ON content_library(content_type, service_type);
CREATE INDEX idx_content_library_age ON content_library(age_range_min, age_range_max);
CREATE INDEX idx_content_requests_user ON content_requests(requested_by_user_id, request_status);
CREATE INDEX idx_content_requests_date ON content_requests(created_at);
CREATE INDEX idx_evaluations_student ON evaluations(student_id, evaluation_date);
CREATE INDEX idx_evaluations_therapist ON evaluations(therapist_id, status);
CREATE INDEX idx_evaluations_template ON evaluations(template_id);
CREATE INDEX idx_evaluation_items_eval ON evaluation_items(evaluation_id);
CREATE INDEX idx_content_ratings_content ON content_ratings(content_id);
CREATE INDEX idx_content_ratings_user ON content_ratings(rated_by_user_id);
CREATE INDEX idx_audit_user_date ON audit_log(user_id, timestamp);
CREATE INDEX idx_audit_student ON audit_log(student_id, timestamp);

-- New table indexes
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

PRINT 'TherapyDocs complete schema created successfully!';
PRINT 'Total tables created: 32';
PRINT '';
PRINT 'Core Features: 15 tables';
PRINT 'Billing & Insurance: 4 tables';
PRINT 'FERPA Compliance: 2 tables';
PRINT 'Teletherapy: 2 tables';
PRINT 'Parent Portal: 3 tables';
PRINT 'Caseload Management: 2 tables';
PRINT 'Integration: 2 tables';
PRINT 'Predictive Analytics: 2 tables';
GO