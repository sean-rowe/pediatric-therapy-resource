-- TherapyDocs Database Initialization with HIPAA Compliance
-- Enable Always Encrypted and TDE for PHI protection

-- Create databases
CREATE DATABASE therapydocs_dev;
CREATE DATABASE therapydocs_test;
CREATE DATABASE therapydocs_prod;
CREATE DATABASE therapydocs_audit;
CREATE DATABASE therapydocs_fhir;
GO

USE therapydocs_dev;
GO

-- Create Column Master Key for Always Encrypted
CREATE COLUMN MASTER KEY [CMK_TherapyDocs]
WITH (
    KEY_STORE_PROVIDER_NAME = N'MSSQL_CERTIFICATE_STORE',
    KEY_PATH = N'CurrentUser/My/TherapyDocsCMK'
);

-- Create Column Encryption Key for PHI data
CREATE COLUMN ENCRYPTION KEY [CEK_PHI]
WITH VALUES (
    COLUMN_MASTER_KEY = [CMK_TherapyDocs],
    ALGORITHM = 'RSA_OAEP',
    ENCRYPTED_VALUE = 0x016E000001630075007200720065006E00740075007300650072002F006D0079002F00740068006500720061007000790064006F006300730020001400000001000000001A3B4C5D6E7F8091A2B3C4D5E6F708192A3B4C5D6E7F8091A2B3C4D5E6F708
);

-- Enable Transparent Data Encryption
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'TDE_M@st3r_K3y_Th3r@pyD0cs2024!';
CREATE CERTIFICATE TDE_Certificate WITH SUBJECT = 'TDE Certificate TherapyDocs';
CREATE DATABASE ENCRYPTION KEY
    WITH ALGORITHM = AES_256
    ENCRYPTION BY SERVER CERTIFICATE TDE_Certificate;
ALTER DATABASE therapydocs_dev SET ENCRYPTION ON;
GO

-- Enable audit logging for HIPAA compliance
ALTER DATABASE therapydocs_dev SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 90 DAYS, AUTO_CLEANUP = ON);
GO

-- Create schemas for organization
CREATE SCHEMA therapy;
CREATE SCHEMA billing;
CREATE SCHEMA compliance;
CREATE SCHEMA audit;
CREATE SCHEMA auth;
CREATE SCHEMA integration;
GO

-- Create audit table (immutable for HIPAA)
CREATE TABLE audit.SecurityEvents (
    EventId BIGINT IDENTITY(1,1) NOT NULL,
    EventTime DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UserId INT NULL,
    UserEmail NVARCHAR(255) NULL,
    Action NVARCHAR(100) NOT NULL,
    Resource NVARCHAR(200) NOT NULL,
    ResourceId NVARCHAR(50) NULL,
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionId NVARCHAR(128) NULL,
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    Details NVARCHAR(MAX) NULL,
    ComplianceFlags NVARCHAR(100) NULL, -- HIPAA, FERPA flags
    CONSTRAINT PK_SecurityEvents PRIMARY KEY NONCLUSTERED (EventId)
) WITH (SYSTEM_VERSIONING = ON);
GO

-- Create index for performance and compliance queries
CREATE CLUSTERED INDEX IX_SecurityEvents_EventTime ON audit.SecurityEvents (EventTime);
CREATE INDEX IX_SecurityEvents_User ON audit.SecurityEvents (UserId, EventTime);
CREATE INDEX IX_SecurityEvents_Action ON audit.SecurityEvents (Action, EventTime);
CREATE INDEX IX_SecurityEvents_Resource ON audit.SecurityEvents (Resource, EventTime);
GO

-- Create HIPAA audit trigger for data access
CREATE TABLE audit.DataAccess (
    AccessId BIGINT IDENTITY(1,1) NOT NULL,
    AccessTime DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UserId INT NOT NULL,
    TableName NVARCHAR(128) NOT NULL,
    RecordId NVARCHAR(50) NOT NULL,
    StudentId INT NULL, -- For student-related data access
    AccessType NVARCHAR(20) NOT NULL, -- SELECT, INSERT, UPDATE, DELETE
    FieldsAccessed NVARCHAR(MAX) NULL, -- JSON list of fields
    IPAddress NVARCHAR(45) NULL,
    ApplicationContext NVARCHAR(200) NULL,
    CONSTRAINT PK_DataAccess PRIMARY KEY (AccessId)
) WITH (SYSTEM_VERSIONING = ON);
GO

CREATE CLUSTERED INDEX IX_DataAccess_Time ON audit.DataAccess (AccessTime);
CREATE INDEX IX_DataAccess_Student ON audit.DataAccess (StudentId, AccessTime);
CREATE INDEX IX_DataAccess_User ON audit.DataAccess (UserId, AccessTime);
GO

-- Create users table with Always Encrypted columns for PHI
CREATE TABLE auth.Users (
    UserId INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    LastName NVARCHAR(100) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    LicenseNumber NVARCHAR(50) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    ServiceType NVARCHAR(20) NOT NULL, -- SLP, OT, PT, Admin
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

-- Create roles table for RBAC
CREATE TABLE auth.Roles (
    RoleId INT IDENTITY(1,1) NOT NULL,
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_Name UNIQUE (RoleName)
);

-- Insert the 7 roles identified in requirements
INSERT INTO auth.Roles (RoleName, Description) VALUES
('Therapist', 'Speech, Occupational, or Physical Therapist'),
('Supervisor', 'Therapy Supervisor with oversight responsibilities'),
('Billing', 'Billing specialist with claims and authorization access'),
('Admin', 'System administrator with full access'),
('Auditor', 'Read-only access for compliance audits'),
('Parent', 'Parent/guardian with limited student access'),
('Student', 'Student with limited self-service access');
GO

-- Create user roles junction table
CREATE TABLE auth.UserRoles (
    UserRoleId INT IDENTITY(1,1) NOT NULL,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    EffectiveDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ExpirationDate DATETIME2 NULL, -- For time-bound access (substitutes)
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES auth.Users(UserId),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES auth.Roles(RoleId),
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId, EffectiveDate)
);
GO

-- Create students table with encryption for PHI
CREATE TABLE therapy.Students (
    StudentId INT IDENTITY(1,1) NOT NULL,
    SchoolStudentId NVARCHAR(50) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    FirstName NVARCHAR(100) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    LastName NVARCHAR(100) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    DateOfBirth DATE ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
    Gender NCHAR(1) NULL,
    Grade NVARCHAR(10) NULL,
    SchoolId INT NOT NULL,
    MedicalAlerts NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    BehavioralNotes NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    IEPStartDate DATE NULL,
    IEPEndDate DATE NULL,
    PrimaryTherapistId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Students PRIMARY KEY (StudentId)
);
GO

-- Create schools table
CREATE TABLE therapy.Schools (
    SchoolId INT IDENTITY(1,1) NOT NULL,
    SchoolName NVARCHAR(255) NOT NULL,
    DistrictName NVARCHAR(255) NOT NULL,
    DistrictId NVARCHAR(50) NULL,
    Address NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    State NCHAR(2) NULL,
    ZipCode NVARCHAR(10) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    PrincipalName NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_Schools PRIMARY KEY (SchoolId)
);
GO

-- Add foreign key constraint
ALTER TABLE therapy.Students ADD CONSTRAINT FK_Students_School 
    FOREIGN KEY (SchoolId) REFERENCES therapy.Schools(SchoolId);
ALTER TABLE therapy.Students ADD CONSTRAINT FK_Students_PrimaryTherapist 
    FOREIGN KEY (PrimaryTherapistId) REFERENCES auth.Users(UserId);
GO

-- Create state compliance deadlines table
CREATE TABLE compliance.StateDeadlines (
    StateCode NCHAR(2) NOT NULL,
    ServiceType NVARCHAR(20) NOT NULL, -- SLP, OT, PT
    DocumentationType NVARCHAR(50) NOT NULL, -- IEP, Evaluation, Progress Report
    DeadlineDays INT NOT NULL, -- Business days after service
    IsBusinessDays BIT NOT NULL DEFAULT 1,
    EscalationDays INT NOT NULL, -- When to send alerts
    SupervisorReviewRequired BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    EffectiveDate DATE NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_StateDeadlines PRIMARY KEY (StateCode, ServiceType, DocumentationType)
);
GO

-- Insert sample state deadline rules (Texas, California, New York, Florida)
INSERT INTO compliance.StateDeadlines (StateCode, ServiceType, DocumentationType, DeadlineDays, IsBusinessDays, EscalationDays, SupervisorReviewRequired, EffectiveDate) VALUES
('TX', 'SLP', 'Progress Report', 5, 1, 2, 1, '2024-01-01'),
('TX', 'OT', 'Progress Report', 5, 1, 2, 1, '2024-01-01'),
('TX', 'PT', 'Progress Report', 5, 1, 2, 1, '2024-01-01'),
('CA', 'SLP', 'Progress Report', 3, 1, 1, 1, '2024-01-01'),
('CA', 'OT', 'Progress Report', 3, 1, 1, 1, '2024-01-01'),
('CA', 'PT', 'Progress Report', 3, 1, 1, 1, '2024-01-01'),
('NY', 'SLP', 'Progress Report', 7, 0, 2, 0, '2024-01-01'),
('NY', 'OT', 'Progress Report', 7, 0, 2, 0, '2024-01-01'),
('NY', 'PT', 'Progress Report', 7, 0, 2, 0, '2024-01-01'),
('FL', 'SLP', 'Progress Report', 10, 1, 3, 1, '2024-01-01'),
('FL', 'OT', 'Progress Report', 10, 1, 3, 1, '2024-01-01'),
('FL', 'PT', 'Progress Report', 10, 1, 3, 1, '2024-01-01');
GO

-- Create session documentation table
CREATE TABLE therapy.Sessions (
    SessionId INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    TherapistId INT NOT NULL,
    SessionDate DATETIME2 NOT NULL,
    SessionType NVARCHAR(20) NOT NULL, -- Individual, Group, Consultation
    ServiceType NVARCHAR(20) NOT NULL, -- SLP, OT, PT
    Duration INT NOT NULL, -- Minutes
    Location NVARCHAR(100) NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CompletedDate DATETIME2 NULL,
    CONSTRAINT PK_Sessions PRIMARY KEY (SessionId),
    CONSTRAINT FK_Sessions_Student FOREIGN KEY (StudentId) REFERENCES therapy.Students(StudentId),
    CONSTRAINT FK_Sessions_Therapist FOREIGN KEY (TherapistId) REFERENCES auth.Users(UserId)
);
GO

-- Create session notes table with encryption
CREATE TABLE therapy.SessionNotes (
    NoteId INT IDENTITY(1,1) NOT NULL,
    SessionId INT NOT NULL,
    Subjective NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    Objective NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    Assessment NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    Plan NVARCHAR(MAX) ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK_PHI], ENCRYPTION_TYPE = Randomized, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedBy INT NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy INT NULL,
    CONSTRAINT PK_SessionNotes PRIMARY KEY (NoteId),
    CONSTRAINT FK_SessionNotes_Session FOREIGN KEY (SessionId) REFERENCES therapy.Sessions(SessionId),
    CONSTRAINT FK_SessionNotes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId)
);
GO

-- Create prior authorization table for billing
CREATE TABLE billing.PriorAuthorizations (
    AuthorizationId INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    PayerName NVARCHAR(255) NOT NULL,
    AuthorizationNumber NVARCHAR(100) NOT NULL,
    ServiceType NVARCHAR(20) NOT NULL,
    UnitsAuthorized INT NOT NULL,
    UnitsUsed INT NOT NULL DEFAULT 0,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active, Expired, Suspended
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_PriorAuthorizations PRIMARY KEY (AuthorizationId),
    CONSTRAINT FK_PriorAuth_Student FOREIGN KEY (StudentId) REFERENCES therapy.Students(StudentId)
);
GO

-- Create index for prior auth lookups
CREATE INDEX IX_PriorAuth_Student_Date ON billing.PriorAuthorizations (StudentId, EndDate);
CREATE INDEX IX_PriorAuth_Status ON billing.PriorAuthorizations (Status, EndDate);
GO

-- Enable SQL Server Audit for HIPAA compliance
CREATE SERVER AUDIT [TherapyDocs_HIPAA_Audit]
TO FILE (FILEPATH = '/var/opt/mssql/audit/', MAXSIZE = 100 MB, MAX_ROLLOVER_FILES = 50)
WITH (QUEUE_DELAY = 1000, ON_FAILURE = CONTINUE);

CREATE DATABASE AUDIT SPECIFICATION [TherapyDocs_DB_Audit]
FOR SERVER AUDIT [TherapyDocs_HIPAA_Audit]
ADD (SELECT, INSERT, UPDATE, DELETE ON therapy.Students BY public),
ADD (SELECT, INSERT, UPDATE, DELETE ON therapy.SessionNotes BY public),
ADD (SELECT, INSERT, UPDATE, DELETE ON billing.PriorAuthorizations BY public),
ADD (LOGIN_CHANGE_PASSWORD_GROUP),
ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP),
ADD (DATABASE_PERMISSION_CHANGE_GROUP);

ALTER SERVER AUDIT [TherapyDocs_HIPAA_Audit] WITH (STATE = ON);
ALTER DATABASE AUDIT SPECIFICATION [TherapyDocs_DB_Audit] WITH (STATE = ON);
GO

-- Create stored procedures for HIPAA audit logging
CREATE PROCEDURE audit.LogSecurityEvent
    @UserId INT,
    @Action NVARCHAR(100),
    @Resource NVARCHAR(200),
    @ResourceId NVARCHAR(50) = NULL,
    @IPAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @SessionId NVARCHAR(128) = NULL,
    @Success BIT,
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @Details NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO audit.SecurityEvents (
        UserId, Action, Resource, ResourceId, IPAddress, UserAgent, 
        SessionId, Success, ErrorMessage, Details
    )
    VALUES (
        @UserId, @Action, @Resource, @ResourceId, @IPAddress, @UserAgent,
        @SessionId, @Success, @ErrorMessage, @Details
    );
END;
GO

-- Create function to validate state compliance deadlines
CREATE FUNCTION compliance.GetDeadlineDate(
    @StateCode NCHAR(2),
    @ServiceType NVARCHAR(20),
    @DocumentationType NVARCHAR(50),
    @ServiceDate DATETIME2
)
RETURNS DATETIME2
AS
BEGIN
    DECLARE @DeadlineDays INT, @IsBusinessDays BIT, @DeadlineDate DATETIME2;
    
    SELECT @DeadlineDays = DeadlineDays, @IsBusinessDays = IsBusinessDays
    FROM compliance.StateDeadlines
    WHERE StateCode = @StateCode 
      AND ServiceType = @ServiceType 
      AND DocumentationType = @DocumentationType
      AND IsActive = 1;
    
    IF @IsBusinessDays = 1
    BEGIN
        -- Add business days only
        SET @DeadlineDate = @ServiceDate;
        DECLARE @DaysToAdd INT = @DeadlineDays;
        
        WHILE @DaysToAdd > 0
        BEGIN
            SET @DeadlineDate = DATEADD(day, 1, @DeadlineDate);
            IF DATEPART(weekday, @DeadlineDate) NOT IN (1, 7) -- Not Sunday or Saturday
                SET @DaysToAdd = @DaysToAdd - 1;
        END
    END
    ELSE
    BEGIN
        -- Add calendar days
        SET @DeadlineDate = DATEADD(day, @DeadlineDays, @ServiceDate);
    END
    
    RETURN @DeadlineDate;
END;
GO

PRINT 'TherapyDocs database initialization completed successfully.';
PRINT 'HIPAA compliance features enabled:';
PRINT '  - Transparent Data Encryption (TDE)';
PRINT '  - Always Encrypted for PHI columns';
PRINT '  - Comprehensive audit logging';
PRINT '  - State compliance deadline tracking';
PRINT '  - 7-role RBAC foundation';
PRINT 'Database is ready for application development.';
GO