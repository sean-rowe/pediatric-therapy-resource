-- Migration: 006_fix_select_star.sql
-- Purpose: Replace SELECT * with explicit column lists in stored procedures
-- Date: 2024-01-15
-- Security: Improves performance and prevents unintended data exposure

-- Drop and recreate sp_user_get_by_email with explicit columns
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_user_get_by_email]') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_user_get_by_email;
GO

CREATE PROCEDURE sp_user_get_by_email
    @email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        id,
        email,
        password_hash,
        first_name,
        last_name,
        phone,
        role,
        license_number,
        license_state,
        service_type,
        subscription_tier,
        subscription_expires,
        monthly_content_generated,
        content_generation_limit,
        timezone,
        preferred_note_template,
        auto_save_notes,
        offline_sync_enabled,
        push_notifications,
        is_active,
        status,
        email_verified,
        last_login,
        account_locked_until,
        failed_login_count,
        last_failed_login,
        password_last_changed,
        password_change_required,
        created_at,
        updated_at
    FROM users 
    WHERE email = @email;
END;
GO

-- Drop and recreate sp_user_get_by_id with explicit columns
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_user_get_by_id]') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_user_get_by_id;
GO

CREATE PROCEDURE sp_user_get_by_id
    @user_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        id,
        email,
        password_hash,
        first_name,
        last_name,
        phone,
        role,
        license_number,
        license_state,
        service_type,
        subscription_tier,
        subscription_expires,
        monthly_content_generated,
        content_generation_limit,
        timezone,
        preferred_note_template,
        auto_save_notes,
        offline_sync_enabled,
        push_notifications,
        is_active,
        status,
        email_verified,
        last_login,
        account_locked_until,
        failed_login_count,
        last_failed_login,
        password_last_changed,
        password_change_required,
        created_at,
        updated_at
    FROM users 
    WHERE id = @user_id;
END;
GO

-- Drop and recreate sp_email_verification_token_get with explicit columns
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_email_verification_token_get]') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_email_verification_token_get;
GO

CREATE PROCEDURE sp_email_verification_token_get
    @token NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        id,
        user_id,
        token,
        expires_at,
        used_at,
        created_at
    FROM email_verification_tokens 
    WHERE token = @token;
END;
GO

-- Create views for commonly accessed user data (without sensitive fields)
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_user_profile')
    DROP VIEW vw_user_profile;
GO

CREATE VIEW vw_user_profile AS
SELECT 
    id,
    email,
    first_name,
    last_name,
    phone,
    role,
    license_number,
    license_state,
    service_type,
    subscription_tier,
    subscription_expires,
    timezone,
    preferred_note_template,
    auto_save_notes,
    offline_sync_enabled,
    push_notifications,
    is_active,
    status,
    email_verified,
    last_login,
    created_at,
    updated_at
FROM users;
GO

-- Create view for user authentication data
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_user_auth')
    DROP VIEW vw_user_auth;
GO

CREATE VIEW vw_user_auth AS
SELECT 
    id,
    email,
    password_hash,
    status,
    email_verified,
    account_locked_until,
    failed_login_count,
    last_failed_login,
    password_last_changed,
    password_change_required
FROM users;
GO

-- Add comments to stored procedures for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves complete user record by email address. Used for authentication and profile management.', 
    @level0type = N'SCHEMA',
    @level0name = N'dbo', 
    @level1type = N'PROCEDURE',
    @level1name = N'sp_user_get_by_email';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves complete user record by user ID. Used for authenticated user operations.', 
    @level0type = N'SCHEMA',
    @level0name = N'dbo', 
    @level1type = N'PROCEDURE',
    @level1name = N'sp_user_get_by_id';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves email verification token details. Used during email verification process.', 
    @level0type = N'SCHEMA',
    @level0name = N'dbo', 
    @level1type = N'PROCEDURE',
    @level1name = N'sp_email_verification_token_get';