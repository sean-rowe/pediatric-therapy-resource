-- V001__initial_schema.sql
-- Initial database schema migration for UPTRMS
-- This creates the foundational tables needed for the application

-- Note: This migration assumes the database and schemas have been created by init scripts

USE UPTRMS;
GO

-- =============================================
-- Integration Schema Tables
-- =============================================

-- External system integrations
CREATE TABLE [Integration].[ExternalSystems]
(
    SystemID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    SystemName NVARCHAR(100) NOT NULL,
    SystemType NVARCHAR(50) NOT NULL, -- 'EHR', 'SSO', 'Payment', 'AI', 'School', 'Storage'
    BaseUrl NVARCHAR(500) NOT NULL,
    -- Authentication
    AuthType NVARCHAR(50) NOT NULL, -- 'OAuth2', 'APIKey', 'Basic', 'Certificate'
    ClientID VARBINARY(MAX) NULL, -- Encrypted
    ClientSecret VARBINARY(MAX) NULL, -- Encrypted
    APIKey VARBINARY(MAX) NULL, -- Encrypted
    CertificateThumbprint NVARCHAR(100) NULL,
    -- Configuration
    Configuration NVARCHAR(MAX) NULL, -- JSON with system-specific config
    RateLimitPerMinute INT NULL,
    TimeoutSeconds INT DEFAULT 30 NOT NULL,
    RetryAttempts INT DEFAULT 3 NOT NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    LastHealthCheckDate DATETIME2 NULL,
    LastHealthCheckStatus NVARCHAR(50) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_ExternalSystems PRIMARY KEY CLUSTERED (SystemID),
    CONSTRAINT UQ_ExternalSystems_Name UNIQUE (SystemName)
);
GO

-- Integration logs
CREATE TABLE [Integration].[IntegrationLogs]
(
    LogID BIGINT IDENTITY(1,1) NOT NULL,
    SystemID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NULL,
    UserID UNIQUEIDENTIFIER NULL,
    -- Request details
    RequestTime DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    RequestMethod NVARCHAR(10) NOT NULL,
    RequestUrl NVARCHAR(2000) NOT NULL,
    RequestHeaders NVARCHAR(MAX) NULL, -- Sanitized JSON
    RequestBody NVARCHAR(MAX) NULL, -- Sanitized JSON
    -- Response details
    ResponseTime DATETIME2 NULL,
    ResponseStatus INT NULL,
    ResponseHeaders NVARCHAR(MAX) NULL, -- Sanitized JSON
    ResponseBody NVARCHAR(MAX) NULL, -- Sanitized JSON
    -- Metrics
    DurationMs AS DATEDIFF(MILLISECOND, RequestTime, ResponseTime) PERSISTED,
    IsSuccess BIT DEFAULT 0 NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    RetryCount INT DEFAULT 0 NOT NULL,
    CONSTRAINT PK_IntegrationLogs PRIMARY KEY CLUSTERED (LogID),
    CONSTRAINT FK_IntegrationLogs_Systems FOREIGN KEY (SystemID) REFERENCES [Integration].[ExternalSystems](SystemID),
    INDEX IX_IntegrationLogs_RequestTime NONCLUSTERED (RequestTime DESC),
    INDEX IX_IntegrationLogs_SystemID NONCLUSTERED (SystemID)
) ON [UPTRMS_Archive];
GO

-- =============================================
-- Analytics Schema Tables
-- =============================================

-- Resource usage analytics
CREATE TABLE [Analytics].[ResourceUsage]
(
    UsageID BIGINT IDENTITY(1,1) NOT NULL,
    ResourceID UNIQUEIDENTIFIER NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NOT NULL,
    StudentID UNIQUEIDENTIFIER NULL,
    SessionID UNIQUEIDENTIFIER NULL,
    -- Usage details
    UsageType NVARCHAR(50) NOT NULL, -- 'View', 'Download', 'Print', 'Assign', 'Complete'
    UsageTime DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    DurationSeconds INT NULL,
    -- Context
    DeviceType NVARCHAR(50) NULL,
    Browser NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    IPAddress NVARCHAR(45) NULL,
    -- Performance metrics
    LoadTimeMs INT NULL,
    InteractionCount INT NULL,
    CompletionRate DECIMAL(5,2) NULL,
    CONSTRAINT PK_ResourceUsage PRIMARY KEY CLUSTERED (UsageID),
    CONSTRAINT FK_ResourceUsage_Resources FOREIGN KEY (ResourceID) REFERENCES [Therapy].[Resources](ResourceID),
    CONSTRAINT FK_ResourceUsage_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_ResourceUsage_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    INDEX IX_ResourceUsage_ResourceID NONCLUSTERED (ResourceID),
    INDEX IX_ResourceUsage_UsageTime NONCLUSTERED (UsageTime DESC)
) ON [UPTRMS_Archive];
GO

-- Student progress analytics
CREATE TABLE [Analytics].[StudentProgress]
(
    ProgressID BIGINT IDENTITY(1,1) NOT NULL,
    StudentID UNIQUEIDENTIFIER NOT NULL,
    GoalID UNIQUEIDENTIFIER NOT NULL,
    MeasurementDate DATE NOT NULL,
    -- Progress data
    BaselineValue DECIMAL(10,2) NULL,
    CurrentValue DECIMAL(10,2) NOT NULL,
    TargetValue DECIMAL(10,2) NOT NULL,
    ProgressPercentage AS ((CurrentValue - ISNULL(BaselineValue, 0)) / NULLIF(TargetValue - ISNULL(BaselineValue, 0), 0) * 100) PERSISTED,
    -- Additional metrics
    TrialsCompleted INT NULL,
    SuccessfulTrials INT NULL,
    AccuracyRate AS (CAST(SuccessfulTrials AS DECIMAL(10,2)) / NULLIF(TrialsCompleted, 0) * 100) PERSISTED,
    IndependenceLevel NVARCHAR(50) NULL, -- 'Dependent', 'MinimalAssist', 'ModerateAssist', 'Independent'
    -- Notes
    Notes NVARCHAR(MAX) NULL,
    -- Audit
    RecordedBy UNIQUEIDENTIFIER NOT NULL,
    RecordedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_StudentProgress PRIMARY KEY CLUSTERED (ProgressID),
    CONSTRAINT FK_StudentProgress_Students FOREIGN KEY (StudentID) REFERENCES [Therapy].[Students](StudentID),
    CONSTRAINT FK_StudentProgress_RecordedBy FOREIGN KEY (RecordedBy) REFERENCES [Core].[Users](UserID),
    INDEX IX_StudentProgress_StudentID NONCLUSTERED (StudentID),
    INDEX IX_StudentProgress_MeasurementDate NONCLUSTERED (MeasurementDate DESC)
);
GO

-- =============================================
-- Security Schema Additional Tables
-- =============================================

-- Password history for HIPAA compliance
CREATE TABLE [Security].[PasswordHistory]
(
    HistoryID BIGINT IDENTITY(1,1) NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_PasswordHistory PRIMARY KEY CLUSTERED (HistoryID),
    CONSTRAINT FK_PasswordHistory_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    INDEX IX_PasswordHistory_UserID_Date NONCLUSTERED (UserID, CreatedDate DESC)
);
GO

-- Account lockout tracking
CREATE TABLE [Security].[AccountLockouts]
(
    LockoutID BIGINT IDENTITY(1,1) NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    LockoutStart DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    LockoutEnd DATETIME2 NOT NULL,
    LockoutReason NVARCHAR(500) NOT NULL,
    UnlockedBy UNIQUEIDENTIFIER NULL,
    UnlockedDate DATETIME2 NULL,
    UnlockReason NVARCHAR(500) NULL,
    CONSTRAINT PK_AccountLockouts PRIMARY KEY CLUSTERED (LockoutID),
    CONSTRAINT FK_AccountLockouts_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_AccountLockouts_UnlockedBy FOREIGN KEY (UnlockedBy) REFERENCES [Core].[Users](UserID),
    INDEX IX_AccountLockouts_UserID NONCLUSTERED (UserID)
);
GO

-- Two-factor authentication
CREATE TABLE [Security].[TwoFactorAuth]
(
    TwoFactorID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    SecretKey VARBINARY(MAX) NOT NULL, -- Encrypted
    RecoveryCodes VARBINARY(MAX) NULL, -- Encrypted JSON array
    EnabledDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    LastUsedDate DATETIME2 NULL,
    CONSTRAINT PK_TwoFactorAuth PRIMARY KEY CLUSTERED (TwoFactorID),
    CONSTRAINT FK_TwoFactorAuth_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_TwoFactorAuth_UserID UNIQUE (UserID)
);
GO

-- =============================================
-- Email and Communication Tables
-- =============================================

-- Email templates
CREATE TABLE [Core].[EmailTemplates]
(
    TemplateID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TemplateName NVARCHAR(100) NOT NULL,
    Subject NVARCHAR(500) NOT NULL,
    HtmlBody NVARCHAR(MAX) NOT NULL,
    PlainTextBody NVARCHAR(MAX) NULL,
    Variables NVARCHAR(MAX) NULL, -- JSON array of variable names
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_EmailTemplates PRIMARY KEY CLUSTERED (TemplateID),
    CONSTRAINT UQ_EmailTemplates_Name UNIQUE (TemplateName)
);
GO

-- Email queue
CREATE TABLE [Core].[EmailQueue]
(
    EmailID BIGINT IDENTITY(1,1) NOT NULL,
    TenantID INT NULL,
    ToAddress NVARCHAR(256) NOT NULL,
    FromAddress NVARCHAR(256) NOT NULL,
    Subject NVARCHAR(500) NOT NULL,
    HtmlBody NVARCHAR(MAX) NOT NULL,
    PlainTextBody NVARCHAR(MAX) NULL,
    -- Status
    Status NVARCHAR(50) DEFAULT 'Pending' NOT NULL, -- 'Pending', 'Sending', 'Sent', 'Failed'
    Priority INT DEFAULT 5 NOT NULL, -- 1-10, 1 is highest
    ScheduledSendTime DATETIME2 NULL,
    ActualSendTime DATETIME2 NULL,
    -- Tracking
    AttemptCount INT DEFAULT 0 NOT NULL,
    LastAttemptTime DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    -- Metadata
    RelatedEntityType NVARCHAR(100) NULL,
    RelatedEntityID NVARCHAR(100) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_EmailQueue PRIMARY KEY CLUSTERED (EmailID),
    CONSTRAINT CK_EmailQueue_Status CHECK (Status IN ('Pending', 'Sending', 'Sent', 'Failed')),
    INDEX IX_EmailQueue_Status_Priority NONCLUSTERED (Status, Priority, ScheduledSendTime)
);
GO

-- =============================================
-- AI/ML Tables
-- =============================================

-- AI Generation requests
CREATE TABLE [Core].[AIGenerations]
(
    GenerationID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NOT NULL,
    -- Request details
    GenerationType NVARCHAR(50) NOT NULL, -- 'Worksheet', 'Activity', 'Assessment', 'VisualAid'
    Prompt NVARCHAR(MAX) NOT NULL,
    Parameters NVARCHAR(MAX) NULL, -- JSON with generation parameters
    -- AI details
    ModelUsed NVARCHAR(100) NOT NULL,
    ModelVersion NVARCHAR(50) NULL,
    ProviderAPI NVARCHAR(50) NOT NULL, -- 'OpenAI', 'StableDiffusion', 'Custom'
    -- Results
    Status NVARCHAR(50) DEFAULT 'Pending' NOT NULL, -- 'Pending', 'Processing', 'Completed', 'Failed', 'Rejected'
    ResultResourceID UNIQUEIDENTIFIER NULL,
    GenerationTimeMs INT NULL,
    TokensUsed INT NULL,
    CostEstimate DECIMAL(10,4) NULL,
    -- Quality control
    QualityScore DECIMAL(5,2) NULL,
    ClinicalReviewRequired BIT DEFAULT 1 NOT NULL,
    RejectionReason NVARCHAR(500) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CompletedDate DATETIME2 NULL,
    CONSTRAINT PK_AIGenerations PRIMARY KEY CLUSTERED (GenerationID),
    CONSTRAINT FK_AIGenerations_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_AIGenerations_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_AIGenerations_Resources FOREIGN KEY (ResultResourceID) REFERENCES [Therapy].[Resources](ResourceID),
    INDEX IX_AIGenerations_UserID NONCLUSTERED (UserID),
    INDEX IX_AIGenerations_Status NONCLUSTERED (Status)
);
GO

-- =============================================
-- Create stored procedures for common operations
-- =============================================

-- Procedure to record resource usage
CREATE PROCEDURE [Analytics].[sp_RecordResourceUsage]
    @ResourceID UNIQUEIDENTIFIER,
    @UserID UNIQUEIDENTIFIER,
    @TenantID INT,
    @UsageType NVARCHAR(50),
    @StudentID UNIQUEIDENTIFIER = NULL,
    @SessionID UNIQUEIDENTIFIER = NULL,
    @DeviceInfo NVARCHAR(MAX) = NULL -- JSON with device details
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Insert usage record
        INSERT INTO [Analytics].[ResourceUsage]
        (ResourceID, UserID, TenantID, StudentID, SessionID, UsageType, 
         DeviceType, Browser, OperatingSystem)
        SELECT 
            @ResourceID, @UserID, @TenantID, @StudentID, @SessionID, @UsageType,
            JSON_VALUE(@DeviceInfo, '$.deviceType'),
            JSON_VALUE(@DeviceInfo, '$.browser'),
            JSON_VALUE(@DeviceInfo, '$.os');
        
        -- Update resource statistics
        UPDATE [Therapy].[Resources]
        SET DownloadCount = DownloadCount + CASE WHEN @UsageType = 'Download' THEN 1 ELSE 0 END,
            ModifiedDate = SYSDATETIME()
        WHERE ResourceID = @ResourceID;
        
    END TRY
    BEGIN CATCH
        -- Log error but don't fail the operation
        INSERT INTO [Audit].[AuditLog]
        (EventType, Action, ErrorMessage, UserID, TenantID)
        VALUES
        ('ResourceUsageError', 'RecordUsage', ERROR_MESSAGE(), @UserID, @TenantID);
    END CATCH
END;
GO

-- =============================================
-- Create triggers for audit logging
-- =============================================

-- Trigger for User changes
CREATE TRIGGER [Core].[tr_Users_Audit]
ON [Core].[Users]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Action NVARCHAR(50);
    DECLARE @UserID UNIQUEIDENTIFIER;
    DECLARE @TenantID INT;
    
    -- Determine action
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @Action = 'Update';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @Action = 'Create';
    ELSE
        SET @Action = 'Delete';
    
    -- Get user context
    SET @UserID = CAST(SESSION_CONTEXT(N'UserID') AS UNIQUEIDENTIFIER);
    SET @TenantID = CAST(SESSION_CONTEXT(N'TenantID') AS INT);
    
    -- Log changes
    INSERT INTO [Audit].[AuditLog]
    (EventType, TableName, RecordID, Action, OldValues, NewValues, UserID, TenantID)
    SELECT 
        'UserChange',
        'Core.Users',
        COALESCE(i.UserID, d.UserID),
        @Action,
        (SELECT * FROM deleted d2 WHERE d2.UserID = d.UserID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT * FROM inserted i2 WHERE i2.UserID = i.UserID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        @UserID,
        @TenantID
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.UserID = d.UserID;
END;
GO

-- Enable trigger
ALTER TABLE [Core].[Users] ENABLE TRIGGER [tr_Users_Audit];
GO

PRINT 'Initial schema migration completed successfully.';
GO