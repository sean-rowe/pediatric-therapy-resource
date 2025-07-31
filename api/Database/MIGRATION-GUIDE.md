# Stored Procedures Migration Guide

## Overview
This guide explains how to migrate from Entity Framework LINQ queries to stored procedures with tSQLt testing.

## Why Stored Procedures?

1. **Performance**: Compiled execution plans, reduced network traffic
2. **Security**: Prevention of SQL injection, principle of least privilege
3. **Maintainability**: Database logic centralized
4. **Testing**: tSQLt provides robust database unit testing

## Migration Steps

### 1. Install tSQLt (Optional for Testing)
```sql
-- Download from https://tsqlt.org/downloads/
-- Run tSQLt installation script
EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;
-- Run tSQLt.class.sql
```

### 2. Create Database Objects
```sql
-- Run the master script to create all stored procedures
SQLCMD -S localhost,1433 -U SA -P TherapyDocs2024! -d TherapyDocs -i Database/Scripts/CreateStoredProcedures.sql

-- Or run individual scripts:
-- User Management
SQLCMD -i Database/StoredProcedures/UserManagement.sql
SQLCMD -i Database/StoredProcedures/UserManagementAdditional.sql

-- Resource Management
SQLCMD -i Database/StoredProcedures/ResourceManagement.sql
```

### 3. Run Tests (if tSQLt installed)
```sql
-- Run all user management tests
EXEC tSQLt.Run 'UserManagementTests';

-- Run specific test
EXEC tSQLt.Run 'UserManagementTests.[test sp_GetUserById returns user when exists]';
```

### 4. Configure Application
In `appsettings.json` or `appsettings.Development.json`:
```json
{
  "UseStoredProcedures": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TherapyDocs;User Id=SA;Password=TherapyDocs2024!;Encrypt=false;TrustServerCertificate=true"
  }
}
```

## Repository Pattern Implementation

### Before (Entity Framework):
```csharp
public async Task<User?> GetByIdAsync(Guid id)
{
    return await _dbSet
        .Where(u => !u.IsDeleted)
        .FirstOrDefaultAsync(u => u.UserId == id);
}
```

### After (Stored Procedure):
```csharp
public async Task<User?> GetByIdAsync(Guid id)
{
    using SqlConnection connection = new SqlConnection(_connectionString);
    using SqlCommand command = new SqlCommand("sp_GetUserById", connection);
    command.CommandType = CommandType.StoredProcedure;
    command.Parameters.AddWithValue("@UserId", id);
    command.Parameters.AddWithValue("@IncludeDeleted", false);

    await connection.OpenAsync();
    using SqlDataReader reader = await command.ExecuteReaderAsync();
    
    if (await reader.ReadAsync())
    {
        return MapUserFromReader(reader);
    }
    
    return null;
}
```

## Stored Procedure Naming Convention

- `sp_Get[Entity]ById` - Get single entity
- `sp_Get[Entities]ByFilter` - Get filtered list with pagination
- `sp_Create[Entity]` - Insert new entity
- `sp_Update[Entity]` - Update existing entity
- `sp_SoftDelete[Entity]` - Soft delete entity
- `sp_[Action][Entity]` - Specific actions (e.g., sp_VerifyUserLicense)

## Parameter Patterns

### Pagination:
```sql
@PageNumber INT = 1,
@PageSize INT = 20,
@TotalCount INT OUTPUT
```

### Soft Delete Support:
```sql
@IncludeDeleted BIT = 0
```

### Optional Updates (NULL preserves existing value):
```sql
UPDATE Users
SET 
    FirstName = ISNULL(@FirstName, FirstName),
    LastName = ISNULL(@LastName, LastName)
```

## JSON Handling

### Storing JSON:
```sql
@Languages NVARCHAR(MAX) = '["English"]',
@Specialties NVARCHAR(MAX) = '[]'
```

### C# Serialization:
```csharp
command.Parameters.AddWithValue("@Languages", JsonConvert.SerializeObject(entity.Languages));
```

### C# Deserialization:
```csharp
string languagesJson = reader.GetString(ordinal);
user.Languages = JsonConvert.DeserializeObject<List<string>>(languagesJson) ?? new List<string>();
```

## Error Handling

### Stored Procedure:
```sql
BEGIN TRY
    BEGIN TRANSACTION;
    -- Operations
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH
```

### C# Repository:
```csharp
try
{
    await connection.OpenAsync();
    await command.ExecuteNonQueryAsync();
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Database error in {Method}", nameof(CreateUser));
    throw;
}
```

## Security Best Practices

1. **Never concatenate SQL strings** - Always use parameters
2. **Use least privilege** - Create specific database users:
   ```sql
   CREATE USER [uptrms_app] WITH PASSWORD = 'StrongPassword123!';
   GRANT EXECUTE ON SCHEMA::[dbo] TO [uptrms_app];
   ```

3. **Validate inputs** in stored procedures:
   ```sql
   IF @Email IS NULL OR @Email = ''
       THROW 50001, 'Email is required', 1;
   ```

4. **Audit sensitive operations**:
   ```sql
   INSERT INTO AuditLogs (Action, UserId, Details, CreatedAt)
   VALUES ('UserDeleted', @UserId, @Details, GETUTCDATE());
   ```

## Performance Optimization

1. **Use appropriate indexes**:
   ```sql
   CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = 0;
   CREATE INDEX IX_Resources_SkillAreas ON Resources(SkillAreas) INCLUDE (Title, ResourceType);
   ```

2. **Avoid N+1 queries** - Use joins or multiple result sets:
   ```sql
   -- Return user and organization in one call
   SELECT u.*, o.*
   FROM Users u
   LEFT JOIN Organizations o ON u.OrganizationId = o.OrganizationId
   WHERE u.UserId = @UserId;
   ```

3. **Use `SET NOCOUNT ON`** to reduce network traffic

## Monitoring and Maintenance

1. **Query execution plans**:
   ```sql
   SET STATISTICS IO ON;
   SET STATISTICS TIME ON;
   EXEC sp_GetResourcesByFilter @SearchTerm = 'math';
   ```

2. **Missing index analysis**:
   ```sql
   SELECT * FROM sys.dm_db_missing_index_details;
   ```

3. **Stored procedure usage stats**:
   ```sql
   SELECT 
       p.name,
       ps.execution_count,
       ps.total_elapsed_time / ps.execution_count AS avg_elapsed_time,
       ps.last_execution_time
   FROM sys.procedures p
   JOIN sys.dm_exec_procedure_stats ps ON p.object_id = ps.object_id
   ORDER BY ps.execution_count DESC;
   ```

## Rollback Strategy

If you need to rollback to Entity Framework:
1. Set `UseStoredProcedures` to `false` in appsettings
2. The original EF repositories are still available
3. No database changes required

## Next Steps

1. Complete migration of remaining entities:
   - [ ] Student Management
   - [ ] Session Management
   - [ ] Marketplace Transactions
   - [ ] Audit Logs

2. Create comprehensive tSQLt tests for all procedures

3. Performance baseline testing

4. Security audit of database permissions

5. Set up automated deployment pipeline for database changes