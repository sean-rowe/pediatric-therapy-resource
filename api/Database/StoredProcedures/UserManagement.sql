-- User Management Stored Procedures

-- =============================================
-- sp_GetUserById
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserById]
    @UserId UNIQUEIDENTIFIER,
    @IncludeDeleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Email,
        u.FirstName,
        u.LastName,
        u.LicenseNumber,
        u.LicenseState,
        u.LicenseType,
        u.LicenseVerified,
        u.LicenseVerifiedAt,
        u.LicenseExpirationDate,
        u.PasswordHash,
        u.SubscriptionTier,
        u.SubscriptionStatus,
        u.SubscriptionStartDate,
        u.SubscriptionEndDate,
        u.OrganizationId,
        u.IsSellerApproved,
        u.Languages,
        u.Specialties,
        u.Role,
        u.CreatedAt,
        u.UpdatedAt,
        u.LastLoginAt,
        u.IsActive,
        u.IsDeleted,
        u.DeletedAt,
        u.EmailVerified,
        u.TwoFactorEnabled,
        u.PreferredLanguage,
        u.ProfileImageUrl,
        u.RefreshToken,
        u.RefreshTokenExpiresAt,
        u.Timezone,
        u.EmailNotificationsEnabled,
        u.Theme,
        u.DefaultView
    FROM Users u
    WHERE u.UserId = @UserId
        AND (@IncludeDeleted = 1 OR u.IsDeleted = 0);
END
GO

-- =============================================
-- sp_CreateUser
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateUser]
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(MAX),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @LicenseNumber NVARCHAR(100) = NULL,
    @LicenseState NVARCHAR(2) = 'CA',
    @LicenseType NVARCHAR(10) = NULL,
    @Languages NVARCHAR(MAX) = '["English"]',
    @Specialties NVARCHAR(MAX) = '[]',
    @UserId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @UserId = NEWID();
        
        INSERT INTO Users (
            UserId, Email, PasswordHash, FirstName, LastName,
            LicenseNumber, LicenseState, LicenseType,
            Languages, Specialties,
            SubscriptionTier, SubscriptionStatus,
            Role, IsActive, IsDeleted, 
            EmailVerified, TwoFactorEnabled,
            PreferredLanguage, EmailNotificationsEnabled,
            CreatedAt, UpdatedAt
        )
        VALUES (
            @UserId, @Email, @PasswordHash, @FirstName, @LastName,
            @LicenseNumber, @LicenseState, @LicenseType,
            @Languages, @Specialties,
            0, -- Free tier
            0, -- Active status
            0, -- Therapist role
            1, 0, -- IsActive = true, IsDeleted = false
            0, 0, -- EmailVerified = false, TwoFactorEnabled = false
            'en', 1, -- PreferredLanguage = 'en', EmailNotificationsEnabled = true
            GETUTCDATE(), GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- sp_UpdateUser
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUser]
    @UserId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @LicenseNumber NVARCHAR(100) = NULL,
    @LicenseState NVARCHAR(2) = NULL,
    @LicenseType NVARCHAR(10) = NULL,
    @Languages NVARCHAR(MAX) = NULL,
    @Specialties NVARCHAR(MAX) = NULL,
    @PreferredLanguage NVARCHAR(10) = NULL,
    @Timezone NVARCHAR(50) = NULL,
    @EmailNotificationsEnabled BIT = NULL,
    @Theme NVARCHAR(20) = NULL,
    @DefaultView NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        FirstName = ISNULL(@FirstName, FirstName),
        LastName = ISNULL(@LastName, LastName),
        LicenseNumber = ISNULL(@LicenseNumber, LicenseNumber),
        LicenseState = ISNULL(@LicenseState, LicenseState),
        LicenseType = ISNULL(@LicenseType, LicenseType),
        Languages = ISNULL(@Languages, Languages),
        Specialties = ISNULL(@Specialties, Specialties),
        PreferredLanguage = ISNULL(@PreferredLanguage, PreferredLanguage),
        Timezone = ISNULL(@Timezone, Timezone),
        EmailNotificationsEnabled = ISNULL(@EmailNotificationsEnabled, EmailNotificationsEnabled),
        Theme = ISNULL(@Theme, Theme),
        DefaultView = ISNULL(@DefaultView, DefaultView),
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_SoftDeleteUser
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SoftDeleteUser]
    @UserId UNIQUEIDENTIFIER,
    @DeletedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Soft delete user
        UPDATE Users
        SET 
            IsDeleted = 1,
            DeletedAt = GETUTCDATE(),
            IsActive = 0,
            UpdatedAt = GETUTCDATE()
        WHERE UserId = @UserId;
        
        -- Anonymize PII for GDPR compliance
        UPDATE Users
        SET
            Email = CONCAT('deleted_', CONVERT(NVARCHAR(36), UserId), '@deleted.com'),
            FirstName = 'DELETED',
            LastName = 'USER',
            LicenseNumber = 'DELETED',
            LicenseState = 'XX',
            PasswordHash = 'DELETED',
            RefreshToken = NULL,
            RefreshTokenExpiresAt = NULL,
            ProfileImageUrl = NULL
        WHERE UserId = @UserId;
        
        -- Create audit log entry
        INSERT INTO AuditLogs (
            AuditLogId,
            UserId,
            EntityType,
            EntityId,
            Action,
            Changes,
            PerformedByUserId,
            CreatedAt
        )
        VALUES (
            NEWID(),
            @UserId,
            'User',
            @UserId,
            'Delete',
            '{"action": "soft_delete", "anonymized": true}',
            @DeletedByUserId,
            GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- sp_GetUsersByFilter
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUsersByFilter]
    @SearchTerm NVARCHAR(255) = NULL,
    @LicenseType NVARCHAR(10) = NULL,
    @IsVerified BIT = NULL,
    @SubscriptionTier INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Users u
    WHERE u.IsDeleted = 0
        AND (@SearchTerm IS NULL OR 
             u.Email LIKE '%' + @SearchTerm + '%' OR 
             u.FirstName LIKE '%' + @SearchTerm + '%' OR 
             u.LastName LIKE '%' + @SearchTerm + '%')
        AND (@LicenseType IS NULL OR u.LicenseType = @LicenseType)
        AND (@IsVerified IS NULL OR u.EmailVerified = @IsVerified)
        AND (@SubscriptionTier IS NULL OR u.SubscriptionTier = @SubscriptionTier);
    
    -- Get paginated results
    WITH UserCTE AS (
        SELECT 
            u.*,
            ROW_NUMBER() OVER (ORDER BY u.CreatedAt DESC) AS RowNum
        FROM Users u
        WHERE u.IsDeleted = 0
            AND (@SearchTerm IS NULL OR 
                 u.Email LIKE '%' + @SearchTerm + '%' OR 
                 u.FirstName LIKE '%' + @SearchTerm + '%' OR 
                 u.LastName LIKE '%' + @SearchTerm + '%')
            AND (@LicenseType IS NULL OR u.LicenseType = @LicenseType)
            AND (@IsVerified IS NULL OR u.EmailVerified = @IsVerified)
            AND (@SubscriptionTier IS NULL OR u.SubscriptionTier = @SubscriptionTier)
    )
    SELECT *
    FROM UserCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
GO

-- =============================================
-- sp_UpdateUserStatus
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserStatus]
    @UserId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @UpdatedByUserId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Update user status
        UPDATE Users
        SET 
            IsActive = @IsActive,
            UpdatedAt = GETUTCDATE()
        WHERE UserId = @UserId
            AND IsDeleted = 0;
        
        -- Create audit log entry
        INSERT INTO AuditLogs (
            AuditLogId,
            UserId,
            EntityType,
            EntityId,
            Action,
            Changes,
            PerformedByUserId,
            CreatedAt
        )
        VALUES (
            NEWID(),
            @UserId,
            'User',
            @UserId,
            CASE WHEN @IsActive = 1 THEN 'Activate' ELSE 'Suspend' END,
            JSON_MODIFY('{}', '$.reason', @Reason),
            @UpdatedByUserId,
            GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- sp_VerifyUserLicense
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_VerifyUserLicense]
    @UserId UNIQUEIDENTIFIER,
    @LicenseNumber NVARCHAR(100),
    @LicenseState NVARCHAR(2),
    @LicenseType NVARCHAR(10),
    @ExpirationDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        LicenseNumber = @LicenseNumber,
        LicenseState = @LicenseState,
        LicenseType = @LicenseType,
        LicenseVerified = 1,
        LicenseVerifiedAt = GETUTCDATE(),
        LicenseExpirationDate = @ExpirationDate,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_UpdateUserPreferences
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserPreferences]
    @UserId UNIQUEIDENTIFIER,
    @PreferredLanguage NVARCHAR(10) = NULL,
    @Timezone NVARCHAR(50) = NULL,
    @EmailNotificationsEnabled BIT = NULL,
    @Theme NVARCHAR(20) = NULL,
    @DefaultView NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        PreferredLanguage = ISNULL(@PreferredLanguage, PreferredLanguage),
        Timezone = ISNULL(@Timezone, Timezone),
        EmailNotificationsEnabled = ISNULL(@EmailNotificationsEnabled, EmailNotificationsEnabled),
        Theme = ISNULL(@Theme, Theme),
        DefaultView = ISNULL(@DefaultView, DefaultView),
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_GetUserWithOrganization
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserWithOrganization]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.*,
        o.OrganizationId AS Org_OrganizationId,
        o.Name AS Org_Name,
        o.Type AS Org_Type,
        o.SubscriptionTier AS Org_SubscriptionTier,
        o.LicenseCount AS Org_LicenseCount,
        o.UsedLicenses AS Org_UsedLicenses,
        o.SsoEnabled AS Org_SsoEnabled,
        o.SsoProvider AS Org_SsoProvider,
        o.IsActive AS Org_IsActive
    FROM Users u
    LEFT JOIN Organizations o ON u.OrganizationId = o.OrganizationId
    WHERE u.UserId = @UserId
        AND u.IsDeleted = 0;
END
GO

-- =============================================
-- sp_GetUsersByOrganization
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUsersByOrganization]
    @OrganizationId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Users u
    WHERE u.OrganizationId = @OrganizationId
        AND u.IsDeleted = 0
        AND (@IncludeInactive = 1 OR u.IsActive = 1);
    
    -- Return paginated results
    SELECT 
        u.UserId,
        u.Email,
        u.FirstName,
        u.LastName,
        u.LicenseNumber,
        u.LicenseState,
        u.LicenseType,
        u.LicenseVerified,
        u.LicenseVerifiedAt,
        u.LicenseExpirationDate,
        u.PasswordHash,
        u.SubscriptionTier,
        u.SubscriptionStatus,
        u.SubscriptionStartDate,
        u.SubscriptionEndDate,
        u.OrganizationId,
        u.IsSellerApproved,
        u.Languages,
        u.Specialties,
        u.Role,
        u.CreatedAt,
        u.UpdatedAt,
        u.LastLoginAt,
        u.IsActive,
        u.IsDeleted,
        u.DeletedAt,
        u.EmailVerified,
        u.TwoFactorEnabled,
        u.PreferredLanguage,
        u.ProfileImageUrl,
        u.RefreshToken,
        u.RefreshTokenExpiresAt,
        u.Timezone,
        u.EmailNotificationsEnabled,
        u.Theme,
        u.DefaultView
    FROM Users u
    WHERE u.OrganizationId = @OrganizationId
        AND u.IsDeleted = 0
        AND (@IncludeInactive = 1 OR u.IsActive = 1)
    ORDER BY u.LastName, u.FirstName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- sp_GetSellers
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSellers]
    @ApprovedOnly BIT = 1,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Users u
    INNER JOIN SellerProfiles sp ON u.UserId = sp.UserId
    WHERE u.IsDeleted = 0
        AND u.IsActive = 1
        AND (@ApprovedOnly = 0 OR u.IsSellerApproved = 1);
    
    -- Return paginated results
    SELECT 
        u.UserId,
        u.Email,
        u.FirstName,
        u.LastName,
        u.LicenseNumber,
        u.LicenseState,
        u.LicenseType,
        u.LicenseVerified,
        u.LicenseVerifiedAt,
        u.LicenseExpirationDate,
        u.PasswordHash,
        u.SubscriptionTier,
        u.SubscriptionStatus,
        u.SubscriptionStartDate,
        u.SubscriptionEndDate,
        u.OrganizationId,
        u.IsSellerApproved,
        u.Languages,
        u.Specialties,
        u.Role,
        u.CreatedAt,
        u.UpdatedAt,
        u.LastLoginAt,
        u.IsActive,
        u.IsDeleted,
        u.DeletedAt,
        u.EmailVerified,
        u.TwoFactorEnabled,
        u.PreferredLanguage,
        u.ProfileImageUrl,
        u.RefreshToken,
        u.RefreshTokenExpiresAt,
        u.Timezone,
        u.EmailNotificationsEnabled,
        u.Theme,
        u.DefaultView
    FROM Users u
    INNER JOIN SellerProfiles sp ON u.UserId = sp.UserId
    WHERE u.IsDeleted = 0
        AND u.IsActive = 1
        AND (@ApprovedOnly = 0 OR u.IsSellerApproved = 1)
    ORDER BY sp.Rating DESC, sp.TotalSales DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO