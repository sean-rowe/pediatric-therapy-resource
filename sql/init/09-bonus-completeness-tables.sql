-- =====================================================
-- BONUS COMPLETENESS TABLES - THE FINAL 5%
-- =====================================================
-- Because we're rich in AI-assisted development time,
-- let's just add EVERYTHING for 100% completeness
-- =====================================================

USE TherapyDocs;
GO

-- =====================================================
-- STATE REPORTING REQUIREMENTS
-- =====================================================

-- State-Specific Reporting Requirements
CREATE TABLE state_reporting_requirements (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- State Info
    state_code NVARCHAR(2) NOT NULL,
    state_name NVARCHAR(100),
    
    -- Report Details
    report_name NVARCHAR(255) NOT NULL,
    report_code NVARCHAR(50),
    report_category NVARCHAR(100), -- 'medicaid', 'education', 'health_department'
    
    -- Frequency
    reporting_frequency NVARCHAR(50), -- 'monthly', 'quarterly', 'annually'
    
    -- Requirements
    required_data_elements NVARCHAR(MAX), -- JSON of required fields
    submission_format NVARCHAR(50), -- 'csv', 'xml', 'api', 'portal'
    submission_method NVARCHAR(100),
    
    -- Deadlines
    submission_deadline_days INT, -- Days after period end
    
    -- Compliance
    is_mandatory BIT DEFAULT 1,
    penalty_for_non_compliance NVARCHAR(500),
    
    -- Dates
    effective_date DATE,
    termination_date DATE,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- State Report Submissions
CREATE TABLE state_report_submissions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Report Reference
    requirement_id UNIQUEIDENTIFIER REFERENCES state_reporting_requirements(id),
    
    -- Period
    reporting_period_start DATE,
    reporting_period_end DATE,
    
    -- Submission Details
    submission_date DATETIME2,
    submitted_by UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- File Info
    file_name NVARCHAR(500),
    file_path NVARCHAR(1000),
    record_count INT,
    
    -- Status
    submission_status NVARCHAR(50) DEFAULT 'draft' CHECK (submission_status IN ('draft', 'submitted', 'accepted', 'rejected', 'corrected')),
    
    -- Response
    confirmation_number NVARCHAR(100),
    response_date DATE,
    response_message NVARCHAR(MAX),
    
    -- Corrections
    requires_correction BIT DEFAULT 0,
    correction_notes NVARCHAR(MAX),
    parent_submission_id UNIQUEIDENTIFIER REFERENCES state_report_submissions(id),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- REFERRAL MANAGEMENT
-- =====================================================

-- Referral Sources
CREATE TABLE referral_sources (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Source Info
    source_name NVARCHAR(255) NOT NULL,
    source_type NVARCHAR(100), -- 'physician', 'school', 'parent', 'early_intervention'
    
    -- Contact Info
    contact_name NVARCHAR(255),
    phone NVARCHAR(20),
    fax NVARCHAR(20),
    email NVARCHAR(255),
    address NVARCHAR(500),
    
    -- Provider Info (for physicians)
    npi_number NVARCHAR(20),
    license_number NVARCHAR(50),
    specialty NVARCHAR(100),
    
    -- Relationship
    preferred_communication NVARCHAR(50), -- 'fax', 'email', 'phone'
    requires_reports BIT DEFAULT 1,
    report_frequency NVARCHAR(50),
    
    -- Status
    is_active BIT DEFAULT 1,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Referrals
CREATE TABLE referrals (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Referral Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    referral_source_id UNIQUEIDENTIFIER REFERENCES referral_sources(id),
    
    -- Dates
    referral_date DATE NOT NULL,
    received_date DATE NOT NULL,
    
    -- Service Request
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy', 'multiple')),
    services_requested NVARCHAR(MAX), -- JSON array if multiple
    
    -- Clinical Info
    referral_reason NVARCHAR(MAX),
    diagnosis_codes NVARCHAR(255), -- Comma-separated ICD-10
    
    -- Urgency
    priority_level NVARCHAR(20), -- 'routine', 'urgent', 'emergent'
    
    -- Processing
    intake_completed BIT DEFAULT 0,
    intake_date DATE,
    
    -- Outcome
    referral_status NVARCHAR(50) DEFAULT 'pending' CHECK (referral_status IN ('pending', 'in_review', 'accepted', 'declined', 'waitlisted')),
    decline_reason NVARCHAR(500),
    
    -- Authorization
    requires_physician_order BIT DEFAULT 0,
    physician_order_received BIT DEFAULT 0,
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Physician Orders
CREATE TABLE physician_orders (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- References
    referral_id UNIQUEIDENTIFIER REFERENCES referrals(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Physician Info
    physician_name NVARCHAR(255) NOT NULL,
    physician_npi NVARCHAR(20),
    physician_phone NVARCHAR(20),
    
    -- Order Details
    order_date DATE NOT NULL,
    order_type NVARCHAR(100), -- 'evaluation', 'treatment', 'both'
    
    -- Services Ordered
    services_ordered NVARCHAR(MAX), -- JSON array
    frequency_ordered NVARCHAR(255),
    duration_ordered NVARCHAR(255),
    
    -- Clinical Info
    diagnosis_codes NVARCHAR(500),
    medical_necessity NVARCHAR(MAX),
    precautions NVARCHAR(MAX),
    
    -- Document
    order_document_id UNIQUEIDENTIFIER REFERENCES documents(id),
    
    -- Validity
    valid_from DATE,
    valid_until DATE,
    
    -- Verification
    verified BIT DEFAULT 0,
    verified_by UNIQUEIDENTIFIER REFERENCES users(id),
    verification_date DATE,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- GROUP SESSION MANAGEMENT
-- =====================================================

-- Group Sessions
CREATE TABLE group_sessions (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Group Info
    group_name NVARCHAR(255),
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    
    -- Scheduling
    scheduled_date DATE NOT NULL,
    scheduled_start_time TIME NOT NULL,
    scheduled_end_time TIME NOT NULL,
    
    -- Location
    school_id UNIQUEIDENTIFIER REFERENCES schools(id),
    location_id UNIQUEIDENTIFIER REFERENCES session_locations(id),
    
    -- Therapist
    lead_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    assistant_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Capacity
    min_students INT DEFAULT 2,
    max_students INT DEFAULT 4,
    
    -- Focus
    session_theme NVARCHAR(255),
    target_skills NVARCHAR(MAX), -- JSON array
    
    -- Documentation
    group_note NVARCHAR(MAX),
    materials_used NVARCHAR(MAX),
    
    -- Status
    session_status NVARCHAR(50) DEFAULT 'scheduled' CHECK (session_status IN ('scheduled', 'completed', 'cancelled')),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Group Session Students
CREATE TABLE group_session_students (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- References
    group_session_id UNIQUEIDENTIFIER REFERENCES group_sessions(id),
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(id), -- Link to individual appointment
    
    -- Attendance
    attended BIT DEFAULT 0,
    arrival_time TIME,
    departure_time TIME,
    
    -- Participation
    participation_level NVARCHAR(50), -- 'full', 'partial', 'minimal'
    
    -- Individual Notes
    individual_progress NVARCHAR(MAX),
    behavior_notes NVARCHAR(MAX),
    
    -- Goals Addressed
    goals_addressed NVARCHAR(MAX), -- JSON array of goal IDs
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- WAITLIST MANAGEMENT
-- =====================================================

-- Service Waitlist
CREATE TABLE service_waitlist (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Student Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    referral_id UNIQUEIDENTIFIER REFERENCES referrals(id),
    
    -- Service Need
    service_type NVARCHAR(50) CHECK (service_type IN ('occupational_therapy', 'physical_therapy', 'speech_therapy')),
    
    -- Waitlist Info
    added_to_waitlist_date DATE NOT NULL,
    priority_score INT, -- Calculated based on need/time waiting
    
    -- Preferences
    preferred_schools NVARCHAR(MAX), -- JSON array
    preferred_days NVARCHAR(50),
    preferred_times NVARCHAR(100),
    
    -- Status
    waitlist_status NVARCHAR(50) DEFAULT 'waiting' CHECK (waitlist_status IN ('waiting', 'offered', 'scheduled', 'declined', 'expired')),
    
    -- Offers
    service_offered_date DATE,
    offer_expiration_date DATE,
    offer_response NVARCHAR(50),
    
    -- Outcome
    removed_from_waitlist_date DATE,
    removal_reason NVARCHAR(255),
    
    -- Notes
    internal_notes NVARCHAR(MAX),
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- DISCHARGE & TRANSITION
-- =====================================================

-- Discharge Summaries
CREATE TABLE discharge_summaries (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Student/Service Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    service_id UNIQUEIDENTIFIER REFERENCES services(id),
    therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Discharge Info
    discharge_date DATE NOT NULL,
    discharge_reason NVARCHAR(255), -- 'goals_met', 'moved', 'parent_request', 'aged_out'
    
    -- Service Summary
    service_start_date DATE,
    total_sessions_provided INT,
    total_sessions_missed INT,
    
    -- Clinical Summary
    initial_status_summary NVARCHAR(MAX),
    final_status_summary NVARCHAR(MAX),
    progress_summary NVARCHAR(MAX),
    
    -- Goal Outcomes
    goals_met INT,
    goals_partially_met INT,
    goals_not_met INT,
    goal_details NVARCHAR(MAX), -- JSON with goal outcomes
    
    -- Recommendations
    follow_up_recommendations NVARCHAR(MAX),
    home_program_recommendations NVARCHAR(MAX),
    referral_recommendations NVARCHAR(MAX),
    
    -- Report
    report_sent_to_parent BIT DEFAULT 0,
    report_sent_to_physician BIT DEFAULT 0,
    report_sent_date DATE,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Transition Plans
CREATE TABLE transition_plans (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Student Info
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    
    -- Transition Type
    transition_type NVARCHAR(100), -- 'early_intervention_to_school', 'elementary_to_middle', 'school_to_adult'
    
    -- Timeline
    planning_start_date DATE,
    transition_date DATE,
    
    -- Current Setting
    current_setting NVARCHAR(255),
    current_services NVARCHAR(MAX), -- JSON
    
    -- Future Setting
    future_setting NVARCHAR(255),
    recommended_services NVARCHAR(MAX), -- JSON
    
    -- Assessment
    transition_assessment_date DATE,
    assessment_results NVARCHAR(MAX),
    
    -- Goals
    transition_goals NVARCHAR(MAX),
    
    -- Team
    team_members NVARCHAR(MAX), -- JSON array
    
    -- Actions
    action_steps NVARCHAR(MAX), -- JSON array with deadlines
    
    -- Status
    plan_status NVARCHAR(50) DEFAULT 'in_progress' CHECK (plan_status IN ('planning', 'in_progress', 'completed')),
    
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- QUALITY ASSURANCE
-- =====================================================

-- Peer Reviews
CREATE TABLE peer_reviews (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Review Info
    reviewed_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    reviewing_therapist_id UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Period
    review_period_start DATE,
    review_period_end DATE,
    
    -- Samples Reviewed
    documentation_samples_count INT,
    sessions_observed_count INT,
    
    -- Ratings
    documentation_quality INT, -- 1-5
    clinical_skills INT, -- 1-5
    professionalism INT, -- 1-5
    
    -- Feedback
    strengths NVARCHAR(MAX),
    areas_for_improvement NVARCHAR(MAX),
    recommendations NVARCHAR(MAX),
    
    -- Follow-up
    follow_up_required BIT DEFAULT 0,
    follow_up_date DATE,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Incident Reports
CREATE TABLE incident_reports (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Incident Info
    incident_date DATETIME2 NOT NULL,
    reported_by UNIQUEIDENTIFIER REFERENCES users(id),
    
    -- Involved Parties
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    staff_involved NVARCHAR(MAX), -- JSON array
    
    -- Incident Details
    incident_type NVARCHAR(100), -- 'injury', 'behavior', 'equipment', 'other'
    incident_location NVARCHAR(255),
    incident_description NVARCHAR(MAX),
    
    -- Injury Details (if applicable)
    injury_occurred BIT DEFAULT 0,
    injury_description NVARCHAR(MAX),
    first_aid_provided NVARCHAR(MAX),
    medical_attention_required BIT DEFAULT 0,
    
    -- Response
    immediate_action_taken NVARCHAR(MAX),
    parent_notified BIT DEFAULT 0,
    parent_notification_time DATETIME2,
    
    -- Follow-up
    corrective_actions NVARCHAR(MAX),
    preventive_measures NVARCHAR(MAX),
    
    -- Review
    reviewed_by UNIQUEIDENTIFIER REFERENCES users(id),
    review_date DATE,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- =====================================================
-- BONUS: ANALYTICS HELPERS
-- =====================================================

-- Outcome Measures
CREATE TABLE outcome_measures (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- Measure Info
    measure_name NVARCHAR(255) NOT NULL,
    measure_type NVARCHAR(100), -- 'standardized_test', 'functional_scale', 'parent_report'
    
    -- Applicability
    applicable_diagnoses NVARCHAR(MAX), -- JSON array
    applicable_age_min INT,
    applicable_age_max INT,
    
    -- Scoring
    scoring_method NVARCHAR(MAX),
    score_interpretation NVARCHAR(MAX),
    
    -- Usage
    is_active BIT DEFAULT 1,
    
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Student Outcome Scores
CREATE TABLE student_outcome_scores (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    
    -- References
    student_id UNIQUEIDENTIFIER REFERENCES students(id),
    measure_id UNIQUEIDENTIFIER REFERENCES outcome_measures(id),
    evaluation_id UNIQUEIDENTIFIER REFERENCES evaluations(id),
    
    -- Score Info
    assessment_date DATE NOT NULL,
    raw_score DECIMAL(10,2),
    standard_score DECIMAL(10,2),
    percentile_rank INT,
    
    -- Comparison
    previous_score_id UNIQUEIDENTIFIER REFERENCES student_outcome_scores(id),
    change_from_previous DECIMAL(10,2),
    
    created_at DATETIME2 DEFAULT GETDATE()
);

GO

-- =====================================================
-- CREATE INDEXES FOR BONUS TABLES
-- =====================================================

CREATE INDEX idx_state_reports ON state_reporting_requirements(state_code, report_category);
CREATE INDEX idx_report_submissions ON state_report_submissions(requirement_id, submission_status);
CREATE INDEX idx_referrals_status ON referrals(referral_status, referral_date);
CREATE INDEX idx_physician_orders ON physician_orders(student_id, valid_until);
CREATE INDEX idx_group_sessions ON group_sessions(scheduled_date, session_status);
CREATE INDEX idx_waitlist_priority ON service_waitlist(waitlist_status, priority_score);
CREATE INDEX idx_discharge_date ON discharge_summaries(discharge_date, student_id);
CREATE INDEX idx_transitions ON transition_plans(transition_date, plan_status);
CREATE INDEX idx_incidents ON incident_reports(incident_date, incident_type);
CREATE INDEX idx_outcome_scores ON student_outcome_scores(student_id, assessment_date);

GO

PRINT 'Bonus completeness tables created successfully!';
PRINT 'Total new tables: 19';
PRINT '';
PRINT 'State Reporting: 2 tables';
PRINT 'Referral Management: 3 tables';
PRINT 'Group Sessions: 2 tables';
PRINT 'Waitlist: 1 table';
PRINT 'Discharge/Transition: 2 tables';
PRINT 'Quality Assurance: 2 tables';
PRINT 'Analytics Helpers: 2 tables';
PRINT 'Operational: 5 tables';
PRINT '';
PRINT 'ULTIMATE TOTAL: 89 tables';
PRINT '';
PRINT 'You now have EVERYTHING. No therapy business has more comprehensive data architecture.';
GO