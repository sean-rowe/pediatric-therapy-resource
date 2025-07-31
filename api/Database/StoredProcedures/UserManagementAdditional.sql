-- Additional User Management Stored Procedures

-- =============================================
-- sp_GetUsersByOrganization
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUsersByOrganization]
    @OrganizationId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
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
    WHERE u.OrganizationId = @OrganizationId
        AND u.IsDeleted = 0
        AND (@IncludeInactive = 1 OR u.IsActive = 1)
    ORDER BY u.LastName, u.FirstName;
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
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Users u
    INNER JOIN SellerProfiles sp ON u.UserId = sp.UserId
    WHERE u.IsDeleted = 0
        AND (@ApprovedOnly = 0 OR u.IsSellerApproved = 1);
    
    -- Get paginated results
    WITH SellerCTE AS (
        SELECT 
            u.*,
            sp.SellerProfileId AS SP_SellerProfileId,
            sp.StoreName AS SP_StoreName,
            sp.StoreUrl AS SP_StoreUrl,
            sp.Bio AS SP_Bio,
            sp.Rating AS SP_Rating,
            sp.TotalSales AS SP_TotalSales,
            sp.CommissionRate AS SP_CommissionRate,
            ROW_NUMBER() OVER (ORDER BY sp.TotalSales DESC) AS RowNum
        FROM Users u
        INNER JOIN SellerProfiles sp ON u.UserId = sp.UserId
        WHERE u.IsDeleted = 0
            AND (@ApprovedOnly = 0 OR u.IsSellerApproved = 1)
    )
    SELECT *
    FROM SellerCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
GO

-- =============================================
-- sp_UpdateUserRefreshToken
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserRefreshToken]
    @UserId UNIQUEIDENTIFIER,
    @RefreshToken NVARCHAR(500),
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        RefreshToken = @RefreshToken,
        RefreshTokenExpiresAt = @ExpiresAt,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_UpdateUserLastLogin
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserLastLogin]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        LastLoginAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_UpdateUserSubscription
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserSubscription]
    @UserId UNIQUEIDENTIFIER,
    @SubscriptionTier INT,
    @SubscriptionStatus INT,
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        SubscriptionTier = @SubscriptionTier,
        SubscriptionStatus = @SubscriptionStatus,
        SubscriptionStartDate = @StartDate,
        SubscriptionEndDate = @EndDate,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_VerifyUserEmail
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_VerifyUserEmail]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        EmailVerified = 1,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_UpdateUserTwoFactor
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserTwoFactor]
    @UserId UNIQUEIDENTIFIER,
    @Enabled BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        TwoFactorEnabled = @Enabled,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_UpdateUserPassword
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUserPassword]
    @UserId UNIQUEIDENTIFIER,
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        PasswordHash = @PasswordHash,
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_ApproveSeller
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_ApproveSeller]
    @UserId UNIQUEIDENTIFIER,
    @ApprovedByUserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Update user as approved seller
        UPDATE Users
        SET 
            IsSellerApproved = 1,
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
            'ApproveSeller',
            '{"status": "approved"}',
            @ApprovedByUserId,
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
-- sp_GetUserNotificationSettings
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserNotificationSettings]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- For now, return basic settings from User table
    -- In production, this would query a separate NotificationSettings table
    SELECT 
        u.EmailNotificationsEnabled,
        u.Timezone,
        u.PreferredLanguage,
        -- Mock additional settings
        CAST(1 AS BIT) AS PushNotificationsEnabled,
        CAST(0 AS BIT) AS SmsNotificationsEnabled,
        '22:00-07:00' AS QuietHours
    FROM Users u
    WHERE u.UserId = @UserId
        AND u.IsDeleted = 0;
END
GO