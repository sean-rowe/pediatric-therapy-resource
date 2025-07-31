-- Additional Resource Management Stored Procedures

-- =============================================
-- sp_GetResourcesByCategory
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourcesByCategory]
    @CategoryId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Resources r
    INNER JOIN ResourceCategories rc ON r.ResourceId = rc.ResourceId
    WHERE rc.CategoryId = @CategoryId
        AND r.IsDeleted = 0;
    
    -- Get paginated results
    WITH ResourceCTE AS (
        SELECT 
            r.*,
            ROW_NUMBER() OVER (ORDER BY r.CreatedAt DESC) AS RowNum
        FROM Resources r
        INNER JOIN ResourceCategories rc ON r.ResourceId = rc.ResourceId
        WHERE rc.CategoryId = @CategoryId
            AND r.IsDeleted = 0
    )
    SELECT * FROM ResourceCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
GO

-- =============================================
-- sp_GetResourceWithDetails
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourceWithDetails]
    @ResourceId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get resource
    SELECT 
        r.*
    FROM Resources r
    WHERE r.ResourceId = @ResourceId
        AND r.IsDeleted = 0;
    
    -- Get categories
    SELECT 
        c.CategoryId,
        c.Name,
        c.Description
    FROM Categories c
    INNER JOIN ResourceCategories rc ON c.CategoryId = rc.CategoryId
    WHERE rc.ResourceId = @ResourceId;
    
    -- Get ratings
    SELECT 
        rr.RatingId,
        rr.UserId,
        rr.Rating,
        rr.Review,
        rr.CreatedAt,
        u.FirstName + ' ' + u.LastName AS ReviewerName
    FROM ResourceRatings rr
    INNER JOIN Users u ON rr.UserId = u.UserId
    WHERE rr.ResourceId = @ResourceId
    ORDER BY rr.CreatedAt DESC;
    
    -- Get download count by date (last 30 days)
    SELECT 
        CAST(DownloadedAt AS DATE) AS DownloadDate,
        COUNT(*) AS DownloadCount
    FROM ResourceDownloads
    WHERE ResourceId = @ResourceId
        AND DownloadedAt >= DATEADD(day, -30, GETUTCDATE())
    GROUP BY CAST(DownloadedAt AS DATE)
    ORDER BY DownloadDate;
END
GO

-- =============================================
-- sp_IncrementResourceViewCount
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IncrementResourceViewCount]
    @ResourceId UNIQUEIDENTIFIER,
    @ViewedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Update view count on resource
        UPDATE Resources
        SET ViewCount = ISNULL(ViewCount, 0) + 1
        WHERE ResourceId = @ResourceId;
        
        -- Log the view
        IF @ViewedByUserId IS NOT NULL
        BEGIN
            INSERT INTO ResourceViews (
                ViewId,
                ResourceId,
                UserId,
                ViewedAt
            )
            VALUES (
                NEWID(),
                @ResourceId,
                @ViewedByUserId,
                GETUTCDATE()
            );
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- sp_GetResourceDownloadCounts
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourceDownloadCounts]
    @ResourceIds NVARCHAR(MAX) -- JSON array of GUIDs
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Parse JSON array of resource IDs
    DECLARE @ResourceIdTable TABLE (ResourceId UNIQUEIDENTIFIER);
    
    INSERT INTO @ResourceIdTable (ResourceId)
    SELECT value
    FROM OPENJSON(@ResourceIds);
    
    -- Get download counts
    SELECT 
        r.ResourceId,
        r.DownloadCount
    FROM Resources r
    WHERE r.ResourceId IN (SELECT ResourceId FROM @ResourceIdTable);
END
GO

-- =============================================
-- sp_AssignResourceReviewer
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_AssignResourceReviewer]
    @ResourceId UNIQUEIDENTIFIER,
    @ReviewerUserId UNIQUEIDENTIFIER,
    @AssignedByUserId UNIQUEIDENTIFIER,
    @DueDate DATETIME2 = NULL,
    @ReviewAssignmentId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @ReviewAssignmentId = NEWID();
        
        -- Create review assignment
        INSERT INTO ReviewAssignments (
            ReviewAssignmentId,
            ResourceId,
            ReviewerUserId,
            AssignedByUserId,
            AssignedAt,
            DueDate,
            Status,
            CreatedAt
        )
        VALUES (
            @ReviewAssignmentId,
            @ResourceId,
            @ReviewerUserId,
            @AssignedByUserId,
            GETUTCDATE(),
            @DueDate,
            0, -- Pending status
            GETUTCDATE()
        );
        
        -- Update resource review status
        UPDATE Resources
        SET ClinicalReviewStatus = 1 -- In Review
        WHERE ResourceId = @ResourceId;
        
        -- Create notification for reviewer
        INSERT INTO Notifications (
            NotificationId,
            UserId,
            Type,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityId,
            IsRead,
            CreatedAt
        )
        VALUES (
            NEWID(),
            @ReviewerUserId,
            'ReviewAssignment',
            'New Resource Review Assignment',
            'You have been assigned to review a resource',
            'Resource',
            @ResourceId,
            0,
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
-- sp_SubmitResourceReviewEvaluation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SubmitResourceReviewEvaluation]
    @ReviewAssignmentId UNIQUEIDENTIFIER,
    @ReviewerUserId UNIQUEIDENTIFIER,
    @ApprovalStatus INT, -- 0=Rejected, 1=ApprovedWithChanges, 2=Approved
    @ClinicalAccuracy INT, -- 1-5 scale
    @AgeAppropriateness INT, -- 1-5 scale
    @EvidenceLevel INT, -- 1-5 scale
    @Comments NVARCHAR(MAX),
    @RequiredChanges NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @ResourceId UNIQUEIDENTIFIER;
        DECLARE @ReviewEvaluationId UNIQUEIDENTIFIER = NEWID();
        
        -- Get resource ID from assignment
        SELECT @ResourceId = ResourceId
        FROM ReviewAssignments
        WHERE ReviewAssignmentId = @ReviewAssignmentId;
        
        -- Create review evaluation
        INSERT INTO ReviewEvaluations (
            ReviewEvaluationId,
            ReviewAssignmentId,
            ResourceId,
            ReviewerUserId,
            ApprovalStatus,
            ClinicalAccuracy,
            AgeAppropriateness,
            EvidenceLevel,
            Comments,
            RequiredChanges,
            ReviewedAt,
            CreatedAt
        )
        VALUES (
            @ReviewEvaluationId,
            @ReviewAssignmentId,
            @ResourceId,
            @ReviewerUserId,
            @ApprovalStatus,
            @ClinicalAccuracy,
            @AgeAppropriateness,
            @EvidenceLevel,
            @Comments,
            @RequiredChanges,
            GETUTCDATE(),
            GETUTCDATE()
        );
        
        -- Update review assignment status
        UPDATE ReviewAssignments
        SET 
            Status = 2, -- Completed
            CompletedAt = GETUTCDATE()
        WHERE ReviewAssignmentId = @ReviewAssignmentId;
        
        -- Update resource based on approval
        IF @ApprovalStatus = 2 -- Approved
        BEGIN
            UPDATE Resources
            SET 
                ClinicalReviewStatus = 2, -- Approved
                EvidenceLevel = @EvidenceLevel,
                UpdatedAt = GETUTCDATE()
            WHERE ResourceId = @ResourceId;
        END
        ELSE IF @ApprovalStatus = 0 -- Rejected
        BEGIN
            UPDATE Resources
            SET 
                ClinicalReviewStatus = 3, -- Rejected
                UpdatedAt = GETUTCDATE()
            WHERE ResourceId = @ResourceId;
        END
        ELSE -- Approved with changes
        BEGIN
            UPDATE Resources
            SET 
                ClinicalReviewStatus = 4, -- Needs Revision
                UpdatedAt = GETUTCDATE()
            WHERE ResourceId = @ResourceId;
        END
        
        -- Create notification for resource creator
        DECLARE @CreatorId UNIQUEIDENTIFIER;
        SELECT @CreatorId = CreatedByUserId FROM Resources WHERE ResourceId = @ResourceId;
        
        INSERT INTO Notifications (
            NotificationId,
            UserId,
            Type,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityId,
            IsRead,
            CreatedAt
        )
        VALUES (
            NEWID(),
            @CreatorId,
            'ReviewCompleted',
            'Resource Review Completed',
            CASE @ApprovalStatus
                WHEN 2 THEN 'Your resource has been approved'
                WHEN 0 THEN 'Your resource requires changes'
                ELSE 'Your resource has been approved with minor changes'
            END,
            'Resource',
            @ResourceId,
            0,
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
-- sp_GetResourceReviews
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourceReviews]
    @ResourceId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        re.ReviewEvaluationId,
        re.ReviewAssignmentId,
        re.ResourceId,
        re.ReviewerUserId,
        re.ApprovalStatus,
        re.ClinicalAccuracy,
        re.AgeAppropriateness,
        re.EvidenceLevel,
        re.Comments,
        re.RequiredChanges,
        re.ReviewedAt,
        re.CreatedAt,
        u.FirstName + ' ' + u.LastName AS ReviewerName,
        u.LicenseType AS ReviewerLicenseType
    FROM ReviewEvaluations re
    INNER JOIN Users u ON re.ReviewerUserId = u.UserId
    WHERE re.ResourceId = @ResourceId
    ORDER BY re.ReviewedAt DESC;
END
GO

-- =============================================
-- sp_GetReviewStatistics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetReviewStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Overall statistics
    SELECT 
        COUNT(DISTINCT ra.ReviewAssignmentId) AS TotalAssignments,
        COUNT(DISTINCT CASE WHEN ra.Status = 2 THEN ra.ReviewAssignmentId END) AS CompletedReviews,
        COUNT(DISTINCT CASE WHEN ra.Status = 0 THEN ra.ReviewAssignmentId END) AS PendingReviews,
        COUNT(DISTINCT CASE WHEN ra.Status = 1 THEN ra.ReviewAssignmentId END) AS InProgressReviews,
        AVG(CASE WHEN ra.Status = 2 THEN DATEDIFF(hour, ra.AssignedAt, ra.CompletedAt) END) AS AvgReviewTimeHours
    FROM ReviewAssignments ra;
    
    -- Approval statistics
    SELECT 
        COUNT(CASE WHEN ApprovalStatus = 2 THEN 1 END) AS ApprovedCount,
        COUNT(CASE WHEN ApprovalStatus = 1 THEN 1 END) AS ApprovedWithChangesCount,
        COUNT(CASE WHEN ApprovalStatus = 0 THEN 1 END) AS RejectedCount,
        AVG(CAST(ClinicalAccuracy AS FLOAT)) AS AvgClinicalAccuracy,
        AVG(CAST(AgeAppropriateness AS FLOAT)) AS AvgAgeAppropriateness,
        AVG(CAST(EvidenceLevel AS FLOAT)) AS AvgEvidenceLevel
    FROM ReviewEvaluations;
    
    -- Top reviewers
    SELECT TOP 10
        u.UserId,
        u.FirstName + ' ' + u.LastName AS ReviewerName,
        COUNT(re.ReviewEvaluationId) AS ReviewsCompleted,
        AVG(DATEDIFF(hour, ra.AssignedAt, ra.CompletedAt)) AS AvgReviewTimeHours
    FROM Users u
    INNER JOIN ReviewEvaluations re ON u.UserId = re.ReviewerUserId
    INNER JOIN ReviewAssignments ra ON re.ReviewAssignmentId = ra.ReviewAssignmentId
    WHERE ra.Status = 2
    GROUP BY u.UserId, u.FirstName, u.LastName
    ORDER BY ReviewsCompleted DESC;
    
    -- Reviews by month
    SELECT 
        YEAR(ReviewedAt) AS Year,
        MONTH(ReviewedAt) AS Month,
        COUNT(*) AS ReviewCount,
        AVG(CAST(EvidenceLevel AS FLOAT)) AS AvgEvidenceLevel
    FROM ReviewEvaluations
    WHERE ReviewedAt >= DATEADD(month, -12, GETUTCDATE())
    GROUP BY YEAR(ReviewedAt), MONTH(ReviewedAt)
    ORDER BY Year, Month;
END
GO

-- =============================================
-- sp_GetResourcesBySeller
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourcesBySeller]
    @SellerUserId UNIQUEIDENTIFIER,
    @IncludeDeleted BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Resources r
    WHERE r.SellerUserId = @SellerUserId
        AND (@IncludeDeleted = 1 OR r.IsDeleted = 0);
    
    -- Get paginated results
    WITH ResourceCTE AS (
        SELECT 
            r.*,
            ROW_NUMBER() OVER (ORDER BY r.CreatedAt DESC) AS RowNum
        FROM Resources r
        WHERE r.SellerUserId = @SellerUserId
            AND (@IncludeDeleted = 1 OR r.IsDeleted = 0)
    )
    SELECT * FROM ResourceCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
GO