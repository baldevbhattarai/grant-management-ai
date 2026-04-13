-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert 2025 Q1 Reports (Draft - Empty)
-- These will be used to demonstrate AI suggestions
-- =============================================

USE GrantDB;
GO

-- Get Grant IDs
DECLARE @Grant1 UNIQUEIDENTIFIER = (SELECT GrantId FROM Grants WHERE GrantNumber = 'H80CS00001');
DECLARE @Grant2 UNIQUEIDENTIFIER = (SELECT GrantId FROM Grants WHERE GrantNumber = 'H80CS00002');
DECLARE @Grant3 UNIQUEIDENTIFIER = (SELECT GrantId FROM Grants WHERE GrantNumber = 'H80CS00003');
DECLARE @Grant4 UNIQUEIDENTIFIER = (SELECT GrantId FROM Grants WHERE GrantNumber = 'H80CS00004');
DECLARE @Grant5 UNIQUEIDENTIFIER = (SELECT GrantId FROM Grants WHERE GrantNumber = 'H80CS00005');

-- =============================================
-- Insert 2025 Q1 Reports (All in Draft status, empty)
-- =============================================

INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status)
VALUES 
    (@Grant1, 2025, 'Q1', 'Progress', 'Draft'),
    (@Grant2, 2025, 'Q1', 'Progress', 'Draft'),
    (@Grant3, 2025, 'Q1', 'Progress', 'Draft'),
    (@Grant4, 2025, 'Q1', 'Progress', 'Draft'),
    (@Grant5, 2025, 'Q1', 'Progress', 'Draft');

PRINT 'Inserted 5 draft reports for 2025 Q1';
GO

-- =============================================
-- Create empty sections for 2025 Q1 reports
-- These will be filled using AI suggestions
-- =============================================

-- Get 2025 Q1 Report IDs
DECLARE @Report1_2025Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'H80CS00001' AND r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
);

DECLARE @Report2_2025Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'H80CS00002' AND r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
);

DECLARE @Report3_2025Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'H80CS00003' AND r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
);

DECLARE @Report4_2025Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'H80CS00004' AND r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
);

DECLARE @Report5_2025Q1 UNIQUEIDENTIFIER = (
    SELECT r.ReportId FROM Reports r
    JOIN Grants g ON r.GrantId = g.GrantId
    WHERE g.GrantNumber = 'H80CS00005' AND r.ReportingYear = 2025 AND r.ReportingQuarter = 'Q1'
);

-- Insert empty sections for all 5 reports
DECLARE @ReportIds TABLE (ReportId UNIQUEIDENTIFIER);
INSERT INTO @ReportIds VALUES (@Report1_2025Q1), (@Report2_2025Q1), (@Report3_2025Q1), (@Report4_2025Q1), (@Report5_2025Q1);

-- Create standard sections for each report
INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT 
    ReportId,
    'PerformanceNarrative',
    'Performance Narrative',
    1,
    'Describe your progress toward grant objectives during this reporting period.',
    'Text',
    NULL, -- Empty - AI will suggest
    2000
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT 
    ReportId,
    'KeyAccomplishments',
    'Key Accomplishments',
    2,
    'List the major accomplishments achieved during this quarter.',
    'Text',
    NULL, -- Empty - AI will suggest
    1500
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT 
    ReportId,
    'ChallengesBarriers',
    'Challenges and Barriers',
    3,
    'Describe any challenges or barriers encountered and your mitigation strategies.',
    'Text',
    NULL, -- Empty - AI will suggest
    1500
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseNumber)
SELECT 
    ReportId,
    'PatientsServed',
    'Total Patients Served',
    4,
    'Enter the total number of unique patients served during this quarter.',
    'Number',
    NULL, -- Empty - user will fill
    NULL
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseOptions)
SELECT 
    ReportId,
    'ServicesProvided',
    'Services Provided',
    5,
    'Select all services provided during this reporting period.',
    'MultiSelect',
    NULL, -- Empty - user will select
    NULL
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseSingle)
SELECT 
    ReportId,
    'TelehealthAdoption',
    'Telehealth Services',
    6,
    'Are you currently providing telehealth services?',
    'Radio',
    NULL, -- Empty - user will select
    NULL
FROM @ReportIds;

INSERT INTO ReportSections (ReportId, SectionName, SectionTitle, SectionOrder, QuestionText, ResponseType, ResponseText, MaxLength)
SELECT 
    ReportId,
    'StaffingUpdates',
    'Staffing Updates',
    7,
    'Describe any significant staffing changes or updates.',
    'Text',
    NULL, -- Empty - AI will suggest
    1000
FROM @ReportIds;

PRINT 'Inserted empty sections for all 2025 Q1 reports';
GO

-- Display 2025 reports
SELECT 
    g.GrantNumber,
    g.GrantType,
    r.ReportingYear,
    r.ReportingQuarter,
    r.Status,
    COUNT(rs.SectionId) as TotalSections,
    SUM(CASE WHEN rs.ResponseText IS NULL AND rs.ResponseType = 'Text' THEN 1 ELSE 0 END) as EmptySections
FROM Reports r
JOIN Grants g ON r.GrantId = g.GrantId
LEFT JOIN ReportSections rs ON r.ReportId = rs.ReportId
WHERE r.ReportingYear = 2025
GROUP BY g.GrantNumber, g.GrantType, r.ReportingYear, r.ReportingQuarter, r.Status
ORDER BY g.GrantNumber;
GO
