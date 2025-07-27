-- V002__security_tables.sql
-- Security-specific tables and configurations for HIPAA compliance

USE UPTRMS;
GO

-- =============================================
-- Email Verification Tokens
-- =============================================
CREATE TABLE [Security].[EmailVerificationTokens]
(
    TokenID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(128) NOT NULL,
    TokenHash AS HASHBYTES('SHA2_256', Token) PERSISTED,
    Purpose NVARCHAR(50) NOT NULL, -- 'EmailConfirmation', 'PasswordReset', 'TwoFactorSetup'
    ExpirationDate DATETIME2 NOT NULL,
    IsUsed BIT DEFAULT 0 NOT NULL,
    UsedDate DATETIME2 NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CreatedFromIP NVARCHAR(45) NULL,
    UsedFromIP NVARCHAR(45) NULL,
    CONSTRAINT PK_EmailVerificationTokens PRIMARY KEY CLUSTERED (TokenID),
    CONSTRAINT FK_EmailVerificationTokens_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_EmailVerificationTokens_Token UNIQUE (TokenHash),
    INDEX IX_EmailVerificationTokens_UserID NONCLUSTERED (UserID),
    INDEX IX_EmailVerificationTokens_ExpirationDate NONCLUSTERED (ExpirationDate)
);
GO

-- =============================================
-- Session Management
-- =============================================
CREATE TABLE [Security].[UserSessions]
(
    SessionID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NOT NULL,
    -- Session details
    SessionToken NVARCHAR(128) NOT NULL,
    SessionTokenHash AS HASHBYTES('SHA2_256', SessionToken) PERSISTED,
    RefreshToken NVARCHAR(128) NULL,
    RefreshTokenHash AS HASHBYTES('SHA2_256', RefreshToken) PERSISTED,
    -- Timestamps
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    LastActivityDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ExpirationDate DATETIME2 NOT NULL,
    RefreshTokenExpirationDate DATETIME2 NULL,
    -- Device/Location info
    DeviceID NVARCHAR(128) NULL,
    DeviceName NVARCHAR(200) NULL,
    DeviceType NVARCHAR(50) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    Browser NVARCHAR(100) NULL,
    IPAddress NVARCHAR(45) NOT NULL,
    Location NVARCHAR(200) NULL, -- City, State, Country from IP
    -- Security
    IsTrustedDevice BIT DEFAULT 0 NOT NULL,
    RequiresMFA BIT DEFAULT 1 NOT NULL,
    MFACompletedDate DATETIME2 NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    RevokedDate DATETIME2 NULL,
    RevokedReason NVARCHAR(500) NULL,
    CONSTRAINT PK_UserSessions PRIMARY KEY CLUSTERED (SessionID),
    CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_UserSessions_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT UQ_UserSessions_SessionToken UNIQUE (SessionTokenHash),
    CONSTRAINT UQ_UserSessions_RefreshToken UNIQUE (RefreshTokenHash),
    INDEX IX_UserSessions_UserID NONCLUSTERED (UserID),
    INDEX IX_UserSessions_ExpirationDate NONCLUSTERED (ExpirationDate)
);
GO

-- =============================================
-- API Keys for Integration
-- =============================================
CREATE TABLE [Security].[APIKeys]
(
    APIKeyID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NOT NULL,
    -- Key details
    KeyName NVARCHAR(100) NOT NULL,
    KeyValue NVARCHAR(128) NOT NULL,
    KeyHash AS HASHBYTES('SHA2_256', KeyValue) PERSISTED,
    KeyPrefix NVARCHAR(10) NOT NULL, -- First 10 chars for identification
    -- Permissions
    Scopes NVARCHAR(MAX) NOT NULL, -- JSON array of allowed scopes
    IPWhitelist NVARCHAR(MAX) NULL, -- JSON array of allowed IPs
    -- Limits
    RateLimitPerMinute INT DEFAULT 60 NOT NULL,
    RateLimitPerHour INT DEFAULT 1000 NOT NULL,
    -- Usage tracking
    LastUsedDate DATETIME2 NULL,
    LastUsedIP NVARCHAR(45) NULL,
    UsageCount BIGINT DEFAULT 0 NOT NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    ExpirationDate DATETIME2 NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    RevokedDate DATETIME2 NULL,
    RevokedBy UNIQUEIDENTIFIER NULL,
    RevokedReason NVARCHAR(500) NULL,
    CONSTRAINT PK_APIKeys PRIMARY KEY CLUSTERED (APIKeyID),
    CONSTRAINT FK_APIKeys_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_APIKeys_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_APIKeys_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_APIKeys_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_APIKeys_KeyHash UNIQUE (KeyHash),
    INDEX IX_APIKeys_UserID NONCLUSTERED (UserID),
    INDEX IX_APIKeys_KeyPrefix NONCLUSTERED (KeyPrefix)
);
GO

-- =============================================
-- OAuth/SSO Connections
-- =============================================
CREATE TABLE [Security].[ExternalAuthProviders]
(
    ProviderID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    ProviderName NVARCHAR(100) NOT NULL,
    ProviderType NVARCHAR(50) NOT NULL, -- 'OAuth2', 'SAML', 'OpenIDConnect'
    -- Configuration
    ClientID VARBINARY(MAX) NOT NULL, -- Encrypted
    ClientSecret VARBINARY(MAX) NULL, -- Encrypted
    AuthorizationEndpoint NVARCHAR(500) NOT NULL,
    TokenEndpoint NVARCHAR(500) NOT NULL,
    UserInfoEndpoint NVARCHAR(500) NULL,
    -- SAML specific
    MetadataUrl NVARCHAR(500) NULL,
    Certificate VARBINARY(MAX) NULL,
    -- Settings
    Scopes NVARCHAR(500) NULL,
    ClaimMappings NVARCHAR(MAX) NULL, -- JSON mapping external claims to internal
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_ExternalAuthProviders PRIMARY KEY CLUSTERED (ProviderID),
    CONSTRAINT UQ_ExternalAuthProviders_Name UNIQUE (ProviderName)
);
GO

-- User external auth connections
CREATE TABLE [Security].[UserExternalAuth]
(
    ExternalAuthID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    UserID UNIQUEIDENTIFIER NOT NULL,
    ProviderID UNIQUEIDENTIFIER NOT NULL,
    ExternalUserID NVARCHAR(256) NOT NULL,
    -- Profile data from provider
    ExternalEmail NVARCHAR(256) NULL,
    ExternalDisplayName NVARCHAR(200) NULL,
    ExternalProfileData NVARCHAR(MAX) NULL, -- JSON
    -- Tokens (encrypted)
    AccessToken VARBINARY(MAX) NULL,
    RefreshToken VARBINARY(MAX) NULL,
    TokenExpirationDate DATETIME2 NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    LastLoginDate DATETIME2 NULL,
    CONSTRAINT PK_UserExternalAuth PRIMARY KEY CLUSTERED (ExternalAuthID),
    CONSTRAINT FK_UserExternalAuth_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_UserExternalAuth_Providers FOREIGN KEY (ProviderID) REFERENCES [Security].[ExternalAuthProviders](ProviderID),
    CONSTRAINT UQ_UserExternalAuth_Provider_External UNIQUE (ProviderID, ExternalUserID),
    INDEX IX_UserExternalAuth_UserID NONCLUSTERED (UserID)
);
GO

-- =============================================
-- Permission System
-- =============================================
CREATE TABLE [Security].[Permissions]
(
    PermissionID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    PermissionName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    ResourceType NVARCHAR(100) NULL, -- 'Therapy.Resources', 'Core.Users', etc.
    AllowedActions NVARCHAR(MAX) NOT NULL, -- JSON array ['Create', 'Read', 'Update', 'Delete']
    IsSystemPermission BIT DEFAULT 0 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (PermissionID),
    CONSTRAINT UQ_Permissions_Name UNIQUE (PermissionName)
);
GO

-- Role permissions mapping
CREATE TABLE [Security].[RolePermissions]
(
    RoleID UNIQUEIDENTIFIER NOT NULL,
    PermissionID UNIQUEIDENTIFIER NOT NULL,
    GrantedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    GrantedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (RoleID, PermissionID),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleID) REFERENCES [Core].[Roles](RoleID),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionID) REFERENCES [Security].[Permissions](PermissionID),
    CONSTRAINT FK_RolePermissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES [Core].[Users](UserID)
);
GO

-- =============================================
-- Data Classification and Sensitivity
-- =============================================
CREATE TABLE [Security].[DataClassifications]
(
    ClassificationID INT IDENTITY(1,1) NOT NULL,
    TableSchema NVARCHAR(128) NOT NULL,
    TableName NVARCHAR(128) NOT NULL,
    ColumnName NVARCHAR(128) NOT NULL,
    DataType NVARCHAR(50) NOT NULL, -- 'PHI', 'PII', 'Financial', 'Confidential', 'Public'
    SensitivityLevel INT NOT NULL, -- 1-5, 5 being most sensitive
    EncryptionRequired BIT DEFAULT 0 NOT NULL,
    MaskingFunction NVARCHAR(100) NULL,
    RetentionDays INT NULL,
    ComplianceNotes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_DataClassifications PRIMARY KEY CLUSTERED (ClassificationID),
    CONSTRAINT UQ_DataClassifications_Column UNIQUE (TableSchema, TableName, ColumnName)
);
GO

-- =============================================
-- Breach Detection and Response
-- =============================================
CREATE TABLE [Security].[SecurityIncidents]
(
    IncidentID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    IncidentType NVARCHAR(100) NOT NULL, -- 'DataBreach', 'UnauthorizedAccess', 'PolicyViolation', etc.
    Severity NVARCHAR(20) NOT NULL, -- 'Critical', 'High', 'Medium', 'Low'
    -- Detection details
    DetectedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    DetectionMethod NVARCHAR(200) NOT NULL,
    AffectedEntities NVARCHAR(MAX) NULL, -- JSON array of affected users/data
    -- Investigation
    InvestigationStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
    InvestigatorID UNIQUEIDENTIFIER NULL,
    InvestigationNotes NVARCHAR(MAX) NULL,
    RootCause NVARCHAR(MAX) NULL,
    -- Response
    ContainmentDate DATETIME2 NULL,
    RemediationDate DATETIME2 NULL,
    NotificationsSent BIT DEFAULT 0 NOT NULL,
    NotificationDate DATETIME2 NULL,
    -- Compliance
    ReportedToAuthorities BIT DEFAULT 0 NOT NULL,
    ReportingDate DATETIME2 NULL,
    ComplianceNotes NVARCHAR(MAX) NULL,
    CONSTRAINT PK_SecurityIncidents PRIMARY KEY CLUSTERED (IncidentID),
    CONSTRAINT FK_SecurityIncidents_Investigator FOREIGN KEY (InvestigatorID) REFERENCES [Core].[Users](UserID),
    INDEX IX_SecurityIncidents_DetectedDate NONCLUSTERED (DetectedDate DESC),
    INDEX IX_SecurityIncidents_Severity NONCLUSTERED (Severity)
);
GO

-- =============================================
-- Stored Procedures for Security Operations
-- =============================================

-- Procedure to validate and record login attempt
CREATE PROCEDURE [Security].[sp_ValidateLogin]
    @Email NVARCHAR(256),
    @TenantID INT,
    @IPAddress NVARCHAR(45),
    @UserAgent NVARCHAR(500),
    @Success BIT OUTPUT,
    @UserID UNIQUEIDENTIFIER OUTPUT,
    @LockoutEnd DATETIME2 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @Success = 0;
    SET @UserID = NULL;
    SET @LockoutEnd = NULL;
    
    -- Find user
    SELECT @UserID = UserID, @LockoutEnd = LockoutEndDateUtc
    FROM [Core].[Users]
    WHERE EmailNormalized = UPPER(@Email) AND TenantID = @TenantID;
    
    IF @UserID IS NULL
    BEGIN
        -- Log failed attempt (user not found)
        INSERT INTO [Security].[FailedLoginAttempts]
        (Username, IPAddress, FailureReason, UserAgent)
        VALUES
        (@Email, @IPAddress, 'UserNotFound', @UserAgent);
        
        RETURN;
    END
    
    -- Check if account is locked
    IF @LockoutEnd IS NOT NULL AND @LockoutEnd > SYSDATETIME()
    BEGIN
        -- Still locked out
        INSERT INTO [Security].[FailedLoginAttempts]
        (Username, IPAddress, FailureReason, UserAgent)
        VALUES
        (@Email, @IPAddress, 'AccountLocked', @UserAgent);
        
        RETURN;
    END
    
    -- Account exists and not locked
    SET @Success = 1;
END;
GO

-- Procedure to record failed login and handle lockout
CREATE PROCEDURE [Security].[sp_RecordFailedLogin]
    @UserID UNIQUEIDENTIFIER,
    @IPAddress NVARCHAR(45),
    @UserAgent NVARCHAR(500),
    @FailureReason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @FailedCount INT;
    DECLARE @LockoutThreshold INT = 5;
    DECLARE @LockoutDuration INT = 30; -- minutes
    
    BEGIN TRANSACTION;
    
    -- Record failed attempt
    INSERT INTO [Security].[FailedLoginAttempts]
    (Username, IPAddress, FailureReason, UserAgent)
    SELECT Email, @IPAddress, @FailureReason, @UserAgent
    FROM [Core].[Users]
    WHERE UserID = @UserID;
    
    -- Update failed count
    UPDATE [Core].[Users]
    SET AccessFailedCount = AccessFailedCount + 1
    WHERE UserID = @UserID;
    
    -- Get updated count
    SELECT @FailedCount = AccessFailedCount
    FROM [Core].[Users]
    WHERE UserID = @UserID;
    
    -- Check if lockout threshold reached
    IF @FailedCount >= @LockoutThreshold
    BEGIN
        -- Lock the account
        UPDATE [Core].[Users]
        SET IsLockedOut = 1,
            LockoutEndDateUtc = DATEADD(MINUTE, @LockoutDuration, SYSDATETIME())
        WHERE UserID = @UserID;
        
        -- Record lockout
        INSERT INTO [Security].[AccountLockouts]
        (UserID, LockoutEnd, LockoutReason)
        VALUES
        (@UserID, DATEADD(MINUTE, @LockoutDuration, SYSDATETIME()), 
         'Exceeded failed login attempts: ' + CAST(@FailedCount AS NVARCHAR(10)));
        
        -- Log security event
        EXEC [Security].[sp_LogSecurityEvent]
            @EventType = 'AccountLocked',
            @EventSeverity = 'High',
            @EventMessage = 'Account locked due to failed login attempts',
            @UserID = @UserID,
            @IPAddress = @IPAddress,
            @UserAgent = @UserAgent;
    END
    
    COMMIT TRANSACTION;
END;
GO

-- =============================================
-- Insert default security permissions
-- =============================================

-- Core permissions
INSERT INTO [Security].[Permissions] (PermissionName, Category, Description, ResourceType, AllowedActions, IsSystemPermission)
VALUES
    ('Users.View', 'User Management', 'View user profiles', 'Core.Users', '["Read"]', 1),
    ('Users.Edit', 'User Management', 'Edit user profiles', 'Core.Users', '["Update"]', 1),
    ('Users.Create', 'User Management', 'Create new users', 'Core.Users', '["Create"]', 1),
    ('Users.Delete', 'User Management', 'Delete users', 'Core.Users', '["Delete"]', 1),
    ('Resources.View', 'Resources', 'View therapy resources', 'Therapy.Resources', '["Read"]', 1),
    ('Resources.Download', 'Resources', 'Download therapy resources', 'Therapy.Resources', '["Read", "Download"]', 1),
    ('Resources.Create', 'Resources', 'Create therapy resources', 'Therapy.Resources', '["Create"]', 1),
    ('Resources.Edit', 'Resources', 'Edit therapy resources', 'Therapy.Resources', '["Update"]', 1),
    ('Resources.Delete', 'Resources', 'Delete therapy resources', 'Therapy.Resources', '["Delete"]', 1),
    ('Students.View', 'Student Management', 'View student information', 'Therapy.Students', '["Read"]', 1),
    ('Students.Edit', 'Student Management', 'Edit student information', 'Therapy.Students', '["Update"]', 1),
    ('Students.Create', 'Student Management', 'Add new students', 'Therapy.Students', '["Create"]', 1),
    ('Sessions.View', 'Sessions', 'View therapy sessions', 'Therapy.Sessions', '["Read"]', 1),
    ('Sessions.Create', 'Sessions', 'Create therapy sessions', 'Therapy.Sessions', '["Create"]', 1),
    ('Sessions.Edit', 'Sessions', 'Edit therapy sessions', 'Therapy.Sessions', '["Update"]', 1),
    ('Marketplace.View', 'Marketplace', 'View marketplace items', 'Marketplace.*', '["Read"]', 1),
    ('Marketplace.Purchase', 'Marketplace', 'Purchase marketplace items', 'Marketplace.Transactions', '["Create"]', 1),
    ('Marketplace.Sell', 'Marketplace', 'Sell items on marketplace', 'Marketplace.*', '["Create", "Update"]', 1),
    ('Reports.View', 'Reporting', 'View reports and analytics', 'Analytics.*', '["Read"]', 1),
    ('Admin.Full', 'Administration', 'Full administrative access', '*', '["Create", "Read", "Update", "Delete"]', 1);
GO

-- =============================================
-- Insert data classifications for HIPAA compliance
-- =============================================

INSERT INTO [Security].[DataClassifications] (TableSchema, TableName, ColumnName, DataType, SensitivityLevel, EncryptionRequired, MaskingFunction)
VALUES
    -- PHI fields
    ('Core', 'Users', 'FirstName', 'PHI', 5, 1, 'default()'),
    ('Core', 'Users', 'LastName', 'PHI', 5, 1, 'default()'),
    ('Core', 'Users', 'PhoneNumber', 'PHI', 5, 1, 'partial(0,"XXX-XXX-",4)'),
    ('Therapy', 'Students', 'FirstName', 'PHI', 5, 1, 'default()'),
    ('Therapy', 'Students', 'LastName', 'PHI', 5, 1, 'default()'),
    ('Therapy', 'Students', 'DateOfBirth', 'PHI', 5, 1, 'default()'),
    ('Therapy', 'Students', 'ParentEmail', 'PHI', 5, 1, 'email()'),
    ('Therapy', 'Students', 'ParentPhone', 'PHI', 5, 1, 'partial(0,"XXX-XXX-",4)'),
    ('Therapy', 'Sessions', 'SessionNotes', 'PHI', 5, 1, NULL),
    -- PII fields
    ('Core', 'Users', 'Email', 'PII', 4, 0, 'email()'),
    ('Core', 'Users', 'LicenseNumber', 'PII', 4, 0, 'partial(0,"****",4)'),
    -- Financial fields
    ('Marketplace', 'Transactions', 'StripePaymentIntentID', 'Financial', 4, 1, 'default()'),
    ('Marketplace', 'SellerProfiles', 'PayoutDetails', 'Financial', 5, 1, NULL);
GO

PRINT 'Security tables and configurations created successfully.';
GO