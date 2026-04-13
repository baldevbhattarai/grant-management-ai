-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert Grants
-- =============================================

USE GrantDB;
GO

-- Get User IDs
DECLARE @User1 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Email = 'john.smith@healthcenter1.org');
DECLARE @User2 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Email = 'sarah.johnson@communityclinic.org');
DECLARE @User3 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Email = 'michael.brown@ruralhealth.org');
DECLARE @User4 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Email = 'emily.davis@urbancare.org');
DECLARE @User5 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Email = 'david.wilson@coastalhealth.org');

-- =============================================
-- Insert 5 Grants (Different Types)
-- =============================================

INSERT INTO Grants (GrantNumber, UserId, GrantType, ProgramName, ProgramTypeCode, FocusAreas, FundingAmount, StartDate, EndDate, Status)
VALUES 
    -- Grant 1: C16 - Community Health Center
    ('H80CS00001', @User1, 'C16', 'Community Health Center Program', 60, 
     '["Primary Care", "Behavioral Health", "Dental Services", "Enabling Services"]', 
     1500000.00, '2023-01-01', '2027-12-31', 'Active'),
    
    -- Grant 2: C17 - Migrant Health Center
    ('H80CS00002', @User2, 'C17', 'Migrant Health Center Program', 61, 
     '["Primary Care", "Agricultural Worker Health", "Mobile Services", "Outreach"]', 
     800000.00, '2023-01-01', '2027-12-31', 'Active'),
    
    -- Grant 3: C16 - Community Health Center (Rural)
    ('H80CS00003', @User3, 'C16', 'Community Health Center Program', 60, 
     '["Primary Care", "Telehealth", "Chronic Disease Management", "Dental Services"]', 
     1200000.00, '2023-01-01', '2027-12-31', 'Active'),
    
    -- Grant 4: H80 - School-Based Health Center
    ('H80CS00004', @User4, 'H80', 'School-Based Health Center Program', 65, 
     '["Adolescent Health", "Mental Health", "Preventive Care", "Health Education"]', 
     600000.00, '2023-01-01', '2027-12-31', 'Active'),
    
    -- Grant 5: C18 - Health Care for the Homeless
    ('H80CS00005', @User5, 'C18', 'Health Care for the Homeless Program', 62, 
     '["Primary Care", "Behavioral Health", "Substance Abuse", "Housing Support"]', 
     900000.00, '2023-01-01', '2027-12-31', 'Active');

PRINT 'Inserted 5 grants';
GO

-- Display grants
SELECT GrantNumber, GrantType, ProgramName, FundingAmount, 
       (SELECT Email FROM Users WHERE UserId = Grants.UserId) AS GranteeEmail
FROM Grants
ORDER BY GrantNumber;
GO
