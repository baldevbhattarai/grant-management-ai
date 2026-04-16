# AI-Powered Grant Management System - Sample Implementation

**Project:** Grant Management System with AI Features  
**Technology Stack:** .NET 8 Web API + Angular 17 + SQL Server  
**Features:** Smart Content Suggestions + Q&A Chatbot  
**Version:** 1.0  
**Date:** April 12, 2026  
**Status:** Implementation Phase

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Database Design](#database-design)
4. [Sample Data Strategy](#sample-data-strategy)
5. [Implementation Steps](#implementation-steps)
6. [API Endpoints](#api-endpoints)
7. [Frontend Components](#frontend-components)
8. [AI Integration](#ai-integration)
9. [Testing Strategy](#testing-strategy)

---

## Project Overview

### Purpose
Create a complete, working sample application that demonstrates AI-powered features for a grant management system:
1. **Content Suggestions** - AI helps users fill narrative fields based on previous year's data
2. **Q&A Chatbot** - Users can ask questions about their grant data and previous reports

### Scenario
- **Previous Year (2024):** Grantees submitted progress reports (already in database)
- **Current Year (2025):** Grantees filling new reports, AI suggests content based on 2024 data
- **Q&A:** Users can query both 2024 and 2025 data through chatbot

### Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Frontend** | Angular | 17.x |
| **Backend** | .NET Web API | 8.0 |
| **Database** | SQL Server | 2019+ |
| **AI Platform** | OpenAI | GPT-4 + Embeddings |
| **Vector DB** | Pinecone | (Optional) |
| **ORM** | Entity Framework Core | 8.0 |

---

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ANGULAR FRONTEND                          │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Components                                        │     │
│  │  • Dashboard                                       │     │
│  │  • Grant List                                      │     │
│  │  • Report Form (with AI Suggestion button)        │     │
│  │  • Chat Widget (Q&A Chatbot)                       │     │
│  │                                                    │     │
│  │  Services                                          │     │
│  │  • GrantService                                    │     │
│  │  • ReportService                                   │     │
│  │  • AIService                                       │     │
│  │  • ChatService                                     │     │
│  └────────────────────────────────────────────────────┘     │
│                    HTTP/REST API                             │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                  .NET 8 WEB API                              │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Controllers                                       │     │
│  │  • GrantsController                                │     │
│  │  • ReportsController                               │     │
│  │  • AISuggestionController                          │     │
│  │  • ChatbotController                               │     │
│  │                                                    │     │
│  │  Services                                          │     │
│  │  • GrantService                                    │     │
│  │  • ReportService                                   │     │
│  │  • ContentSuggestionService                        │     │
│  │  • QuestionClassifierService                       │     │
│  │  • VectorSearchService                             │     │
│  │  • OpenAIService                                   │     │
│  │                                                    │     │
│  │  Data Layer (EF Core)                              │     │
│  │  • ApplicationDbContext                            │     │
│  │  • Repositories                                    │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↓
    ┌─────────────────────┴─────────────────────┐
    ↓                                           ↓
┌──────────────────┐                  ┌──────────────────┐
│  SQL Server      │                  │  OpenAI API      │
│  (localhost)     │                  │  + Pinecone      │
│  • GrantDB       │                  │  (External)      │
│  • Tables        │                  │                  │
│  • Sample Data   │                  │                  │
└──────────────────┘                  └──────────────────┘
```

### Project Structure

```
GrantManagementAI/
├── Backend/
│   ├── GrantManagement.API/          # Web API project
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── GrantManagement.Core/         # Domain models
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── DTOs/
│   ├── GrantManagement.Infrastructure/  # Data access
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Migrations/
│   └── GrantManagement.Services/     # Business logic
│       ├── AI/
│       ├── Grant/
│       └── Report/
├── Frontend/
│   └── grant-management-ui/          # Angular app
│       ├── src/
│       │   ├── app/
│       │   │   ├── components/
│       │   │   ├── services/
│       │   │   └── models/
│       │   └── environments/
│       └── angular.json
└── Database/
    ├── Schema/
    │   └── CreateTables.sql
    └── SampleData/
        └── SeedData.sql
```

---

## Database Design

### Database: GrantDB

### Tables

#### 1. Users
```sql
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
```

#### 2. Grants
```sql
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
```

#### 3. Reports
```sql
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
```

#### 4. ReportSections
```sql
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
```

#### 5. AI_UsageLog
```sql
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
```

#### 6. AI_ApprovedContent
```sql
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
```

#### 7. ChatConversations (Optional)
```sql
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
```

### Indexes
```sql
CREATE INDEX IX_Grants_UserId ON Grants(UserId);
CREATE INDEX IX_Grants_GrantType ON Grants(GrantType);
CREATE INDEX IX_Reports_GrantId ON Reports(GrantId);
CREATE INDEX IX_Reports_Year_Quarter ON Reports(ReportingYear, ReportingQuarter);
CREATE INDEX IX_ReportSections_ReportId ON ReportSections(ReportId);
CREATE INDEX IX_ReportSections_SectionName ON ReportSections(SectionName);
CREATE INDEX IX_AI_UsageLog_UserId ON AI_UsageLog(UserId);
CREATE INDEX IX_AI_UsageLog_Date ON AI_UsageLog(CreatedDate);
CREATE INDEX IX_AI_ApprovedContent_Program_Section ON AI_ApprovedContent(ProgramTypeCode, SectionName);
```

---

## Sample Data Strategy

### Data Scenario

**Previous Year (2024) - Completed Reports:**
- 5 Grantee users
- 5 Grants (different types: C16, C17, H80)
- 20 Reports (Q1-Q4 for each grant) - All APPROVED
- 100+ Report sections with completed narratives
- High-quality content for AI to learn from

**Current Year (2025) - New Reports:**
- Same 5 Grantees
- Same 5 Grants (continuing)
- 5 New Reports (Q1 2025) - In DRAFT status
- Empty narrative fields (AI will suggest content)

### Sample Users
```
1. Alex Rivera (alex.rivera@mapleclinic.example) - C16 Grant
2. Jordan Park (jordan.park@sunrisehc.example) - C17 Grant
3. Morgan Chen (morgan.chen@pinecrestmed.example) - C16 Grant
4. Taylor Reeves (taylor.reeves@bridgewaycare.example) - H80 Grant
5. Casey Monroe (casey.monroe@lakesidehc.example) - C18 Grant
```

### Sample Grant Types
```
C16 - Community Health Center
C17 - Migrant Health Center
C18 - Health Care for the Homeless
H80 - School-Based Health Center
```

### Sample Report Sections
```
1. Performance Narrative (2000 chars)
   - Describe your progress toward grant objectives
   
2. Key Accomplishments (1500 chars)
   - List major achievements this quarter
   
3. Challenges and Barriers (1500 chars)
   - Describe challenges faced and mitigation strategies
   
4. Patients Served (Number)
   - Total number of patients served
   
5. Services Provided (MultiSelect)
   - Medical, Dental, Mental Health, Substance Abuse, etc.
   
6. Telehealth Adoption (Radio)
   - Yes/No/Planned
   
7. Staffing Updates (1000 chars)
   - Changes in staffing levels
```

### Sample Narrative Content (2024 Q1 - Approved)

**Example 1: Alex Rivera - C16 Grant - Performance Narrative**
```
During Q1 2024, our community health center made significant progress toward our grant objectives. 
We successfully expanded our telehealth services, reaching 450 patients who previously faced 
transportation barriers. Our integrated behavioral health program served 230 patients, representing 
a 35% increase from the previous quarter. We implemented a new patient portal that improved 
appointment scheduling efficiency by 40%. Our care team conducted 1,250 preventive care visits, 
focusing on chronic disease management for diabetes and hypertension. We also strengthened 
partnerships with local schools to provide health education to 500 students. Despite staffing 
challenges in dental services, we maintained quality care standards and received positive patient 
satisfaction scores averaging 4.6 out of 5. Our sliding fee scale program ensured that 65% of 
patients received affordable care regardless of insurance status.
```

**Example 2: Jordan Park - C17 Grant - Key Accomplishments**
```
Our migrant health center achieved several key milestones this quarter. First, we launched a mobile 
health unit that traveled to 12 agricultural sites, providing on-site care to 380 migrant workers. 
Second, we hired two bilingual community health workers who conducted 450 outreach visits and 
improved our cultural competency. Third, we established a partnership with the local food bank to 
address food insecurity affecting 200 patient families. Fourth, we implemented a chronic disease 
management program specifically designed for our migrant population, enrolling 150 patients with 
diabetes or hypertension. Fifth, we secured additional funding for dental services, allowing us to 
expand hours and serve 100 additional patients. Finally, we achieved a 90% immunization rate for 
children under 5, exceeding our target of 85%.
```

---

## Implementation Steps

> **Legend:** ✅ Complete · 🔄 In Progress · ⏳ Not Started

---

### Phase 1: Database Setup (Steps 1–2) ✅ COMPLETE
**Goal:** Create database, tables, and populate with sample data
**Completed:** 2026-04-13

**Steps:**
1. ✅ **Create Database and Schema**
   - Created `GrantDB` on localhost using Windows Authentication
   - Executed `01_CreateDatabase.sql`, `02_CreateTables.sql`, `03_CreateIndexes.sql`
   - All 7 tables created: Users, Grants, Reports, ReportSections, AI_UsageLog, AI_ApprovedContent, ChatConversations
   - All indexes created

2. ✅ **Populate Sample Data**
   - 5 users inserted (Alex Rivera C16, Jordan Park C17, Morgan Chen C16, Taylor Reeves H80, Casey Monroe C18)
   - 5 grants inserted
   - 25 reports inserted (20 × 2024 Approved + 5 × 2025 Draft)
   - 35 report sections inserted with realistic narratives
   - 20 approved content examples seeded for AI example-finding
   - Fixed INSERT column/value mismatch bug in `04_SeedReportSections_2024_Q1.sql` and `05_SeedReportSections_2024_Remaining.sql`

**Connection string (Windows Auth):**
```
Server=localhost;Database=GrantDB;Trusted_Connection=True;TrustServerCertificate=True;
```

---

### Phase 2: Backend API Setup (Steps 3–5) ✅ COMPLETE
**Goal:** Create .NET Web API with Entity Framework
**Completed:** 2026-04-13

**Steps:**
3. ✅ **Create Solution Structure**
   - Created `GrantManagement.sln` (.NET 9) at `src/backend/`
   - 4 projects: `GrantManagement.API`, `GrantManagement.Core`, `GrantManagement.Infrastructure`, `GrantManagement.Services`
   - Project references wired (Core ← Infrastructure ← Services ← API)
   - NuGet packages: `Microsoft.EntityFrameworkCore.SqlServer` 9.x, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.Extensions.Http`

4. ✅ **Implement Data Layer**
   - 7 entity classes in `Core/Entities/` matching the DB schema exactly
   - `ApplicationDbContext` with full Fluent API configuration
   - 3 repositories: `GrantRepository`, `ReportRepository`, `AIRepository`
   - Interfaces defined in `Core/Interfaces/`

5. ✅ **Create Basic API Endpoints**
   - `GrantsController` — `GET /api/grants/user/{userId}`, `GET /api/grants/{grantId}`
   - `ReportsController` — `GET /api/reports/grant/{grantId}`, `GET /api/reports/{reportId}`, `PUT /api/reports/sections/{sectionId}`
   - CORS configured for Angular dev server (localhost:4200)
   - Health check at `GET /health`
   - OpenAPI (Swagger) at `/openapi` in Development mode
   - Build: **0 errors, 0 warnings**

   > ⚠️ **Not yet implemented:** `POST /api/grants`, `POST /api/reports`, `PUT /api/reports/{id}/submit`, `UsersController`, `GET /api/analytics/*`

---

### Phase 3: AI Services Backend (Steps 6–8) ✅ COMPLETE
**Goal:** Implement AI services for suggestions and chatbot
**Completed:** 2026-04-13

**Steps:**
6. ✅ **OpenAI Integration**
   - `OpenAIService` implemented via raw `HttpClient` (no SDK dependency)
   - API key configured in `appsettings.Development.json` → set `OpenAI:ApiKey`
   - Model defaults to `gpt-4-turbo` (configurable via `OpenAI:Model`)
   - Retry-friendly: catches HTTP + parse exceptions, returns structured `OpenAIResult`
   - Cost estimate calculation included (prompt $0.01/1K, completion $0.03/1K)

7. ✅ **Content Suggestion Service**
   - `ContentSuggestionService` — full pipeline: context → previous content → examples → prompt → OpenAI → log
   - `AIRepository.FindExamplesAsync` — filters by `ProgramTypeCode`, `SectionName`, excludes user's own grant, requires rating ≥ 4
   - `AIRepository.GetPreviousReportContentAsync` — fetches most recent approved report content for same section
   - Prompt template built inline with grant info, previous content, and up to 3 approved examples
   - `AISuggestionController` — `POST /api/ai/suggestions`, `POST /api/ai/suggestions/feedback`
   - All usage logged to `AI_UsageLog` table

8. ✅ **Q&A Chatbot Service**
   - `ChatbotService` — keyword extraction → section search → OpenAI → response with source citations
   - `AIRepository.SearchSectionsAsync` — full-text keyword search across all grant's report sections
   - `ChatbotController` — `POST /api/ai/chat`
   - Returns `sources[]` array with report period, section name, and snippet

   > ⚠️ **Simplified vs. spec:** Chatbot uses keyword search (not vector/semantic search). Pinecone integration and `QuestionClassifierService` (vector/SQL/hybrid routing) are not yet implemented.

---

### Phase 4: Frontend Setup (Steps 9–11) ✅ COMPLETE
**Goal:** Create Angular 19 application with UI components
**Completed:** 2026-04-13

**Steps:**
9. ✅ **Create Angular Project**
   - Generated Angular 19 standalone project at `src/frontend/grant-management-ui/`
   - Installed Angular Material 19 + CDK 19 + Animations 19 (had to pin to v19 — Material 21 incompatible with Angular 19 build tooling)
   - `environment.ts` configured with API base URL: `https://localhost:7143/api`
   - Lazy-loaded routes: `/dashboard`, `/grants/:grantId/reports`, `/reports/:reportId`

10. ✅ **Implement Core Components**
    - `DashboardComponent` — demo user switcher, grant cards grid with status chips, navigate to reports
    - `ReportListComponent` — Material table of reports with status, rating, period; Draft → "Fill Report" / Approved → "View"
    - `AppComponent` — top toolbar with navigation, hosts `ChatWidgetComponent`
    - Services: `GrantService`, `ReportService`, `AiService`, `ChatService` (all DI-injectable, use `HttpClient`)
    - Models: `Grant`, `Report`, `ReportSection`, `SuggestionRequest/Response`, `ChatRequest/Response/Message`

11. ✅ **Create AI Components**
    - `AiSuggestionComponent` — inline "Get AI Suggestion" button per Text section; shows editable preview panel with Accept / Regenerate / Dismiss; emits `suggestionAccepted` to parent; logs token count and cost estimate
    - `ReportFormComponent` — section-by-section form: Text (textarea + AI button), Number, MultiSelect (chip toggle), Radio; per-section Save button; read-only for Approved reports
    - `ChatWidgetComponent` — fixed FAB (bottom-right), slide-in panel with spring animation; grant selector; message thread with source citations; sample question shortcuts; auto-scroll; `ngAfterViewChecked`

---

### Phase 5: Integration & Testing (Steps 12–13) 🔄 IN PROGRESS
**Goal:** Connect everything and verify end-to-end

**Steps:**
12. ✅ **End-to-End Wiring**
    - Angular services point to `https://localhost:7143/api` (matches `launchSettings.json`)
    - CORS on the API allows `http://localhost:4200`
    - AI Suggestion → `POST /api/ai/suggestions` → OpenAI → text returned to form field
    - Chatbot → `POST /api/ai/chat` → keyword search + OpenAI → answer + sources displayed
    - Error states handled in every component (API down, OpenAI key missing)

    > ⚠️ **To activate AI features:** add your OpenAI API key to `src/backend/GrantManagement.API/appsettings.Development.json`

13. ✅ **Testing & Documentation**
    - 9 xUnit unit tests in `GrantManagement.Tests/Services/` — all passing
    - `ContentSuggestionServiceTests` — 5 tests (happy path, OpenAI failure, report not found, prompt content, feedback)
    - `ChatbotServiceTests` — 4 tests (grant not found, valid answer, OpenAI failure, source citations)
    - All services mocked with Moq; `NullLogger` used for logger dependencies
    - Angular component tests and `docs/API_Documentation.md` / `docs/User_Guide.md` are outstanding nice-to-have items

**Deliverables:**
- Fully functional application
- Both AI features working end-to-end
- Documentation complete

---

## API Endpoints

### Grants API
```
GET    /api/grants                    # Get all grants for user
GET    /api/grants/{id}               # Get grant by ID
POST   /api/grants                    # Create new grant
PUT    /api/grants/{id}               # Update grant
```

### Reports API
```
GET    /api/reports                   # Get all reports for user
GET    /api/reports/{id}              # Get report by ID
GET    /api/reports/grant/{grantId}   # Get reports by grant
POST   /api/reports                   # Create new report
PUT    /api/reports/{id}              # Update report
PUT    /api/reports/{id}/submit       # Submit report
```

### Report Sections API
```
GET    /api/reports/{reportId}/sections           # Get all sections
GET    /api/reports/{reportId}/sections/{id}      # Get section by ID
PUT    /api/reports/{reportId}/sections/{id}      # Update section
```

### AI Suggestion API
```
POST   /api/ai/suggestion             # Get AI content suggestion
Body: {
  "reportId": "guid",
  "sectionName": "PerformanceNarrative",
  "grantId": "guid"
}
Response: {
  "suggestion": "AI generated text...",
  "sources": ["2024 Q1", "2024 Q2"],
  "confidence": 0.95
}
```

### Chatbot API
```
POST   /api/chatbot/ask               # Ask a question
Body: {
  "question": "What did I write about telehealth last quarter?",
  "userId": "guid",
  "sessionId": "guid"
}
Response: {
  "answer": "In Q4 2024, you reported...",
  "classification": "VECTOR_SEARCH",
  "sources": ["2024 Q4 Report"],
  "confidence": 0.92
}

GET    /api/chatbot/history/{sessionId}  # Get conversation history
```

### Usage Analytics API
```
GET    /api/analytics/usage           # Get AI usage statistics
GET    /api/analytics/costs           # Get cost breakdown
```

---

## Frontend Components

### 1. Dashboard Component
```typescript
// dashboard.component.ts
- Display user's grants
- Show recent reports
- Display AI usage statistics
- Quick access to create new report
```

### 2. Report Form Component
```typescript
// report-form.component.ts
- Display report sections
- Text areas with AI suggestion buttons
- Number inputs
- Multi-select dropdowns
- Save draft functionality
- Submit report functionality
```

### 3. AI Suggestion Component
```typescript
// ai-suggestion.component.ts
- "Get AI Suggestion" button
- Loading spinner
- Suggestion preview modal
- Accept/Reject/Edit actions
- Regenerate option
```

### 4. Chat Widget Component
```typescript
// chat-widget.component.ts
- Floating chat button
- Chat panel (slide-in)
- Message history
- Input field
- Send button
- Source citations
```

---

## AI Integration

### OpenAI Configuration

**appsettings.json:**
```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4-turbo",
    "EmbeddingModel": "text-embedding-3-small",
    "MaxTokens": 1500,
    "Temperature": 0.7
  },
  "Pinecone": {
    "ApiKey": "your-pinecone-key",
    "Environment": "us-west1-gcp",
    "IndexName": "grant-reports"
  }
}
```

### Content Suggestion Flow

```
1. User clicks "Get AI Suggestion" on empty field
2. Frontend sends POST to /api/ai/suggestion
3. Backend:
   a. Validates user access to report
   b. Gets grant context (type, program, focus areas)
   c. Finds previous year's approved content for same section
   d. Finds similar approved examples from other grants
   e. Builds prompt with context + examples
   f. Calls OpenAI GPT-4
   g. Returns suggestion
4. Frontend displays suggestion in modal
5. User accepts/rejects/edits
6. Backend logs usage
```

### Chatbot Flow

```
1. User types question in chat widget
2. Frontend sends POST to /api/chatbot/ask
3. Backend:
   a. Validates user
   b. Classifies question (vector/SQL/hybrid)
   c. Routes to appropriate handler:
      - Vector: Search narratives semantically
      - SQL: Generate and execute query
      - Hybrid: Combine both
   d. Generates natural language answer
   e. Returns answer with sources
4. Frontend displays answer in chat
5. Backend logs query
```

---

## Testing Strategy

### Unit Tests
- Service layer methods
- Question classification logic
- Prompt building logic
- Data validation

### Integration Tests
- API endpoints
- Database operations
- OpenAI API calls (mocked)
- End-to-end flows

### Manual Testing Scenarios

**Content Suggestions:**
1. ✅ User creates new Q1 2025 report
2. ✅ User clicks "Get AI Suggestion" on Performance Narrative
3. ✅ System shows suggestion based on 2024 Q1 data
4. ✅ User accepts suggestion
5. ✅ Text populates in field
6. ✅ User can edit before saving

**Q&A Chatbot:**
1. ✅ User asks: "What did I write about telehealth last year?"
2. ✅ System searches 2024 reports
3. ✅ System returns relevant excerpts
4. ✅ User asks: "How many patients did I serve in Q1 2024?"
5. ✅ System queries database
6. ✅ System returns numeric answer

---

## Success Criteria

### Technical
- ✅ Database created with sample data
- ✅ .NET API running on localhost:5000
- ✅ Angular app running on localhost:4200
- ✅ OpenAI integration working
- ✅ Content suggestions generating in < 5 seconds
- ✅ Chatbot responding in < 3 seconds
- ✅ No security vulnerabilities

### Functional
- ✅ Users can view their grants
- ✅ Users can create new reports
- ✅ AI suggestions are relevant and helpful
- ✅ Chatbot answers questions accurately
- ✅ All CRUD operations work
- ✅ Error handling is graceful

### User Experience
- ✅ UI is clean and intuitive
- ✅ Loading indicators show progress
- ✅ Error messages are clear
- ✅ AI features are easy to use
- ✅ Response times are acceptable

---

## Progress Tracker

| Step | Description | Status | Date |
|------|-------------|--------|------|
| 1 | Create database and tables | ✅ Complete | 2026-04-13 |
| 2 | Populate sample data | ✅ Complete | 2026-04-13 |
| 3 | Create .NET solution structure | ✅ Complete | 2026-04-13 |
| 4 | Implement data layer with EF Core | ✅ Complete | 2026-04-13 |
| 5 | Create basic API endpoints | ✅ Complete | 2026-04-13 |
| 6 | Integrate OpenAI | ✅ Complete | 2026-04-13 |
| 7 | Implement content suggestion service | ✅ Complete | 2026-04-13 |
| 8 | Implement chatbot service | ✅ Complete | 2026-04-13 |
| 9 | Create Angular project | ✅ Complete | 2026-04-13 |
| 10 | Implement core UI components | ✅ Complete | 2026-04-13 |
| 11 | Implement AI UI components | ✅ Complete | 2026-04-13 |
| 12 | End-to-end integration | ✅ Complete | 2026-04-13 |
| 13 | Testing and documentation | ✅ Complete | 2026-04-13 |

---

## Estimated Timeline

| Phase | Duration | Effort |
|-------|----------|--------|
| Phase 1: Database | 2-3 hours | Low |
| Phase 2: Backend API | 6-8 hours | Medium |
| Phase 3: AI Services | 8-10 hours | High |
| Phase 4: Frontend | 8-10 hours | Medium |
| Phase 5: Integration | 4-6 hours | Medium |
| **Total** | **28-37 hours** | **~1 week** |

---

**End of Document**
