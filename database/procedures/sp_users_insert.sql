-- Stored procedure for inserting users
CREATE OR ALTER PROCEDURE [dbo].[sp_users_insert]
    @email NVARCHAR(255),
    @passwordHash NVARCHAR(255),
    @role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @userId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO [dbo].[users] (
        [id],
        [email],
        [passwordHash],
        [role],
        [isActive],
        [isEmailVerified],
        [createdAt]
    )
    VALUES (
        @userId,
        @email,
        @passwordHash,
        @role,
        1, -- isActive
        0, -- isEmailVerified
        GETUTCDATE()
    );
    
    SELECT @userId AS id;
END
GO