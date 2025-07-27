-- Create Multi-tenant Schema Structure for UPTRMS
-- This script creates the schema and table structure with multi-tenant isolation

USE UPTRMS;
GO

-- =============================================
-- Core Schema Tables
-- =============================================

-- Tenants table for multi-tenant support
CREATE TABLE [Core].[Tenants]
(
    TenantID INT IDENTITY(1,1) NOT NULL,
    TenantGuid UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantName NVARCHAR(200) NOT NULL,
    TenantType NVARCHAR(50) NOT NULL, -- 'Individual', 'SmallGroup', 'LargeGroup', 'Enterprise', 'SchoolDistrict'
    SubscriptionTier NVARCHAR(50) NOT NULL, -- 'Basic', 'Pro', 'Enterprise'
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ExpirationDate DATETIME2 NULL,
    -- Multi-tenant configuration
    DatabaseSchema NVARCHAR(128) NULL, -- For dedicated schema per tenant (Enterprise only)
    StorageQuotaGB INT DEFAULT 10 NOT NULL,
    UserQuota INT DEFAULT 5 NOT NULL,
    -- Contact information
    PrimaryContactEmail NVARCHAR(256) NOT NULL,
    PrimaryContactName NVARCHAR(200) NOT NULL,
    BillingEmail NVARCHAR(256) NULL,
    -- Compliance
    IsHIPAABAAigned BIT DEFAULT 0 NOT NULL,
    BAASignedDate DATETIME2 NULL,
    BAADocument VARBINARY(MAX) NULL,
    CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (TenantID),
    CONSTRAINT UQ_Tenants_Guid UNIQUE (TenantGuid),
    CONSTRAINT CK_Tenants_Type CHECK (TenantType IN ('Individual', 'SmallGroup', 'LargeGroup', 'Enterprise', 'SchoolDistrict'))
);
GO

-- Enable Row-Level Security on Tenants
CREATE SECURITY POLICY [Core].[TenantPolicy]
ADD FILTER PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[Tenants],
ADD BLOCK PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[Tenants]
WITH (STATE = ON);
GO

-- Users table with PHI encryption
CREATE TABLE [Core].[Users]
(
    UserID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    EmailNormalized AS UPPER(Email) PERSISTED,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    -- Personal information (encrypted)
    FirstName VARBINARY(MAX) NOT NULL, -- Encrypted with Always Encrypted
    LastName VARBINARY(MAX) NOT NULL, -- Encrypted with Always Encrypted
    PhoneNumber VARBINARY(MAX) NULL, -- Encrypted
    -- Professional information
    LicenseNumber NVARCHAR(100) NULL,
    LicenseState CHAR(2) NULL,
    LicenseVerified BIT DEFAULT 0 NOT NULL,
    LicenseVerifiedDate DATETIME2 NULL,
    Specialties NVARCHAR(MAX) NULL, -- JSON array
    YearsOfExperience INT NULL,
    -- Account status
    IsActive BIT DEFAULT 1 NOT NULL,
    IsEmailConfirmed BIT DEFAULT 0 NOT NULL,
    IsTwoFactorEnabled BIT DEFAULT 0 NOT NULL,
    IsLockedOut BIT DEFAULT 0 NOT NULL,
    LockoutEndDateUtc DATETIME2 NULL,
    AccessFailedCount INT DEFAULT 0 NOT NULL,
    -- Audit fields
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    LastLoginDate DATETIME2 NULL,
    LastPasswordChangedDate DATETIME2 NULL,
    -- Marketplace
    IsMarketplaceSeller BIT DEFAULT 0 NOT NULL,
    SellerApplicationDate DATETIME2 NULL,
    SellerApprovedDate DATETIME2 NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserID),
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT UQ_Users_Email_Tenant UNIQUE (EmailNormalized, TenantID),
    INDEX IX_Users_Email NONCLUSTERED (EmailNormalized),
    INDEX IX_Users_TenantID NONCLUSTERED (TenantID)
);
GO

-- Apply Always Encrypted to PHI columns
ALTER TABLE [Core].[Users]
ALTER COLUMN FirstName VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NOT NULL;

ALTER TABLE [Core].[Users]
ALTER COLUMN LastName VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NOT NULL;

ALTER TABLE [Core].[Users]
ALTER COLUMN PhoneNumber VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NULL;
GO

-- Enable Row-Level Security on Users
CREATE SECURITY POLICY [Core].[UserPolicy]
ADD FILTER PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[Users],
ADD BLOCK PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[Users]
WITH (STATE = ON);
GO

-- Roles table
CREATE TABLE [Core].[Roles]
(
    RoleID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    RoleName NVARCHAR(100) NOT NULL,
    NormalizedRoleName AS UPPER(RoleName) PERSISTED,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT DEFAULT 0 NOT NULL,
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (RoleID),
    CONSTRAINT FK_Roles_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT UQ_Roles_Name_Tenant UNIQUE (NormalizedRoleName, TenantID)
);
GO

-- User roles mapping
CREATE TABLE [Core].[UserRoles]
(
    UserID UNIQUEIDENTIFIER NOT NULL,
    RoleID UNIQUEIDENTIFIER NOT NULL,
    AssignedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    AssignedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserID, RoleID),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleID) REFERENCES [Core].[Roles](RoleID),
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES [Core].[Users](UserID)
);
GO

-- =============================================
-- Therapy Schema Tables
-- =============================================

-- Students table with PHI encryption
CREATE TABLE [Therapy].[Students]
(
    StudentID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    TherapistID UNIQUEIDENTIFIER NOT NULL,
    -- Personal information (encrypted)
    FirstName VARBINARY(MAX) NOT NULL, -- Encrypted
    LastName VARBINARY(MAX) NOT NULL, -- Encrypted
    DateOfBirth VARBINARY(MAX) NOT NULL, -- Encrypted
    -- School information
    SchoolID UNIQUEIDENTIFIER NULL,
    Grade NVARCHAR(20) NULL,
    TeacherName NVARCHAR(200) NULL,
    -- IEP information
    HasIEP BIT DEFAULT 1 NOT NULL,
    IEPGoals NVARCHAR(MAX) NULL, -- JSON, encrypted
    PrimaryDisability NVARCHAR(100) NULL,
    SecondaryDisabilities NVARCHAR(MAX) NULL, -- JSON array
    -- Parent information (encrypted)
    ParentEmail VARBINARY(MAX) NULL, -- Encrypted
    ParentPhone VARBINARY(MAX) NULL, -- Encrypted
    ParentName VARBINARY(MAX) NULL, -- Encrypted
    -- Access
    StudentAccessCode VARCHAR(10) NOT NULL,
    ParentAccessEnabled BIT DEFAULT 0 NOT NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    DischargeDate DATETIME2 NULL,
    DischargeReason NVARCHAR(500) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (StudentID),
    CONSTRAINT FK_Students_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_Students_Therapists FOREIGN KEY (TherapistID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_Students_AccessCode UNIQUE (StudentAccessCode),
    INDEX IX_Students_TherapistID NONCLUSTERED (TherapistID),
    INDEX IX_Students_TenantID NONCLUSTERED (TenantID)
);
GO

-- Apply encryption to Student PHI
ALTER TABLE [Therapy].[Students]
ALTER COLUMN FirstName VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NOT NULL;

ALTER TABLE [Therapy].[Students]
ALTER COLUMN LastName VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NOT NULL;

ALTER TABLE [Therapy].[Students]
ALTER COLUMN DateOfBirth VARBINARY(MAX) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI_Protection],
    ENCRYPTION_TYPE = Randomized,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
) NOT NULL;
GO

-- Enable Row-Level Security on Students
CREATE SECURITY POLICY [Therapy].[StudentPolicy]
ADD FILTER PREDICATE [Security].[fn_TherapistAccessPredicate](TherapistID) ON [Therapy].[Students],
ADD BLOCK PREDICATE [Security].[fn_TherapistAccessPredicate](TherapistID) ON [Therapy].[Students]
WITH (STATE = ON);
GO

-- Therapy Resources table
CREATE TABLE [Therapy].[Resources]
(
    ResourceID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NULL, -- NULL for platform resources
    CreatorID UNIQUEIDENTIFIER NULL, -- NULL for platform resources
    -- Resource information
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ResourceType NVARCHAR(50) NOT NULL, -- 'Worksheet', 'Digital', 'Video', 'Assessment', etc.
    FileUrl NVARCHAR(2000) NULL,
    ThumbnailUrl NVARCHAR(2000) NULL,
    -- Categorization
    SkillAreas NVARCHAR(MAX) NOT NULL, -- JSON array
    AgeRanges NVARCHAR(MAX) NOT NULL, -- JSON array
    GradeLevels NVARCHAR(MAX) NULL, -- JSON array
    TherapyTypes NVARCHAR(MAX) NOT NULL, -- JSON array ['OT', 'PT', 'SLP', 'ABA']
    -- AI Generation tracking
    GenerationMethod NVARCHAR(50) DEFAULT 'Manual' NOT NULL, -- 'Manual', 'AI_Generated', 'AI_Assisted'
    AIGenerationID UNIQUEIDENTIFIER NULL,
    -- Clinical review
    ClinicalReviewStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
    ClinicalReviewDate DATETIME2 NULL,
    ClinicalReviewerID UNIQUEIDENTIFIER NULL,
    EvidenceLevel INT NULL, -- 1-5 scale
    -- Metadata
    Languages NVARCHAR(MAX) DEFAULT '["en"]' NOT NULL, -- JSON array
    IsInteractive BIT DEFAULT 0 NOT NULL,
    HasAudio BIT DEFAULT 0 NOT NULL,
    RequiresSubscription BIT DEFAULT 1 NOT NULL,
    -- Marketplace
    IsMarketplaceItem BIT DEFAULT 0 NOT NULL,
    Price DECIMAL(10,2) NULL,
    LicenseType NVARCHAR(100) NULL,
    -- Usage tracking
    DownloadCount INT DEFAULT 0 NOT NULL,
    FavoriteCount INT DEFAULT 0 NOT NULL,
    AverageRating DECIMAL(3,2) NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    PublishedDate DATETIME2 NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_Resources PRIMARY KEY CLUSTERED (ResourceID),
    CONSTRAINT FK_Resources_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_Resources_Creators FOREIGN KEY (CreatorID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_Resources_Reviewers FOREIGN KEY (ClinicalReviewerID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT CK_Resources_ReviewStatus CHECK (ClinicalReviewStatus IN ('Pending', 'Approved', 'Rejected', 'Revision_Required')),
    INDEX IX_Resources_Type NONCLUSTERED (ResourceType),
    INDEX IX_Resources_ReviewStatus NONCLUSTERED (ClinicalReviewStatus),
    INDEX IX_Resources_TenantID NONCLUSTERED (TenantID)
);
GO

-- Therapy Sessions table
CREATE TABLE [Therapy].[Sessions]
(
    SessionID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    TherapistID UNIQUEIDENTIFIER NOT NULL,
    StudentID UNIQUEIDENTIFIER NOT NULL,
    -- Session details
    SessionDate DATETIME2 NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    DurationMinutes AS DATEDIFF(MINUTE, StartTime, EndTime) PERSISTED,
    SessionType NVARCHAR(50) NOT NULL, -- 'Individual', 'Group', 'Evaluation', 'Teletherapy'
    Location NVARCHAR(200) NULL,
    -- Session data
    GoalsAddressed NVARCHAR(MAX) NULL, -- JSON array of goal IDs
    ResourcesUsed NVARCHAR(MAX) NULL, -- JSON array of resource IDs
    ActivitiesCompleted NVARCHAR(MAX) NULL, -- JSON array
    DataCollected NVARCHAR(MAX) NULL, -- JSON object with performance data
    -- Notes (encrypted if contains PHI)
    SessionNotes VARBINARY(MAX) NULL, -- Encrypted
    ParentCommunication VARBINARY(MAX) NULL, -- Encrypted
    -- Billing
    BillingCode NVARCHAR(20) NULL,
    BillingUnits DECIMAL(3,1) NULL,
    IsBillable BIT DEFAULT 1 NOT NULL,
    -- Status
    Status NVARCHAR(50) DEFAULT 'Scheduled' NOT NULL, -- 'Scheduled', 'Completed', 'Cancelled', 'No Show'
    CancellationReason NVARCHAR(500) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    DocumentedDate DATETIME2 NULL,
    CONSTRAINT PK_Sessions PRIMARY KEY CLUSTERED (SessionID),
    CONSTRAINT FK_Sessions_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_Sessions_Therapists FOREIGN KEY (TherapistID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_Sessions_Students FOREIGN KEY (StudentID) REFERENCES [Therapy].[Students](StudentID),
    CONSTRAINT CK_Sessions_Status CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled', 'No Show')),
    INDEX IX_Sessions_Date NONCLUSTERED (SessionDate DESC),
    INDEX IX_Sessions_TherapistID NONCLUSTERED (TherapistID),
    INDEX IX_Sessions_StudentID NONCLUSTERED (StudentID)
);
GO

-- =============================================
-- Marketplace Schema Tables
-- =============================================

-- Seller Profiles
CREATE TABLE [Marketplace].[SellerProfiles]
(
    SellerID UNIQUEIDENTIFIER NOT NULL,
    StoreName NVARCHAR(200) NOT NULL,
    StoreUrl NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(MAX) NULL,
    Specialties NVARCHAR(MAX) NULL, -- JSON array
    ProfileImageUrl NVARCHAR(2000) NULL,
    BannerImageUrl NVARCHAR(2000) NULL,
    -- Metrics
    TotalSales INT DEFAULT 0 NOT NULL,
    TotalRevenue DECIMAL(12,2) DEFAULT 0 NOT NULL,
    AverageRating DECIMAL(3,2) NULL,
    FollowerCount INT DEFAULT 0 NOT NULL,
    -- Settings
    CommissionRate DECIMAL(5,4) DEFAULT 0.3000 NOT NULL, -- 30% default
    PayoutMethod NVARCHAR(50) NULL,
    PayoutDetails VARBINARY(MAX) NULL, -- Encrypted
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    IsFeatured BIT DEFAULT 0 NOT NULL,
    VerificationStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_SellerProfiles PRIMARY KEY CLUSTERED (SellerID),
    CONSTRAINT FK_SellerProfiles_Users FOREIGN KEY (SellerID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_SellerProfiles_StoreUrl UNIQUE (StoreUrl)
);
GO

-- Marketplace Transactions
CREATE TABLE [Marketplace].[Transactions]
(
    TransactionID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    OrderNumber VARCHAR(20) NOT NULL,
    BuyerID UNIQUEIDENTIFIER NOT NULL,
    SellerID UNIQUEIDENTIFIER NOT NULL,
    -- Transaction details
    ResourceID UNIQUEIDENTIFIER NOT NULL,
    Quantity INT DEFAULT 1 NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal AS (Quantity * UnitPrice) PERSISTED,
    CommissionAmount DECIMAL(10,2) NOT NULL,
    SellerEarnings AS (Quantity * UnitPrice - CommissionAmount) PERSISTED,
    -- Payment
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaymentStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
    PaymentDate DATETIME2 NULL,
    StripePaymentIntentID NVARCHAR(200) NULL,
    -- Fulfillment
    FulfillmentStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
    DownloadUrl NVARCHAR(2000) NULL,
    DownloadExpiration DATETIME2 NULL,
    DownloadCount INT DEFAULT 0 NOT NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    RefundedDate DATETIME2 NULL,
    RefundAmount DECIMAL(10,2) NULL,
    RefundReason NVARCHAR(500) NULL,
    CONSTRAINT PK_Transactions PRIMARY KEY CLUSTERED (TransactionID),
    CONSTRAINT FK_Transactions_Buyers FOREIGN KEY (BuyerID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_Transactions_Sellers FOREIGN KEY (SellerID) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_Transactions_Resources FOREIGN KEY (ResourceID) REFERENCES [Therapy].[Resources](ResourceID),
    CONSTRAINT UQ_Transactions_OrderNumber UNIQUE (OrderNumber),
    INDEX IX_Transactions_BuyerID NONCLUSTERED (BuyerID),
    INDEX IX_Transactions_SellerID NONCLUSTERED (SellerID),
    INDEX IX_Transactions_CreatedDate NONCLUSTERED (CreatedDate DESC)
);
GO

-- =============================================
-- Audit Schema Tables
-- =============================================

-- Comprehensive audit log
CREATE TABLE [Audit].[AuditLog]
(
    AuditID BIGINT IDENTITY(1,1) NOT NULL,
    EventTime DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    UserID UNIQUEIDENTIFIER NULL,
    TenantID INT NULL,
    -- Event details
    EventType NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(128) NULL,
    RecordID NVARCHAR(100) NULL,
    Action NVARCHAR(50) NOT NULL, -- 'Create', 'Read', 'Update', 'Delete', 'Login', 'Logout', etc.
    -- Change tracking
    OldValues NVARCHAR(MAX) NULL, -- JSON
    NewValues NVARCHAR(MAX) NULL, -- JSON
    ChangedColumns NVARCHAR(MAX) NULL, -- JSON array
    -- Context
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionID NVARCHAR(100) NULL,
    CorrelationID UNIQUEIDENTIFIER NULL,
    -- Additional data
    AdditionalData NVARCHAR(MAX) NULL, -- JSON
    ErrorMessage NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (AuditID),
    INDEX IX_AuditLog_EventTime NONCLUSTERED (EventTime DESC),
    INDEX IX_AuditLog_UserID NONCLUSTERED (UserID),
    INDEX IX_AuditLog_TenantID NONCLUSTERED (TenantID),
    INDEX IX_AuditLog_TableName_RecordID NONCLUSTERED (TableName, RecordID)
) ON [UPTRMS_Archive];
GO

-- Enable CDC on critical tables
EXEC sys.sp_cdc_enable_table
    @source_schema = N'Core',
    @source_name = N'Users',
    @role_name = N'AuditorRole',
    @supports_net_changes = 1;
GO

EXEC sys.sp_cdc_enable_table
    @source_schema = N'Therapy',
    @source_name = N'Students',
    @role_name = N'AuditorRole',
    @supports_net_changes = 1;
GO

EXEC sys.sp_cdc_enable_table
    @source_schema = N'Therapy',
    @source_name = N'Sessions',
    @role_name = N'AuditorRole',
    @supports_net_changes = 1;
GO

-- =============================================
-- Create initial data
-- =============================================

-- Insert system roles
INSERT INTO [Core].[Roles] (TenantID, RoleName, Description, IsSystemRole)
SELECT 
    t.TenantID,
    r.RoleName,
    r.Description,
    1
FROM [Core].[Tenants] t
CROSS JOIN (VALUES
    ('Administrator', 'Full system access'),
    ('Therapist', 'Standard therapist access'),
    ('Supervisor', 'Clinical supervisor access'),
    ('Billing', 'Billing and financial access'),
    ('Marketplace Seller', 'Can sell resources on marketplace'),
    ('Auditor', 'Read-only audit access')
) AS r(RoleName, Description);
GO

PRINT 'Multi-tenant schema structure created successfully.';
GO