-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert 2024 Reports (Approved)
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
-- Insert 2024 Reports (Q1-Q4 for each grant)
-- All reports are APPROVED with high ratings
-- =============================================

-- Grant 1 - 2024 Reports
INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status, SubmittedDate, ApprovedDate, ReviewerRating)
VALUES 
    (@Grant1, 2024, 'Q1', 'Progress', 'Approved', '2024-04-15', '2024-04-20', 5),
    (@Grant1, 2024, 'Q2', 'Progress', 'Approved', '2024-07-15', '2024-07-20', 5),
    (@Grant1, 2024, 'Q3', 'Progress', 'Approved', '2024-10-15', '2024-10-20', 4),
    (@Grant1, 2024, 'Q4', 'Progress', 'Approved', '2025-01-15', '2025-01-20', 5);

-- Grant 2 - 2024 Reports
INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status, SubmittedDate, ApprovedDate, ReviewerRating)
VALUES 
    (@Grant2, 2024, 'Q1', 'Progress', 'Approved', '2024-04-15', '2024-04-20', 4),
    (@Grant2, 2024, 'Q2', 'Progress', 'Approved', '2024-07-15', '2024-07-20', 5),
    (@Grant2, 2024, 'Q3', 'Progress', 'Approved', '2024-10-15', '2024-10-20', 5),
    (@Grant2, 2024, 'Q4', 'Progress', 'Approved', '2025-01-15', '2025-01-20', 4);

-- Grant 3 - 2024 Reports
INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status, SubmittedDate, ApprovedDate, ReviewerRating)
VALUES 
    (@Grant3, 2024, 'Q1', 'Progress', 'Approved', '2024-04-15', '2024-04-20', 5),
    (@Grant3, 2024, 'Q2', 'Progress', 'Approved', '2024-07-15', '2024-07-20', 4),
    (@Grant3, 2024, 'Q3', 'Progress', 'Approved', '2024-10-15', '2024-10-20', 5),
    (@Grant3, 2024, 'Q4', 'Progress', 'Approved', '2025-01-15', '2025-01-20', 5);

-- Grant 4 - 2024 Reports
INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status, SubmittedDate, ApprovedDate, ReviewerRating)
VALUES 
    (@Grant4, 2024, 'Q1', 'Progress', 'Approved', '2024-04-15', '2024-04-20', 4),
    (@Grant4, 2024, 'Q2', 'Progress', 'Approved', '2024-07-15', '2024-07-20', 5),
    (@Grant4, 2024, 'Q3', 'Progress', 'Approved', '2024-10-15', '2024-10-20', 4),
    (@Grant4, 2024, 'Q4', 'Progress', 'Approved', '2025-01-15', '2025-01-20', 5);

-- Grant 5 - 2024 Reports
INSERT INTO Reports (GrantId, ReportingYear, ReportingQuarter, ReportType, Status, SubmittedDate, ApprovedDate, ReviewerRating)
VALUES 
    (@Grant5, 2024, 'Q1', 'Progress', 'Approved', '2024-04-15', '2024-04-20', 5),
    (@Grant5, 2024, 'Q2', 'Progress', 'Approved', '2024-07-15', '2024-07-20', 4),
    (@Grant5, 2024, 'Q3', 'Progress', 'Approved', '2024-10-15', '2024-10-20', 5),
    (@Grant5, 2024, 'Q4', 'Progress', 'Approved', '2025-01-15', '2025-01-20', 4);

PRINT 'Inserted 20 reports for 2024 (all approved)';
GO

-- Display reports
SELECT 
    g.GrantNumber,
    r.ReportingYear,
    r.ReportingQuarter,
    r.Status,
    r.ReviewerRating
FROM Reports r
JOIN Grants g ON r.GrantId = g.GrantId
WHERE r.ReportingYear = 2024
ORDER BY g.GrantNumber, r.ReportingQuarter;
GO
