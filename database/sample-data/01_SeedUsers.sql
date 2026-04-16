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
    (@User1, 'alex.rivera@mapleclinic.example', 'Alex', 'Rivera', 'Grantee', 'Maple Valley Health Center', 1),
    (@User2, 'jordan.park@sunrisehc.example', 'Jordan', 'Park', 'Grantee', 'Sunrise Community Health', 1),
    (@User3, 'morgan.chen@pinecrestmed.example', 'Morgan', 'Chen', 'Grantee', 'Pinecrest Medical Services', 1),
    (@User4, 'taylor.reeves@bridgewaycare.example', 'Taylor', 'Reeves', 'Grantee', 'Bridgeway Care Center', 1),
    (@User5, 'casey.monroe@lakesidehc.example', 'Casey', 'Monroe', 'Grantee', 'Lakeside Health Collective', 1);

PRINT 'Inserted 5 users';
GO

-- Store UserIds for reference in next scripts
SELECT UserId, Email, FirstName, LastName, OrganizationName 
FROM Users 
ORDER BY CreatedDate;
GO
