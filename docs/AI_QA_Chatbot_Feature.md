# AI Q&A Chatbot Feature - Design Document

**Project:** Grant Management AI  
**Feature:** AI-Powered Q&A Chatbot for Grant Data  
**Version:** 1.0  
**Date:** April 10, 2026  
**Status:** Design Phase

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Feature Overview](#feature-overview)
3. [Architecture Design](#architecture-design)
4. [Question Classification System](#question-classification-system)
5. [Security Design](#security-design)
6. [Data Architecture](#data-architecture)
7. [Vector Database Strategy](#vector-database-strategy)
8. [Implementation Plan](#implementation-plan)
9. [Cost Analysis](#cost-analysis)
10. [Risk Management](#risk-management)
11. [Success Metrics](#success-metrics)

---

## Executive Summary

### Purpose
Implement an AI-powered Q&A chatbot that allows users to ask natural language questions about their grant data, previous reports, and system information, receiving instant, accurate answers without leaving their current workflow.

### Key Features
1. **Natural Language Q&A** - Ask questions in plain English
2. **Semantic Search** - Find relevant content across narrative reports
3. **Structured Data Queries** - Query database using natural language
4. **Hybrid Queries** - Combine semantic search with structured data
5. **Secure Access** - Row-level security ensuring users only access their own data
6. **Multi-User Support** - Different capabilities for Grantee, Reviewer, and Admin roles

### Technology Stack
- **AI Platform:** OpenAI GPT-4 & Embeddings API
- **Vector Database:** Pinecone (for semantic search)
- **Backend:** ASP.NET WebForms (C#)
- **Database:** SQL Server (GrantDB / GrantPortal)
- **Authentication:** Existing ASP.NET authentication system

### Expected Benefits
- Instant access to historical data without searching through reports
- Reduced time to find information (80% faster)
- Better decision-making with data-driven insights
- Enhanced user experience and productivity

---

## Feature Overview

### Use Case 1: Grantee Q&A (Own Data)

**Scenario:** Grantee user needs to reference previous report data while filling out current report

**User Flow:**
1. User clicks "Ask AI" button or opens chat panel
2. User types question: "What did I write about telehealth last quarter?"
3. System:
   - Authenticates user
   - Identifies accessible grants (user's own)
   - Classifies question type (narrative search)
   - Searches user's previous reports
   - Synthesizes answer from relevant passages
4. System displays answer with source citations
5. User can ask follow-up questions

**Example Questions:**
- "What did I write about telehealth last quarter?"
- "How many patients did we serve in Q1 2024?"
- "What challenges did I mention in my last report?"
- "Show me my previous accomplishments related to mental health"

**Value:** Instant access to own historical data, no context switching

---

### Use Case 2: Admin/Reviewer Cross-Grant Analysis

**Scenario:** Admin needs to analyze trends across multiple grants

**User Flow:**
1. Admin opens Q&A interface
2. Admin asks: "How many grants mentioned staffing challenges in Q1 2024?"
3. System:
   - Authenticates admin (has access to all grants)
   - Classifies question (hybrid: semantic + count)
   - Performs vector search for "staffing challenges"
   - Counts matching grants
   - Generates comprehensive answer with statistics
4. System displays answer with breakdown by grant type
5. Admin can drill down with follow-up questions

**Example Questions:**
- "How many C16 grants mentioned telehealth?"
- "What are common challenges across all grants?"
- "Which grants exceeded their patient targets?"
- "Show me trends in mental health services"

**Value:** Data-driven insights, trend analysis, reporting support

---

### Use Case 3: Hybrid Queries

**Scenario:** User needs both narrative content and structured data

**User Flow:**
1. User asks: "Show me grants that served more than 2000 patients and mentioned telehealth success"
2. System:
   - Classifies as hybrid query
   - Step 1: SQL query for grants with > 2000 patients
   - Step 2: Vector search for "telehealth success" in those grants
   - Combines results
3. System displays matching grants with relevant excerpts

**Value:** Complex queries made simple through natural language

---

## Architecture Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERFACE                            │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Q&A Chat Interface                                │     │
│  │  ┌──────────────────────────────────────────┐     │     │
│  │  │  Chat History                            │     │     │
│  │  │  • User: "What did I write about..."     │     │     │
│  │  │  • AI: "In your Q1 2024 report..."       │     │     │
│  │  │  • User: "How many patients..."          │     │     │
│  │  │  • AI: "You served 2,850 patients..."    │     │     │
│  │  └──────────────────────────────────────────┘     │     │
│  │                                                    │     │
│  │  Input Box: [Type your question here...]          │     │
│  │  [Send] button                                     │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│              SECURITY & AUTHENTICATION LAYER                 │
│  • Authenticate User                                         │
│  • Get User Role (Grantee/Reviewer/Admin)                    │
│  • Get Accessible Grants                                     │
│  • Build Security Context                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                 QUESTION CLASSIFIER                          │
│  Analyzes question and determines:                           │
│  • VECTOR_SEARCH - Narrative/semantic questions              │
│  • SQL_QUERY - Structured data questions                     │
│  • HYBRID - Combination of both                              │
│  • AMBIGUOUS - Needs clarification                           │
└─────────────────────────────────────────────────────────────┘
                          ↓
                    ┌─────┴─────┐
                    │           │
        ┌───────────┴─┐   ┌─────┴──────────┐   ┌──────────────┐
        │  VECTOR     │   │  SQL QUERY     │   │   HYBRID     │
        │  SEARCH     │   │  GENERATOR     │   │   QUERY      │
        │  SERVICE    │   │  SERVICE       │   │   SERVICE    │
        └─────────────┘   └────────────────┘   └──────────────┘
                ↓               ↓                      ↓
        ┌───────┴───────┐   ┌──┴────────┐    ┌───────┴────────┐
        │   Pinecone    │   │   SQL     │    │  Pinecone +    │
        │   (Vectors)   │   │  Server   │    │  SQL Server    │
        └───────────────┘   └───────────┘    └────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                 ANSWER GENERATOR                             │
│  • Combines retrieved data                                   │
│  • Generates natural language answer                         │
│  • Adds source citations                                     │
│  • Formats for display                                       │
└─────────────────────────────────────────────────────────────┘
```

### Component Details

#### 1. Chat Interface

**Features:**
- Chat-style conversation UI
- Message history (session-based)
- Typing indicators
- Source citations with links
- Copy/share functionality
- Clear conversation button

**UI Placement Options:**
- **Option A:** Slide-in panel from right side
- **Option B:** Modal dialog
- **Option C:** Dedicated page with chat interface
- **Recommended:** Slide-in panel (accessible from any page)

#### 2. Question Classifier

**Purpose:** Determine the best strategy to answer the question

**Classification Types:**
- **VECTOR_SEARCH** - Semantic/content questions about narratives
- **SQL_QUERY** - Structured data queries (counts, lists, filters)
- **HYBRID** - Questions needing both approaches
- **AMBIGUOUS** - Unclear intent, needs clarification

**Classification Methods:**
1. **Rule-Based (Fast)** - Pattern matching for 80% of questions
2. **AI-Based (Smart)** - OpenAI classification for ambiguous 20%

**Decision Logic:**
```
Question Keywords → Classification

"what did", "how did", "why did" → VECTOR_SEARCH
"how many", "count", "total" → SQL_QUERY
"how many...mentioned" → HYBRID
Unclear → AMBIGUOUS (ask for clarification)
```

#### 3. Vector Search Service

**Purpose:** Search narrative content semantically

**Process:**
1. Convert question to embedding (OpenAI)
2. Search Pinecone with security filters
3. Retrieve top K similar passages
4. Get full text from SQL Server
5. Return relevant excerpts

**Security Filters:**
```javascript
{
  queryVector: [embedding],
  topK: 10,
  filter: {
    "grantId": { "$in": [user's accessible grant IDs] }
  }
}
```

#### 4. SQL Query Generator

**Purpose:** Convert natural language to SQL queries

**Process:**
1. Analyze question for data requirements
2. Generate SQL with security filters
3. Validate SQL (block dangerous operations)
4. Execute query
5. Return results

**Security Injection:**
```sql
-- Generated SQL automatically includes:
WHERE g.GrantId IN (@AccessibleGrants)
```

#### 5. Hybrid Query Service

**Purpose:** Combine semantic search with structured queries

**Process:**
1. Identify semantic and structured components
2. Execute vector search for semantic part
3. Execute SQL query for structured part
4. Merge results
5. Generate combined answer

**Example:**
```
Question: "How many grants mentioned telehealth?"

Step 1: Vector search for "telehealth" → Get grant IDs
Step 2: SQL count of those grant IDs → Get number
Step 3: Combine: "5 grants mentioned telehealth: [list]"
```

#### 6. Answer Generator

**Purpose:** Create natural language answers from data

**Process:**
1. Receive data from search/query services
2. Build context-aware prompt
3. Call OpenAI GPT-4
4. Generate natural language answer
5. Add source citations
6. Format for display

---

## Question Classification System

### Purpose
Automatically determine whether to use vector search, SQL query, or hybrid approach based on the user's question.

### Classification Strategy

#### Method 1: Rule-Based Classification (Fast - 80% of cases)

**Vector Search Indicators:**
```
Keywords: "what", "how", "why", "describe", "explain", "show me"
Content words: "wrote", "mentioned", "discussed", "said", "described"
Topics: "challenges", "accomplishments", "strategies", "approaches"

Examples:
✓ "What did I write about telehealth?"
✓ "How did other grantees handle staffing?"
✓ "Show me challenges related to recruitment"
```

**SQL Query Indicators:**
```
Keywords: "how many", "count", "total", "list", "show all"
Structured words: "status", "approved", "type", "date", "number"
Operators: "greater than", "less than", "between", "equals"

Examples:
✓ "How many C16 grants are there?"
✓ "List all approved reports from Q1 2024"
✓ "Show me grants that served more than 2000 patients"
```

**Hybrid Indicators:**
```
Combination: Counting/listing + semantic content

Examples:
✓ "How many grants mentioned telehealth?"
✓ "List grants that described mental health success"
✓ "Count reports with staffing challenges"
```

#### Method 2: AI-Based Classification (Smart - 20% of cases)

For ambiguous questions, use OpenAI to classify:

**Classification Prompt:**
```
You are a query classifier for a grant management system.

Database contains:
- Structured data: Grant types, statuses, numbers, dates, multiselect fields
- Unstructured data: Narrative text, descriptions, explanations

Classify questions into:
1. VECTOR_SEARCH - For semantic/content questions about narratives
2. SQL_QUERY - For structured data queries (counts, lists, filters)
3. HYBRID - For questions needing both approaches
4. AMBIGUOUS - For unclear questions needing clarification

Examples:

Q: "What challenges did grantees face with staffing?"
A: {"type": "VECTOR_SEARCH", "reason": "Searching narrative content", "confidence": 0.95}

Q: "How many C16 grants are there?"
A: {"type": "SQL_QUERY", "reason": "Counting structured data", "confidence": 0.98}

Q: "How many grants mentioned telehealth?"
A: {"type": "HYBRID", "reason": "Counting + semantic search", "confidence": 0.92}

Q: "Show me mental health grants"
A: {"type": "AMBIGUOUS", "reason": "Unclear if service type or narrative content", "confidence": 0.60}

Now classify: {userQuestion}

Return JSON with type, reason, and confidence (0.0-1.0).
```

### Decision Tree

```
User Question
  ↓
Quick Pattern Match (< 1ms)
  ↓
  ├─ Obvious? ──Yes──> Execute Strategy
  │
  └─ No ──> AI Classification (200-500ms)
              ↓
              ├─ Confidence > 90%? ──Yes──> Execute
              │
              ├─ Confidence 70-90%? ──Yes──> Execute with explanation
              │
              ├─ Confidence 50-70%? ──Yes──> Ask for clarification
              │
              └─ Confidence < 50%? ──> Request rephrase
```

### Confidence Thresholds

| Confidence | Action | User Experience |
|------------|--------|-----------------|
| > 90% | Execute immediately | Show answer |
| 70-90% | Execute with explanation | "I interpreted this as... Is that correct?" |
| 50-70% | Ask for clarification | "Did you mean: Option A or Option B?" |
| < 50% | Request rephrase | "I'm not sure what you're asking. Could you rephrase?" |

### Classification Examples

| Question | Type | Strategy | Reasoning |
|----------|------|----------|-----------|
| "What did I write about telehealth last quarter?" | VECTOR_SEARCH | Search user's reports for "telehealth" | Seeking narrative content |
| "How many patients did we serve in Q1 2024?" | SQL_QUERY | Query ResponseNumber field | Structured numeric data |
| "How many grants mentioned staffing challenges?" | HYBRID | Vector search + count | Semantic search + aggregation |
| "Show me grants with mental health services" | SQL_QUERY | Query ResponseOptions field | Structured multiselect field |
| "What are common challenges?" | VECTOR_SEARCH | Search all accessible reports | Semantic analysis of narratives |
| "List approved reports from 2024" | SQL_QUERY | Query Status and Date fields | Structured filters |

---

## Security Design

### Security Requirements

1. **Authentication** - User must be logged in
2. **Authorization** - User can only query their accessible grants
3. **Data Isolation** - Grantee sees only own data, Admin sees all
4. **Query Validation** - All generated SQL validated for safety
5. **Audit Trail** - All questions and answers logged

### Three-Layer Security Model

#### Layer 1: Authentication & Authorization

**Build User Security Context:**
```csharp
public async Task<UserContext> GetUserContextAsync(Guid userId)
{
    var data = await _repository.ExecuteAsync<dynamic>(@"
        SELECT 
            u.UserId,
            CASE 
                WHEN u.IsAdmin = 1 THEN 'Admin'
                WHEN EXISTS (
                    SELECT 1 FROM ReviewerAssignments ra 
                    WHERE ra.UserId = u.UserId
                ) THEN 'Reviewer'
                ELSE 'Grantee'
            END as UserRole,
            gr.GrantId
        FROM AC_User u
        LEFT JOIN GrantsRegistration gr ON u.UserId = gr.UserId
        WHERE u.UserId = @UserId
          AND (gr.IsActive = 1 OR gr.IsActive IS NULL)
    ", new { UserId = userId });
    
    return new UserContext
    {
        UserId = userId,
        UserRole = data.FirstOrDefault()?.UserRole ?? "Grantee",
        AccessibleGrantIds = data.Select(d => (Guid)d.GrantId).ToList()
    };
}
```

#### Layer 2: Query-Level Security

**For Vector Search:**
```csharp
var results = await _vectorDb.SearchAsync(new VectorSearchRequest
{
    QueryVector = embedding,
    TopK = 10,
    Filter = new Dictionary<string, object>
    {
        { "grantId", new Dictionary<string, object>
            {
                { "$in", userContext.AccessibleGrantIds.Select(g => g.ToString()).ToList() }
            }
        }
    }
});
```

**For SQL Queries:**
```csharp
private string InjectSecurityFilter(string sql, UserContext userContext)
{
    if (userContext.UserRole == "Admin")
        return sql;  // Admin sees all
    
    var grantIdList = string.Join(",", 
        userContext.AccessibleGrantIds.Select(g => $"'{g}'"));
    
    // Inject WHERE clause
    if (sql.ToUpper().Contains("WHERE"))
    {
        return sql.Replace("WHERE", 
            $"WHERE g.GrantId IN ({grantIdList}) AND ");
    }
    else
    {
        return $"{sql} WHERE g.GrantId IN ({grantIdList})";
    }
}
```

#### Layer 3: Result Validation

**Validate all results belong to user:**
```csharp
private List<T> ValidateResults<T>(List<T> results, UserContext userContext)
{
    return results.Where(r => 
        userContext.AccessibleGrantIds.Contains(r.GrantId)
    ).ToList();
}
```

### Access Control Matrix

| User Role | Own Grant Data | Other Grants (Approved) | All Grants | Cross-Grant Analysis |
|-----------|----------------|-------------------------|------------|---------------------|
| Grantee   | ✅ Full Access | ❌ No Access           | ❌ No      | ❌ No               |
| Reviewer  | N/A            | ✅ Assigned Only       | ❌ No      | ✅ Assigned Only    |
| Admin     | N/A            | ✅ Yes                 | ✅ Yes     | ✅ Yes              |

### Security Validation Checklist

- ✅ User authentication verified
- ✅ User role identified
- ✅ Accessible grants determined
- ✅ Vector search filtered by grantId
- ✅ SQL queries include WHERE clause with grant filter
- ✅ Generated SQL validated (no DROP, DELETE, etc.)
- ✅ Results validated against accessible grants
- ✅ All queries logged with user ID
- ✅ Errors logged for security review

---

## Data Architecture

### SQL Server Schema

#### Existing Tables (No Changes)
- `Grants` - Grant master data
- `GrantDeliverables` - Progress reports
- `GrantsRegistration` - User-grant access control
- `AC_User` - User accounts

#### New Tables

**1. DeliverableResponses** (Shared with Content Suggestions)
```sql
CREATE TABLE DeliverableResponses (
    ResponseId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DeliverableId UNIQUEIDENTIFIER NOT NULL,
    FormType VARCHAR(50) NOT NULL,
    QuestionNumber INT NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    QuestionType VARCHAR(50) NOT NULL,  -- 'Text', 'Number', 'MultiSelect', 'Radio'
    
    -- Response data
    ResponseText NVARCHAR(MAX),      -- For Text type
    ResponseNumber DECIMAL(18,2),    -- For Number type
    ResponseOptions NVARCHAR(MAX),   -- For MultiSelect
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

**2. AI_QueryAuditLog**
```sql
CREATE TABLE AI_QueryAuditLog (
    AuditId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    UserRole VARCHAR(50) NOT NULL,
    
    Question NVARCHAR(MAX) NOT NULL,
    Classification VARCHAR(50),  -- 'VECTOR_SEARCH', 'SQL_QUERY', 'HYBRID'
    ClassificationConfidence DECIMAL(5,2),
    
    GeneratedSQL NVARCHAR(MAX) NULL,
    SecurityFilterApplied BIT NOT NULL,
    AccessibleGrants NVARCHAR(MAX),  -- JSON array of grant IDs
    
    -- Results
    ResultCount INT NULL,
    ResponseTimeMs INT NULL,
    
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    Timestamp DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_QueryAuditLog_User FOREIGN KEY (UserId) 
        REFERENCES AC_User(UserId)
);

CREATE INDEX IX_AI_QueryAuditLog_User ON AI_QueryAuditLog(UserId);
CREATE INDEX IX_AI_QueryAuditLog_Timestamp ON AI_QueryAuditLog(Timestamp);
CREATE INDEX IX_AI_QueryAuditLog_Classification ON AI_QueryAuditLog(Classification);
```

**3. AI_ConversationHistory** (Optional - for context)
```sql
CREATE TABLE AI_ConversationHistory (
    ConversationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    SessionId VARCHAR(100) NOT NULL,
    
    MessageType VARCHAR(50) NOT NULL,  -- 'UserQuestion', 'AIAnswer'
    MessageText NVARCHAR(MAX) NOT NULL,
    
    Timestamp DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_ConversationHistory_User FOREIGN KEY (UserId) 
        REFERENCES AC_User(UserId)
);

CREATE INDEX IX_AI_ConversationHistory_Session 
    ON AI_ConversationHistory(SessionId, Timestamp);
```

### Pinecone Vector Database

**Index Configuration:**
```javascript
{
  name: "bphc-progress-reports",
  dimension: 1536,  // text-embedding-3-small
  metric: "cosine",
  podType: "p1.x1"
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

### Vectorization Strategy

**What to Vectorize:**
- ✅ All text field responses (narrative content)
- ✅ Approved reports only (initially)
- ✅ Individual questions (not entire forms)

**What NOT to Vectorize:**
- ❌ Number fields
- ❌ Multi-select dropdowns
- ❌ Radio buttons
- ❌ Draft/incomplete reports

**Vectorization Process:**
1. Extract text responses from DeliverableResponses
2. Build context-enriched text (question + response)
3. Generate embedding via OpenAI
4. Store in Pinecone with metadata
5. Original text stays in SQL Server

---

## Vector Database Strategy

### Why Vector Database for Q&A

**Benefits:**
- **Semantic Search** - Find content by meaning, not just keywords
- **Better Relevance** - Understands context and intent
- **Cross-Document Search** - Search across all reports simultaneously
- **Fuzzy Matching** - Handles variations in phrasing

**Use Cases:**
- "What did I write about telehealth?" → Finds all telehealth mentions
- "Show me challenges related to staffing" → Semantic understanding of "staffing challenges"
- "How did grantees handle recruitment?" → Finds various recruitment-related content

### Implementation Approach

**Phase 1: Start Simple (SQL Full-Text Search)**
- Use SQL Server full-text search
- Good enough for keyword matching
- No additional cost
- Faster to implement

**Phase 2: Add Vector DB (After 3-6 months)**
- Migrate to Pinecone for semantic search
- Significantly better relevance
- Handles complex queries
- Cost: ~$70/month

### Hybrid Approach (Recommended)

**Combine SQL + Vector Search:**
1. Use SQL for structured data queries
2. Use Pinecone for semantic narrative search
3. Merge results for hybrid queries

**Example:**
```
Question: "How many grants with > 2000 patients mentioned telehealth?"

Step 1 (SQL): Find grants with > 2000 patients
Step 2 (Vector): Search those grants for "telehealth"
Step 3 (Merge): Count and list results
```

---

## Implementation Plan

### Phase 1: Foundation (Weeks 1-2)

**Goal:** Set up core infrastructure

**Tasks:**
1. **Database Setup**
   - Create DeliverableResponses table
   - Create AI_QueryAuditLog table
   - Add indexes
   - Migrate existing report data

2. **Security Layer**
   - Implement UserContext service
   - Create security filter injection
   - Add validation chain
   - Test with different roles

3. **OpenAI Integration**
   - Set up API credentials
   - Create embedding service
   - Create completion service
   - Test connectivity

**Deliverables:**
- Database schema deployed
- Security layer functional
- OpenAI integration working

**Success Criteria:**
- All tests pass
- Security validations work
- API calls successful

---

### Phase 2: Question Classification (Weeks 3-4)

**Goal:** Build intelligent question routing

**Tasks:**
1. **Rule-Based Classifier**
   - Implement pattern matching
   - Define classification rules
   - Test with sample questions

2. **AI-Based Classifier**
   - Build classification prompt
   - Implement OpenAI classification
   - Add confidence scoring

3. **Decision Logic**
   - Implement decision tree
   - Add confidence thresholds
   - Handle ambiguous questions

4. **Testing**
   - Test with 100+ sample questions
   - Measure accuracy
   - Iterate on rules

**Deliverables:**
- Question classifier functional
- 90%+ classification accuracy
- Handles ambiguous cases

**Success Criteria:**
- 90%+ accuracy on test set
- < 100ms classification time
- Graceful handling of edge cases

---

### Phase 3: Vector Search (Weeks 5-6)

**Goal:** Implement semantic search for narratives

**Tasks:**
1. **Pinecone Setup**
   - Create account and index
   - Configure security
   - Test connectivity

2. **Data Vectorization**
   - Extract all text responses
   - Generate embeddings
   - Upload to Pinecone
   - Verify data integrity

3. **Search Service**
   - Implement vector search
   - Add security filters
   - Test relevance

4. **Answer Generation**
   - Build answer prompts
   - Generate natural language answers
   - Add source citations

**Deliverables:**
- Pinecone operational
- All data vectorized
- Search service working
- Answers generated

**Success Criteria:**
- All data migrated
- Relevant results returned
- < 3 second response time
- Natural language answers

---

### Phase 4: SQL Query Generation (Weeks 7-8)

**Goal:** Enable structured data queries

**Tasks:**
1. **Text-to-SQL Service**
   - Build SQL generation prompts
   - Implement query generator
   - Add security injection
   - Validate generated SQL

2. **Query Executor**
   - Execute queries safely
   - Handle errors
   - Format results

3. **Answer Formatter**
   - Convert results to natural language
   - Add context
   - Format for display

4. **Testing**
   - Test with various questions
   - Validate security
   - Measure accuracy

**Deliverables:**
- SQL generation working
- Security validated
- Answers formatted

**Success Criteria:**
- 85%+ query accuracy
- All queries include security filters
- No SQL injection vulnerabilities

---

### Phase 5: Hybrid Queries (Weeks 9-10)

**Goal:** Combine vector search + SQL

**Tasks:**
1. **Hybrid Service**
   - Implement query orchestration
   - Combine vector + SQL results
   - Merge and rank results

2. **Complex Query Handling**
   - Multi-step queries
   - Result aggregation
   - Context preservation

3. **Optimization**
   - Cache frequent queries
   - Optimize performance
   - Reduce API calls

**Deliverables:**
- Hybrid queries working
- Performance optimized

**Success Criteria:**
- Complex queries work correctly
- < 5 second response time
- Accurate results

---

### Phase 6: UI & Deployment (Weeks 11-12)

**Goal:** Build UI and deploy

**Tasks:**
1. **Chat Interface**
   - Build chat UI component
   - Add to application
   - Implement conversation history
   - Add source citations

2. **User Experience**
   - Loading indicators
   - Error messages
   - Help/examples
   - Feedback mechanism

3. **Testing & Deployment**
   - User acceptance testing
   - Performance testing
   - Security audit
   - Production deployment

**Deliverables:**
- Chat UI functional
- User testing complete
- Production deployed

**Success Criteria:**
- Users can ask questions easily
- Positive feedback (4.0/5.0+)
- No security issues
- Stable performance

---

## Cost Analysis

### Initial Setup Costs

| Item | Cost | Notes |
|------|------|-------|
| OpenAI API Setup | $0 | Pay-as-you-go |
| Pinecone Account | $0 | Free tier initially |
| Development Time | Internal | 12 weeks |
| **Total Initial** | **$0** | No upfront costs |

### Monthly Operating Costs

#### Scenario 1: 100 Users, 1,000 Questions/Month

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Embeddings** | 1,000 questions × 1 embedding | $0.00002/1K tokens | $0.50 |
| **OpenAI Completions** | 1,000 answers × 500 tokens | $0.01/1K tokens | $10.00 |
| **Pinecone** | 10,000 vectors | $70 | $70.00 |
| **SQL Server** | Existing | $0 | $0 |
| **Total Monthly** | | | **$80.50** |

#### Scenario 2: 500 Users, 10,000 Questions/Month

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Embeddings** | 10,000 × 1 embedding | $0.00002/1K tokens | $5.00 |
| **OpenAI Completions** | 10,000 × 500 tokens | $0.01/1K tokens | $100.00 |
| **Pinecone** | 50,000 vectors | $70 | $70.00 |
| **Total Monthly** | | | **$175.00** |

### Cost Optimization

1. **Caching** - Cache frequent questions (save 30%)
2. **Classification** - Use rule-based first (save API calls)
3. **Batch Processing** - Vectorize in batches
4. **Model Selection** - Use GPT-3.5 for simple answers

---

## Risk Management

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **OpenAI API Downtime** | High | Retry logic, cached responses, graceful errors |
| **Poor Answer Quality** | Medium | Iterate on prompts, user feedback, improve examples |
| **Slow Response Times** | Medium | Caching, optimization, timeout limits |
| **Classification Errors** | Medium | Improve rules, AI backup, user feedback |

### Security Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Unauthorized Data Access** | Critical | Multi-layer security, audit all queries, validate results |
| **SQL Injection** | Critical | Validate all SQL, whitelist operations, parameterized queries |
| **Data Leakage** | Critical | Filter by grantId, validate results, audit logs |
| **Prompt Injection** | Medium | Sanitize inputs, validate outputs, structured prompts |

### Business Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Low User Adoption** | Medium | Training, clear value, gather feedback |
| **Cost Overruns** | Medium | Monitor usage, budget alerts, cost controls |
| **Inaccurate Answers** | High | Confidence scores, source citations, user feedback |

---

## Success Metrics

### User Adoption

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Feature Awareness** | 80% | Survey |
| **Trial Rate** | 40% | First-time usage |
| **Regular Usage** | 25% | Weekly active users |
| **Questions per User** | 5/month | Usage logs |

### Quality

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Answer Accuracy** | 85% | User feedback + manual review |
| **Classification Accuracy** | 90% | Manual validation |
| **Response Time** | < 3 seconds | Server logs |
| **User Rating** | 4.0/5.0 | In-app ratings |

### Business Impact

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Time Savings** | 80% faster | Time tracking |
| **User Satisfaction** | 4.0/5.0 | Survey |
| **Support Tickets** | 20% reduction | Help desk metrics |

---

## Appendix: Example Prompts

### Question Classification Prompt

```
You are a query classifier for a grant management system.

Database contains:
- Structured data: Grant types, statuses, numbers, dates, multiselect fields
- Unstructured data: Narrative text, descriptions, explanations

Classify into: VECTOR_SEARCH, SQL_QUERY, HYBRID, or AMBIGUOUS

Question: {userQuestion}

Return JSON: {"type": "...", "reason": "...", "confidence": 0.0-1.0}
```

### Answer Generation Prompt

```
You are a helpful assistant for a grant management system.

User Question: {question}

Retrieved Data:
{retrievedData}

Generate a clear, accurate answer based on the data provided.
Include specific details and cite sources.
If data is insufficient, say so clearly.

Answer:
```

### Text-to-SQL Prompt

```
You are an expert SQL query generator.

Database Schema:
{schema}

Security Context:
User can access grants: {accessibleGrants}

CRITICAL: Include WHERE g.GrantId IN ({accessibleGrants}) in EVERY query.

User Question: {question}

Generate ONLY the SQL query. Use SELECT only (no INSERT, UPDATE, DELETE).

SQL:
```

---

## Conversation Memory

### Overview

The chatbot maintains conversation context within a browser session. Each exchange (question + answer) is persisted to the `ChatConversations` table and injected into subsequent prompts, enabling natural follow-up questions without the user repeating context.

**Scope:** Session-only. Refreshing the page or opening a new tab starts a fresh conversation. No cross-session persistence is required for the current implementation.

---

### How It Works

**Session lifecycle:**
1. First message → frontend sends `conversationId: null` → backend generates a new `SessionId` (Guid) and returns it
2. Subsequent messages → frontend sends the same `SessionId` → backend loads prior turns and includes them in the prompt
3. Panel closed / page refreshed → frontend state cleared → next message starts a new session

**Prompt structure with history:**
```
[System]: You are an assistant for grant GX-2024-00001 (C16)...

[Report context — from vector search or keyword fallback]:
  [2024 Q1 - PerformanceNarrative]: We served 2,850 patients...
  [2024 Q2 - PerformanceNarrative]: We served 3,100 patients...

[Conversation so far]:
  User: What was the patient count in Q1 2024?
  Assistant: You served 2,850 patients in Q1 2024.
  User: And in Q2?
  Assistant: In Q2 2024, you served 3,100 patients.

[Current question]: How does that compare to the full year?
```

**History depth:** Last 5 turns (10 messages: 5 user + 5 assistant) are loaded. Older turns are dropped to protect the LLM context window. For small local models (qwen2.5-coder:7b), this is the safe limit.

---

### Data Model

**`ChatConversations` table (updated schema):**

```sql
CREATE TABLE ChatConversations (
    ConversationId  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),  -- per-message PK
    SessionId       UNIQUEIDENTIFIER NOT NULL,   -- groups messages into one conversation
    UserId          UNIQUEIDENTIFIER NOT NULL,
    GrantId         UNIQUEIDENTIFIER NOT NULL,
    Role            NVARCHAR(20) NOT NULL,       -- 'user' | 'assistant'
    Content         NVARCHAR(MAX) NOT NULL,
    CreatedDate     DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ChatConversations_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
CREATE INDEX IX_ChatConversations_Session ON ChatConversations(SessionId, CreatedDate);
```

**Key distinction:** `ConversationId` is a per-message primary key. `SessionId` is the conversation group identifier passed between frontend and backend.

---

### Architecture Changes

**New interface — `IChatRepository`:**
```csharp
Task SaveTurnAsync(Guid sessionId, Guid userId, Guid grantId, string question, string answer);
Task<List<ChatConversation>> GetHistoryAsync(Guid sessionId, int maxTurns = 5);
```

**`ChatbotService` flow (updated):**
1. Generate or reuse `SessionId` from request
2. Load history via `IChatRepository.GetHistoryAsync`
3. Build prompt: report context + history block + current question
4. Call LLM
5. Save Q+A pair via `IChatRepository.SaveTurnAsync`
6. Return response with `SessionId` (as `ConversationId` in the DTO)

**Frontend (`chat-widget.component.ts`):** No changes needed — `conversationId` is already tracked in component state and sent with every request.

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-04-10 | AI Design Team | Initial Q&A chatbot document |
| 1.1 | 2026-04-16 | AI Design Team | Added conversation memory — session-scoped history, ChatConversations schema update, IChatRepository |

---

**End of Document**
