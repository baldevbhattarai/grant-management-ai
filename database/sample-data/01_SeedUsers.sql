-- =============================================
-- Grant Management AI - Sample Data
-- Step 2: Insert Users
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Insert 5 Grantee Users
-- =============================================

DECLARE @User1 UNIQUEIDENTIFIER = NEWID();
DECLARE @User2 UNIQUEIDENTIFIER = NEWID();
DECLARE @User3 UNIQUEIDENTIFIER = NEWID();
DECLARE @User4 UNIQUEIDENTIFIER = NEWID();
DECLARE @User5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (UserId, Email, FirstName, LastName, Role, OrganizationName, IsActive)
VALUES 
    (@User1, 'john.smith@healthcenter1.org', 'John', 'Smith', 'Grantee', 'Community Health Center of Springfield', 1),
    (@User2, 'sarah.johnson@communityclinic.org', 'Sarah', 'Johnson', 'Grantee', 'Migrant Community Clinic', 1),
    (@User3, 'michael.brown@ruralhealth.org', 'Michael', 'Brown', 'Grantee', 'Rural Health Services', 1),
    (@User4, 'emily.davis@urbancare.org', 'Emily', 'Davis', 'Grantee', 'Urban Care Center', 1),
    (@User5, 'david.wilson@coastalhealth.org', 'David', 'Wilson', 'Grantee', 'Coastal Health Network', 1);

PRINT 'Inserted 5 users';
GO

-- Store UserIds for reference in next scripts
SELECT UserId, Email, FirstName, LastName, OrganizationName 
FROM Users 
ORDER BY CreatedDate;
GO
