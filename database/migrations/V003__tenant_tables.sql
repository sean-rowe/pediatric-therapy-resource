-- V003__tenant_tables.sql
-- Multi-tenant support tables and configurations

USE UPTRMS;
GO

-- =============================================
-- Tenant Configuration Tables
-- =============================================

-- Tenant features and limits
CREATE TABLE [Core].[TenantFeatures]
(
    FeatureID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    FeatureName NVARCHAR(100) NOT NULL,
    IsEnabled BIT DEFAULT 1 NOT NULL,
    -- Limits and quotas
    NumericLimit INT NULL, -- e.g., max users, storage GB
    DateLimit DATETIME2 NULL, -- e.g., trial expiration
    JsonConfiguration NVARCHAR(MAX) NULL, -- Feature-specific config
    -- Audit
    EnabledDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    EnabledBy UNIQUEIDENTIFIER NULL,
    DisabledDate DATETIME2 NULL,
    DisabledBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_TenantFeatures PRIMARY KEY CLUSTERED (FeatureID),
    CONSTRAINT FK_TenantFeatures_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_TenantFeatures_EnabledBy FOREIGN KEY (EnabledBy) REFERENCES [Core].[Users](UserID),
    CONSTRAINT FK_TenantFeatures_DisabledBy FOREIGN KEY (DisabledBy) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_TenantFeatures_Tenant_Feature UNIQUE (TenantID, FeatureName),
    INDEX IX_TenantFeatures_TenantID NONCLUSTERED (TenantID)
);
GO

-- Tenant branding/white-label configuration
CREATE TABLE [Core].[TenantBranding]
(
    BrandingID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    -- Basic branding
    CompanyName NVARCHAR(200) NOT NULL,
    LogoUrl NVARCHAR(2000) NULL,
    FaviconUrl NVARCHAR(2000) NULL,
    -- Color scheme
    PrimaryColor NVARCHAR(7) DEFAULT '#1976D2' NOT NULL, -- Hex color
    SecondaryColor NVARCHAR(7) DEFAULT '#424242' NOT NULL,
    AccentColor NVARCHAR(7) DEFAULT '#82B1FF' NOT NULL,
    -- Custom CSS
    CustomCSS NVARCHAR(MAX) NULL,
    -- Email branding
    EmailHeaderHtml NVARCHAR(MAX) NULL,
    EmailFooterHtml NVARCHAR(MAX) NULL,
    FromEmailName NVARCHAR(100) NULL,
    ReplyToEmail NVARCHAR(256) NULL,
    -- Domain configuration
    CustomDomain NVARCHAR(256) NULL,
    SSLCertificateID UNIQUEIDENTIFIER NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT PK_TenantBranding PRIMARY KEY CLUSTERED (BrandingID),
    CONSTRAINT FK_TenantBranding_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT FK_TenantBranding_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES [Core].[Users](UserID),
    CONSTRAINT UQ_TenantBranding_TenantID UNIQUE (TenantID),
    CONSTRAINT UQ_TenantBranding_CustomDomain UNIQUE (CustomDomain)
);
GO

-- Tenant billing and subscriptions
CREATE TABLE [Core].[TenantSubscriptions]
(
    SubscriptionID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    -- Subscription details
    PlanName NVARCHAR(100) NOT NULL,
    PlanType NVARCHAR(50) NOT NULL, -- 'Monthly', 'Annual', 'Custom'
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    RenewalDate DATETIME2 NULL,
    -- Pricing
    BasePrice DECIMAL(10,2) NOT NULL,
    UserPrice DECIMAL(10,2) NULL, -- Per user pricing
    DiscountPercentage DECIMAL(5,2) DEFAULT 0 NOT NULL,
    TotalPrice AS (BasePrice * (1 - DiscountPercentage/100)) PERSISTED,
    -- Billing
    BillingCycle NVARCHAR(50) NOT NULL, -- 'Monthly', 'Quarterly', 'Annual'
    PaymentMethod NVARCHAR(50) NULL,
    StripeCustomerID VARBINARY(MAX) NULL, -- Encrypted
    StripeSubscriptionID VARBINARY(MAX) NULL, -- Encrypted
    -- Status
    Status NVARCHAR(50) DEFAULT 'Active' NOT NULL, -- 'Trial', 'Active', 'PastDue', 'Canceled', 'Expired'
    IsTrial BIT DEFAULT 0 NOT NULL,
    TrialEndDate DATETIME2 NULL,
    CancelationDate DATETIME2 NULL,
    CancelationReason NVARCHAR(500) NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_TenantSubscriptions PRIMARY KEY CLUSTERED (SubscriptionID),
    CONSTRAINT FK_TenantSubscriptions_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT CK_TenantSubscriptions_Status CHECK (Status IN ('Trial', 'Active', 'PastDue', 'Canceled', 'Expired')),
    INDEX IX_TenantSubscriptions_TenantID NONCLUSTERED (TenantID),
    INDEX IX_TenantSubscriptions_Status NONCLUSTERED (Status)
);
GO

-- Tenant usage tracking
CREATE TABLE [Core].[TenantUsage]
(
    UsageID BIGINT IDENTITY(1,1) NOT NULL,
    TenantID INT NOT NULL,
    UsageDate DATE NOT NULL,
    -- User metrics
    ActiveUsers INT DEFAULT 0 NOT NULL,
    TotalUsers INT DEFAULT 0 NOT NULL,
    NewUsers INT DEFAULT 0 NOT NULL,
    -- Storage metrics
    StorageUsedGB DECIMAL(10,2) DEFAULT 0 NOT NULL,
    FileCount INT DEFAULT 0 NOT NULL,
    -- Activity metrics
    SessionCount INT DEFAULT 0 NOT NULL,
    ResourceDownloads INT DEFAULT 0 NOT NULL,
    AIGenerations INT DEFAULT 0 NOT NULL,
    MarketplacePurchases INT DEFAULT 0 NOT NULL,
    -- API usage
    APICallCount BIGINT DEFAULT 0 NOT NULL,
    APIErrorCount INT DEFAULT 0 NOT NULL,
    -- Calculated metrics
    DailyActiveRate AS (CAST(ActiveUsers AS DECIMAL(10,2)) / NULLIF(TotalUsers, 0)) PERSISTED,
    CONSTRAINT PK_TenantUsage PRIMARY KEY CLUSTERED (UsageID),
    CONSTRAINT FK_TenantUsage_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT UQ_TenantUsage_Tenant_Date UNIQUE (TenantID, UsageDate),
    INDEX IX_TenantUsage_TenantID_Date NONCLUSTERED (TenantID, UsageDate DESC)
);
GO

-- =============================================
-- School District Specific Tables
-- =============================================

-- School districts (for enterprise tenants)
CREATE TABLE [Core].[SchoolDistricts]
(
    DistrictID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    TenantID INT NOT NULL,
    -- District information
    DistrictName NVARCHAR(200) NOT NULL,
    DistrictCode NVARCHAR(50) NULL,
    StateCode CHAR(2) NOT NULL,
    -- Contact information
    AdminContactName NVARCHAR(200) NOT NULL,
    AdminContactEmail NVARCHAR(256) NOT NULL,
    AdminContactPhone NVARCHAR(20) NULL,
    -- Integration
    SISProvider NVARCHAR(100) NULL, -- 'PowerSchool', 'Infinite Campus', etc.
    SSOProvider NVARCHAR(100) NULL, -- 'Clever', 'ClassLink', 'Google', 'Azure'
    LMSProvider NVARCHAR(100) NULL, -- 'Canvas', 'Schoology', 'Google Classroom'
    -- Configuration
    IPRanges NVARCHAR(MAX) NULL, -- JSON array of allowed IP ranges
    DefaultTimezone NVARCHAR(100) DEFAULT 'America/New_York' NOT NULL,
    SchoolYearStart INT DEFAULT 8 NOT NULL, -- Month number
    -- Compliance
    DataRetentionYears INT DEFAULT 7 NOT NULL,
    RequireParentConsent BIT DEFAULT 1 NOT NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_SchoolDistricts PRIMARY KEY CLUSTERED (DistrictID),
    CONSTRAINT FK_SchoolDistricts_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    CONSTRAINT UQ_SchoolDistricts_TenantID UNIQUE (TenantID)
);
GO

-- Schools within districts
CREATE TABLE [Core].[Schools]
(
    SchoolID UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
    DistrictID UNIQUEIDENTIFIER NOT NULL,
    TenantID INT NOT NULL,
    -- School information
    SchoolName NVARCHAR(200) NOT NULL,
    SchoolCode NVARCHAR(50) NULL,
    SchoolType NVARCHAR(50) NOT NULL, -- 'Elementary', 'Middle', 'High', 'Other'
    -- Address
    StreetAddress NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    StateCode CHAR(2) NOT NULL,
    ZipCode NVARCHAR(10) NULL,
    -- Contact
    PrincipalName NVARCHAR(200) NULL,
    PrincipalEmail NVARCHAR(256) NULL,
    MainPhone NVARCHAR(20) NULL,
    -- Configuration
    GradeLevels NVARCHAR(MAX) NULL, -- JSON array
    StudentCount INT NULL,
    TherapistCount INT NULL,
    -- Status
    IsActive BIT DEFAULT 1 NOT NULL,
    -- Audit
    CreatedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    ModifiedDate DATETIME2 DEFAULT SYSDATETIME() NOT NULL,
    CONSTRAINT PK_Schools PRIMARY KEY CLUSTERED (SchoolID),
    CONSTRAINT FK_Schools_Districts FOREIGN KEY (DistrictID) REFERENCES [Core].[SchoolDistricts](DistrictID),
    CONSTRAINT FK_Schools_Tenants FOREIGN KEY (TenantID) REFERENCES [Core].[Tenants](TenantID),
    INDEX IX_Schools_DistrictID NONCLUSTERED (DistrictID)
);
GO

-- =============================================
-- Tenant Isolation Procedures
-- =============================================

-- Procedure to set up new tenant
CREATE PROCEDURE [Core].[sp_SetupNewTenant]
    @TenantName NVARCHAR(200),
    @TenantType NVARCHAR(50),
    @SubscriptionTier NVARCHAR(50),
    @PrimaryContactEmail NVARCHAR(256),
    @PrimaryContactName NVARCHAR(200),
    @TenantID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Create tenant
        INSERT INTO [Core].[Tenants]
        (TenantName, TenantType, SubscriptionTier, PrimaryContactEmail, PrimaryContactName)
        VALUES
        (@TenantName, @TenantType, @SubscriptionTier, @PrimaryContactEmail, @PrimaryContactName);
        
        SET @TenantID = SCOPE_IDENTITY();
        
        -- Set up default features based on subscription tier
        IF @SubscriptionTier = 'Basic'
        BEGIN
            INSERT INTO [Core].[TenantFeatures] (TenantID, FeatureName, NumericLimit)
            VALUES
                (@TenantID, 'MaxUsers', 1),
                (@TenantID, 'StorageGB', 5),
                (@TenantID, 'ResourceAccess', 1),
                (@TenantID, 'BasicReporting', 1);
        END
        ELSE IF @SubscriptionTier = 'Pro'
        BEGIN
            INSERT INTO [Core].[TenantFeatures] (TenantID, FeatureName, NumericLimit)
            VALUES
                (@TenantID, 'MaxUsers', 5),
                (@TenantID, 'StorageGB', 50),
                (@TenantID, 'ResourceAccess', 1),
                (@TenantID, 'AdvancedReporting', 1),
                (@TenantID, 'AIGeneration', 100),
                (@TenantID, 'MarketplaceAccess', 1);
        END
        ELSE IF @SubscriptionTier = 'Enterprise'
        BEGIN
            INSERT INTO [Core].[TenantFeatures] (TenantID, FeatureName, NumericLimit)
            VALUES
                (@TenantID, 'MaxUsers', 9999),
                (@TenantID, 'StorageGB', 1000),
                (@TenantID, 'ResourceAccess', 1),
                (@TenantID, 'AdvancedReporting', 1),
                (@TenantID, 'AIGeneration', 9999),
                (@TenantID, 'MarketplaceAccess', 1),
                (@TenantID, 'WhiteLabel', 1),
                (@TenantID, 'APIAccess', 1),
                (@TenantID, 'SSOIntegration', 1);
        END
        
        -- Create default branding
        INSERT INTO [Core].[TenantBranding] (TenantID, CompanyName)
        VALUES (@TenantID, @TenantName);
        
        -- Create initial subscription
        DECLARE @TrialDays INT = CASE 
            WHEN @SubscriptionTier = 'Basic' THEN 14
            WHEN @SubscriptionTier = 'Pro' THEN 30
            ELSE 30
        END;
        
        INSERT INTO [Core].[TenantSubscriptions]
        (TenantID, PlanName, PlanType, StartDate, TrialEndDate, BasePrice, BillingCycle, Status, IsTrial)
        VALUES
        (@TenantID, @SubscriptionTier, 'Monthly', SYSDATETIME(), 
         DATEADD(DAY, @TrialDays, SYSDATETIME()), 0, 'Monthly', 'Trial', 1);
        
        -- Create system roles for tenant
        INSERT INTO [Core].[Roles] (TenantID, RoleName, Description, IsSystemRole)
        VALUES
            (@TenantID, 'Administrator', 'Full system access', 1),
            (@TenantID, 'Therapist', 'Standard therapist access', 1),
            (@TenantID, 'Supervisor', 'Clinical supervisor access', 1),
            (@TenantID, 'Parent', 'Parent/caregiver access', 1);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Function to check tenant resource limits
CREATE FUNCTION [Core].[fn_CheckTenantLimit]
(
    @TenantID INT,
    @FeatureName NVARCHAR(100),
    @CurrentUsage INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @Limit INT;
    DECLARE @IsWithinLimit BIT = 0;
    
    SELECT @Limit = NumericLimit
    FROM [Core].[TenantFeatures]
    WHERE TenantID = @TenantID 
        AND FeatureName = @FeatureName
        AND IsEnabled = 1;
    
    IF @Limit IS NULL OR @CurrentUsage < @Limit
        SET @IsWithinLimit = 1;
    
    RETURN @IsWithinLimit;
END;
GO

-- =============================================
-- Row-Level Security Policies
-- =============================================

-- Create policy for tenant branding
CREATE SECURITY POLICY [Core].[TenantBrandingPolicy]
ADD FILTER PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[TenantBranding],
ADD BLOCK PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[TenantBranding]
WITH (STATE = ON);
GO

-- Create policy for tenant features
CREATE SECURITY POLICY [Core].[TenantFeaturesPolicy]
ADD FILTER PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[TenantFeatures],
ADD BLOCK PREDICATE [Security].[fn_TenantAccessPredicate](TenantID) ON [Core].[TenantFeatures]
WITH (STATE = ON);
GO

-- =============================================
-- Insert default tenant features
-- =============================================

-- Feature definitions (master list)
CREATE TABLE #TempFeatures
(
    FeatureName NVARCHAR(100),
    Description NVARCHAR(500),
    Category NVARCHAR(50)
);

INSERT INTO #TempFeatures VALUES
    ('MaxUsers', 'Maximum number of users', 'Limits'),
    ('StorageGB', 'Storage quota in gigabytes', 'Limits'),
    ('ResourceAccess', 'Access to therapy resources', 'Core'),
    ('BasicReporting', 'Basic reporting features', 'Analytics'),
    ('AdvancedReporting', 'Advanced analytics and reporting', 'Analytics'),
    ('AIGeneration', 'AI content generation monthly limit', 'AI'),
    ('MarketplaceAccess', 'Access to marketplace', 'Marketplace'),
    ('MarketplaceSelling', 'Ability to sell on marketplace', 'Marketplace'),
    ('WhiteLabel', 'White-label branding', 'Branding'),
    ('CustomDomain', 'Custom domain support', 'Branding'),
    ('APIAccess', 'API access for integrations', 'Integration'),
    ('SSOIntegration', 'Single sign-on integration', 'Integration'),
    ('EHRIntegration', 'EHR system integration', 'Integration'),
    ('LMSIntegration', 'Learning management system integration', 'Integration'),
    ('TeletherapyTools', 'Teletherapy-specific features', 'Features'),
    ('OfflineMode', 'Offline access to resources', 'Features'),
    ('ParentPortal', 'Parent/caregiver portal access', 'Features'),
    ('ClinicalSupervision', 'Clinical supervision tools', 'Features'),
    ('PECSProtocol', 'PECS implementation tools', 'Protocols'),
    ('ABATools', 'ABA data collection tools', 'Protocols'),
    ('BulkOperations', 'Bulk import/export operations', 'Administration'),
    ('CustomRoles', 'Custom role creation', 'Administration'),
    ('AuditLogAccess', 'Access to audit logs', 'Compliance'),
    ('DataExport', 'Full data export capability', 'Compliance');

DROP TABLE #TempFeatures;
GO

PRINT 'Multi-tenant tables and configurations created successfully.';
GO