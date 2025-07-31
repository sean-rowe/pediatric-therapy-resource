-- =============================================
-- Session Management Stored Procedures
-- =============================================

-- Get session by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessionById]
    @SessionId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT s.*, st.FirstNameEncrypted, st.LastNameEncrypted, st.GradeLevel
    FROM Sessions s
    INNER JOIN Students st ON s.StudentId = st.StudentId
    WHERE s.SessionId = @SessionId
        AND (@TherapistId IS NULL OR s.TherapistId = @TherapistId);
END
GO

-- Get sessions with filters
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessions]
    @TherapistId UNIQUEIDENTIFIER,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @StudentId UNIQUEIDENTIFIER = NULL,
    @SessionType INT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Sessions s
    WHERE s.TherapistId = @TherapistId
        AND (@StartDate IS NULL OR s.SessionDate >= @StartDate)
        AND (@EndDate IS NULL OR s.SessionDate <= @EndDate)
        AND (@StudentId IS NULL OR s.StudentId = @StudentId)
        AND (@SessionType IS NULL OR s.SessionType = @SessionType);
    
    -- Return paginated results with student info
    SELECT s.*, st.FirstNameEncrypted, st.LastNameEncrypted, st.GradeLevel
    FROM Sessions s
    INNER JOIN Students st ON s.StudentId = st.StudentId
    WHERE s.TherapistId = @TherapistId
        AND (@StartDate IS NULL OR s.SessionDate >= @StartDate)
        AND (@EndDate IS NULL OR s.SessionDate <= @EndDate)
        AND (@StudentId IS NULL OR s.StudentId = @StudentId)
        AND (@SessionType IS NULL OR s.SessionType = @SessionType)
    ORDER BY s.SessionDate DESC, s.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Create new session
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateSession]
    @SessionId UNIQUEIDENTIFIER OUTPUT,
    @TherapistId UNIQUEIDENTIFIER,
    @StudentId UNIQUEIDENTIFIER,
    @SessionDate DATETIME2,
    @DurationMinutes INT,
    @SessionType INT,
    @ResourcesUsed NVARCHAR(MAX),
    @DataPoints NVARCHAR(MAX),
    @NotesEncrypted NVARCHAR(MAX) = NULL,
    @Location NVARCHAR(200) = NULL,
    @IsBillable BIT = 1,
    @BillingCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SessionId IS NULL OR @SessionId = '00000000-0000-0000-0000-000000000000'
        SET @SessionId = NEWID();
    
    -- Verify student belongs to therapist
    IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId AND TherapistId = @TherapistId)
    BEGIN
        RAISERROR ('Student does not belong to this therapist', 16, 1);
        RETURN;
    END
    
    INSERT INTO Sessions (
        SessionId, TherapistId, StudentId, SessionDate,
        DurationMinutes, SessionType, ResourcesUsed, DataPoints,
        NotesEncrypted, Location, IsBillable, BillingCode,
        CreatedAt, UpdatedAt
    )
    VALUES (
        @SessionId, @TherapistId, @StudentId, @SessionDate,
        @DurationMinutes, @SessionType, @ResourcesUsed, @DataPoints,
        @NotesEncrypted, @Location, @IsBillable, @BillingCode,
        GETUTCDATE(), GETUTCDATE()
    );
END
GO

-- Update session
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateSession]
    @SessionId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER,
    @SessionDate DATETIME2,
    @DurationMinutes INT,
    @SessionType INT,
    @ResourcesUsed NVARCHAR(MAX),
    @DataPoints NVARCHAR(MAX),
    @NotesEncrypted NVARCHAR(MAX) = NULL,
    @Location NVARCHAR(200) = NULL,
    @IsBillable BIT,
    @BillingCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Sessions
    SET SessionDate = @SessionDate,
        DurationMinutes = @DurationMinutes,
        SessionType = @SessionType,
        ResourcesUsed = @ResourcesUsed,
        DataPoints = @DataPoints,
        NotesEncrypted = @NotesEncrypted,
        Location = @Location,
        IsBillable = @IsBillable,
        BillingCode = @BillingCode,
        UpdatedAt = GETUTCDATE()
    WHERE SessionId = @SessionId
        AND TherapistId = @TherapistId;
END
GO

-- Delete session
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteSession]
    @SessionId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Sessions
    WHERE SessionId = @SessionId
        AND TherapistId = @TherapistId;
END
GO

-- Get session statistics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessionStatistics]
    @TherapistId UNIQUEIDENTIFIER,
    @StartDate DATETIME2,
    @EndDate DATETIME2,
    @StudentId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Total sessions
    DECLARE @TotalSessions INT;
    SELECT @TotalSessions = COUNT(*)
    FROM Sessions
    WHERE TherapistId = @TherapistId
        AND SessionDate BETWEEN @StartDate AND @EndDate
        AND (@StudentId IS NULL OR StudentId = @StudentId);
    
    -- Total duration
    DECLARE @TotalMinutes INT;
    SELECT @TotalMinutes = ISNULL(SUM(DurationMinutes), 0)
    FROM Sessions
    WHERE TherapistId = @TherapistId
        AND SessionDate BETWEEN @StartDate AND @EndDate
        AND (@StudentId IS NULL OR StudentId = @StudentId);
    
    -- Sessions by type
    SELECT 
        SessionType,
        COUNT(*) as SessionCount,
        SUM(DurationMinutes) as TotalMinutes
    FROM Sessions
    WHERE TherapistId = @TherapistId
        AND SessionDate BETWEEN @StartDate AND @EndDate
        AND (@StudentId IS NULL OR StudentId = @StudentId)
    GROUP BY SessionType;
    
    -- Return summary
    SELECT 
        @TotalSessions as TotalSessions,
        @TotalMinutes as TotalMinutes,
        @TotalMinutes / 60.0 as TotalHours,
        CASE WHEN @TotalSessions > 0 
            THEN @TotalMinutes / @TotalSessions 
            ELSE 0 
        END as AverageMinutesPerSession;
END
GO

-- Get sessions by resource
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessionsByResource]
    @ResourceId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Sessions s
    WHERE JSON_VALUE(s.ResourcesUsed, '$') LIKE '%' + CAST(@ResourceId AS NVARCHAR(36)) + '%'
        AND (@TherapistId IS NULL OR s.TherapistId = @TherapistId)
        AND (@StartDate IS NULL OR s.SessionDate >= @StartDate)
        AND (@EndDate IS NULL OR s.SessionDate <= @EndDate);
    
    -- Return paginated results
    SELECT s.*, st.FirstNameEncrypted, st.LastNameEncrypted
    FROM Sessions s
    INNER JOIN Students st ON s.StudentId = st.StudentId
    WHERE JSON_VALUE(s.ResourcesUsed, '$') LIKE '%' + CAST(@ResourceId AS NVARCHAR(36)) + '%'
        AND (@TherapistId IS NULL OR s.TherapistId = @TherapistId)
        AND (@StartDate IS NULL OR s.SessionDate >= @StartDate)
        AND (@EndDate IS NULL OR s.SessionDate <= @EndDate)
    ORDER BY s.SessionDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Record session progress
CREATE OR ALTER PROCEDURE [dbo].[sp_RecordSessionProgress]
    @SessionId UNIQUEIDENTIFIER,
    @GoalId UNIQUEIDENTIFIER,
    @ProgressValue INT,
    @Notes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update goal progress
    UPDATE StudentGoals
    SET CurrentProgress = @ProgressValue,
        UpdatedAt = GETUTCDATE()
    WHERE GoalId = @GoalId;
    
    -- Store progress in session data points
    DECLARE @DataPoints NVARCHAR(MAX);
    SELECT @DataPoints = DataPoints FROM Sessions WHERE SessionId = @SessionId;
    
    -- Add progress to data points JSON
    -- This is simplified - in production would use proper JSON manipulation
    UPDATE Sessions
    SET DataPoints = JSON_MODIFY(
            ISNULL(@DataPoints, '{}'),
            '$.goalProgress.' + CAST(@GoalId AS NVARCHAR(36)),
            JSON_QUERY('{"value":' + CAST(@ProgressValue AS NVARCHAR(10)) + 
                      ',"notes":"' + ISNULL(@Notes, '') + '"' +
                      ',"timestamp":"' + CONVERT(NVARCHAR(30), GETUTCDATE(), 126) + '"}')
        ),
        UpdatedAt = GETUTCDATE()
    WHERE SessionId = @SessionId;
END
GO

-- Get upcoming sessions
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUpcomingSessions]
    @TherapistId UNIQUEIDENTIFIER,
    @Days INT = 7,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EndDate DATETIME2 = DATEADD(DAY, @Days, GETUTCDATE());
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Sessions s
    WHERE s.TherapistId = @TherapistId
        AND s.SessionDate BETWEEN GETUTCDATE() AND @EndDate;
    
    -- Return paginated results
    SELECT s.*, st.FirstNameEncrypted, st.LastNameEncrypted, st.GradeLevel
    FROM Sessions s
    INNER JOIN Students st ON s.StudentId = st.StudentId
    WHERE s.TherapistId = @TherapistId
        AND s.SessionDate BETWEEN GETUTCDATE() AND @EndDate
    ORDER BY s.SessionDate ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Clone session (for recurring sessions)
CREATE OR ALTER PROCEDURE [dbo].[sp_CloneSession]
    @SourceSessionId UNIQUEIDENTIFIER,
    @NewSessionDate DATETIME2,
    @NewSessionId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @NewSessionId = NEWID();
    
    INSERT INTO Sessions (
        SessionId, TherapistId, StudentId, SessionDate,
        DurationMinutes, SessionType, ResourcesUsed, DataPoints,
        NotesEncrypted, Location, IsBillable, BillingCode,
        CreatedAt, UpdatedAt
    )
    SELECT 
        @NewSessionId, TherapistId, StudentId, @NewSessionDate,
        DurationMinutes, SessionType, ResourcesUsed, '{}', -- Empty data points for new session
        NULL, -- Clear notes for new session
        Location, IsBillable, BillingCode,
        GETUTCDATE(), GETUTCDATE()
    FROM Sessions
    WHERE SessionId = @SourceSessionId;
END
GO