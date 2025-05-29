-- Create TherapyDocs database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TherapyDocs')
BEGIN
    CREATE DATABASE TherapyDocs;
END
GO

USE TherapyDocs;
GO