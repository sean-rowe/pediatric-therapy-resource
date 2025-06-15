-- Stored procedure for getting user by email
CREATE OR ALTER PROCEDURE [dbo].[sp_users_getByEmail]
    @email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [id],
        [email],
        [passwordHash],
        [role],
        [isActive],
        [isEmailVerified],
        [createdAt],
        [updatedAt]
    FROM [dbo].[users]
    WHERE [email] = @email;
END
GO