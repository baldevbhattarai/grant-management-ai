-- =============================================
-- Grant Management AI - Table Creation
-- Step 1: Create All Tables
-- =============================================

USE GrantDB;
GO

-- =============================================
-- Table 1: Users
-- =============================================
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL, -- 'Grantee', 'Reviewer', 'Admin'
    OrganizationName NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

PRINT 'Table Users created';
GO

-- =============================================
-- Table 2: Grants
-- =============================================
CREATE TABLE Grants (
    GrantId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrantNumber NVARCHAR(50) NOT NULL UNIQUE,
    UserId UNIQUEIDENTIFIER NOT NULL,
    GrantType NVARCHAR(50) NOT NULL, -- 'C16', 'C17', 'C18', 'H80'
    ProgramName NVARCHAR(255) NOT NULL,
    ProgramTypeCode INT NOT NULL, -- 60, 61, 62, etc.
    FocusAreas NVARCHAR(MAX), -- JSON array
    FundingAmount DECIMAL(18,2),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Active', -- 'Active', 'Closed', 'Suspended'
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Grants_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

PRINT 'Table Grants created';
GO

-- =============================================
-- Table 3: Reports
-- =============================================
CREATE TABLE Reports (
    ReportId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrantId UNIQUEIDENTIFIER NOT NULL,
    ReportingYear INT NOT NULL,
    ReportingQuarter NVARCHAR(10) NOT NULL, -- 'Q1', 'Q2', 'Q3', 'Q4', 'Annual'
    ReportType NVARCHAR(50) NOT NULL, -- 'Progress', 'Final'
    Status NVARCHAR(50) DEFAULT 'Draft', -- 'Draft', 'Submitted', 'Approved', 'Rejected'
    SubmittedDate DATETIME NULL,
    ApprovedDate DATETIME NULL,
    ReviewerRating INT NULL, -- 1-5
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Reports_Grants FOREIGN KEY (GrantId) REFERENCES Grants(GrantId)
);
GO

PRINT 'Table Reports created';
GO

-- =============================================
-- Table 4: ReportSections
-- =============================================
CREATE TABLE ReportSections (
    SectionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReportId UNIQUEIDENTIFIER NOT NULL,
    SectionName NVARCHAR(100) NOT NULL, -- 'PerformanceNarrative', 'Accomplishments', etc.
    SectionTitle NVARCHAR(255) NOT NULL,
    SectionOrder INT NOT NULL,
    QuestionText NVARCHAR(MAX) NOT NULL,
    ResponseType NVARCHAR(50) NOT NULL, -- 'Text', 'Number', 'MultiSelect', 'Radio'
    ResponseText NVARCHAR(MAX) NULL, -- For text responses
    ResponseNumber DECIMAL(18,2) NULL, -- For numeric responses
    ResponseOptions NVARCHAR(MAX) NULL, -- For multiselect (JSON array)
    ResponseSingle NVARCHAR(255) NULL, -- For radio/single select
    IsRequired BIT DEFAULT 1,
    MaxLength INT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ReportSections_Reports FOREIGN KEY (ReportId) REFERENCES Reports(ReportId)
);
GO

PRINT 'Table ReportSections created';
GO

-- =============================================
-- Table 5: AI_UsageLog
-- =============================================
CREATE TABLE AI_UsageLog (
    LogId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    GrantId UNIQUEIDENTIFIER NOT NULL,
    ReportId UNIQUEIDENTIFIER NULL,
    FeatureType NVARCHAR(50) NOT NULL, -- 'ContentSuggestion', 'QA_Chatbot'
    SectionName NVARCHAR(100) NULL,
    Question NVARCHAR(MAX) NULL, -- For chatbot
    ModelName NVARCHAR(50) NOT NULL, -- 'gpt-4-turbo'
    PromptTokens INT NULL,
    CompletionTokens INT NULL,
    TotalTokens INT NULL,
    EstimatedCost DECIMAL(10,6) NULL,
    ResponseTimeMs INT NULL,
    UserAction NVARCHAR(50) NULL, -- 'Accepted', 'Rejected', 'Edited'
    UserRating INT NULL, -- 1-5
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_AI_UsageLog_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

PRINT 'Table AI_UsageLog created';
GO

-- =============================================
-- Table 6: AI_ApprovedContent
-- =============================================
CREATE TABLE AI_ApprovedContent (
    ContentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrantId UNIQUEIDENTIFIER NOT NULL,
    ReportId UNIQUEIDENTIFIER NOT NULL,
    ProgramTypeCode INT NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ApprovalDate DATETIME NOT NULL,
    ReviewerRating INT NULL, -- 1-5
    GrantType NVARCHAR(50) NULL,
    Keywords NVARCHAR(500) NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_AI_ApprovedContent_Grants FOREIGN KEY (GrantId) REFERENCES Grants(GrantId)
);
GO

PRINT 'Table AI_ApprovedContent created';
GO

-- =============================================
-- Table 7: ChatConversations
-- =============================================
CREATE TABLE ChatConversations (
    ConversationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    SessionId NVARCHAR(100) NOT NULL,
    MessageType NVARCHAR(50) NOT NULL, -- 'UserQuestion', 'AIAnswer'
    MessageText NVARCHAR(MAX) NOT NULL,
    Classification NVARCHAR(50) NULL, -- 'VECTOR_SEARCH', 'SQL_QUERY', 'HYBRID'
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ChatConversations_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

PRINT 'Table ChatConversations created';
GO

PRINT 'All tables created successfully';
GO
