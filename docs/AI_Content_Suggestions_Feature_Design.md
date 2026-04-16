# AI Content Suggestions & Q&A Feature - Design Document

**Project:** Grant Management AI  
**Feature:** Smart Content Suggestions & AI-Powered Q&A  
**Version:** 1.0  
**Date:** April 9, 2026  
**Status:** Design Phase

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Feature Overview](#feature-overview)
3. [Architecture Design](#architecture-design)
4. [Security Design](#security-design)
5. [Question Classification System](#question-classification-system)
6. [Data Architecture](#data-architecture)
7. [Vector Database Strategy](#vector-database-strategy)
8. [Implementation Plan](#implementation-plan)
9. [Cost Analysis](#cost-analysis)
10. [Risk Management](#risk-management)
11. [Success Metrics](#success-metrics)

---

## Executive Summary

### Purpose
Implement AI-powered features to assist grantee users in completing progress reports by providing intelligent content suggestions and answering questions based on historical data and best practices.

### Key Features
1. **Smart Content Suggestions** - AI-generated suggestions for narrative text fields
2. **Q&A Assistant** - Answer questions about grant data and previous reports
3. **Semantic Search** - Find relevant content across historical reports
4. **Secure Access** - Row-level security ensuring users only access their own data

### Technology Stack
- **AI Platform:** OpenAI GPT-4 & Embeddings API
- **Vector Database:** Pinecone (or Azure AI Search)
- **Backend:** ASP.NET WebForms (C#)
- **Database:** SQL Server (GrantDB / GrantPortal)
- **Authentication:** Existing ASP.NET authentication system

### Expected Benefits
- 40-60% reduction in time to complete narrative sections
- Improved quality and consistency of reports
- Better alignment with HRSA expectations
- Enhanced user experience

---

## Feature Overview

### Use Case 1: Content Suggestions

**Scenario:** Grantee user filling out Performance Narrative (Question 10)

**User Flow:**
1. User navigates to Progress Report form
2. User clicks "Get AI Suggestion" button next to text field
3. System analyzes grant context and finds similar approved examples
4. AI generates contextual suggestion based on examples
5. User reviews, edits, and accepts/rejects suggestion

**Value:** Reduces writer's block, provides structure, ensures completeness

---

### Use Case 2: Q&A While Filling Form

**Scenario:** Grantee user needs to reference previous report data

**User Flow:**
1. User types question: "What did I write about telehealth last quarter?"
2. System searches user's previous reports (vector search)
3. AI synthesizes answer from relevant passages
4. User gets quick answer without leaving the form

**Value:** Instant access to historical data, reduces context switching

---

### Use Case 3: Cross-Grant Analysis (Admin/Reviewer)

**Scenario:** Admin needs to analyze trends across grants

**User Flow:**
1. Admin asks: "How many grants mentioned staffing challenges in Q1 2024?"
2. System performs hybrid search (vector + SQL)
3. AI generates comprehensive answer with statistics
4. Admin gets insights for reporting

**Value:** Data-driven decision making, trend analysis

---

## Architecture Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER LAYER                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Grantee    │  │   Reviewer   │  │    Admin     │      │
│  │   User       │  │   User       │  │    User      │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
          ↓                 ↓                  ↓
┌─────────────────────────────────────────────────────────────┐
│              SECURITY & AUTHENTICATION LAYER                 │
│  • Authenticate User (ASP.NET Session/Auth)                  │
│  • Identify User Role (Grantee/Reviewer/Admin)               │
│  • Get Accessible Grants (from GrantsRegistration)           │
│  • Build Security Context                                    │
└─────────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────────┐
│                   AI ORCHESTRATION LAYER                     │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Question Classifier                               │     │
│  │  • Content Suggestion                              │     │
│  │  • Q&A (Narrative)                                 │     │
│  │  • Q&A (Structured Data)                           │     │
│  │  • Hybrid Query                                    │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
          ↓
    ┌─────┴─────┐
    ↓           ↓
┌──────────┐  ┌──────────────┐
│  Vector  │  │  SQL Query   │
│  Search  │  │  Generation  │
└──────────┘  └──────────────┘
    ↓               ↓
┌──────────┐  ┌──────────────┐
│ Pinecone │  │ SQL Server   │
│ (Vectors)│  │ (GrantDB / GrantPortal)│
└──────────┘  └──────────────┘
```

### Component Breakdown

#### 1. Security & Authentication Layer
**Responsibility:** Establish user identity and access rights

**Components:**
- User authentication (existing ASP.NET system)
- Security context builder
- Grant access resolver (via GrantsRegistration table)

**Output:** UserContext object
```csharp
{
  UserId: GUID,
  Role: "Grantee" | "Reviewer" | "Admin",
  AccessibleGrants: [GrantId1, GrantId2, ...]
}
```

#### 2. AI Orchestration Layer
**Responsibility:** Route requests to appropriate AI strategy

**Components:**
- Question classifier (determines vector vs SQL vs hybrid)
- Strategy selector
- Result aggregator

#### 3. Vector Search Service
**Responsibility:** Semantic search across narrative content

**Components:**
- Embedding service (OpenAI text-embedding-3-small)
- Vector database client (Pinecone)
- Result retriever (SQL Server)

#### 4. SQL Query Generator
**Responsibility:** Convert natural language to SQL queries

**Components:**
- Text-to-SQL converter (OpenAI GPT-4)
- Query validator (security checks)
- Query executor

#### 5. Content Suggestion Service
**Responsibility:** Generate AI suggestions for text fields

**Components:**
- Context builder (grant info, previous reports)
- Example finder (vector search)
- Prompt builder
- Suggestion generator (OpenAI GPT-4)

---

## Security Design

### Three-Layer Security Model

#### Layer 1: Authentication & Authorization

**Purpose:** Establish WHO the user is and WHAT they can access

**Process:**
```
User Login
  ↓
ASP.NET Authentication (existing)
  ↓
Session/Token with UserId
  ↓
Query GrantsRegistration Table
  ↓
Build User Security Context:
{
  UserId: GUID
  Role: "Grantee" | "Reviewer" | "Admin"
  AccessibleGrants: [GrantId1, GrantId2, ...]
}
```

**SQL Query:**
```sql
SELECT 
    u.UserId,
    CASE 
        WHEN u.IsAdmin = 1 THEN 'Admin'
        WHEN EXISTS (SELECT 1 FROM ReviewerAssignments WHERE UserId = u.UserId) 
            THEN 'Reviewer'
        ELSE 'Grantee'
    END as UserRole,
    gr.GrantId
FROM AC_User u
LEFT JOIN GrantsRegistration gr ON u.UserId = gr.UserId
WHERE u.UserId = @UserId
  AND gr.IsActive = 1
```

#### Layer 2: Query-Level Security

**Purpose:** Apply security filters to ALL data access

**For Vector Search (Pinecone):**
```javascript
{
  queryVector: [embedding],
  topK: 10,
  filter: {
    "grantId": { "$in": [user's accessible grant IDs] }
  }
}
```

**For SQL Queries:**
```sql
-- Base query (unsafe)
SELECT * FROM GrantDeliverables

-- Secure query (with injection)
SELECT * FROM GrantDeliverables
WHERE GrantId IN (@AccessibleGrants)
```

**Security Injection Strategy:**
- Programmatically inject WHERE clauses
- Never trust AI-generated SQL alone
- Validate filters exist before execution

#### Layer 3: Result Validation

**Purpose:** Verify returned data belongs to user

**Validation Flow:**
```
Query Results
  ↓
Check each result
  ↓
Verify GrantId in user's accessible list
  ↓
Filter out unauthorized results
  ↓
Return only authorized data
```

### Security Patterns

#### Pattern 1: Security Context Object
Every AI request includes security context, passed to every function, validated at every layer.

#### Pattern 2: Filter Injection
Automatically inject security filters into all queries programmatically.

#### Pattern 3: Validation Chain
```
Request → Authenticate → Authorize → Filter → Execute → Validate → Return
```
Each step can fail and deny access. Logged at each step. Fail-closed approach.

### Access Control Matrix

| User Role | Own Grant Data | Other Grants (Approved) | All Grants | Cross-Grant Analysis |
|-----------|----------------|-------------------------|------------|---------------------|
| Grantee   | ✅ Full Access | ❌ No Access           | ❌ No      | ❌ No               |
| Reviewer  | N/A            | ✅ Assigned Only       | ❌ No      | ✅ Assigned Only    |
| Admin     | N/A            | ✅ Yes                 | ✅ Yes     | ✅ Yes              |

---

## Question Classification System

### Purpose
Determine whether to use vector search, SQL query, or hybrid approach based on user's question.

### Classification Strategy

#### Method 1: Rule-Based Classification (Fast - 80% of cases)

**Vector Search Indicators:**
- Keywords: "what", "how", "why", "describe", "explain"
- Content words: "challenges", "accomplishments", "strategies", "mentioned", "discussed"
- Seeking: Narrative content, qualitative information, semantic meaning

**SQL Query Indicators:**
- Keywords: "how many", "count", "total", "list", "show me"
- Structured words: "status", "approved", "type", "date"
- Operators: "greater than", "less than", "between"
- Seeking: Facts, numbers, quantitative data, structured fields

**Hybrid Indicators:**
- Combination: "how many grants that mentioned X"
- Pattern: Counting + semantic content

#### Method 2: AI-Based Classification (Smart - 20% of cases)

For ambiguous questions, use OpenAI to classify:

**Classification Prompt:**
```
You are a query classifier for a grant management system.

Database contains:
- Structured data: Grant types, statuses, numbers, dates, selections
- Unstructured data: Narrative text, descriptions, explanations

Classify questions into:
1. VECTOR_SEARCH - For semantic/content questions
2. SQL_QUERY - For structured data queries
3. HYBRID - For questions needing both

Question: {user_question}

Return JSON:
{
  "type": "VECTOR_SEARCH" | "SQL_QUERY" | "HYBRID",
  "reason": "explanation",
  "confidence": 0.0-1.0
}
```

### Decision Tree

```
User Question
  ↓
Quick Pattern Match (< 1ms)
  ↓
Obvious? ──Yes──> Classify & Execute
  ↓ No
AI Classification (200-500ms)
  ↓
Confidence > 90%? ──Yes──> Execute
  ↓ No
Confidence 70-90%? ──Yes──> Execute with explanation
  ↓ No
Confidence 50-70%? ──Yes──> Ask for clarification
  ↓ No
Request rephrase
```

### Classification Examples

| Question | Type | Reasoning |
|----------|------|-----------|
| "What challenges did grantees face?" | VECTOR_SEARCH | Seeking narrative content |
| "How many C16 grants are there?" | SQL_QUERY | Counting with filter |
| "How many grants mentioned telehealth?" | HYBRID | Count + semantic search |
| "Show me grants with mental health services" | SQL_QUERY | Structured multiselect field |
| "What did I write about staffing?" | VECTOR_SEARCH | Content search in own reports |
| "List approved reports from Q1 2024" | SQL_QUERY | Structured filters (status, date) |

---

## Data Architecture

### SQL Server Schema

#### Existing Tables (No Changes)
- `Grants` - Grant master data
- `GrantDeliverables` - Progress reports
- `GrantsRegistration` - User-grant access control
- `AC_User` - User accounts

#### New Tables (AI-Specific)

**1. DeliverableResponses**
```sql
CREATE TABLE DeliverableResponses (
    ResponseId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DeliverableId UNIQUEIDENTIFIER NOT NULL,
    FormType VARCHAR(50) NOT NULL,  -- 'Performance', 'Accomplishments', 'Challenges'
    QuestionNumber INT NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    QuestionType VARCHAR(50) NOT NULL,  -- 'Text', 'Number', 'MultiSelect', 'Radio'
    
    -- Response data (based on QuestionType)
    ResponseText NVARCHAR(MAX),      -- For Text type
    ResponseNumber DECIMAL(18,2),    -- For Number type
    ResponseOptions NVARCHAR(MAX),   -- For MultiSelect (comma-separated)
    ResponseSingle VARCHAR(100),     -- For Radio type
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Response_Deliverable FOREIGN KEY (DeliverableId) 
        REFERENCES GrantDeliverables(DeliverableId)
);

CREATE INDEX IX_DeliverableResponses_Deliverable 
    ON DeliverableResponses(DeliverableId);
CREATE INDEX IX_DeliverableResponses_Type 
    ON DeliverableResponses(QuestionType);
```

**2. AI_UsageLog**
```sql
CREATE TABLE AI_UsageLog (
    LogId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    GrantId UNIQUEIDENTIFIER NULL,
    DeliverableId UNIQUEIDENTIFIER NULL,
    
    QueryType VARCHAR(50) NOT NULL,  -- 'Suggestion', 'QA_Narrative', 'QA_SQL', 'Hybrid'
    UserQuestion NVARCHAR(MAX),
    
    -- AI Provider info
    ProviderName VARCHAR(50) NOT NULL,  -- 'OpenAI'
    ModelName VARCHAR(50) NOT NULL,     -- 'gpt-4-turbo', 'text-embedding-3-small'
    
    -- Usage metrics
    PromptTokens INT NULL,
    CompletionTokens INT NULL,
    TotalTokens INT NULL,
    EstimatedCost DECIMAL(10,6) NULL,
    ResponseTimeMs INT NULL,
    
    -- User feedback
    SuggestionAccepted BIT NULL,
    UserRating INT NULL,  -- 1-5 scale
    
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_UsageLog_User FOREIGN KEY (UserId) 
        REFERENCES AC_User(UserId)
);

CREATE INDEX IX_AI_UsageLog_User ON AI_UsageLog(UserId);
CREATE INDEX IX_AI_UsageLog_Date ON AI_UsageLog(CreatedDate);
CREATE INDEX IX_AI_UsageLog_QueryType ON AI_UsageLog(QueryType);
```

**3. AI_ApprovedContent (Optional - for seeding)**
```sql
CREATE TABLE AI_ApprovedContent (
    ContentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrantId UNIQUEIDENTIFIER NOT NULL,
    DeliverableId UNIQUEIDENTIFIER NULL,
    
    ProgramTypeCode INT NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,  -- 'PerformanceNarrative', 'Accomplishments', etc.
    Content NVARCHAR(MAX) NOT NULL,
    
    ApprovalDate DATETIME NOT NULL,
    ReviewerRating INT NULL,  -- 1-5 scale
    
    Keywords NVARCHAR(500) NULL,
    GrantType NVARCHAR(50) NULL,
    DeliverableType NVARCHAR(100) NULL,
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_ApprovedContent_Grant FOREIGN KEY (GrantId) 
        REFERENCES Grants(GrantId)
);

CREATE INDEX IX_AI_ApprovedContent_Program 
    ON AI_ApprovedContent(ProgramTypeCode, SectionName);
CREATE INDEX IX_AI_ApprovedContent_Rating 
    ON AI_ApprovedContent(ReviewerRating);
```

**4. AI_QueryAuditLog**
```sql
CREATE TABLE AI_QueryAuditLog (
    AuditId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    UserRole VARCHAR(50) NOT NULL,
    
    Question NVARCHAR(MAX) NOT NULL,
    QueryType VARCHAR(50) NOT NULL,
    Classification VARCHAR(50),  -- 'VECTOR_SEARCH', 'SQL_QUERY', 'HYBRID'
    
    GeneratedSQL NVARCHAR(MAX) NULL,
    SecurityFilterApplied BIT NOT NULL,
    AccessibleGrants NVARCHAR(MAX),  -- JSON array of grant IDs
    
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    Timestamp DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_QueryAuditLog_User FOREIGN KEY (UserId) 
        REFERENCES AC_User(UserId)
);

CREATE INDEX IX_AI_QueryAuditLog_User ON AI_QueryAuditLog(UserId);
CREATE INDEX IX_AI_QueryAuditLog_Timestamp ON AI_QueryAuditLog(Timestamp);
```

### Pinecone Vector Database

**Index Configuration:**
```javascript
{
  name: "bphc-progress-reports",
  dimension: 1536,  // text-embedding-3-small
  metric: "cosine",
  podType: "p1.x1"  // Free tier or paid
}
```

**Vector Structure:**
```javascript
{
  id: "ResponseId-GUID",
  values: [0.234, -0.567, ..., 0.123],  // 1536 dimensions
  metadata: {
    // Core identifiers
    responseId: "...",
    deliverableId: "...",
    grantId: "...",          // CRITICAL for security filtering
    grantNumber: "...",
    
    // Classification
    formType: "Performance" | "Accomplishments" | "Challenges",
    questionNumber: 1-15,
    questionText: "...",
    questionType: "Text",
    
    // Context
    reportingPeriod: "Q1 2024",
    status: "Approved",
    programTypeCode: 60,
    grantType: "C16",
    
    // Metadata
    responseLength: 1250,
    vectorizedDate: "2024-03-15"
  }
}
```

**Why Store Metadata:**
- Enable filtering by grant, form, question
- Security: Filter by grantId
- Context: Understand where content came from
- Analytics: Track usage patterns

---

## Vector Database Strategy

### Vectorization Approach

#### What to Vectorize
**✅ Vectorize:**
- Text fields (5000 char narrative fields)
- Free-text responses
- Accomplishments, challenges, explanations

**❌ Don't Vectorize:**
- Number fields
- Multi-select dropdowns
- Radio buttons
- Dates, statuses

**Strategy:** Vectorize text fields individually, store structured data as metadata

#### Vectorization Process

**For Each Text Field:**
1. Extract text from SQL Server
2. Build context-aware text (include question, grant info)
3. Send to OpenAI Embeddings API
4. Receive 1536-dimension vector
5. Store in Pinecone with rich metadata
6. Original text stays in SQL Server

**Example:**
```
Text Field (Q1 - Performance Narrative):
"During Q1 2024, we expanded primary care services by 15%..."

Context-Enriched Text (what gets vectorized):
"Question: Describe your performance during this reporting period
Form: Performance
Grant: GX-2024-99999

Response:
During Q1 2024, we expanded primary care services by 15%..."

↓ OpenAI Embeddings API ↓

Vector: [0.234, -0.567, 0.891, ..., 0.123] (1536 numbers)

↓ Store in Pinecone ↓

{
  id: "ABC-123",
  values: [vector],
  metadata: {
    grantId: "...",
    formType: "Performance",
    questionNumber: 1,
    ...
  }
}
```

### When to Use Vector Database

**Use Vector DB When:**
- ✅ You have 1,000+ approved reports
- ✅ You want semantic search (meaning-based)
- ✅ You need to find similar content across different wordings
- ✅ You want best possible AI suggestions
- ✅ You plan to build Q&A features

**Skip Vector DB When:**
- ❌ Just starting (< 100 reports)
- ❌ Budget is very tight
- ❌ Simple keyword matching is enough
- ❌ Want to keep it simple initially

### Recommended Approach

**Phase 1: Start Simple (No Vector DB)**
- Use traditional SQL queries for examples
- Cost: ~$20-40/month (OpenAI only)
- Good enough to start

**Phase 2: Add Vector DB (After 3-6 months)**
- Migrate approved content to Pinecone
- Enable semantic search
- Cost: ~$70-100/month
- Significantly better suggestions

### Cost Calculation

**Scenario:** 100 progress reports, 15 text fields each

**Vectorization Cost:**
- 1,500 text fields × 250 tokens average = 375,000 tokens
- OpenAI Embeddings: $0.00002 per 1K tokens
- Cost: 375 × $0.00002 = **$0.0075** (less than 1 cent!)

**Storage Cost:**
- Pinecone Free Tier: 100,000 vectors
- 1,500 vectors: **$0** (within free tier)

**Ongoing Cost:**
- Each suggestion: 1 embedding + 1 completion
- Embedding: ~$0.00001
- Completion: ~$0.02
- Per suggestion: **~$0.02**
- 1,000 suggestions/month: **~$20/month**

---

## Implementation Plan

### Phase 1: Foundation (Weeks 1-2)

**Goal:** Set up core infrastructure and security

**Tasks:**
1. **Database Setup**
   - Create new tables (DeliverableResponses, AI_UsageLog, etc.)
   - Add indexes
   - Migrate existing data to DeliverableResponses

2. **Security Layer**
   - Implement UserContext service
   - Create security filter injection logic
   - Add validation chain
   - Test access control

3. **OpenAI Integration**
   - Set up OpenAI API credentials
   - Create embedding service wrapper
   - Create completion service wrapper
   - Test API connectivity

4. **Audit Logging**
   - Implement logging service
   - Log all AI requests
   - Log security validations

**Deliverables:**
- Database schema deployed
- Security layer functional
- OpenAI integration tested
- Audit logging active

**Success Criteria:**
- All tests pass
- Security validations work
- API calls successful

---

### Phase 2: Content Suggestions (Weeks 3-4)

**Goal:** Implement AI suggestions for text fields

**Tasks:**
1. **UI Components**
   - Add "Get AI Suggestion" button to text fields
   - Create suggestion display panel
   - Add accept/reject/edit controls
   - Loading indicators

2. **Backend Services**
   - Implement suggestion service
   - Build prompt templates
   - Create context builder (grant info, previous reports)
   - Implement example finder (SQL-based initially)

3. **API Endpoints**
   - Create AJAX handler for suggestions
   - Add security checks
   - Return JSON responses

4. **Testing**
   - Unit tests for services
   - Integration tests
   - User acceptance testing with 5-10 grantees

**Deliverables:**
- Functional suggestion feature
- UI integrated into forms
- API endpoints secured
- Test results documented

**Success Criteria:**
- Suggestions generate in < 5 seconds
- 70%+ acceptance rate in testing
- No security violations
- Positive user feedback

---

### Phase 3: Q&A Basic (Weeks 5-6)

**Goal:** Enable users to ask questions about their own data

**Tasks:**
1. **Question Classifier**
   - Implement rule-based classification
   - Add AI-based classification for ambiguous cases
   - Test classification accuracy

2. **Narrative Search**
   - Implement vector search (if using Pinecone)
   - Or implement SQL full-text search (simpler)
   - Add security filters
   - Test result relevance

3. **Structured Query**
   - Implement text-to-SQL converter
   - Add SQL validation
   - Test query generation

4. **UI Components**
   - Add Q&A chat interface
   - Display answers with sources
   - Show confidence indicators

**Deliverables:**
- Question classification working
- Narrative search functional
- SQL generation working
- Chat UI integrated

**Success Criteria:**
- 85%+ classification accuracy
- Relevant answers returned
- < 3 second response time
- Users find it helpful

---

### Phase 4: Vector Database (Weeks 7-8) - Optional

**Goal:** Enhance with semantic search capabilities

**Tasks:**
1. **Pinecone Setup**
   - Create Pinecone account
   - Set up index
   - Configure security

2. **Data Migration**
   - Extract approved content from SQL
   - Vectorize all text fields
   - Upload to Pinecone
   - Verify data integrity

3. **Service Updates**
   - Update suggestion service to use vector search
   - Update Q&A service to use vector search
   - Add hybrid query support

4. **Testing**
   - Compare vector vs SQL results
   - Measure improvement in relevance
   - Performance testing

**Deliverables:**
- Pinecone operational
- Historical data migrated
- Services updated
- Performance metrics

**Success Criteria:**
- All data migrated successfully
- Improved suggestion relevance
- Response time < 3 seconds
- Cost within budget

---

### Phase 5: Advanced Features (Weeks 9-10)

**Goal:** Add hybrid queries and cross-grant analysis

**Tasks:**
1. **Hybrid Queries**
   - Combine vector search + SQL
   - Implement result merging
   - Test complex queries

2. **Admin/Reviewer Features**
   - Cross-grant analysis
   - Trend identification
   - Reporting dashboards

3. **Optimization**
   - Cache frequently used data
   - Optimize SQL queries
   - Reduce API calls

4. **User Feedback System**
   - Add rating system
   - Collect improvement suggestions
   - Track acceptance rates

**Deliverables:**
- Hybrid queries working
- Admin features functional
- Performance optimized
- Feedback system active

**Success Criteria:**
- Complex queries work correctly
- Admins find insights valuable
- Response times improved
- High user satisfaction

---

### Phase 6: Production Hardening (Weeks 11-12)

**Goal:** Prepare for production deployment

**Tasks:**
1. **Security Audit**
   - Penetration testing
   - Code review
   - Vulnerability assessment
   - Fix any issues

2. **Performance Testing**
   - Load testing (100+ concurrent users)
   - Stress testing
   - Identify bottlenecks
   - Optimize as needed

3. **Documentation**
   - User guide
   - Admin guide
   - API documentation
   - Troubleshooting guide

4. **Training**
   - Create training materials
   - Conduct user training sessions
   - Train support staff
   - Create FAQ

5. **Deployment**
   - Deploy to production
   - Monitor closely
   - Gather feedback
   - Iterate

**Deliverables:**
- Security audit passed
- Performance benchmarks met
- Complete documentation
- Users trained
- Production deployment

**Success Criteria:**
- Zero security vulnerabilities
- Handles expected load
- Users can use features independently
- Smooth deployment

---

## Cost Analysis

### Initial Setup Costs

| Item | Cost | Notes |
|------|------|-------|
| OpenAI API Setup | $0 | Pay-as-you-go |
| Pinecone Account | $0 | Free tier initially |
| Development Time | Internal | 12 weeks |
| Testing | Internal | Included in phases |
| **Total Initial** | **$0** | No upfront costs |

### Monthly Operating Costs

#### Scenario: 100 Active Users, 500 Suggestions/Month

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Embeddings** | 500 suggestions × 1 embedding | $0.00002/1K tokens | $0.25 |
| **OpenAI Completions** | 500 suggestions × 1000 tokens | $0.01/1K tokens | $10.00 |
| **Pinecone** | 1,500 vectors (free tier) | $0 (or $70 paid) | $0 - $70 |
| **SQL Server** | Existing infrastructure | $0 | $0 |
| **Bandwidth** | Minimal | Included | $0 |
| **Total Monthly** | | | **$10 - $80** |

#### Scenario: 500 Active Users, 5,000 Suggestions/Month

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Embeddings** | 5,000 × 1 embedding | $0.00002/1K tokens | $2.50 |
| **OpenAI Completions** | 5,000 × 1000 tokens | $0.01/1K tokens | $100.00 |
| **Pinecone** | 10,000 vectors | $70 | $70.00 |
| **Total Monthly** | | | **$172.50** |

### Cost Optimization Strategies

1. **Caching**
   - Cache similar suggestions
   - Reduce duplicate API calls
   - Save 20-30% on costs

2. **Batch Processing**
   - Vectorize in batches during off-hours
   - Reduce API overhead

3. **Smart Triggering**
   - Only generate suggestions when requested
   - Don't auto-generate for all fields

4. **Model Selection**
   - Use GPT-3.5 for simple tasks (cheaper)
   - Use GPT-4 only for complex suggestions

---

## Risk Management

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **OpenAI API Downtime** | Low | High | Implement retry logic, fallback to cached suggestions, show graceful error messages |
| **Slow Response Times** | Medium | Medium | Optimize queries, implement caching, set timeout limits, show loading indicators |
| **Poor Suggestion Quality** | Medium | Medium | Iterate on prompts, collect user feedback, use better examples, fine-tune approach |
| **Vector DB Performance** | Low | Medium | Start without vector DB, add only when needed, optimize indexes |
| **SQL Injection** | Low | Critical | Validate all generated SQL, use parameterized queries, whitelist operations |

### Security Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Unauthorized Data Access** | Low | Critical | Multi-layer security, audit all queries, validate at every step, fail-closed approach |
| **Data Leakage via AI** | Low | Critical | Filter all queries by user's grants, validate results, log all access |
| **Prompt Injection** | Medium | Medium | Sanitize user inputs, validate AI outputs, use structured prompts |
| **API Key Exposure** | Low | Critical | Store in secure config, never log keys, rotate regularly |

### Business Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Low User Adoption** | Medium | Medium | User training, clear value proposition, gather feedback, iterate on UX |
| **Cost Overruns** | Low | Medium | Monitor usage closely, set budget alerts, implement cost controls |
| **Regulatory Concerns** | Low | High | Ensure HIPAA compliance if needed, audit trail, data privacy controls |
| **Dependency on OpenAI** | Medium | Medium | Design for provider flexibility, consider alternatives (Anthropic, Azure) |

---

## Success Metrics

### User Adoption Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Feature Awareness** | 80% of users aware | Survey after 1 month |
| **Trial Rate** | 50% try feature | Track first-time usage |
| **Regular Usage** | 30% use weekly | Track active users |
| **Suggestion Acceptance** | 60% accept/edit | Track accept/reject rates |

### Quality Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Suggestion Relevance** | 4.0/5.0 rating | User ratings |
| **Answer Accuracy** | 85% correct | User feedback + manual review |
| **Response Time** | < 3 seconds | Server logs |
| **Error Rate** | < 5% | Error logs |

### Business Impact Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Time Savings** | 40% reduction | Time tracking study |
| **Report Quality** | 20% improvement | Reviewer ratings |
| **User Satisfaction** | 4.0/5.0 | Post-implementation survey |
| **Support Tickets** | No increase | Help desk metrics |

### Technical Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Uptime** | 99.5% | Monitoring |
| **Security Incidents** | 0 | Audit logs |
| **Cost per Suggestion** | < $0.05 | Usage logs |
| **Classification Accuracy** | 90% | Manual validation |

---

## Appendix A: Example Prompts

### Content Suggestion Prompt Template

```
You are an expert grant writer for HRSA Health Center Program reports.

Generate a Performance Narrative for the following grant:

Grant Information:
- Grant Number: {grantNumber}
- Grant Type: {grantType}
- Program: {programName}
- Focus Areas: {focusAreas}
- Reporting Period: {reportingPeriod}

Previous Report Content (for continuity):
{previousContent}

Examples of High-Quality Approved Reports:

Example 1 (Rating: 5/5):
{example1}

Example 2 (Rating: 5/5):
{example2}

Example 3 (Rating: 4/5):
{example3}

Please generate a professional performance narrative that:
1. Maintains consistency with the previous report
2. Addresses all focus areas
3. Follows the style and structure of the approved examples
4. Includes specific, measurable accomplishments
5. Is 500-1000 words
6. Uses professional, clear language

Performance Narrative:
```

### Question Classification Prompt

```
You are a query classifier for a grant management system.

Database contains:
- Structured data: Grant types, statuses, numbers, dates, multiselect fields
- Unstructured data: Narrative text, descriptions, explanations

Classify questions into:
1. VECTOR_SEARCH - For semantic/content questions about narratives
2. SQL_QUERY - For structured data queries (counts, lists, filters)
3. HYBRID - For questions needing both approaches

Examples:

Q: "What challenges did grantees face with staffing?"
A: {"type": "VECTOR_SEARCH", "reason": "Searching narrative content for semantic meaning", "confidence": 0.95}

Q: "How many C16 grants are there?"
A: {"type": "SQL_QUERY", "reason": "Counting structured data with filter", "confidence": 0.98}

Q: "How many grants mentioned telehealth?"
A: {"type": "HYBRID", "reason": "Counting (SQL) + semantic search (vector)", "confidence": 0.92}

Now classify this question:
{userQuestion}

Return only valid JSON with type, reason, and confidence.
```

### Text-to-SQL Prompt

```
You are an expert SQL query generator for a grant management system.

Database Schema:

Grants Table:
- GrantId (UNIQUEIDENTIFIER, PK)
- GrantNumber (VARCHAR) - Format: GX-2024-99999
- GrantType (VARCHAR) - Values: C16, C17, C18, H80
- ProgramTypeCode (INT)
- Status (VARCHAR) - Values: Active, Closed, Suspended

GrantDeliverables Table:
- DeliverableId (UNIQUEIDENTIFIER, PK)
- GrantId (UNIQUEIDENTIFIER, FK)
- ReportingPeriod (VARCHAR) - Format: Q1 2024
- Status (VARCHAR) - Values: Draft, Submitted, Approved
- SubmittedDate (DATETIME)

DeliverableResponses Table:
- ResponseId (UNIQUEIDENTIFIER, PK)
- DeliverableId (UNIQUEIDENTIFIER, FK)
- FormType (VARCHAR) - Values: Performance, Accomplishments, Challenges
- QuestionNumber (INT)
- QuestionType (VARCHAR) - Values: Text, Number, MultiSelect, Radio
- ResponseText (NVARCHAR) - For Text type
- ResponseNumber (DECIMAL) - For Number type
- ResponseOptions (NVARCHAR) - For MultiSelect (comma-separated)
- ResponseSingle (VARCHAR) - For Radio type

Common Questions:
- Q2: Number of patients served (Number)
- Q3: Services provided (MultiSelect: Primary Care, Mental Health, Dental, etc.)
- Q4: Met goals? (Radio: Yes, No, Partially)

Security Context:
User Role: {userRole}
Accessible Grant IDs: {accessibleGrants}

CRITICAL SECURITY RULE:
You MUST include this WHERE clause in EVERY query:
WHERE g.GrantId IN ({accessibleGrants})

User Question: {userQuestion}

Generate ONLY the SQL query, no explanation.
Use proper JOINs and WHERE clauses.
Return only SELECT statements.

SQL Query:
```

---

## Appendix B: Testing Checklist

### Security Testing

- [ ] Grantee cannot access other grants' data
- [ ] Reviewer can only access assigned grants
- [ ] Admin can access all grants
- [ ] SQL injection attempts blocked
- [ ] Prompt injection attempts handled
- [ ] API keys not exposed in logs
- [ ] All queries include security filters
- [ ] Audit logs capture all access

### Functional Testing

- [ ] Content suggestions generate correctly
- [ ] Q&A returns relevant answers
- [ ] Question classification accurate
- [ ] Vector search returns similar content
- [ ] SQL queries execute correctly
- [ ] Hybrid queries work properly
- [ ] Error handling graceful
- [ ] Loading indicators show

### Performance Testing

- [ ] Response time < 3 seconds
- [ ] Handles 100 concurrent users
- [ ] No memory leaks
- [ ] Database queries optimized
- [ ] API rate limits respected
- [ ] Caching works correctly

### User Experience Testing

- [ ] UI intuitive and clear
- [ ] Suggestions helpful
- [ ] Answers understandable
- [ ] Error messages clear
- [ ] Mobile responsive
- [ ] Accessibility compliant

---

## Appendix C: Glossary

**Embedding** - A vector (array of numbers) representing the semantic meaning of text

**Vector Database** - A specialized database for storing and searching embeddings

**Semantic Search** - Finding content by meaning, not just keywords

**Text-to-SQL** - Converting natural language questions to SQL queries

**RAG (Retrieval Augmented Generation)** - Using retrieved context to generate better AI responses

**Prompt Engineering** - Crafting effective instructions for AI models

**Token** - Unit of text for AI processing (roughly 4 characters)

**Row-Level Security (RLS)** - Restricting data access at the row level based on user

**Cosine Similarity** - Measure of similarity between two vectors (0-1 scale)

**Metadata Filtering** - Filtering vector search results by structured attributes

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-04-09 | AI Design Team | Initial design document |

---

**End of Document**
