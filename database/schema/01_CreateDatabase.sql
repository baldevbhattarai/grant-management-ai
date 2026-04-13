-- =============================================
-- Grant Management AI - Database Creation
-- Step 1: Create Database
-- =============================================

USE master;
GO

-- Drop database if exists (for clean setup)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'GrantDB')
BEGIN
    ALTER DATABASE GrantDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GrantDB;
END
GO

-- Create database
CREATE DATABASE GrantDB;
GO

USE GrantDB;
GO

PRINT 'Database GrantDB created successfully';
GO
