-- Master script to create all stored procedures
-- Run this script to set up the database for stored procedure usage

-- First, ensure tSQLt is installed (if using tests)
-- Download from: https://tsqlt.org/downloads/

-- Create stored procedures
PRINT 'Creating User Management stored procedures...'
:r ../StoredProcedures/UserManagement.sql
GO

PRINT 'Creating additional User Management stored procedures...'
:r ../StoredProcedures/UserManagementAdditional.sql
GO

-- Create other entity stored procedures as they are developed
-- :r ../StoredProcedures/ResourceManagement.sql
-- :r ../StoredProcedures/StudentManagement.sql
-- :r ../StoredProcedures/MarketplaceManagement.sql

PRINT 'All stored procedures created successfully!'
GO

-- To run tests (requires tSQLt):
-- :r ../Tests/UserManagementTests.sql
-- EXEC tSQLt.Run 'UserManagementTests';