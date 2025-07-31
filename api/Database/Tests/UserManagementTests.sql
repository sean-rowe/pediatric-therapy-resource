-- tSQLt Tests for User Management Stored Procedures

-- Create test class
EXEC tSQLt.NewTestClass 'UserManagementTests';
GO

-- =============================================
-- Test: sp_GetUserById - Should return user when exists
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_GetUserById returns user when exists]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @TestEmail NVARCHAR(255) = 'test@example.com';
    
    INSERT INTO dbo.Users (
        UserId, Email, FirstName, LastName, PasswordHash,
        SubscriptionTier, Role, IsActive, IsDeleted,
        CreatedAt, UpdatedAt
    )
    VALUES (
        @TestUserId, @TestEmail, 'Test', 'User', 'hash123',
        0, 0, 1, 0,
        GETUTCDATE(), GETUTCDATE()
    );
    
    -- Act
    CREATE TABLE #actual (
        UserId UNIQUEIDENTIFIER,
        Email NVARCHAR(255),
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        IsDeleted BIT
    );
    
    INSERT INTO #actual
    EXEC dbo.sp_GetUserById @UserId = @TestUserId, @IncludeDeleted = 0;
    
    -- Assert
    EXEC tSQLt.AssertEquals @Expected = @TestUserId, @Actual = (SELECT UserId FROM #actual);
    EXEC tSQLt.AssertEquals @Expected = @TestEmail, @Actual = (SELECT Email FROM #actual);
    EXEC tSQLt.AssertEquals @Expected = 'Test', @Actual = (SELECT FirstName FROM #actual);
END;
GO

-- =============================================
-- Test: sp_GetUserById - Should not return deleted user when IncludeDeleted is false
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_GetUserById excludes deleted users by default]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO dbo.Users (
        UserId, Email, FirstName, LastName, PasswordHash,
        SubscriptionTier, Role, IsActive, IsDeleted,
        CreatedAt, UpdatedAt
    )
    VALUES (
        @TestUserId, 'deleted@example.com', 'Deleted', 'User', 'hash123',
        0, 0, 0, 1, -- IsDeleted = 1
        GETUTCDATE(), GETUTCDATE()
    );
    
    -- Act
    CREATE TABLE #actual (
        UserId UNIQUEIDENTIFIER
    );
    
    INSERT INTO #actual
    EXEC dbo.sp_GetUserById @UserId = @TestUserId, @IncludeDeleted = 0;
    
    -- Assert
    EXEC tSQLt.AssertEmptyTable '#actual';
END;
GO

-- =============================================
-- Test: sp_CreateUser - Should create user with default values
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_CreateUser creates user with defaults]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @Email NVARCHAR(255) = 'newuser@example.com';
    DECLARE @PasswordHash NVARCHAR(MAX) = 'hashedpassword123';
    DECLARE @FirstName NVARCHAR(100) = 'New';
    DECLARE @LastName NVARCHAR(100) = 'User';
    DECLARE @OutputUserId UNIQUEIDENTIFIER;
    
    -- Act
    EXEC dbo.sp_CreateUser 
        @Email = @Email,
        @PasswordHash = @PasswordHash,
        @FirstName = @FirstName,
        @LastName = @LastName,
        @UserId = @OutputUserId OUTPUT;
    
    -- Assert
    DECLARE @ActualCount INT = (SELECT COUNT(*) FROM dbo.Users);
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualCount;
    
    DECLARE @ActualEmail NVARCHAR(255) = (SELECT Email FROM dbo.Users WHERE UserId = @OutputUserId);
    EXEC tSQLt.AssertEquals @Expected = @Email, @Actual = @ActualEmail;
    
    DECLARE @ActualSubscriptionTier INT = (SELECT SubscriptionTier FROM dbo.Users WHERE UserId = @OutputUserId);
    EXEC tSQLt.AssertEquals @Expected = 0, @Actual = @ActualSubscriptionTier; -- Free tier
    
    DECLARE @ActualIsActive BIT = (SELECT IsActive FROM dbo.Users WHERE UserId = @OutputUserId);
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualIsActive;
END;
GO

-- =============================================
-- Test: sp_CreateUser - Should fail on duplicate email
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_CreateUser fails on duplicate email]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    -- Add unique constraint for email
    ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
    
    DECLARE @Email NVARCHAR(255) = 'duplicate@example.com';
    DECLARE @OutputUserId UNIQUEIDENTIFIER;
    
    -- Insert first user
    INSERT INTO dbo.Users (UserId, Email, FirstName, LastName, PasswordHash, IsDeleted, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @Email, 'First', 'User', 'hash1', 0, GETUTCDATE(), GETUTCDATE());
    
    -- Act & Assert
    EXEC tSQLt.ExpectException @ExpectedMessagePattern = '%Violation of UNIQUE KEY constraint%';
    
    EXEC dbo.sp_CreateUser 
        @Email = @Email,
        @PasswordHash = 'hash2',
        @FirstName = 'Second',
        @LastName = 'User',
        @UserId = @OutputUserId OUTPUT;
END;
GO

-- =============================================
-- Test: sp_UpdateUser - Should update only provided fields
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_UpdateUser updates only provided fields]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @OriginalFirstName NVARCHAR(100) = 'Original';
    DECLARE @OriginalLastName NVARCHAR(100) = 'Name';
    DECLARE @OriginalTimezone NVARCHAR(50) = 'UTC';
    
    INSERT INTO dbo.Users (
        UserId, Email, FirstName, LastName, Timezone, 
        PasswordHash, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES (
        @TestUserId, 'test@example.com', @OriginalFirstName, @OriginalLastName, @OriginalTimezone,
        'hash123', 0, GETUTCDATE(), DATEADD(day, -1, GETUTCDATE())
    );
    
    -- Act
    EXEC dbo.sp_UpdateUser 
        @UserId = @TestUserId,
        @FirstName = 'Updated',
        @LastName = NULL, -- Should not update
        @Timezone = NULL; -- Should not update
    
    -- Assert
    DECLARE @ActualFirstName NVARCHAR(100) = (SELECT FirstName FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualLastName NVARCHAR(100) = (SELECT LastName FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualTimezone NVARCHAR(50) = (SELECT Timezone FROM dbo.Users WHERE UserId = @TestUserId);
    
    EXEC tSQLt.AssertEquals @Expected = 'Updated', @Actual = @ActualFirstName;
    EXEC tSQLt.AssertEquals @Expected = @OriginalLastName, @Actual = @ActualLastName;
    EXEC tSQLt.AssertEquals @Expected = @OriginalTimezone, @Actual = @ActualTimezone;
END;
GO

-- =============================================
-- Test: sp_SoftDeleteUser - Should anonymize user data
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_SoftDeleteUser anonymizes user data]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    EXEC tSQLt.FakeTable 'dbo.AuditLogs';
    
    DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @OriginalEmail NVARCHAR(255) = 'user@example.com';
    
    INSERT INTO dbo.Users (
        UserId, Email, FirstName, LastName, LicenseNumber,
        PasswordHash, IsDeleted, IsActive, CreatedAt, UpdatedAt
    )
    VALUES (
        @TestUserId, @OriginalEmail, 'John', 'Doe', 'LIC123',
        'hash123', 0, 1, GETUTCDATE(), GETUTCDATE()
    );
    
    -- Act
    EXEC dbo.sp_SoftDeleteUser @UserId = @TestUserId, @DeletedByUserId = NULL;
    
    -- Assert
    DECLARE @ActualIsDeleted BIT = (SELECT IsDeleted FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualIsActive BIT = (SELECT IsActive FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualEmail NVARCHAR(255) = (SELECT Email FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualFirstName NVARCHAR(100) = (SELECT FirstName FROM dbo.Users WHERE UserId = @TestUserId);
    DECLARE @ActualLicense NVARCHAR(100) = (SELECT LicenseNumber FROM dbo.Users WHERE UserId = @TestUserId);
    
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualIsDeleted;
    EXEC tSQLt.AssertEquals @Expected = 0, @Actual = @ActualIsActive;
    EXEC tSQLt.AssertEquals @Expected = 'DELETED', @Actual = @ActualFirstName;
    EXEC tSQLt.AssertEquals @Expected = 'DELETED', @Actual = @ActualLicense;
    EXEC tSQLt.AssertNotEquals @Expected = @OriginalEmail, @Actual = @ActualEmail;
    EXEC tSQLt.AssertLike @ExpectedPattern = 'deleted_%@deleted.com', @Actual = @ActualEmail;
    
    -- Check audit log
    DECLARE @AuditCount INT = (SELECT COUNT(*) FROM dbo.AuditLogs WHERE UserId = @TestUserId);
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @AuditCount;
END;
GO

-- =============================================
-- Test: sp_GetUsersByFilter - Should filter by search term
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_GetUsersByFilter filters by search term]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    -- Insert test data
    INSERT INTO dbo.Users (UserId, Email, FirstName, LastName, PasswordHash, IsDeleted, CreatedAt, UpdatedAt)
    VALUES 
        (NEWID(), 'john.doe@example.com', 'John', 'Doe', 'hash1', 0, GETUTCDATE(), GETUTCDATE()),
        (NEWID(), 'jane.smith@example.com', 'Jane', 'Smith', 'hash2', 0, GETUTCDATE(), GETUTCDATE()),
        (NEWID(), 'bob.johnson@example.com', 'Bob', 'Johnson', 'hash3', 0, GETUTCDATE(), GETUTCDATE()),
        (NEWID(), 'deleted@example.com', 'Deleted', 'User', 'hash4', 1, GETUTCDATE(), GETUTCDATE()); -- Deleted user
    
    -- Act
    DECLARE @TotalCount INT;
    CREATE TABLE #results (
        UserId UNIQUEIDENTIFIER,
        Email NVARCHAR(255),
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        RowNum BIGINT
    );
    
    INSERT INTO #results
    EXEC dbo.sp_GetUsersByFilter 
        @SearchTerm = 'john',
        @PageNumber = 1,
        @PageSize = 10,
        @TotalCount = @TotalCount OUTPUT;
    
    -- Assert
    DECLARE @ActualCount INT = (SELECT COUNT(*) FROM #results);
    EXEC tSQLt.AssertEquals @Expected = 2, @Actual = @ActualCount; -- john.doe and bob.johnson
    EXEC tSQLt.AssertEquals @Expected = 2, @Actual = @TotalCount;
END;
GO

-- =============================================
-- Test: sp_VerifyUserLicense - Should update license information
-- =============================================
CREATE OR ALTER PROCEDURE UserManagementTests.[test sp_VerifyUserLicense updates license info]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @TestUserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @LicenseNumber NVARCHAR(100) = 'OT-12345';
    DECLARE @LicenseState NVARCHAR(2) = 'CA';
    DECLARE @LicenseType NVARCHAR(10) = 'OT';
    DECLARE @ExpirationDate DATETIME2 = DATEADD(year, 2, GETUTCDATE());
    
    INSERT INTO dbo.Users (
        UserId, Email, FirstName, LastName, PasswordHash,
        LicenseVerified, IsDeleted, CreatedAt, UpdatedAt
    )
    VALUES (
        @TestUserId, 'therapist@example.com', 'Test', 'Therapist', 'hash123',
        0, 0, GETUTCDATE(), GETUTCDATE()
    );
    
    -- Act
    EXEC dbo.sp_VerifyUserLicense 
        @UserId = @TestUserId,
        @LicenseNumber = @LicenseNumber,
        @LicenseState = @LicenseState,
        @LicenseType = @LicenseType,
        @ExpirationDate = @ExpirationDate;
    
    -- Assert
    SELECT 
        LicenseNumber,
        LicenseState,
        LicenseType,
        LicenseVerified,
        LicenseVerifiedAt,
        LicenseExpirationDate
    INTO #actual
    FROM dbo.Users 
    WHERE UserId = @TestUserId;
    
    DECLARE @ActualLicenseNumber NVARCHAR(100) = (SELECT LicenseNumber FROM #actual);
    DECLARE @ActualLicenseState NVARCHAR(2) = (SELECT LicenseState FROM #actual);
    DECLARE @ActualLicenseType NVARCHAR(10) = (SELECT LicenseType FROM #actual);
    DECLARE @ActualVerified BIT = (SELECT LicenseVerified FROM #actual);
    
    EXEC tSQLt.AssertEquals @Expected = @LicenseNumber, @Actual = @ActualLicenseNumber;
    EXEC tSQLt.AssertEquals @Expected = @LicenseState, @Actual = @ActualLicenseState;
    EXEC tSQLt.AssertEquals @Expected = @LicenseType, @Actual = @ActualLicenseType;
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualVerified;
END;
GO

-- =============================================
-- Run all tests
-- =============================================
EXEC tSQLt.Run 'UserManagementTests';
GO