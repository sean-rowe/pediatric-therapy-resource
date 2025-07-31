-- =============================================
-- Student Management Stored Procedures
-- =============================================

-- Get student by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetStudentById]
    @StudentId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM Students
    WHERE StudentId = @StudentId
        AND IsActive = 1
        AND (@TherapistId IS NULL OR TherapistId = @TherapistId);
END
GO

-- Get students by therapist
CREATE OR ALTER PROCEDURE [dbo].[sp_GetStudentsByTherapist]
    @TherapistId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Students
    WHERE TherapistId = @TherapistId
        AND (@IncludeInactive = 1 OR IsActive = 1);
    
    -- Return paginated results
    SELECT * FROM Students
    WHERE TherapistId = @TherapistId
        AND (@IncludeInactive = 1 OR IsActive = 1)
    ORDER BY LastNameEncrypted, FirstNameEncrypted
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Create new student
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateStudent]
    @StudentId UNIQUEIDENTIFIER OUTPUT,
    @TherapistId UNIQUEIDENTIFIER,
    @FirstNameEncrypted NVARCHAR(500),
    @LastNameEncrypted NVARCHAR(500),
    @DateOfBirthEncrypted NVARCHAR(500) = NULL,
    @GradeLevel INT,
    @ParentEmailEncrypted NVARCHAR(500) = NULL,
    @IepGoalsEncrypted NVARCHAR(MAX) = NULL,
    @AccessCode NVARCHAR(10),
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @StudentId IS NULL OR @StudentId = '00000000-0000-0000-0000-000000000000'
        SET @StudentId = NEWID();
    
    -- Ensure access code is unique
    WHILE EXISTS (SELECT 1 FROM Students WHERE AccessCode = @AccessCode)
    BEGIN
        -- Generate new 6-character access code
        SET @AccessCode = LEFT(REPLACE(NEWID(), '-', ''), 6);
    END
    
    INSERT INTO Students (
        StudentId, TherapistId, FirstNameEncrypted, LastNameEncrypted,
        DateOfBirthEncrypted, GradeLevel, ParentEmailEncrypted,
        IepGoalsEncrypted, AccessCode, IsActive, CreatedAt
    )
    VALUES (
        @StudentId, @TherapistId, @FirstNameEncrypted, @LastNameEncrypted,
        @DateOfBirthEncrypted, @GradeLevel, @ParentEmailEncrypted,
        @IepGoalsEncrypted, @AccessCode, @IsActive, GETUTCDATE()
    );
END
GO

-- Update student
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateStudent]
    @StudentId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER,
    @FirstNameEncrypted NVARCHAR(500),
    @LastNameEncrypted NVARCHAR(500),
    @DateOfBirthEncrypted NVARCHAR(500) = NULL,
    @GradeLevel INT,
    @ParentEmailEncrypted NVARCHAR(500) = NULL,
    @IepGoalsEncrypted NVARCHAR(MAX) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Students
    SET FirstNameEncrypted = @FirstNameEncrypted,
        LastNameEncrypted = @LastNameEncrypted,
        DateOfBirthEncrypted = @DateOfBirthEncrypted,
        GradeLevel = @GradeLevel,
        ParentEmailEncrypted = @ParentEmailEncrypted,
        IepGoalsEncrypted = @IepGoalsEncrypted,
        IsActive = @IsActive,
        UpdatedAt = GETUTCDATE()
    WHERE StudentId = @StudentId
        AND TherapistId = @TherapistId;
END
GO

-- Soft delete student
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteStudent]
    @StudentId UNIQUEIDENTIFIER,
    @TherapistId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Students
    SET IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE StudentId = @StudentId
        AND TherapistId = @TherapistId;
END
GO

-- Search students
CREATE OR ALTER PROCEDURE [dbo].[sp_SearchStudents]
    @TherapistId UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(100) = NULL,
    @GradeLevel INT = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM Students
    WHERE TherapistId = @TherapistId
        AND (@SearchTerm IS NULL OR 
             FirstNameEncrypted LIKE '%' + @SearchTerm + '%' OR
             LastNameEncrypted LIKE '%' + @SearchTerm + '%')
        AND (@GradeLevel IS NULL OR GradeLevel = @GradeLevel)
        AND (@IsActive IS NULL OR IsActive = @IsActive);
    
    -- Return paginated results
    SELECT * FROM Students
    WHERE TherapistId = @TherapistId
        AND (@SearchTerm IS NULL OR 
             FirstNameEncrypted LIKE '%' + @SearchTerm + '%' OR
             LastNameEncrypted LIKE '%' + @SearchTerm + '%')
        AND (@GradeLevel IS NULL OR GradeLevel = @GradeLevel)
        AND (@IsActive IS NULL OR IsActive = @IsActive)
    ORDER BY LastNameEncrypted, FirstNameEncrypted
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Get student goals
CREATE OR ALTER PROCEDURE [dbo].[sp_GetStudentGoals]
    @StudentId UNIQUEIDENTIFIER = NULL,
    @TherapistId UNIQUEIDENTIFIER,
    @GoalStatus INT = NULL,
    @GoalArea NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM StudentGoals sg
    INNER JOIN Students s ON sg.StudentId = s.StudentId
    WHERE s.TherapistId = @TherapistId
        AND (@StudentId IS NULL OR sg.StudentId = @StudentId)
        AND (@GoalStatus IS NULL OR sg.Status = @GoalStatus)
        AND (@GoalArea IS NULL OR sg.GoalArea LIKE '%' + @GoalArea + '%');
    
    -- Return paginated results
    SELECT sg.*, s.FirstNameEncrypted, s.LastNameEncrypted
    FROM StudentGoals sg
    INNER JOIN Students s ON sg.StudentId = s.StudentId
    WHERE s.TherapistId = @TherapistId
        AND (@StudentId IS NULL OR sg.StudentId = @StudentId)
        AND (@GoalStatus IS NULL OR sg.Status = @GoalStatus)
        AND (@GoalArea IS NULL OR sg.GoalArea LIKE '%' + @GoalArea + '%')
    ORDER BY s.LastNameEncrypted, s.FirstNameEncrypted, sg.TargetDate
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Create student goal
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateStudentGoal]
    @GoalId UNIQUEIDENTIFIER OUTPUT,
    @StudentId UNIQUEIDENTIFIER,
    @GoalArea NVARCHAR(200),
    @GoalDescription NVARCHAR(MAX),
    @Objectives NVARCHAR(MAX),
    @Baseline INT,
    @Target INT,
    @TargetDate DATE,
    @Frequency NVARCHAR(100),
    @Status INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @GoalId IS NULL OR @GoalId = '00000000-0000-0000-0000-000000000000'
        SET @GoalId = NEWID();
    
    INSERT INTO StudentGoals (
        GoalId, StudentId, GoalArea, GoalDescription,
        Objectives, Baseline, Target, TargetDate,
        Frequency, Status, CreatedAt
    )
    VALUES (
        @GoalId, @StudentId, @GoalArea, @GoalDescription,
        @Objectives, @Baseline, @Target, @TargetDate,
        @Frequency, @Status, GETUTCDATE()
    );
END
GO

-- Update student goal
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateStudentGoal]
    @GoalId UNIQUEIDENTIFIER,
    @GoalArea NVARCHAR(200),
    @GoalDescription NVARCHAR(MAX),
    @Objectives NVARCHAR(MAX),
    @Baseline INT,
    @Target INT,
    @CurrentProgress INT,
    @TargetDate DATE,
    @Frequency NVARCHAR(100),
    @Status INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE StudentGoals
    SET GoalArea = @GoalArea,
        GoalDescription = @GoalDescription,
        Objectives = @Objectives,
        Baseline = @Baseline,
        Target = @Target,
        CurrentProgress = @CurrentProgress,
        TargetDate = @TargetDate,
        Frequency = @Frequency,
        Status = @Status,
        UpdatedAt = GETUTCDATE()
    WHERE GoalId = @GoalId;
END
GO

-- Delete student goal
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteStudentGoal]
    @GoalId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM StudentGoals
    WHERE GoalId = @GoalId;
END
GO

-- Generate unique access code
CREATE OR ALTER PROCEDURE [dbo].[sp_GenerateUniqueAccessCode]
    @AccessCode NVARCHAR(10) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxAttempts INT = 100;
    DECLARE @Attempts INT = 0;
    
    WHILE @Attempts < @MaxAttempts
    BEGIN
        -- Generate 6-character alphanumeric code
        SET @AccessCode = LEFT(REPLACE(NEWID(), '-', ''), 6);
        
        IF NOT EXISTS (SELECT 1 FROM Students WHERE AccessCode = @AccessCode)
            BREAK;
            
        SET @Attempts = @Attempts + 1;
    END
    
    IF @Attempts = @MaxAttempts
        RAISERROR ('Could not generate unique access code', 16, 1);
END
GO