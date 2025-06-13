-- Migration: 004_account_lockout.sql
-- Purpose: Add account lockout mechanism to prevent brute force attacks
-- Date: 2024-01-15

-- Table to track failed login attempts
CREATE TABLE failed_login_attempts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL,
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    attempted_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_failed_login_attempts_email_time (email, attempted_at DESC)
);

-- Add lockout fields to users table
ALTER TABLE users
ADD account_locked_until DATETIME2 NULL,
    failed_login_count INT NOT NULL DEFAULT 0,
    last_failed_login DATETIME2 NULL;

-- Stored procedure to record failed login attempt
CREATE PROCEDURE sp_record_failed_login
    @email NVARCHAR(255),
    @ip_address NVARCHAR(45) = NULL,
    @user_agent NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @user_id INT;
    DECLARE @failed_count INT;
    DECLARE @lockout_duration_minutes INT = 15; -- Initial lockout duration
    DECLARE @max_attempts INT = 5;
    
    -- Get user info
    SELECT @user_id = id, @failed_count = failed_login_count
    FROM users
    WHERE email = @email;
    
    IF @user_id IS NOT NULL
    BEGIN
        -- Record the failed attempt
        INSERT INTO failed_login_attempts (email, ip_address, user_agent)
        VALUES (@email, @ip_address, @user_agent);
        
        -- Update user's failed login count
        UPDATE users
        SET failed_login_count = failed_login_count + 1,
            last_failed_login = GETUTCDATE()
        WHERE id = @user_id;
        
        -- Check if account should be locked
        SET @failed_count = @failed_count + 1;
        
        IF @failed_count >= @max_attempts
        BEGIN
            -- Progressive lockout: 15 min, 30 min, 1 hour, 2 hours, 4 hours
            SET @lockout_duration_minutes = CASE 
                WHEN @failed_count = @max_attempts THEN 15
                WHEN @failed_count <= 7 THEN 30
                WHEN @failed_count <= 10 THEN 60
                WHEN @failed_count <= 15 THEN 120
                ELSE 240
            END;
            
            UPDATE users
            SET account_locked_until = DATEADD(MINUTE, @lockout_duration_minutes, GETUTCDATE())
            WHERE id = @user_id;
        END
    END
    ELSE
    BEGIN
        -- Still record attempt even if user doesn't exist (constant time)
        INSERT INTO failed_login_attempts (email, ip_address, user_agent)
        VALUES (@email, @ip_address, @user_agent);
    END
END;
GO

-- Stored procedure to check if account is locked
CREATE PROCEDURE sp_check_account_lockout
    @email NVARCHAR(255),
    @is_locked BIT OUTPUT,
    @locked_until DATETIME2 OUTPUT,
    @remaining_attempts INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @user_id INT;
    DECLARE @failed_count INT;
    DECLARE @max_attempts INT = 5;
    
    SELECT @user_id = id,
           @failed_count = failed_login_count,
           @locked_until = account_locked_until
    FROM users
    WHERE email = @email;
    
    IF @user_id IS NULL
    BEGIN
        -- User doesn't exist, but return consistent response
        SET @is_locked = 0;
        SET @locked_until = NULL;
        SET @remaining_attempts = @max_attempts;
        RETURN;
    END
    
    -- Check if currently locked
    IF @locked_until IS NOT NULL AND @locked_until > GETUTCDATE()
    BEGIN
        SET @is_locked = 1;
    END
    ELSE
    BEGIN
        SET @is_locked = 0;
        -- Clear lockout if expired
        IF @locked_until IS NOT NULL AND @locked_until <= GETUTCDATE()
        BEGIN
            UPDATE users
            SET account_locked_until = NULL
            WHERE id = @user_id;
            
            SET @locked_until = NULL;
        END
    END
    
    SET @remaining_attempts = CASE 
        WHEN @failed_count >= @max_attempts THEN 0
        ELSE @max_attempts - @failed_count
    END;
END;
GO

-- Stored procedure to clear failed login attempts on successful login
CREATE PROCEDURE sp_clear_failed_login_attempts
    @email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update user record
    UPDATE users
    SET failed_login_count = 0,
        last_failed_login = NULL,
        account_locked_until = NULL
    WHERE email = @email;
    
    -- Delete old failed attempts (keep for audit for 30 days)
    DELETE FROM failed_login_attempts
    WHERE email = @email
    AND attempted_at < DATEADD(DAY, -30, GETUTCDATE());
END;
GO

-- Index for cleanup job
CREATE INDEX IX_failed_login_attempts_cleanup
ON failed_login_attempts(attempted_at)
WHERE attempted_at < DATEADD(DAY, -30, GETUTCDATE());