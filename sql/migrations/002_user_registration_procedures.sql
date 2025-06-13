-- =====================================================
-- User Registration Stored Procedures
-- =====================================================

USE TherapyDocs;
GO

-- Drop existing procedures if they exist
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_register')
    DROP PROCEDURE sp_user_register;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_get_by_email')
    DROP PROCEDURE sp_user_get_by_email;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_get_by_id')
    DROP PROCEDURE sp_user_get_by_id;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_email_exists')
    DROP PROCEDURE sp_user_email_exists;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_license_exists')
    DROP PROCEDURE sp_license_exists;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_update')
    DROP PROCEDURE sp_user_update;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_user_verify_email')
    DROP PROCEDURE sp_user_verify_email;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_email_verification_token_create')
    DROP PROCEDURE sp_email_verification_token_create;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_email_verification_token_get')
    DROP PROCEDURE sp_email_verification_token_get;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_email_verification_token_use')
    DROP PROCEDURE sp_email_verification_token_use;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_email_verification_token_has_valid')
    DROP PROCEDURE sp_email_verification_token_has_valid;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_registration_audit_log')
    DROP PROCEDURE sp_registration_audit_log;
GO

-- Create sp_user_register
CREATE PROCEDURE sp_user_register
    @email NVARCHAR(255),
    @password_hash NVARCHAR(255),
    @first_name NVARCHAR(100),
    @last_name NVARCHAR(100),
    @phone NVARCHAR(20) = NULL,
    @license_number NVARCHAR(50) = NULL,
    @license_state NVARCHAR(2) = NULL,
    @service_type NVARCHAR(50),
    @user_id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @user_id = NEWID();
        
        -- Insert user
        INSERT INTO users (
            id, email, password_hash, first_name, last_name, phone,
            license_number, license_state, service_type, status, email_verified
        ) VALUES (
            @user_id, @email, @password_hash, @first_name, @last_name, @phone,
            @license_number, @license_state, @service_type, 'pending', 0
        );
        
        -- Insert professional license record if provided
        IF @license_number IS NOT NULL AND @license_state IS NOT NULL
        BEGIN
            INSERT INTO professional_licenses (
                user_id, license_number, license_state, license_type, verification_status
            ) VALUES (
                @user_id, @license_number, @license_state, @service_type, 'verified'
            );
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Create sp_user_get_by_email
CREATE PROCEDURE sp_user_get_by_email
    @email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM users WHERE email = @email;
END;
GO

-- Create sp_user_get_by_id
CREATE PROCEDURE sp_user_get_by_id
    @user_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM users WHERE id = @user_id;
END;
GO

-- Create sp_user_email_exists
CREATE PROCEDURE sp_user_email_exists
    @email NVARCHAR(255),
    @exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM users WHERE email = @email)
        SET @exists = 1;
    ELSE
        SET @exists = 0;
END;
GO

-- Create sp_license_exists
CREATE PROCEDURE sp_license_exists
    @license_number NVARCHAR(100),
    @license_state NVARCHAR(2),
    @exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM users WHERE license_number = @license_number AND license_state = @license_state)
        SET @exists = 1;
    ELSE
        SET @exists = 0;
END;
GO

-- Create sp_user_update
CREATE PROCEDURE sp_user_update
    @user_id UNIQUEIDENTIFIER,
    @email NVARCHAR(255),
    @first_name NVARCHAR(100),
    @last_name NVARCHAR(100),
    @phone NVARCHAR(20) = NULL,
    @email_verified BIT,
    @status NVARCHAR(50),
    @is_active BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE users SET
        email = @email,
        first_name = @first_name,
        last_name = @last_name,
        phone = @phone,
        email_verified = @email_verified,
        status = @status,
        is_active = @is_active,
        updated_at = GETDATE()
    WHERE id = @user_id;
END;
GO

-- Create sp_user_verify_email
CREATE PROCEDURE sp_user_verify_email
    @user_id UNIQUEIDENTIFIER,
    @success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE users SET
            email_verified = 1,
            status = 'active',
            updated_at = GETDATE()
        WHERE id = @user_id AND email_verified = 0;
        
        IF @@ROWCOUNT > 0
            SET @success = 1;
        ELSE
            SET @success = 0;
    END TRY
    BEGIN CATCH
        SET @success = 0;
        THROW;
    END CATCH
END;
GO

-- Create sp_email_verification_token_create
CREATE PROCEDURE sp_email_verification_token_create
    @user_id UNIQUEIDENTIFIER,
    @token NVARCHAR(255),
    @expires_at DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO email_verification_tokens (user_id, token, expires_at)
    VALUES (@user_id, @token, @expires_at);
END;
GO

-- Create sp_email_verification_token_get
CREATE PROCEDURE sp_email_verification_token_get
    @token NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM email_verification_tokens WHERE token = @token;
END;
GO

-- Create sp_email_verification_token_use
CREATE PROCEDURE sp_email_verification_token_use
    @token NVARCHAR(255),
    @success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE email_verification_tokens SET
            used_at = GETDATE()
        WHERE token = @token 
            AND used_at IS NULL 
            AND expires_at > GETDATE();
        
        IF @@ROWCOUNT > 0
            SET @success = 1;
        ELSE
            SET @success = 0;
    END TRY
    BEGIN CATCH
        SET @success = 0;
        THROW;
    END CATCH
END;
GO

-- Create sp_email_verification_token_has_valid
CREATE PROCEDURE sp_email_verification_token_has_valid
    @user_id UNIQUEIDENTIFIER,
    @has_valid BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1 FROM email_verification_tokens 
        WHERE user_id = @user_id 
            AND used_at IS NULL 
            AND expires_at > GETDATE()
    )
        SET @has_valid = 1;
    ELSE
        SET @has_valid = 0;
END;
GO

-- Create sp_registration_audit_log
CREATE PROCEDURE sp_registration_audit_log
    @email NVARCHAR(255),
    @license_number NVARCHAR(100) = NULL,
    @license_state NVARCHAR(2) = NULL,
    @success BIT,
    @failure_reason NVARCHAR(500) = NULL,
    @ip_address NVARCHAR(45) = NULL,
    @user_agent NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO registration_audit_log (
        email, license_number, license_state, success, 
        failure_reason, ip_address, user_agent
    ) VALUES (
        @email, @license_number, @license_state, @success, 
        @failure_reason, @ip_address, @user_agent
    );
END;
GO