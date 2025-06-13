-- Migration: 005_password_history.sql
-- Purpose: Add password history tracking to prevent password reuse
-- Date: 2024-01-15

-- Table to track password history
CREATE TABLE password_history (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id),
    INDEX IX_password_history_user_created (user_id, created_at DESC)
);

-- Configuration table for password policy
CREATE TABLE password_policy (
    id INT IDENTITY(1,1) PRIMARY KEY,
    policy_name NVARCHAR(50) NOT NULL UNIQUE,
    policy_value INT NOT NULL,
    description NVARCHAR(255),
    updated_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Insert default password policy settings
INSERT INTO password_policy (policy_name, policy_value, description) VALUES
('password_history_count', 12, 'Number of previous passwords to check against'),
('password_min_age_days', 1, 'Minimum days before password can be changed'),
('password_max_age_days', 90, 'Maximum days before password must be changed'),
('password_expiry_warning_days', 14, 'Days before expiry to start warning user');

-- Add password change tracking to users table
ALTER TABLE users
ADD password_last_changed DATETIME2 NULL,
    password_change_required BIT NOT NULL DEFAULT 0;

-- Update existing users to set password_last_changed
UPDATE users 
SET password_last_changed = created_at
WHERE password_last_changed IS NULL;

-- Stored procedure to check password history
CREATE PROCEDURE sp_check_password_history
    @user_id INT,
    @password_hash NVARCHAR(255),
    @is_reused BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @history_count INT;
    
    -- Get password history count from policy
    SELECT @history_count = policy_value
    FROM password_policy
    WHERE policy_name = 'password_history_count';
    
    -- Check if password exists in history
    IF EXISTS (
        SELECT TOP 1 1
        FROM (
            SELECT TOP (@history_count) password_hash
            FROM password_history
            WHERE user_id = @user_id
            ORDER BY created_at DESC
        ) AS recent_passwords
        WHERE password_hash = @password_hash
    )
    BEGIN
        SET @is_reused = 1;
    END
    ELSE
    BEGIN
        SET @is_reused = 0;
    END
END;
GO

-- Stored procedure to add password to history
CREATE PROCEDURE sp_add_password_history
    @user_id INT,
    @password_hash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Add to password history
    INSERT INTO password_history (user_id, password_hash)
    VALUES (@user_id, @password_hash);
    
    -- Update user's password change date
    UPDATE users
    SET password_last_changed = GETUTCDATE(),
        password_change_required = 0
    WHERE id = @user_id;
    
    -- Clean up old password history (keep only what's needed per policy)
    DECLARE @history_count INT;
    SELECT @history_count = policy_value
    FROM password_policy
    WHERE policy_name = 'password_history_count';
    
    -- Delete old entries beyond the history count
    DELETE FROM password_history
    WHERE user_id = @user_id
    AND id NOT IN (
        SELECT TOP (@history_count) id
        FROM password_history
        WHERE user_id = @user_id
        ORDER BY created_at DESC
    );
END;
GO

-- Stored procedure to check if password change is required
CREATE PROCEDURE sp_check_password_change_required
    @user_id INT,
    @change_required BIT OUTPUT,
    @days_until_expiry INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @last_changed DATETIME2;
    DECLARE @max_age_days INT;
    DECLARE @current_required BIT;
    
    -- Get user's password info
    SELECT @last_changed = password_last_changed,
           @current_required = password_change_required
    FROM users
    WHERE id = @user_id;
    
    -- Get max age policy
    SELECT @max_age_days = policy_value
    FROM password_policy
    WHERE policy_name = 'password_max_age_days';
    
    -- Calculate days until expiry
    SET @days_until_expiry = @max_age_days - DATEDIFF(DAY, @last_changed, GETUTCDATE());
    
    -- Check if change is required
    IF @current_required = 1 OR @days_until_expiry <= 0
    BEGIN
        SET @change_required = 1;
    END
    ELSE
    BEGIN
        SET @change_required = 0;
    END
END;
GO

-- Add current passwords to history for existing users
INSERT INTO password_history (user_id, password_hash, created_at)
SELECT id, password_hash, ISNULL(password_last_changed, created_at)
FROM users
WHERE password_hash IS NOT NULL;