# TODO: Migrate to Stored Procedures with tSQLt Testing

## Overview
All SQL queries in the codebase must be replaced with stored procedures, and all stored procedures must be tested using the tSQLt framework.

## Current State Analysis

### Entity Framework LINQ Queries to Replace
Currently, the application uses Entity Framework with LINQ queries directly in the code. These need to be replaced with stored procedure calls.

#### Examples of Current Implementation:
```csharp
// Current - Direct LINQ query
List<Resource> resources = await _context.Resources
    .Where(r => r.IsPublished && r.IsActive)
    .OrderBy(r => r.Title)
    .ToListAsync();

// Should be replaced with:
List<Resource> resources = await _context.ExecuteStoredProcedureAsync<Resource>(
    "sp_GetPublishedResources",
    new SqlParameter("@IsActive", true));
```

## Stored Procedures to Create

### 1. User Management Procedures

#### sp_GetUserById
```sql
CREATE PROCEDURE [dbo].[sp_GetUserById]
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
        u.IsActive,
        u.CreatedAt,
        u.UpdatedAt
    FROM Users u
    WHERE u.UserId = @UserId
        AND (@IncludeDeleted = 1 OR u.IsDeleted = 0);
END
```

#### sp_CreateUser
```sql
CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @LicenseNumber NVARCHAR(50) = NULL,
    @LicenseState NVARCHAR(2) = NULL,
    @LicenseType NVARCHAR(10) = NULL,
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
            IsActive, IsDeleted, CreatedAt, UpdatedAt
        )
        VALUES (
            @UserId, @Email, @PasswordHash, @FirstName, @LastName,
            @LicenseNumber, @LicenseState, @LicenseType,
            1, 0, GETUTCDATE(), GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
```

#### sp_UpdateUser
```sql
CREATE PROCEDURE [dbo].[sp_UpdateUser]
    @UserId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @LicenseNumber NVARCHAR(50) = NULL,
    @LicenseState NVARCHAR(2) = NULL,
    @LicenseType NVARCHAR(10) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER
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
        UpdatedAt = GETUTCDATE()
    WHERE UserId = @UserId
        AND IsDeleted = 0;
        
    -- Audit trail
    EXEC sp_CreateAuditLog @UserId, 'User', 'Update', @ModifiedBy;
END
```

#### sp_SoftDeleteUser
```sql
CREATE PROCEDURE [dbo].[sp_SoftDeleteUser]
    @UserId UNIQUEIDENTIFIER,
    @DeletedBy UNIQUEIDENTIFIER,
    @Reason NVARCHAR(MAX) = NULL
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
            DeletedBy = @DeletedBy,
            UpdatedAt = GETUTCDATE()
        WHERE UserId = @UserId;
        
        -- Anonymize PII
        UPDATE Users
        SET
            Email = CONCAT('deleted_', UserId, '@deleted.com'),
            FirstName = 'Deleted',
            LastName = 'User',
            LicenseNumber = NULL,
            PasswordHash = NULL
        WHERE UserId = @UserId;
        
        -- Audit trail
        EXEC sp_CreateAuditLog @UserId, 'User', 'Delete', @DeletedBy, @Reason;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
```

### 2. Resource Management Procedures

#### sp_GetResources
```sql
CREATE PROCEDURE [dbo].[sp_GetResources]
    @SearchTerm NVARCHAR(500) = NULL,
    @SkillArea NVARCHAR(100) = NULL,
    @GradeLevel INT = NULL,
    @ResourceType NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Resources r
    WHERE r.IsActive = 1
        AND r.IsDeleted = 0
        AND (@SearchTerm IS NULL OR r.Title LIKE '%' + @SearchTerm + '%' OR r.Description LIKE '%' + @SearchTerm + '%')
        AND (@SkillArea IS NULL OR JSON_VALUE(r.SkillAreas, '$') LIKE '%' + @SkillArea + '%')
        AND (@GradeLevel IS NULL OR @GradeLevel BETWEEN r.MinGradeLevel AND r.MaxGradeLevel)
        AND (@ResourceType IS NULL OR r.ResourceType = @ResourceType);
    
    -- Get paginated results
    WITH ResourceCTE AS (
        SELECT 
            r.*,
            ROW_NUMBER() OVER (
                ORDER BY 
                    CASE WHEN @SortBy = 'Title' AND @SortDirection = 'ASC' THEN r.Title END ASC,
                    CASE WHEN @SortBy = 'Title' AND @SortDirection = 'DESC' THEN r.Title END DESC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortDirection = 'ASC' THEN r.CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortDirection = 'DESC' THEN r.CreatedAt END DESC,
                    CASE WHEN @SortBy = 'Rating' AND @SortDirection = 'ASC' THEN r.Rating END ASC,
                    CASE WHEN @SortBy = 'Rating' AND @SortDirection = 'DESC' THEN r.Rating END DESC
            ) AS RowNum
        FROM Resources r
        WHERE r.IsActive = 1
            AND r.IsDeleted = 0
            AND (@SearchTerm IS NULL OR r.Title LIKE '%' + @SearchTerm + '%' OR r.Description LIKE '%' + @SearchTerm + '%')
            AND (@SkillArea IS NULL OR JSON_VALUE(r.SkillAreas, '$') LIKE '%' + @SkillArea + '%')
            AND (@GradeLevel IS NULL OR @GradeLevel BETWEEN r.MinGradeLevel AND r.MaxGradeLevel)
            AND (@ResourceType IS NULL OR r.ResourceType = @ResourceType)
    )
    SELECT *
    FROM ResourceCTE
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);
END
```

### 3. Student Management Procedures

#### sp_GetStudentsByTherapist
```sql
CREATE PROCEDURE [dbo].[sp_GetStudentsByTherapist]
    @TherapistId UNIQUEIDENTIFIER,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.StudentId,
        s.FirstNameEncrypted,
        s.LastNameEncrypted,
        s.DateOfBirthEncrypted,
        s.GradeLevel,
        s.AccessCode,
        s.IsActive,
        s.CreatedAt,
        s.UpdatedAt,
        COUNT(g.GoalId) as ActiveGoalCount
    FROM Students s
    LEFT JOIN StudentGoals g ON s.StudentId = g.StudentId AND g.Status = 'Active'
    WHERE s.TherapistId = @TherapistId
        AND s.IsDeleted = 0
        AND (@IncludeInactive = 1 OR s.IsActive = 1)
    GROUP BY 
        s.StudentId, s.FirstNameEncrypted, s.LastNameEncrypted,
        s.DateOfBirthEncrypted, s.GradeLevel, s.AccessCode,
        s.IsActive, s.CreatedAt, s.UpdatedAt
    ORDER BY s.LastNameEncrypted, s.FirstNameEncrypted;
END
```

### 4. Marketplace Procedures

#### sp_ProcessMarketplacePurchase
```sql
CREATE PROCEDURE [dbo].[sp_ProcessMarketplacePurchase]
    @BuyerId UNIQUEIDENTIFIER,
    @ProductIds NVARCHAR(MAX), -- JSON array of product IDs
    @CouponCode NVARCHAR(50) = NULL,
    @PaymentIntentId NVARCHAR(255),
    @TransactionId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @Products TABLE (
            ProductId UNIQUEIDENTIFIER,
            SellerId UNIQUEIDENTIFIER,
            Price DECIMAL(10,2)
        );
        
        -- Parse product IDs and get details
        INSERT INTO @Products
        SELECT 
            r.ResourceId,
            r.CreatedByUserId,
            r.Price
        FROM Resources r
        INNER JOIN OPENJSON(@ProductIds) AS p ON r.ResourceId = p.value
        WHERE r.IsPublished = 1 AND r.Price > 0;
        
        -- Calculate totals and commission
        DECLARE @Subtotal DECIMAL(10,2);
        DECLARE @DiscountAmount DECIMAL(10,2) = 0;
        DECLARE @Commission DECIMAL(10,2);
        
        SELECT @Subtotal = SUM(Price) FROM @Products;
        
        -- Apply coupon if valid
        IF @CouponCode IS NOT NULL
        BEGIN
            EXEC sp_ApplyCoupon @CouponCode, @Subtotal, @DiscountAmount OUTPUT;
        END
        
        SET @Commission = (@Subtotal - @DiscountAmount) * 0.30; -- 30% platform fee
        
        -- Create transaction
        SET @TransactionId = NEWID();
        
        INSERT INTO MarketplaceTransactions (
            TransactionId, BuyerId, Amount, Commission,
            CouponCode, DiscountAmount, PaymentStatus,
            StripePaymentIntentId, CreatedAt
        )
        VALUES (
            @TransactionId, @BuyerId, @Subtotal - @DiscountAmount, @Commission,
            @CouponCode, @DiscountAmount, 'Completed',
            @PaymentIntentId, GETUTCDATE()
        );
        
        -- Create transaction items
        INSERT INTO MarketplaceTransactionItems (
            TransactionId, ResourceId, SellerId, Amount, Commission
        )
        SELECT 
            @TransactionId,
            ProductId,
            SellerId,
            Price,
            Price * 0.30
        FROM @Products;
        
        -- Grant access to purchased resources
        INSERT INTO UserResources (UserId, ResourceId, PurchasedAt, TransactionId)
        SELECT @BuyerId, ProductId, GETUTCDATE(), @TransactionId
        FROM @Products;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
```

## tSQLt Test Examples

### 1. Test User Creation
```sql
EXEC tSQLt.NewTestClass 'UserTests';
GO

CREATE PROCEDURE [UserTests].[test sp_CreateUser creates user successfully]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @Email NVARCHAR(256) = 'test@example.com';
    DECLARE @PasswordHash NVARCHAR(MAX) = 'hashedpassword';
    DECLARE @FirstName NVARCHAR(100) = 'John';
    DECLARE @LastName NVARCHAR(100) = 'Doe';
    DECLARE @UserId UNIQUEIDENTIFIER;
    
    -- Act
    EXEC sp_CreateUser 
        @Email = @Email,
        @PasswordHash = @PasswordHash,
        @FirstName = @FirstName,
        @LastName = @LastName,
        @UserId = @UserId OUTPUT;
    
    -- Assert
    EXEC tSQLt.AssertEquals 1, (SELECT COUNT(*) FROM Users);
    
    IF NOT EXISTS (
        SELECT 1 FROM Users 
        WHERE Email = @Email 
            AND FirstName = @FirstName 
            AND LastName = @LastName
            AND UserId = @UserId
    )
    BEGIN
        EXEC tSQLt.Fail 'User was not created with correct values';
    END
END;
GO
```

### 2. Test User Soft Delete
```sql
CREATE PROCEDURE [UserTests].[test sp_SoftDeleteUser anonymizes user data]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    EXEC tSQLt.FakeTable 'dbo.AuditLogs';
    
    DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
    DECLARE @DeletedBy UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Users (UserId, Email, FirstName, LastName, IsDeleted, IsActive)
    VALUES (@UserId, 'john@example.com', 'John', 'Doe', 0, 1);
    
    -- Act
    EXEC sp_SoftDeleteUser 
        @UserId = @UserId,
        @DeletedBy = @DeletedBy,
        @Reason = 'User requested deletion';
    
    -- Assert
    DECLARE @ActualEmail NVARCHAR(256);
    DECLARE @ActualFirstName NVARCHAR(100);
    DECLARE @ActualIsDeleted BIT;
    
    SELECT 
        @ActualEmail = Email,
        @ActualFirstName = FirstName,
        @ActualIsDeleted = IsDeleted
    FROM Users
    WHERE UserId = @UserId;
    
    EXEC tSQLt.AssertEquals 1, @ActualIsDeleted;
    EXEC tSQLt.AssertEquals 'Deleted', @ActualFirstName;
    EXEC tSQLt.AssertLike '%deleted%', @ActualEmail;
END;
GO
```

### 3. Test Resource Search
```sql
EXEC tSQLt.NewTestClass 'ResourceTests';
GO

CREATE PROCEDURE [ResourceTests].[test sp_GetResources filters by search term]
AS
BEGIN
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Resources';
    
    INSERT INTO Resources (ResourceId, Title, Description, IsActive, IsDeleted)
    VALUES 
        (NEWID(), 'Math Worksheet', 'Basic addition', 1, 0),
        (NEWID(), 'Science Activity', 'Chemistry lab', 1, 0),
        (NEWID(), 'Math Game', 'Multiplication practice', 1, 0),
        (NEWID(), 'Reading Exercise', 'Comprehension', 1, 0);
    
    DECLARE @TotalCount INT;
    
    -- Act
    EXEC sp_GetResources 
        @SearchTerm = 'Math',
        @PageNumber = 1,
        @PageSize = 10,
        @TotalCount = @TotalCount OUTPUT;
    
    -- Assert
    EXEC tSQLt.AssertEquals 2, @TotalCount;
END;
GO
```

### 4. Test Concurrent Updates
```sql
CREATE PROCEDURE [UserTests].[test sp_UpdateUser handles concurrent updates]
AS
BEGIN
    -- This test would use tSQLt's ability to test for expected exceptions
    -- when concurrent updates violate constraints
    
    -- Arrange
    EXEC tSQLt.FakeTable 'dbo.Users';
    
    DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Users (UserId, FirstName, LastName, UpdatedAt, IsDeleted)
    VALUES (@UserId, 'John', 'Doe', GETUTCDATE(), 0);
    
    -- Act & Assert
    -- Would need to simulate concurrent access using tSQLt features
    -- This is a placeholder for the actual implementation
END;
GO
```

## Repository Pattern Updates

### Current Repository Implementation
```csharp
public class UserRepository : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        // Current - Direct EF query
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}
```

### Updated Repository with Stored Procedures
```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        SqlParameter[] parameters = {
            new SqlParameter("@UserId", userId),
            new SqlParameter("@IncludeDeleted", false)
        };
        
        List<User> users = await _context.Users
            .FromSqlRaw("EXEC sp_GetUserById @UserId, @IncludeDeleted", parameters)
            .ToListAsync();
            
        return users.FirstOrDefault();
    }
    
    public async Task<Guid> CreateAsync(User user)
    {
        SqlParameter userIdParam = new SqlParameter("@UserId", SqlDbType.UniqueIdentifier)
        {
            Direction = ParameterDirection.Output
        };
        
        SqlParameter[] parameters = {
            new SqlParameter("@Email", user.Email),
            new SqlParameter("@PasswordHash", user.PasswordHash),
            new SqlParameter("@FirstName", user.FirstName),
            new SqlParameter("@LastName", user.LastName),
            new SqlParameter("@LicenseNumber", user.LicenseNumber ?? (object)DBNull.Value),
            new SqlParameter("@LicenseState", user.LicenseState ?? (object)DBNull.Value),
            new SqlParameter("@LicenseType", user.LicenseType ?? (object)DBNull.Value),
            userIdParam
        };
        
        await _context.Database
            .ExecuteSqlRawAsync("EXEC sp_CreateUser @Email, @PasswordHash, @FirstName, @LastName, @LicenseNumber, @LicenseState, @LicenseType, @UserId OUTPUT", parameters);
            
        return (Guid)userIdParam.Value;
    }
}
```

## Migration Steps

1. **Create Stored Procedures**
   - Create all stored procedures in SQL Server
   - Add proper error handling and transaction management
   - Include audit trail procedures

2. **Create tSQLt Tests**
   - Install tSQLt framework
   - Create test classes for each module
   - Write comprehensive tests for all procedures

3. **Update Repository Layer**
   - Replace all LINQ queries with stored procedure calls
   - Add proper parameter handling
   - Handle output parameters and return values

4. **Update Service Layer**
   - Ensure services work with updated repositories
   - Add proper error handling for SQL exceptions
   - Update unit tests to mock stored procedure calls

5. **Performance Testing**
   - Compare performance of stored procedures vs LINQ
   - Optimize stored procedures with proper indexing
   - Add execution plan analysis

## Security Considerations

1. **SQL Injection Prevention**
   - Always use parameterized stored procedures
   - Never concatenate user input into SQL
   - Validate all inputs before passing to procedures

2. **Permission Management**
   - Create database roles for different access levels
   - Grant EXECUTE permissions only on required procedures
   - Deny direct table access

3. **Audit Trail**
   - All data modifications through procedures
   - Comprehensive audit logging
   - Cannot be bypassed by application

## Performance Benefits

1. **Query Plan Caching**
   - Stored procedures compile once
   - Execution plans are cached
   - Better performance for complex queries

2. **Reduced Network Traffic**
   - Only parameters sent to database
   - Complex logic executed on server
   - Less data transferred

3. **Better Security**
   - Reduced attack surface
   - Parameterized queries prevent SQL injection
   - Database-level security enforcement