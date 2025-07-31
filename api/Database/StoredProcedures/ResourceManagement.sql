-- Resource Management Stored Procedures

-- =============================================
-- sp_GetResourceById
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourceById]
    @ResourceId UNIQUEIDENTIFIER,
    @IncludeDeleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.ResourceId,
        r.Title,
        r.Description,
        r.ResourceType,
        r.FileUrl,
        r.ThumbnailUrl,
        r.SkillAreas,
        r.GradeLevels,
        r.Languages,
        r.IsInteractive,
        r.HasAudio,
        r.GenerationMethod,
        r.AiGenerationMetadata,
        r.ClinicalReviewStatus,
        r.EvidenceLevel,
        r.CreatedByUserId,
        r.CreatedAt,
        r.UpdatedAt,
        r.IsDeleted,
        r.DeletedAt,
        r.Price,
        r.IsFree,
        r.SellerUserId,
        r.CommissionRate,
        r.DownloadCount,
        r.AverageRating,
        r.ReviewCount
    FROM Resources r
    WHERE r.ResourceId = @ResourceId
        AND (@IncludeDeleted = 1 OR r.IsDeleted = 0);
END
GO

-- =============================================
-- sp_GetResourcesByFilter
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetResourcesByFilter]
    @SearchTerm NVARCHAR(500) = NULL,
    @ResourceType INT = NULL,
    @IsFree BIT = NULL,
    @MinEvidenceLevel INT = NULL,
    @MaxEvidenceLevel INT = NULL,
    @SkillArea NVARCHAR(100) = NULL,
    @GradeLevel INT = NULL,
    @Language NVARCHAR(50) = NULL,
    @IsInteractive BIT = NULL,
    @HasAudio BIT = NULL,
    @ClinicalReviewStatus INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Build dynamic WHERE clause
    DECLARE @Where NVARCHAR(MAX) = ' WHERE r.IsDeleted = 0 ';
    
    IF @SearchTerm IS NOT NULL
        SET @Where = @Where + ' AND (r.Title LIKE ''%'' + @SearchTerm + ''%'' OR r.Description LIKE ''%'' + @SearchTerm + ''%'') ';
    
    IF @ResourceType IS NOT NULL
        SET @Where = @Where + ' AND r.ResourceType = @ResourceType ';
    
    IF @IsFree IS NOT NULL
        SET @Where = @Where + ' AND r.IsFree = @IsFree ';
    
    IF @MinEvidenceLevel IS NOT NULL
        SET @Where = @Where + ' AND r.EvidenceLevel >= @MinEvidenceLevel ';
    
    IF @MaxEvidenceLevel IS NOT NULL
        SET @Where = @Where + ' AND r.EvidenceLevel <= @MaxEvidenceLevel ';
    
    IF @SkillArea IS NOT NULL
        SET @Where = @Where + ' AND r.SkillAreas LIKE ''%"'' + @SkillArea + ''"%'' ';
    
    IF @GradeLevel IS NOT NULL
        SET @Where = @Where + ' AND r.GradeLevels LIKE ''%' + CAST(@GradeLevel AS NVARCHAR) + '%'' ';
    
    IF @Language IS NOT NULL
        SET @Where = @Where + ' AND r.Languages LIKE ''%"'' + @Language + ''"%'' ';
    
    IF @IsInteractive IS NOT NULL
        SET @Where = @Where + ' AND r.IsInteractive = @IsInteractive ';
    
    IF @HasAudio IS NOT NULL
        SET @Where = @Where + ' AND r.HasAudio = @HasAudio ';
    
    IF @ClinicalReviewStatus IS NOT NULL
        SET @Where = @Where + ' AND r.ClinicalReviewStatus = @ClinicalReviewStatus ';
    
    -- Get total count
    DECLARE @CountSql NVARCHAR(MAX) = 'SELECT @TotalCount = COUNT(*) FROM Resources r ' + @Where;
    
    EXEC sp_executesql @CountSql, 
        N'@TotalCount INT OUTPUT, @SearchTerm NVARCHAR(500), @ResourceType INT, @IsFree BIT, 
          @MinEvidenceLevel INT, @MaxEvidenceLevel INT, @SkillArea NVARCHAR(100), 
          @GradeLevel INT, @Language NVARCHAR(50), @IsInteractive BIT, @HasAudio BIT, 
          @ClinicalReviewStatus INT',
        @TotalCount OUTPUT, @SearchTerm, @ResourceType, @IsFree, 
        @MinEvidenceLevel, @MaxEvidenceLevel, @SkillArea, 
        @GradeLevel, @Language, @IsInteractive, @HasAudio, 
        @ClinicalReviewStatus;
    
    -- Get paginated results
    DECLARE @OrderBy NVARCHAR(100) = 
        CASE @SortBy
            WHEN 'Title' THEN 'r.Title'
            WHEN 'CreatedAt' THEN 'r.CreatedAt'
            WHEN 'UpdatedAt' THEN 'r.UpdatedAt'
            WHEN 'DownloadCount' THEN 'r.DownloadCount'
            WHEN 'AverageRating' THEN 'r.AverageRating'
            WHEN 'Price' THEN 'r.Price'
            ELSE 'r.CreatedAt'
        END + ' ' + @SortDirection;
    
    DECLARE @ResultSql NVARCHAR(MAX) = '
        WITH ResourceCTE AS (
            SELECT r.*, 
                   ROW_NUMBER() OVER (ORDER BY ' + @OrderBy + ') AS RowNum
            FROM Resources r ' + @Where + '
        )
        SELECT * FROM ResourceCTE
        WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)';
    
    EXEC sp_executesql @ResultSql, 
        N'@SearchTerm NVARCHAR(500), @ResourceType INT, @IsFree BIT, 
          @MinEvidenceLevel INT, @MaxEvidenceLevel INT, @SkillArea NVARCHAR(100), 
          @GradeLevel INT, @Language NVARCHAR(50), @IsInteractive BIT, @HasAudio BIT, 
          @ClinicalReviewStatus INT, @PageNumber INT, @PageSize INT',
        @SearchTerm, @ResourceType, @IsFree, 
        @MinEvidenceLevel, @MaxEvidenceLevel, @SkillArea, 
        @GradeLevel, @Language, @IsInteractive, @HasAudio, 
        @ClinicalReviewStatus, @PageNumber, @PageSize;
END
GO

-- =============================================
-- sp_CreateResource
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateResource]
    @Title NVARCHAR(500),
    @Description NVARCHAR(MAX),
    @ResourceType INT,
    @FileUrl NVARCHAR(500) = NULL,
    @ThumbnailUrl NVARCHAR(500) = NULL,
    @SkillAreas NVARCHAR(MAX),
    @GradeLevels NVARCHAR(MAX),
    @Languages NVARCHAR(MAX) = '["English"]',
    @IsInteractive BIT = 0,
    @HasAudio BIT = 0,
    @GenerationMethod INT = 0,
    @AiGenerationMetadata NVARCHAR(MAX) = NULL,
    @ClinicalReviewStatus INT = 0,
    @EvidenceLevel INT = 3,
    @CreatedByUserId UNIQUEIDENTIFIER,
    @Price DECIMAL(10,2) = 0,
    @IsFree BIT = 1,
    @SellerUserId UNIQUEIDENTIFIER = NULL,
    @CommissionRate DECIMAL(3,2) = 0.30,
    @ResourceId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SET @ResourceId = NEWID();
        
        INSERT INTO Resources (
            ResourceId, Title, Description, ResourceType,
            FileUrl, ThumbnailUrl, SkillAreas, GradeLevels,
            Languages, IsInteractive, HasAudio,
            GenerationMethod, AiGenerationMetadata,
            ClinicalReviewStatus, EvidenceLevel,
            CreatedByUserId, CreatedAt, UpdatedAt,
            IsDeleted, Price, IsFree, SellerUserId,
            CommissionRate, DownloadCount, AverageRating, ReviewCount
        )
        VALUES (
            @ResourceId, @Title, @Description, @ResourceType,
            @FileUrl, @ThumbnailUrl, @SkillAreas, @GradeLevels,
            @Languages, @IsInteractive, @HasAudio,
            @GenerationMethod, @AiGenerationMetadata,
            @ClinicalReviewStatus, @EvidenceLevel,
            @CreatedByUserId, GETUTCDATE(), GETUTCDATE(),
            0, @Price, @IsFree, @SellerUserId,
            @CommissionRate, 0, 0, 0
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
-- sp_UpdateResource
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateResource]
    @ResourceId UNIQUEIDENTIFIER,
    @Title NVARCHAR(500) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @FileUrl NVARCHAR(500) = NULL,
    @ThumbnailUrl NVARCHAR(500) = NULL,
    @SkillAreas NVARCHAR(MAX) = NULL,
    @GradeLevels NVARCHAR(MAX) = NULL,
    @Languages NVARCHAR(MAX) = NULL,
    @IsInteractive BIT = NULL,
    @HasAudio BIT = NULL,
    @ClinicalReviewStatus INT = NULL,
    @EvidenceLevel INT = NULL,
    @Price DECIMAL(10,2) = NULL,
    @IsFree BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Resources
    SET 
        Title = ISNULL(@Title, Title),
        Description = ISNULL(@Description, Description),
        FileUrl = ISNULL(@FileUrl, FileUrl),
        ThumbnailUrl = ISNULL(@ThumbnailUrl, ThumbnailUrl),
        SkillAreas = ISNULL(@SkillAreas, SkillAreas),
        GradeLevels = ISNULL(@GradeLevels, GradeLevels),
        Languages = ISNULL(@Languages, Languages),
        IsInteractive = ISNULL(@IsInteractive, IsInteractive),
        HasAudio = ISNULL(@HasAudio, HasAudio),
        ClinicalReviewStatus = ISNULL(@ClinicalReviewStatus, ClinicalReviewStatus),
        EvidenceLevel = ISNULL(@EvidenceLevel, EvidenceLevel),
        Price = ISNULL(@Price, Price),
        IsFree = ISNULL(@IsFree, IsFree),
        UpdatedAt = GETUTCDATE()
    WHERE ResourceId = @ResourceId
        AND IsDeleted = 0;
END
GO

-- =============================================
-- sp_SoftDeleteResource
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SoftDeleteResource]
    @ResourceId UNIQUEIDENTIFIER,
    @DeletedByUserId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Soft delete resource
        UPDATE Resources
        SET 
            IsDeleted = 1,
            DeletedAt = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE ResourceId = @ResourceId;
        
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
            @DeletedByUserId,
            'Resource',
            @ResourceId,
            'Delete',
            '{"action": "soft_delete"}',
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
-- sp_GetUserFavoriteResources
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserFavoriteResources]
    @UserId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM UserFavorites uf
    INNER JOIN Resources r ON uf.ResourceId = r.ResourceId
    WHERE uf.UserId = @UserId 
        AND r.IsDeleted = 0;
    
    -- Get paginated results
    WITH FavoriteCTE AS (
        SELECT 
            r.*,
            uf.CreatedAt AS FavoritedAt,
            ROW_NUMBER() OVER (ORDER BY uf.CreatedAt DESC) AS RowNum
        FROM UserFavorites uf
        INNER JOIN Resources r ON uf.ResourceId = r.ResourceId
        WHERE uf.UserId = @UserId 
            AND r.IsDeleted = 0
    )
    SELECT * FROM FavoriteCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
GO

-- =============================================
-- sp_AddResourceToFavorites
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_AddResourceToFavorites]
    @UserId UNIQUEIDENTIFIER,
    @ResourceId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if already favorited
    IF NOT EXISTS (SELECT 1 FROM UserFavorites WHERE UserId = @UserId AND ResourceId = @ResourceId)
    BEGIN
        INSERT INTO UserFavorites (UserId, ResourceId, CreatedAt)
        VALUES (@UserId, @ResourceId, GETUTCDATE());
    END
END
GO

-- =============================================
-- sp_RemoveResourceFromFavorites
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_RemoveResourceFromFavorites]
    @UserId UNIQUEIDENTIFIER,
    @ResourceId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM UserFavorites
    WHERE UserId = @UserId AND ResourceId = @ResourceId;
END
GO

-- =============================================
-- sp_IncrementResourceDownloadCount
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_IncrementResourceDownloadCount]
    @ResourceId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Increment download count
        UPDATE Resources
        SET DownloadCount = DownloadCount + 1
        WHERE ResourceId = @ResourceId;
        
        -- Log download
        INSERT INTO ResourceDownloads (
            DownloadId,
            ResourceId,
            UserId,
            DownloadedAt
        )
        VALUES (
            NEWID(),
            @ResourceId,
            @UserId,
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
-- sp_UpdateResourceRating
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateResourceRating]
    @ResourceId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Rating INT,
    @Review NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if user already rated
        IF EXISTS (SELECT 1 FROM ResourceRatings WHERE ResourceId = @ResourceId AND UserId = @UserId)
        BEGIN
            -- Update existing rating
            UPDATE ResourceRatings
            SET Rating = @Rating,
                Review = @Review,
                UpdatedAt = GETUTCDATE()
            WHERE ResourceId = @ResourceId AND UserId = @UserId;
        END
        ELSE
        BEGIN
            -- Insert new rating
            INSERT INTO ResourceRatings (
                RatingId,
                ResourceId,
                UserId,
                Rating,
                Review,
                CreatedAt
            )
            VALUES (
                NEWID(),
                @ResourceId,
                @UserId,
                @Rating,
                @Review,
                GETUTCDATE()
            );
        END
        
        -- Update resource average rating
        UPDATE Resources
        SET AverageRating = (
                SELECT AVG(CAST(Rating AS FLOAT))
                FROM ResourceRatings
                WHERE ResourceId = @ResourceId
            ),
            ReviewCount = (
                SELECT COUNT(*)
                FROM ResourceRatings
                WHERE ResourceId = @ResourceId
            )
        WHERE ResourceId = @ResourceId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO