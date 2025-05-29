-- =====================================================
-- THERAPYDOCS: MOBILE DOCUMENTATION + AI CONTENT GENERATION
-- MSSQL Version
-- =====================================================

USE TherapyDocs;
GO

-- =====================================================
-- CORE TABLES
-- =====================================================

-- TABLE 1: USERS (Therapists)
CREATE TABLE users (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Info
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) DEFAULT 'therapist' CHECK (role IN ('therapist', 'admin')),
    
    -- Professional Info
    license_number NVARCHAR(50),
    license_state NVARCHAR(2),
    service_type NVARCHAR(50) NOT NULL CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    
    -- Subscription
    subscription_tier NVARCHAR(50) DEFAULT 'basic',
    subscription_expires DATE,
    
    -- AI Content Usage
    monthly_content_generated INT DEFAULT 0,
    content_generation_limit INT DEFAULT 50,
    
    -- Settings
    timezone NVARCHAR(50) DEFAULT 'America/New_York',
    preferred_note_template NVARCHAR(50) DEFAULT 'soap',
    auto_save_notes BIT DEFAULT 1,
    
    -- Mobile Settings
    offline_sync_enabled BIT DEFAULT 1,
    push_notifications BIT DEFAULT 1,
    
    -- Status
    is_active BIT DEFAULT 1,
    last_login DATETIME2,
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 2: SCHOOLS
CREATE TABLE schools (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Info
    name NVARCHAR(255) NOT NULL,
    district NVARCHAR(255),
    address NVARCHAR(500),
    
    -- Contact
    phone NVARCHAR(20),
    principal_name NVARCHAR(255),
    
    -- Settings
    timezone NVARCHAR(50) DEFAULT 'America/New_York',
    typical_schedule NVARCHAR(MAX) DEFAULT '{"start": "08:00", "end": "15:30"}',
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 3: STUDENTS
CREATE TABLE students (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Demographics
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    date_of_birth DATE NOT NULL,
    grade_level NVARCHAR(10),
    
    -- School Info  
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    student_id NVARCHAR(50),
    case_manager NVARCHAR(255),
    
    -- IEP Basics
    has_iep BIT DEFAULT 0,
    iep_start_date DATE,
    iep_end_date DATE,
    primary_disability NVARCHAR(100),
    
    -- Quick Reference
    medical_alerts NVARCHAR(MAX),
    behavioral_notes NVARCHAR(MAX),
    communication_method NVARCHAR(100),
    
    -- Evaluation Tracking
    last_evaluation_date DATE,
    next_evaluation_due DATE,
    
    -- Parent Contact
    parent_name NVARCHAR(255),
    parent_phone NVARCHAR(20),
    parent_email NVARCHAR(255),
    
    -- Status
    is_active BIT DEFAULT 1,
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 4: IEP GOALS
CREATE TABLE iep_goals (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    
    -- Goal Basics
    goal_number NVARCHAR(20),
    goal_area NVARCHAR(100),
    goal_text NVARCHAR(MAX) NOT NULL,
    
    -- Simple Measurement
    measurement_method NVARCHAR(255),
    baseline NVARCHAR(MAX),
    
    -- Timeline
    target_date DATE,
    
    -- Current Status
    status NVARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'met', 'discontinued', 'not_met')),
    current_performance NVARCHAR(MAX),
    
    -- Service Connection
    responsible_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 5: SERVICES
CREATE TABLE services (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    
    -- Service Details
    service_type NVARCHAR(50) NOT NULL CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    minutes_per_week INT DEFAULT 30,
    frequency_per_week INT DEFAULT 1,
    
    -- IEP Mandate Info
    is_iep_service BIT DEFAULT 1,
    service_location NVARCHAR(100),
    
    -- Timeline
    start_date DATE NOT NULL,
    end_date DATE,
    
    -- Quick Notes
    service_notes NVARCHAR(MAX),
    
    status NVARCHAR(50) DEFAULT 'active',
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 6: APPOINTMENTS
CREATE TABLE appointments (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    service_id UNIQUEIDENTIFIER REFERENCES services(id),
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    
    -- Scheduling
    scheduled_date DATE NOT NULL,
    scheduled_start_time TIME NOT NULL,
    scheduled_end_time TIME NOT NULL,
    
    -- Actual Session
    actual_start_time TIME,
    actual_end_time TIME,
    session_duration_minutes INT,
    
    -- Location & Context
    location NVARCHAR(100),
    session_type NVARCHAR(50) DEFAULT 'individual',
    
    -- Documentation
    subjective NVARCHAR(MAX),
    objective NVARCHAR(MAX),
    assessment NVARCHAR(MAX),
    [plan] NVARCHAR(MAX),
    
    -- Goal Progress
    goals_addressed NVARCHAR(MAX) DEFAULT '[]',
    goal_progress_notes NVARCHAR(MAX),
    
    -- Session Details
    activities_completed NVARCHAR(MAX),
    materials_used NVARCHAR(MAX),
    student_behavior NVARCHAR(MAX),
    
    -- Generated Content Used
    generated_content_used NVARCHAR(MAX) DEFAULT '[]',
    content_effectiveness_rating INT,
    
    -- Next Session Planning
    next_session_focus NVARCHAR(MAX),
    homework_assigned NVARCHAR(MAX),
    
    -- Quick Ratings
    student_engagement INT,
    session_productivity INT,
    
    -- Status
    status NVARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled', 'completed', 'cancelled', 'no_show')),
    
    -- Mobile/Offline Support
    synced_to_server BIT DEFAULT 1,
    offline_created BIT DEFAULT 0,
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- AI CONTENT GENERATION TABLES
-- =====================================================

-- TABLE 7: CONTENT LIBRARY
CREATE TABLE content_library (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Content Details
    title NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    content_type NVARCHAR(50) NOT NULL CHECK (content_type IN ('coloring_sheet', 'maze', 'puzzle', 'game', 'worksheet', 'exercise_guide')),
    
    -- Therapy Targeting
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    skill_areas NVARCHAR(MAX) DEFAULT '[]',
    age_range_min INT,
    age_range_max INT,
    difficulty_level INT,
    
    -- Content Data
    file_url NVARCHAR(1000),
    file_type NVARCHAR(20),
    thumbnail_url NVARCHAR(1000),
    
    -- AI Generation Info
    is_ai_generated BIT DEFAULT 0,
    generation_prompt NVARCHAR(MAX),
    generated_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    generation_date DATETIME2,
    
    -- Usage Tracking
    download_count INT DEFAULT 0,
    average_rating DECIMAL(3,2),
    rating_count INT DEFAULT 0,
    
    -- Content Quality
    is_approved BIT DEFAULT 1,
    approved_by NVARCHAR(100),
    quality_score DECIMAL(3,2),
    
    -- Searchability
    tags NVARCHAR(MAX) DEFAULT '[]',
    search_keywords NVARCHAR(MAX),
    
    -- Status
    is_active BIT DEFAULT 1,
    is_featured BIT DEFAULT 0,
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 8: CONTENT GENERATION REQUESTS
CREATE TABLE content_requests (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Request Details
    requested_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    content_type NVARCHAR(50) NOT NULL CHECK (content_type IN ('coloring_sheet', 'maze', 'puzzle', 'game', 'worksheet', 'exercise_guide')),
    
    -- Generation Parameters
    skill_focus NVARCHAR(MAX) NOT NULL,
    target_age INT,
    difficulty_level INT,
    specific_requirements NVARCHAR(MAX),
    
    -- AI Prompt
    generation_prompt NVARCHAR(MAX) NOT NULL,
    
    -- Processing
    request_status NVARCHAR(50) DEFAULT 'pending',
    started_at DATETIME2,
    completed_at DATETIME2,
    processing_time_seconds INT,
    
    -- Results
    generated_content_id UNIQUEIDENTIFIER REFERENCES content_library(id),
    generation_error NVARCHAR(MAX),
    
    -- Usage Context
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- DIGITAL EVALUATION SYSTEM
-- =====================================================

-- TABLE 9: EVALUATION TEMPLATES
CREATE TABLE evaluation_templates (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Assessment Details
    assessment_name NVARCHAR(255) NOT NULL,
    assessment_type NVARCHAR(100),
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    
    -- Administration Info
    age_range_min INT,
    age_range_max INT,
    estimated_time_minutes INT,
    
    -- Assessment Structure
    sections NVARCHAR(MAX) NOT NULL,
    scoring_criteria NVARCHAR(MAX) NOT NULL,
    interpretation_guidelines NVARCHAR(MAX) DEFAULT '{}',
    
    -- Norms & Standards
    normative_data NVARCHAR(MAX) DEFAULT '{}',
    cut_off_scores NVARCHAR(MAX) DEFAULT '{}',
    
    -- Usage
    is_public BIT DEFAULT 1,
    usage_count INT DEFAULT 0,
    
    -- Version Control
    version NVARCHAR(20) DEFAULT '1.0',
    previous_version_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    created_by UNIQUEIDENTIFIER REFERENCES users(id)
);

-- TABLE 10: EVALUATIONS
CREATE TABLE evaluations (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id) ON DELETE CASCADE,
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    template_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    
    -- Evaluation Details
    evaluation_type NVARCHAR(100),
    evaluation_date DATE NOT NULL,
    
    -- Administration
    start_time DATETIME2,
    end_time DATETIME2,
    total_time_minutes INT,
    
    -- Environment
    testing_location NVARCHAR(255),
    environmental_factors NVARCHAR(MAX),
    
    -- Student Factors
    student_cooperation NVARCHAR(100),
    accommodations_used NVARCHAR(MAX) DEFAULT '[]',
    breaks_taken INT DEFAULT 0,
    
    -- Raw Data
    raw_responses NVARCHAR(MAX) DEFAULT '{}',
    section_scores NVARCHAR(MAX) DEFAULT '{}',
    
    -- Calculated Scores
    standard_scores NVARCHAR(MAX) DEFAULT '{}',
    percentile_ranks NVARCHAR(MAX) DEFAULT '{}',
    age_equivalents NVARCHAR(MAX) DEFAULT '{}',
    
    -- Interpretation
    overall_interpretation NVARCHAR(MAX),
    strengths NVARCHAR(MAX),
    areas_of_concern NVARCHAR(MAX),
    clinical_observations NVARCHAR(MAX),
    
    -- Recommendations
    therapy_recommendations NVARCHAR(MAX),
    educational_recommendations NVARCHAR(MAX),
    home_recommendations NVARCHAR(MAX),
    
    -- Validity
    results_valid BIT DEFAULT 1,
    validity_concerns NVARCHAR(MAX),
    
    -- Report Generation
    report_generated BIT DEFAULT 0,
    report_url NVARCHAR(1000),
    report_sent_to_team BIT DEFAULT 0,
    
    -- Status
    status NVARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled', 'in_progress', 'completed', 'cancelled')),
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 11: EVALUATION ITEMS
CREATE TABLE evaluation_items (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    evaluation_id UNIQUEIDENTIFIER REFERENCES evaluations(id) ON DELETE CASCADE,
    template_id UNIQUEIDENTIFIER REFERENCES evaluation_templates(id),
    
    -- Item Details
    section_name NVARCHAR(255),
    item_number NVARCHAR(20),
    item_description NVARCHAR(MAX),
    
    -- Response
    response_value NVARCHAR(500),
    response_type NVARCHAR(50),
    
    -- Scoring
    raw_score DECIMAL(8,2),
    weighted_score DECIMAL(8,2),
    max_possible_score DECIMAL(8,2),
    
    -- Context
    trials_attempted INT,
    trials_successful INT,
    assistance_level NVARCHAR(100),
    
    -- Observations
    item_notes NVARCHAR(MAX),
    behavioral_observations NVARCHAR(MAX),
    
    -- Timing
    item_start_time DATETIME2,
    item_end_time DATETIME2,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- ENHANCED TEMPLATES & PREFERENCES
-- =====================================================

-- TABLE 12: NOTE TEMPLATES
CREATE TABLE note_templates (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Template Info
    name NVARCHAR(255) NOT NULL,
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    template_type NVARCHAR(50),
    
    -- Template Content
    subjective_template NVARCHAR(MAX),
    objective_template NVARCHAR(MAX),
    assessment_template NVARCHAR(MAX),
    plan_template NVARCHAR(MAX),
    
    -- AI Integration
    suggest_activities BIT DEFAULT 0,
    suggest_materials BIT DEFAULT 0,
    auto_generate_content BIT DEFAULT 0,
    
    -- Common Content
    common_activities NVARCHAR(MAX) DEFAULT '[]',
    common_materials NVARCHAR(MAX) DEFAULT '[]',
    preferred_content_types NVARCHAR(MAX) DEFAULT '[]',
    
    -- Usage
    created_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    is_public BIT DEFAULT 0,
    usage_count INT DEFAULT 0,
    
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 13: CONTENT RATINGS
CREATE TABLE content_ratings (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Rating Details
    content_id UNIQUEIDENTIFIER REFERENCES content_library(id) ON DELETE CASCADE,
    rated_by_user_id UNIQUEIDENTIFIER REFERENCES users(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    
    -- Ratings (1-5 scale)
    overall_rating INT NOT NULL,
    age_appropriateness INT,
    skill_targeting INT,
    student_engagement INT,
    difficulty_level INT,
    
    -- Feedback
    written_feedback NVARCHAR(MAX),
    improvement_suggestions NVARCHAR(MAX),
    
    -- Context
    student_age INT,
    skill_areas_targeted NVARCHAR(MAX) DEFAULT '[]',
    session_outcome NVARCHAR(100),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 14: GOAL PROGRESS ENTRIES
CREATE TABLE goal_progress_entries (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Basic Info
    goal_id UNIQUEIDENTIFIER REFERENCES iep_goals(id) ON DELETE CASCADE,
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Progress Data
    date_recorded DATE NOT NULL,
    
    -- Simple Progress Tracking
    progress_rating INT,
    trials_attempted INT,
    trials_successful INT,
    independence_level NVARCHAR(50),
    
    -- Quick Notes
    progress_notes NVARCHAR(MAX),
    next_steps NVARCHAR(MAX),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- TABLE 15: AUDIT LOG
CREATE TABLE audit_log (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Who & When
    user_id UNIQUEIDENTIFIER REFERENCES users(id),
    action NVARCHAR(100) NOT NULL,
    timestamp DATETIME2 DEFAULT GETDATE(),
    
    -- What & Where
    entity_type NVARCHAR(50),
    entity_id UNIQUEIDENTIFIER,
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Details
    description NVARCHAR(MAX),
    ip_address NVARCHAR(45),
    
    -- Mobile Context
    device_type NVARCHAR(50),
    offline_action BIT DEFAULT 0
);

GO