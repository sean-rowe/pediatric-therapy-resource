-- Missing tables to complete the 89 table requirement
-- These tables were identified from the feature files but not yet created

-- AI Content Generation Support
CREATE TABLE content_generation_prompts (
    prompt_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    prompt_name NVARCHAR(200) NOT NULL,
    prompt_template NVARCHAR(MAX) NOT NULL,
    content_type NVARCHAR(50) NOT NULL,
    category NVARCHAR(100),
    parameters NVARCHAR(MAX), -- JSON format
    is_active BIT DEFAULT 1,
    created_by UNIQUEIDENTIFIER REFERENCES users(user_id),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Session Materials and Resources
CREATE TABLE session_materials (
    material_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    appointment_id UNIQUEIDENTIFIER REFERENCES appointments(appointment_id),
    material_type NVARCHAR(50) NOT NULL,
    material_name NVARCHAR(200) NOT NULL,
    file_path NVARCHAR(500),
    notes NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Teletherapy Technical Logs
CREATE TABLE teletherapy_logs (
    log_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    session_id UNIQUEIDENTIFIER REFERENCES virtual_sessions(session_id),
    event_type NVARCHAR(50) NOT NULL,
    event_timestamp DATETIME2 NOT NULL,
    connection_quality NVARCHAR(20),
    technical_issues NVARCHAR(MAX),
    resolution NVARCHAR(MAX)
);

-- Parent Portal Activity
CREATE TABLE parent_portal_activity (
    activity_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    parent_id UNIQUEIDENTIFIER REFERENCES parent_accounts(parent_id),
    student_id UNIQUEIDENTIFIER REFERENCES students(student_id),
    activity_type NVARCHAR(50) NOT NULL,
    activity_details NVARCHAR(MAX),
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Compliance Training Records
CREATE TABLE compliance_training (
    training_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER REFERENCES users(user_id),
    training_type NVARCHAR(100) NOT NULL,
    completion_date DATE,
    expiration_date DATE,
    certificate_path NVARCHAR(500),
    score DECIMAL(5,2),
    passed BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Caseload Transfer History
CREATE TABLE caseload_transfers (
    transfer_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    student_id UNIQUEIDENTIFIER REFERENCES students(student_id),
    from_therapist_id UNIQUEIDENTIFIER REFERENCES users(user_id),
    to_therapist_id UNIQUEIDENTIFIER REFERENCES users(user_id),
    transfer_date DATE NOT NULL,
    transfer_reason NVARCHAR(500),
    notes NVARCHAR(MAX),
    created_by UNIQUEIDENTIFIER REFERENCES users(user_id),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Report Schedules
CREATE TABLE report_schedules (
    schedule_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    report_name NVARCHAR(200) NOT NULL,
    report_type NVARCHAR(50) NOT NULL,
    frequency NVARCHAR(20) NOT NULL, -- daily, weekly, monthly, quarterly
    parameters NVARCHAR(MAX), -- JSON format
    recipients NVARCHAR(MAX), -- JSON array of emails
    next_run_date DATETIME2,
    last_run_date DATETIME2,
    is_active BIT DEFAULT 1,
    created_by UNIQUEIDENTIFIER REFERENCES users(user_id),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- System Backup History
CREATE TABLE backup_history (
    backup_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    backup_type NVARCHAR(50) NOT NULL, -- full, differential, log
    backup_path NVARCHAR(500) NOT NULL,
    backup_size_mb BIGINT,
    duration_seconds INT,
    status NVARCHAR(20) NOT NULL,
    error_message NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- Data Archive Records
CREATE TABLE archive_records (
    archive_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    table_name NVARCHAR(128) NOT NULL,
    record_count INT NOT NULL,
    date_range_start DATE,
    date_range_end DATE,
    archive_path NVARCHAR(500),
    archive_size_mb BIGINT,
    retention_years INT NOT NULL,
    archived_by UNIQUEIDENTIFIER REFERENCES users(user_id),
    archived_at DATETIME2 DEFAULT GETDATE()
);