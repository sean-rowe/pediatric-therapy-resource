-- UPTRMS Database Creation Script with HIPAA Compliance Features
-- This script creates the database with encryption and security features enabled

USE master;
GO

-- Drop database if exists (for development only)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'UPTRMS')
BEGIN
    ALTER DATABASE UPTRMS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE UPTRMS;
END
GO

-- Create database with specific file locations and growth settings
CREATE DATABASE UPTRMS
ON PRIMARY 
(
    NAME = 'UPTRMS_Primary',
    FILENAME = '/var/opt/mssql/data/UPTRMS_Primary.mdf',
    SIZE = 1GB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 256MB
),
FILEGROUP UPTRMS_Data DEFAULT
(
    NAME = 'UPTRMS_Data',
    FILENAME = '/var/opt/mssql/data/UPTRMS_Data.ndf',
    SIZE = 5GB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 512MB
),
FILEGROUP UPTRMS_Indexes
(
    NAME = 'UPTRMS_Indexes',
    FILENAME = '/var/opt/mssql/data/UPTRMS_Indexes.ndf',
    SIZE = 2GB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 256MB
),
FILEGROUP UPTRMS_Archive
(
    NAME = 'UPTRMS_Archive',
    FILENAME = '/var/opt/mssql/data/UPTRMS_Archive.ndf',
    SIZE = 10GB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 1GB
)
LOG ON 
(
    NAME = 'UPTRMS_Log',
    FILENAME = '/var/opt/mssql/log/UPTRMS_Log.ldf',
    SIZE = 2GB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 256MB
);
GO

-- Set database options for HIPAA compliance
ALTER DATABASE UPTRMS SET RECOVERY FULL;
ALTER DATABASE UPTRMS SET PAGE_VERIFY CHECKSUM;
ALTER DATABASE UPTRMS SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE UPTRMS SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE UPTRMS SET CHANGE_TRACKING = ON (RETENTION_PERIOD = 7 DAYS, AUTO_CLEANUP = ON);
ALTER DATABASE UPTRMS SET QUERY_STORE = ON;
GO

-- Configure Query Store for performance monitoring
ALTER DATABASE UPTRMS SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    MAX_STORAGE_SIZE_MB = 1024,
    INTERVAL_LENGTH_MINUTES = 60,
    SIZE_BASED_CLEANUP_MODE = AUTO,
    QUERY_CAPTURE_MODE = ALL,
    MAX_PLANS_PER_QUERY = 200
);
GO

USE UPTRMS;
GO

-- Create master key for encryption
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'UPTRMS_M@st3r_K3y_2025!#Secure';
GO

-- Create certificate for TDE
CREATE CERTIFICATE UPTRMS_TDE_Certificate
WITH SUBJECT = 'UPTRMS TDE Certificate',
EXPIRY_DATE = '2035-12-31';
GO

-- Backup certificate immediately (critical for recovery)
BACKUP CERTIFICATE UPTRMS_TDE_Certificate
TO FILE = '/var/opt/mssql/backup/UPTRMS_TDE_Certificate.cer'
WITH PRIVATE KEY (
    FILE = '/var/opt/mssql/backup/UPTRMS_TDE_Certificate.pvk',
    ENCRYPTION BY PASSWORD = 'UPTRMS_C3rt_B@ckup_2025!#Secure'
);
GO

-- Create database encryption key
CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE UPTRMS_TDE_Certificate;
GO

-- Enable Transparent Data Encryption
ALTER DATABASE UPTRMS SET ENCRYPTION ON;
GO

-- Create schemas for logical separation
CREATE SCHEMA [Core] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Security] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Therapy] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Marketplace] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Integration] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Analytics] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Audit] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Staging] AUTHORIZATION [dbo];
GO
CREATE SCHEMA [Archive] AUTHORIZATION [dbo];
GO

-- Create audit specification database
CREATE DATABASE AUDIT SPECIFICATION UPTRMS_Database_Audit_Spec
FOR SERVER AUDIT UPTRMS_Server_Audit
ADD (SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Core] BY [public]),
ADD (SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Security] BY [public]),
ADD (SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Therapy] BY [public]),
ADD (SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Marketplace] BY [public]),
ADD (EXECUTE ON SCHEMA::[dbo] BY [public])
WITH (STATE = ON);
GO

-- Create service broker for async operations
ALTER DATABASE UPTRMS SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
GO

-- Create message types for service broker
CREATE MESSAGE TYPE [//UPTRMS/Audit/Request] VALIDATION = WELL_FORMED_XML;
CREATE MESSAGE TYPE [//UPTRMS/Audit/Response] VALIDATION = WELL_FORMED_XML;
CREATE MESSAGE TYPE [//UPTRMS/Email/Request] VALIDATION = WELL_FORMED_XML;
CREATE MESSAGE TYPE [//UPTRMS/Email/Response] VALIDATION = WELL_FORMED_XML;
GO

-- Create contracts
CREATE CONTRACT [//UPTRMS/Audit/Contract]
(
    [//UPTRMS/Audit/Request] SENT BY INITIATOR,
    [//UPTRMS/Audit/Response] SENT BY TARGET
);

CREATE CONTRACT [//UPTRMS/Email/Contract]
(
    [//UPTRMS/Email/Request] SENT BY INITIATOR,
    [//UPTRMS/Email/Response] SENT BY TARGET
);
GO

-- Create queues
CREATE QUEUE [Audit].[AuditQueue]
WITH STATUS = ON,
RETENTION = OFF,
ACTIVATION (
    STATUS = ON,
    PROCEDURE_NAME = [Audit].[ProcessAuditQueue],
    MAX_QUEUE_READERS = 5,
    EXECUTE AS OWNER
);

CREATE QUEUE [Core].[EmailQueue]
WITH STATUS = ON,
RETENTION = OFF;
GO

-- Create services
CREATE SERVICE [//UPTRMS/Audit/Service]
ON QUEUE [Audit].[AuditQueue]
([//UPTRMS/Audit/Contract]);

CREATE SERVICE [//UPTRMS/Email/Service]
ON QUEUE [Core].[EmailQueue]
([//UPTRMS/Email/Contract]);
GO

-- Create database roles for security
CREATE ROLE [TherapistRole];
CREATE ROLE [AdminRole];
CREATE ROLE [MarketplaceSellerRole];
CREATE ROLE [IntegrationRole];
CREATE ROLE [ReportingRole];
CREATE ROLE [AuditorRole];
GO

-- Grant permissions to roles
-- TherapistRole: Basic user access
GRANT SELECT, INSERT, UPDATE ON SCHEMA::[Therapy] TO [TherapistRole];
GRANT SELECT ON SCHEMA::[Core] TO [TherapistRole];
GRANT EXECUTE ON SCHEMA::[Therapy] TO [TherapistRole];

-- AdminRole: Full administrative access
GRANT CONTROL ON SCHEMA::[Core] TO [AdminRole];
GRANT CONTROL ON SCHEMA::[Security] TO [AdminRole];
GRANT CONTROL ON SCHEMA::[Therapy] TO [AdminRole];
GRANT CONTROL ON SCHEMA::[Marketplace] TO [AdminRole];
GRANT VIEW ANY DEFINITION TO [AdminRole];

-- MarketplaceSellerRole: Marketplace operations
GRANT SELECT, INSERT, UPDATE ON SCHEMA::[Marketplace] TO [MarketplaceSellerRole];
GRANT SELECT ON SCHEMA::[Core] TO [MarketplaceSellerRole];
GRANT EXECUTE ON SCHEMA::[Marketplace] TO [MarketplaceSellerRole];

-- IntegrationRole: External system access
GRANT SELECT, INSERT, UPDATE ON SCHEMA::[Integration] TO [IntegrationRole];
GRANT EXECUTE ON SCHEMA::[Integration] TO [IntegrationRole];

-- ReportingRole: Read-only analytics access
GRANT SELECT ON SCHEMA::[Core] TO [ReportingRole];
GRANT SELECT ON SCHEMA::[Therapy] TO [ReportingRole];
GRANT SELECT ON SCHEMA::[Marketplace] TO [ReportingRole];
GRANT SELECT ON SCHEMA::[Analytics] TO [ReportingRole];

-- AuditorRole: Audit and compliance access
GRANT SELECT ON SCHEMA::[Audit] TO [AuditorRole];
GRANT VIEW ANY DEFINITION TO [AuditorRole];
GRANT VIEW SERVER STATE TO [AuditorRole];
GO

-- Create application users
CREATE USER [UPTRMS_API_User] WITHOUT LOGIN;
CREATE USER [UPTRMS_Integration_User] WITHOUT LOGIN;
CREATE USER [UPTRMS_Reporting_User] WITHOUT LOGIN;
CREATE USER [UPTRMS_Audit_User] WITHOUT LOGIN;
GO

-- Assign users to roles
ALTER ROLE [TherapistRole] ADD MEMBER [UPTRMS_API_User];
ALTER ROLE [AdminRole] ADD MEMBER [UPTRMS_API_User];
ALTER ROLE [IntegrationRole] ADD MEMBER [UPTRMS_Integration_User];
ALTER ROLE [ReportingRole] ADD MEMBER [UPTRMS_Reporting_User];
ALTER ROLE [AuditorRole] ADD MEMBER [UPTRMS_Audit_User];
GO

-- Create symmetric keys for column encryption
CREATE SYMMETRIC KEY PHI_SymmetricKey
WITH ALGORITHM = AES_256
ENCRYPTION BY CERTIFICATE UPTRMS_TDE_Certificate;
GO

-- Create functions for encryption/decryption
CREATE FUNCTION [Security].[EncryptPHI]
(
    @PlainText NVARCHAR(MAX)
)
RETURNS VARBINARY(MAX)
WITH ENCRYPTION
AS
BEGIN
    DECLARE @EncryptedData VARBINARY(MAX);
    
    OPEN SYMMETRIC KEY PHI_SymmetricKey
    DECRYPTION BY CERTIFICATE UPTRMS_TDE_Certificate;
    
    SET @EncryptedData = ENCRYPTBYKEY(KEY_GUID('PHI_SymmetricKey'), @PlainText);
    
    CLOSE SYMMETRIC KEY PHI_SymmetricKey;
    
    RETURN @EncryptedData;
END;
GO

CREATE FUNCTION [Security].[DecryptPHI]
(
    @EncryptedData VARBINARY(MAX)
)
RETURNS NVARCHAR(MAX)
WITH ENCRYPTION
AS
BEGIN
    DECLARE @PlainText NVARCHAR(MAX);
    
    OPEN SYMMETRIC KEY PHI_SymmetricKey
    DECRYPTION BY CERTIFICATE UPTRMS_TDE_Certificate;
    
    SET @PlainText = CONVERT(NVARCHAR(MAX), DECRYPTBYKEY(@EncryptedData));
    
    CLOSE SYMMETRIC KEY PHI_SymmetricKey;
    
    RETURN @PlainText;
END;
GO

-- Enable CDC for audit tables (will be created in next script)
EXEC sys.sp_cdc_enable_db;
GO

PRINT 'UPTRMS database created successfully with HIPAA compliance features enabled.';
PRINT 'TDE Status: ' + (SELECT CASE encryption_state 
    WHEN 0 THEN 'No encryption'
    WHEN 1 THEN 'Unencrypted'
    WHEN 2 THEN 'Encryption in progress'
    WHEN 3 THEN 'Encrypted'
    WHEN 4 THEN 'Key change in progress'
    WHEN 5 THEN 'Decryption in progress'
    WHEN 6 THEN 'Protection change in progress'
    ELSE 'Unknown'
    END
FROM sys.dm_database_encryption_keys
WHERE database_id = DB_ID('UPTRMS'));
GO