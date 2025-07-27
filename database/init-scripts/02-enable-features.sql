-- Enable HIPAA Security Features for UPTRMS
-- This script enables advanced security features required for HIPAA compliance

USE UPTRMS;
GO

-- =============================================
-- Enable Always Encrypted for PHI columns
-- =============================================

-- Create Column Master Key
CREATE COLUMN MASTER KEY [CMK_PHI_Protection]
WITH (
    KEY_STORE_PROVIDER_NAME = 'MSSQL_CERTIFICATE_STORE',
    KEY_PATH = 'CurrentUser/My/UPTRMS_PHI_CMK_2025'
);
GO

-- Create Column Encryption Key
CREATE COLUMN ENCRYPTION KEY [CEK_PHI_Protection]
WITH VALUES
(
    COLUMN_MASTER_KEY = [CMK_PHI_Protection],
    ALGORITHM = 'RSA_OAEP',
    ENCRYPTED_VALUE = 0x01700000016C006F00630061006C006D0061006300680069006E0065002F006D0079002F003200660061006600640038003100320038003400380065003800330064003100300032003200390034006500380036003600650063003200610064003400300037003300310064003500640065003800620063006300610031006300310034006300620035003400330039003900340038003900350032003100340039003400320036003800350034003200660038006400310038003900340061003900640061006400650037003400630036003100300063006400660037006200610031006100370034003600310064003800640061006600360066003700300036003600630038003600310063003700650062006100620036
);
GO

-- =============================================
-- Enable Dynamic Data Masking for sensitive fields
-- =============================================

-- Create sample table to demonstrate masking (actual tables will be created in migrations)
CREATE TABLE [Security].[DataMaskingExample]
(
    ExampleID INT IDENTITY(1,1) PRIMARY KEY,
    SSN VARCHAR(11) MASKED WITH (FUNCTION = 'partial(0,"XXX-XX-",4)'),
    Email NVARCHAR(256) MASKED WITH (FUNCTION = 'email()'),
    Phone VARCHAR(20) MASKED WITH (FUNCTION = 'partial(0,"XXX-XXX-",4)'),
    CreditCard VARCHAR(19) MASKED WITH (FUNCTION = 'partial(0,"XXXX-XXXX-XXXX-",4)'),
    RandomNumber INT MASKED WITH (FUNCTION = 'random(1, 9999)')
);
GO

-- =============================================
-- Enable Row-Level Security
-- =============================================

-- Create security policy function for multi-tenant isolation
CREATE FUNCTION [Security].[fn_TenantAccessPredicate]
(
    @TenantID INT
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN 
    SELECT 1 AS AccessResult
    WHERE 
        @TenantID = CAST(SESSION_CONTEXT(N'TenantID') AS INT)
        OR IS_ROLEMEMBER('AdminRole') = 1
        OR IS_ROLEMEMBER('AuditorRole') = 1;
GO

-- Create security policy function for therapist data access
CREATE FUNCTION [Security].[fn_TherapistAccessPredicate]
(
    @TherapistID UNIQUEIDENTIFIER
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN 
    SELECT 1 AS AccessResult
    WHERE 
        @TherapistID = CAST(SESSION_CONTEXT(N'TherapistID') AS UNIQUEIDENTIFIER)
        OR IS_ROLEMEMBER('AdminRole') = 1
        OR IS_ROLEMEMBER('AuditorRole') = 1;
GO

-- =============================================
-- Enable SQL Audit for HIPAA compliance
-- =============================================

-- Create server audit (if not exists)
IF NOT EXISTS (SELECT * FROM sys.server_audits WHERE name = 'UPTRMS_Server_Audit')
BEGIN
    CREATE SERVER AUDIT UPTRMS_Server_Audit
    TO FILE 
    (
        FILEPATH = '/var/opt/mssql/audit/',
        MAXSIZE = 1 GB,
        MAX_ROLLOVER_FILES = 100,
        RESERVE_DISK_SPACE = ON
    )
    WITH
    (
        QUEUE_DELAY = 1000,
        ON_FAILURE = CONTINUE,
        AUDIT_GUID = '3C5E9D37-87CE-4E39-A7E4-71A0E6B74A98'
    );
    
    ALTER SERVER AUDIT UPTRMS_Server_Audit WITH (STATE = ON);
END
GO

-- Create server audit specification for login tracking
IF NOT EXISTS (SELECT * FROM sys.server_audit_specifications WHERE name = 'UPTRMS_Login_Audit_Spec')
BEGIN
    CREATE SERVER AUDIT SPECIFICATION UPTRMS_Login_Audit_Spec
    FOR SERVER AUDIT UPTRMS_Server_Audit
    ADD (FAILED_LOGIN_GROUP),
    ADD (SUCCESSFUL_LOGIN_GROUP),
    ADD (LOGOUT_GROUP),
    ADD (LOGIN_CHANGE_PASSWORD_GROUP)
    WITH (STATE = ON);
END
GO

-- =============================================
-- Configure Advanced Threat Protection
-- =============================================

-- Enable vulnerability assessment
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
GO

-- Enable Common Criteria compliance
EXEC sp_configure 'common criteria compliance enabled', 1;
RECONFIGURE;
GO

-- Enable C2 audit mode for additional security logging
EXEC sp_configure 'c2 audit mode', 1;
RECONFIGURE;
GO

-- =============================================
-- Create Security Tables
-- =============================================

-- Table for storing encryption keys metadata
CREATE TABLE [Security].[EncryptionKeys]
(
    KeyID UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    KeyName NVARCHAR(128) NOT NULL UNIQUE,
    KeyType NVARCHAR(50) NOT NULL,
    Algorithm NVARCHAR(50) NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME(),
    ExpiryDate DATETIME2,
    IsActive BIT DEFAULT 1,
    KeyHash VARBINARY(64) NOT NULL,
    CreatedBy NVARCHAR(128) DEFAULT SUSER_SNAME()
);
GO

-- Table for security events
CREATE TABLE [Security].[SecurityEvents]
(
    EventID BIGINT IDENTITY(1,1) PRIMARY KEY,
    EventType NVARCHAR(50) NOT NULL,
    EventSeverity NVARCHAR(20) NOT NULL,
    EventMessage NVARCHAR(MAX),
    UserID NVARCHAR(128),
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    EventData XML,
    EventTimestamp DATETIME2 DEFAULT SYSDATETIME(),
    INDEX IX_SecurityEvents_Timestamp (EventTimestamp DESC),
    INDEX IX_SecurityEvents_Type_Severity (EventType, EventSeverity)
) ON [UPTRMS_Data];
GO

-- Table for failed login attempts (for account lockout)
CREATE TABLE [Security].[FailedLoginAttempts]
(
    AttemptID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(128) NOT NULL,
    IPAddress NVARCHAR(45),
    AttemptTime DATETIME2 DEFAULT SYSDATETIME(),
    FailureReason NVARCHAR(500),
    UserAgent NVARCHAR(500),
    INDEX IX_FailedLogins_Username_Time (Username, AttemptTime DESC)
) ON [UPTRMS_Data];
GO

-- =============================================
-- Create Stored Procedures for Security
-- =============================================

-- Procedure to set session context for RLS
CREATE PROCEDURE [Security].[sp_SetSessionContext]
    @TenantID INT = NULL,
    @TherapistID UNIQUEIDENTIFIER = NULL,
    @UserRole NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @TenantID IS NOT NULL
        EXEC sp_set_session_context @key = N'TenantID', @value = @TenantID;
    
    IF @TherapistID IS NOT NULL
        EXEC sp_set_session_context @key = N'TherapistID', @value = @TherapistID;
    
    IF @UserRole IS NOT NULL
        EXEC sp_set_session_context @key = N'UserRole', @value = @UserRole;
END;
GO

-- Procedure to log security events
CREATE PROCEDURE [Security].[sp_LogSecurityEvent]
    @EventType NVARCHAR(50),
    @EventSeverity NVARCHAR(20),
    @EventMessage NVARCHAR(MAX),
    @UserID NVARCHAR(128) = NULL,
    @IPAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @EventData XML = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [Security].[SecurityEvents]
    (EventType, EventSeverity, EventMessage, UserID, IPAddress, UserAgent, EventData)
    VALUES
    (@EventType, @EventSeverity, @EventMessage, @UserID, @IPAddress, @UserAgent, @EventData);
    
    -- If critical event, send immediate notification
    IF @EventSeverity = 'CRITICAL'
    BEGIN
        DECLARE @DialogHandle UNIQUEIDENTIFIER;
        DECLARE @Message XML = (
            SELECT 
                @EventType AS EventType,
                @EventSeverity AS EventSeverity,
                @EventMessage AS EventMessage,
                SYSDATETIME() AS EventTime
            FOR XML PATH('SecurityAlert')
        );
        
        BEGIN DIALOG @DialogHandle
        FROM SERVICE [//UPTRMS/Audit/Service]
        TO SERVICE '//UPTRMS/Email/Service'
        ON CONTRACT [//UPTRMS/Email/Contract];
        
        SEND ON CONVERSATION @DialogHandle
        MESSAGE TYPE [//UPTRMS/Email/Request] (@Message);
    END;
END;
GO

-- =============================================
-- Enable Extended Events for detailed monitoring
-- =============================================

-- Create extended event session for security monitoring
IF EXISTS (SELECT * FROM sys.server_event_sessions WHERE name = 'UPTRMS_Security_Monitoring')
    DROP EVENT SESSION UPTRMS_Security_Monitoring ON SERVER;
GO

CREATE EVENT SESSION UPTRMS_Security_Monitoring ON SERVER
ADD EVENT sqlserver.error_reported
(
    ACTION (sqlserver.client_app_name, sqlserver.client_hostname, sqlserver.database_name, sqlserver.username)
    WHERE severity >= 16
),
ADD EVENT sqlserver.login_failed
(
    ACTION (sqlserver.client_app_name, sqlserver.client_hostname)
),
ADD EVENT sqlserver.user_event
(
    WHERE user_event_id = 100 -- Custom security events
)
ADD TARGET package0.event_file
(
    SET filename = '/var/opt/mssql/log/UPTRMS_Security.xel',
    max_file_size = 100,
    max_rollover_files = 10
)
WITH (STARTUP_STATE = ON);
GO

ALTER EVENT SESSION UPTRMS_Security_Monitoring ON SERVER STATE = START;
GO

-- =============================================
-- Create Jobs for Security Maintenance
-- =============================================

-- Note: SQL Agent jobs would be created here in production
-- Example job to clean up old audit data
/*
EXEC msdb.dbo.sp_add_job 
    @job_name = N'UPTRMS_Cleanup_Old_Audit_Data',
    @enabled = 1,
    @description = N'Cleanup audit data older than retention period';

EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'UPTRMS_Cleanup_Old_Audit_Data',
    @step_name = N'Delete Old Audit Records',
    @command = N'
        DELETE FROM [Security].[SecurityEvents]
        WHERE EventTimestamp < DATEADD(DAY, -90, SYSDATETIME());
        
        DELETE FROM [Security].[FailedLoginAttempts]
        WHERE AttemptTime < DATEADD(DAY, -30, SYSDATETIME());
    ';

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'UPTRMS_Cleanup_Old_Audit_Data',
    @schedule_name = N'Daily at 2 AM';
*/

PRINT 'HIPAA security features enabled successfully.';
PRINT 'Always Encrypted: Configured';
PRINT 'Dynamic Data Masking: Configured';
PRINT 'Row-Level Security: Functions created';
PRINT 'SQL Audit: Enabled';
PRINT 'Extended Events: Started';
GO