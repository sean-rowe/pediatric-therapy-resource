-- Migration: 003_performance_indexes.sql
-- Purpose: Add database indexes for performance optimization
-- Date: 2024-01-15
-- Security: Addresses timing attack vulnerabilities through improved query performance

-- Index for email lookups (used in registration and login)
CREATE NONCLUSTERED INDEX IX_users_email 
ON users(email) 
INCLUDE (id, password_hash, email_verified, status);

-- Index for license lookups (used in registration validation)
CREATE NONCLUSTERED INDEX IX_users_license 
ON users(license_number, license_state) 
INCLUDE (id);

-- Index for professional licenses lookup
CREATE NONCLUSTERED INDEX IX_professional_licenses_lookup
ON professional_licenses(license_number, license_state, license_type)
INCLUDE (valid, expiry_date, disciplinary_actions);

-- Index for email verification tokens
CREATE NONCLUSTERED INDEX IX_email_verification_tokens_token
ON email_verification_tokens(token)
WHERE used_at IS NULL; -- Partial index for unused tokens only

-- Index for email verification tokens by user
CREATE NONCLUSTERED INDEX IX_email_verification_tokens_user
ON email_verification_tokens(user_id)
WHERE used_at IS NULL AND expires_at > GETUTCDATE(); -- Active tokens only

-- Index for registration audit log (for rate limiting checks)
CREATE NONCLUSTERED INDEX IX_registration_audit_log_ip
ON registration_audit_log(ip_address, created_at DESC)
INCLUDE (success);

-- Index for registration audit log by email
CREATE NONCLUSTERED INDEX IX_registration_audit_log_email
ON registration_audit_log(email, created_at DESC)
INCLUDE (success, failure_reason);

-- Add index hints to improve query plan stability
-- This helps prevent timing variations based on data distribution
EXEC sp_create_plan_guide 
    @name = N'PG_UserEmailLookup',
    @stmt = N'SELECT id, password_hash, email_verified, status FROM users WHERE email = @email',
    @type = N'SQL',
    @hints = N'OPTION (OPTIMIZE FOR UNKNOWN)';

EXEC sp_create_plan_guide 
    @name = N'PG_LicenseLookup',
    @stmt = N'SELECT id FROM users WHERE license_number = @license_number AND license_state = @license_state',
    @type = N'SQL',
    @hints = N'OPTION (OPTIMIZE FOR UNKNOWN)';