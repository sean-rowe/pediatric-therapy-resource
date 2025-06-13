-- =====================================================
-- User Registration Tables Migration
-- =====================================================

USE TherapyDocs;
GO

-- Add missing columns to users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'phone')
ALTER TABLE users ADD phone NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'status')
ALTER TABLE users ADD status NVARCHAR(50) DEFAULT 'pending';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND name = 'email_verified')
ALTER TABLE users ADD email_verified BIT DEFAULT 0;
GO

-- Create professional_licenses table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[professional_licenses]') AND type in (N'U'))
CREATE TABLE professional_licenses (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    license_number NVARCHAR(100) NOT NULL,
    license_state NVARCHAR(2) NOT NULL,
    license_type NVARCHAR(50) NOT NULL,
    verification_status NVARCHAR(50) DEFAULT 'pending',
    verified_at DATETIME2,
    expires_at DATE,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_professional_licenses_users FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT UQ_license_number_state UNIQUE(license_number, license_state)
);
GO

-- Create email_verification_tokens table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[email_verification_tokens]') AND type in (N'U'))
CREATE TABLE email_verification_tokens (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    user_id UNIQUEIDENTIFIER NOT NULL,
    token NVARCHAR(255) UNIQUE NOT NULL,
    expires_at DATETIME2 NOT NULL,
    used_at DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_email_verification_tokens_users FOREIGN KEY (user_id) REFERENCES users(id)
);
GO

-- Create registration_audit_log table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[registration_audit_log]') AND type in (N'U'))
CREATE TABLE registration_audit_log (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    email NVARCHAR(255) NOT NULL,
    license_number NVARCHAR(100),
    license_state NVARCHAR(2),
    success BIT NOT NULL,
    failure_reason NVARCHAR(500),
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);
GO

-- Create indexes
CREATE INDEX IX_professional_licenses_user_id ON professional_licenses(user_id);
CREATE INDEX IX_email_verification_tokens_user_id ON email_verification_tokens(user_id);
CREATE INDEX IX_email_verification_tokens_token ON email_verification_tokens(token);
CREATE INDEX IX_registration_audit_log_email ON registration_audit_log(email);
CREATE INDEX IX_registration_audit_log_created_at ON registration_audit_log(created_at);
GO