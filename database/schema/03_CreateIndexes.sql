-- =============================================
-- Grant Management AI - Index Creation
-- Step 1: Create Indexes for Performance
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Indexes for Grants Table
-- =============================================
CREATE INDEX IX_Grants_UserId ON Grants(UserId);
CREATE INDEX IX_Grants_GrantType ON Grants(GrantType);
CREATE INDEX IX_Grants_ProgramTypeCode ON Grants(ProgramTypeCode);
CREATE INDEX IX_Grants_Status ON Grants(Status);
GO

PRINT 'Indexes created for Grants table';
GO

-- =============================================
-- Indexes for Reports Table
-- =============================================
CREATE INDEX IX_Reports_GrantId ON Reports(GrantId);
CREATE INDEX IX_Reports_Year_Quarter ON Reports(ReportingYear, ReportingQuarter);
CREATE INDEX IX_Reports_Status ON Reports(Status);
CREATE INDEX IX_Reports_Year ON Reports(ReportingYear);
GO

PRINT 'Indexes created for Reports table';
GO

-- =============================================
-- Indexes for ReportSections Table
-- =============================================
CREATE INDEX IX_ReportSections_ReportId ON ReportSections(ReportId);
CREATE INDEX IX_ReportSections_SectionName ON ReportSections(SectionName);
CREATE INDEX IX_ReportSections_ResponseType ON ReportSections(ResponseType);
GO

PRINT 'Indexes created for ReportSections table';
GO

-- =============================================
-- Indexes for AI_UsageLog Table
-- =============================================
CREATE INDEX IX_AI_UsageLog_UserId ON AI_UsageLog(UserId);
CREATE INDEX IX_AI_UsageLog_GrantId ON AI_UsageLog(GrantId);
CREATE INDEX IX_AI_UsageLog_Date ON AI_UsageLog(CreatedDate);
CREATE INDEX IX_AI_UsageLog_FeatureType ON AI_UsageLog(FeatureType);
GO

PRINT 'Indexes created for AI_UsageLog table';
GO

-- =============================================
-- Indexes for AI_ApprovedContent Table
-- =============================================
CREATE INDEX IX_AI_ApprovedContent_GrantId ON AI_ApprovedContent(GrantId);
CREATE INDEX IX_AI_ApprovedContent_Program_Section ON AI_ApprovedContent(ProgramTypeCode, SectionName);
CREATE INDEX IX_AI_ApprovedContent_Rating ON AI_ApprovedContent(ReviewerRating);
CREATE INDEX IX_AI_ApprovedContent_GrantType ON AI_ApprovedContent(GrantType);
GO

PRINT 'Indexes created for AI_ApprovedContent table';
GO

-- =============================================
-- Indexes for ChatConversations Table
-- =============================================
CREATE INDEX IX_ChatConversations_UserId ON ChatConversations(UserId);
CREATE INDEX IX_ChatConversations_SessionId ON ChatConversations(SessionId);
CREATE INDEX IX_ChatConversations_Date ON ChatConversations(CreatedDate);
GO

PRINT 'Indexes created for ChatConversations table';
GO

PRINT 'All indexes created successfully';
GO
