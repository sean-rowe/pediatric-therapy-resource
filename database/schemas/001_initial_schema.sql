-- Initial database schema for therapy-docs
-- This file is required for CI/CD pipeline to run successfully

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TherapyDocs')
BEGIN
    CREATE DATABASE TherapyDocs;
END
GO

USE TherapyDocs;
GO

-- Users table (already exists based on the codebase)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[users] (
        [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [email] NVARCHAR(255) NOT NULL,
        [passwordHash] NVARCHAR(255) NOT NULL,
        [role] NVARCHAR(50) NOT NULL,
        [isActive] BIT NOT NULL DEFAULT 1,
        [isEmailVerified] BIT NOT NULL DEFAULT 0,
        [createdAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [updatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [UQ_users_email] UNIQUE NONCLUSTERED ([email] ASC)
    );
END
GO

-- Add indexes for performance (as mentioned in PR review)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_users_email' AND object_id = OBJECT_ID('users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_users_email] ON [dbo].[users] ([email] ASC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_users_isActive' AND object_id = OBJECT_ID('users'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_users_isActive] ON [dbo].[users] ([isActive] ASC);
END
GO