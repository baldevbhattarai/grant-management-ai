-- =============================================
-- Grant Management AI - MASTER SETUP SCRIPT
-- Execute this script to set up the complete database
-- =============================================

PRINT '========================================';
PRINT 'Starting Grant Management AI Database Setup';
PRINT '========================================';
PRINT '';

-- Step 1: Create Database
PRINT 'Step 1: Creating Database...';
:r "schema\01_CreateDatabase.sql"
PRINT '';

-- Step 2: Create Tables
PRINT 'Step 2: Creating Tables...';
:r "schema\02_CreateTables.sql"
PRINT '';

-- Step 3: Create Indexes
PRINT 'Step 3: Creating Indexes...';
:r "schema\03_CreateIndexes.sql"
PRINT '';

-- Step 4: Seed Users
PRINT 'Step 4: Seeding Users...';
:r "sample-data\01_SeedUsers.sql"
PRINT '';

-- Step 5: Seed Grants
PRINT 'Step 5: Seeding Grants...';
:r "sample-data\02_SeedGrants.sql"
PRINT '';

-- Step 6: Seed 2024 Reports
PRINT 'Step 6: Seeding 2024 Reports...';
:r "sample-data\03_SeedReports_2024.sql"
PRINT '';

-- Step 7: Seed 2024 Q1 Report Sections
PRINT 'Step 7: Seeding 2024 Q1 Report Sections...';
:r "sample-data\04_SeedReportSections_2024_Q1.sql"
PRINT '';

-- Step 8: Seed Remaining 2024 Q1 Sections
PRINT 'Step 8: Seeding Remaining 2024 Q1 Sections...';
:r "sample-data\05_SeedReportSections_2024_Remaining.sql"
PRINT '';

-- Step 9: Seed 2025 Draft Reports
PRINT 'Step 9: Seeding 2025 Draft Reports (Empty)...';
:r "sample-data\06_SeedReports_2025_Draft.sql"
PRINT '';

-- Step 10: Seed Approved Content
PRINT 'Step 10: Populating AI Approved Content...';
:r "sample-data\07_SeedApprovedContent.sql"
PRINT '';

PRINT '========================================';
PRINT 'Database Setup Complete!';
PRINT '========================================';
PRINT '';

-- Final Summary
USE GrantDB;
GO

PRINT 'DATABASE SUMMARY:';
PRINT '==================';
PRINT '';

PRINT 'Users:';
SELECT COUNT(*) as TotalUsers FROM Users;
PRINT '';

PRINT 'Grants:';
SELECT COUNT(*) as TotalGrants FROM Grants;
PRINT '';

PRINT 'Reports by Year:';
SELECT ReportingYear, Status, COUNT(*) as Count
FROM Reports
GROUP BY ReportingYear, Status
ORDER BY ReportingYear, Status;
PRINT '';

PRINT 'Report Sections:';
SELECT COUNT(*) as TotalSections FROM ReportSections;
PRINT '';

PRINT 'Approved Content for AI:';
SELECT COUNT(*) as TotalApprovedContent FROM AI_ApprovedContent;
PRINT '';

PRINT '2025 Q1 Reports (Ready for AI Suggestions):';
SELECT 
    g.GrantNumber,
    g.GrantType,
    u.FirstName + ' ' + u.LastName as Grantee,
    COUNT(rs.SectionId) as TotalSections,
    SUM(CASE WHEN rs.ResponseText IS NULL AND rs.ResponseType = 'Text' THEN 1 ELSE 0 END) as EmptyTextSections
FROM Reports r
JOIN Grants g ON r.GrantId = g.GrantId
JOIN Users u ON g.UserId = u.UserId
LEFT JOIN ReportSections rs ON r.ReportId = rs.ReportId
WHERE r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
GROUP BY g.GrantNumber, g.GrantType, u.FirstName, u.LastName
ORDER BY g.GrantNumber;

PRINT '';
PRINT '========================================';
PRINT 'Ready to implement .NET Web API!';
PRINT '========================================';
GO
