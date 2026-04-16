# AI Content Suggestions Feature - Design Document

**Project:** Grant Management AI  
**Feature:** Smart Content Suggestions for Narrative Text Fields  
**Version:** 1.1  
**Date:** April 15, 2026  
**Status:** Active Development

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Feature Overview](#feature-overview)
3. [Architecture Design](#architecture-design)
4. [Security Design](#security-design)
5. [Data Architecture](#data-architecture)
6. [Vector Database Strategy](#vector-database-strategy)
7. [Implementation Plan](#implementation-plan)
8. [Cost Analysis](#cost-analysis)
9. [Risk Management](#risk-management)
10. [Success Metrics](#success-metrics)

---

## Executive Summary

### Purpose
Implement AI-powered content suggestions to assist grantee users in completing narrative text fields in progress reports by providing intelligent, contextual suggestions based on their grant information and approved examples from similar grants.

### Key Features
1. **Smart Content Suggestions** - AI-generated suggestions for narrative text fields
2. **Context-Aware Generation** - Uses grant info, previous reports, and approved examples
3. **User Key Points Input** - User provides current-period highlights (e.g. "90% improvement in health service delivery") that are woven into the generated suggestion
4. **One-Click Integration** - "Get AI Suggestion" button next to text fields
5. **Regenerate Support** - Re-submit with updated key points without reloading the page
6. **Secure Access** - Row-level security ensuring users only access appropriate data

### Technology Stack
- **AI Platform:** OpenAI GPT-4 & Embeddings API
- **Vector Database:** Pinecone (optional, for enhanced suggestions)
- **Backend:** ASP.NET WebForms (C#)
- **Database:** SQL Server (GEMS/BHCMIS)
- **Authentication:** Existing ASP.NET authentication system

### Expected Benefits
- 40-60% reduction in time to complete narrative sections
- Improved quality and consistency of reports
- Better alignment with HRSA expectations
- Reduced writer's block for grantees

---

## Feature Overview

### Use Case: Content Suggestions for Progress Reports

**Scenario:** Grantee user filling out Performance Narrative (Question 10 on Page1.aspx)

**User Flow:**
1. User navigates to Progress Report form
2. User sees "Get AI Suggestion" button next to a narrative text field
3. User optionally enters **key points for the current period** in a textarea, e.g.:
   - `90% improvement in health service delivery`
   - `Extended service to 3 new rural counties`
   - `Telehealth adoption increased by 45%`
4. User clicks "Get AI Suggestion"
5. System shows loading indicator
6. System analyzes:
   - Current grant information (type, program, focus areas)
   - User's previous report content (for continuity)
   - Approved examples from similar grants (for best practices)
   - **User-provided key points** (current period achievements)
7. AI generates contextual suggestion (500-1000 words) that incorporates all inputs
8. Suggestion appears in preview panel
9. User can:
   - **Accept** - Copy to text field
   - **Edit** - Modify suggestion before accepting
   - **Reject** - Dismiss and write manually
   - **Regenerate** - Update key points and regenerate without reloading
10. System logs usage and user feedback

**Value Proposition:**
- **Reduces writer's block** - Provides starting point
- **Ensures completeness** - Covers all required elements
- **Maintains consistency** - Aligns with previous reports
- **Follows best practices** - Based on approved examples
- **Saves time** - 40-60% faster completion

---

## Architecture Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERFACE                            │
│  ┌────────────────────────────────────────────────────┐     │
│  │  Progress Report Form (Page1.aspx)                 │     │
│  │  ┌──────────────────────────────────────────┐     │     │
│  │  │  Performance Narrative Text Field        │     │     │
│  │  │  [5000 char limit]                       │     │     │
│  │  │                                          │     │     │
│  │  │  [Get AI Suggestion] button              │     │     │
│  │  └──────────────────────────────────────────┘     │     │
│  │                                                    │     │
│  │  Suggestion Preview Panel (appears on click)      │     │
│  │  ┌──────────────────────────────────────────┐     │     │
│  │  │  AI-Generated Suggestion                 │     │     │
│  │  │  (editable preview)                      │     │     │
│  │  │                                          │     │     │
│  │  │  [Accept] [Edit] [Reject] [Regenerate]  │     │     │
│  │  └──────────────────────────────────────────┘     │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│              SECURITY & AUTHENTICATION LAYER                 │
│  • Authenticate User (ASP.NET Session)                       │
│  • Get User's Grant Access (GrantsRegistration)              │
│  • Validate Deliverable Ownership                            │
│  • Build Security Context                                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│              CONTENT SUGGESTION SERVICE                      │
│  ┌────────────────────────────────────────────────────┐     │
│  │  1. Context Builder                                │     │
│  │     • Get grant information                        │     │
│  │     • Get previous report content                  │     │
│  │     • Get deliverable details                      │     │
│  │                                                    │     │
│  │  2. Example Finder                                 │     │
│  │     • Find similar approved reports                │     │
│  │     • Filter by grant type, program                │     │
│  │     • Rank by quality/rating                       │     │
│  │                                                    │     │
│  │  3. Prompt Builder                                 │     │
│  │     • Combine context + examples                   │     │
│  │     • Add instructions and constraints             │     │
│  │     • Format for OpenAI                            │     │
│  │                                                    │     │
│  │  4. AI Generator                                   │     │
│  │     • Call OpenAI GPT-4                            │     │
│  │     • Generate suggestion                          │     │
│  │     • Validate output                              │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                          ↓
    ┌─────────────────────┴─────────────────────┐
    ↓                                           ↓
┌──────────────────┐                  ┌──────────────────┐
│  Vector Search   │                  │   SQL Server     │
│  (Pinecone)      │                  │  (GEMS/BHCMIS)   │
│  • Find similar  │                  │  • Grant info    │
│    examples      │                  │  • Previous      │
│  • Semantic      │                  │    reports       │
│    search        │                  │  • Approved      │
│                  │                  │    content       │
└──────────────────┘                  └──────────────────┘
```

### Component Details

#### 1. UI Components

**Button Component:**
```html
<asp:Button ID="btnGetAISuggestion" 
    runat="server" 
    Text="✨ Get AI Suggestion" 
    CssClass="ai-suggestion-btn"
    OnClick="btnGetAISuggestion_Click" />
```

**Suggestion Panel:**
- Modal or slide-in panel
- Editable text area with suggestion
- Action buttons (Accept, Edit, Reject, Regenerate)
- Loading spinner during generation
- Error message display

#### 2. Security Context Service

**Responsibilities:**
- Authenticate user
- Get accessible grants from GrantsRegistration
- Validate user owns the deliverable
- Build UserContext object

**UserContext Structure:**
```csharp
public class UserContext
{
    public Guid UserId { get; set; }
    public string UserRole { get; set; }  // "Grantee", "Reviewer", "Admin"
    public List<Guid> AccessibleGrantIds { get; set; }
    public Guid CurrentGrantId { get; set; }
    public Guid CurrentDeliverableId { get; set; }
}
```

#### 3. Content Suggestion Service

**Main Method:**
```csharp
public async Task<SuggestionResult> GenerateSuggestionAsync(
    Guid deliverableId,
    string sectionName,
    UserContext userContext)
{
    // 1. Validate access
    // 2. Build context
    // 3. Find examples
    // 4. Generate suggestion
    // 5. Log usage
    // 6. Return result
}
```

**Context Builder:**
- Queries grant information (type, program, focus areas)
- Retrieves previous report content from same grant
- Gets deliverable metadata (reporting period, status)

**Example Finder:**
- Searches for approved examples from similar grants
- Filters by program type, grant type
- Excludes user's own previous reports (to avoid circular suggestions)
- Returns top 3-5 examples

**Prompt Builder:**
- Combines all context into structured prompt
- Includes instructions for tone, length, structure
- Adds constraints (word count, required elements)

**AI Generator:**
- Calls OpenAI GPT-4 API
- Handles errors and retries
- Validates output quality
- Returns formatted suggestion

#### 4. Logging Service

**Tracks:**
- User ID and grant ID
- Section requested
- Tokens used (prompt + completion)
- Cost estimate
- Response time
- User action (accepted/rejected/edited)
- User rating (optional)

---

## Security Design

### Security Requirements

1. **Authentication** - User must be logged in
2. **Authorization** - User must own the deliverable
3. **Data Isolation** - User only sees their grant's context
4. **Example Filtering** - Examples from other grants only (not user's own)
5. **Audit Trail** - All requests logged

### Security Implementation

#### Step 1: Validate User Access

```csharp
public async Task<bool> ValidateDeliverableAccessAsync(
    Guid userId, 
    Guid deliverableId)
{
    var hasAccess = await _repository.ExecuteScalarAsync<bool>(@"
        SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
        FROM GrantDeliverables d
        JOIN GrantsRegistration gr ON d.GrantId = gr.GrantId
        WHERE d.DeliverableId = @DeliverableId
          AND gr.UserId = @UserId
          AND gr.IsActive = 1
    ", new { DeliverableId = deliverableId, UserId = userId });
    
    return hasAccess;
}
```

#### Step 2: Build Grant Context (User's Own Data)

```csharp
public async Task<GrantContext> GetGrantContextAsync(Guid deliverableId)
{
    return await _repository.ExecuteAsync<GrantContext>(@"
        SELECT 
            g.GrantId,
            g.GrantNumber,
            g.GrantType,
            g.ProgramName,
            g.ProgramTypeCode,
            g.FocusAreas,
            d.ReportingPeriod,
            d.DeliverableId
        FROM GrantDeliverables d
        JOIN Grants g ON d.GrantId = g.GrantId
        WHERE d.DeliverableId = @DeliverableId
    ", new { DeliverableId = deliverableId });
}
```

#### Step 3: Get Previous Report (User's Own)

```csharp
public async Task<string> GetPreviousReportContentAsync(
    Guid grantId, 
    string sectionName)
{
    return await _repository.ExecuteScalarAsync<string>(@"
        SELECT TOP 1 r.ResponseText
        FROM DeliverableResponses r
        JOIN GrantDeliverables d ON r.DeliverableId = d.DeliverableId
        WHERE d.GrantId = @GrantId
          AND r.SectionName = @SectionName
          AND d.Status = 'Approved'
        ORDER BY d.ReportingPeriodEnd DESC
    ", new { GrantId = grantId, SectionName = sectionName });
}
```

#### Step 4: Find Examples (Other Grants Only)

```csharp
public async Task<List<ApprovedExample>> FindExamplesAsync(
    int programTypeCode,
    string sectionName,
    Guid excludeGrantId)  // Exclude user's own grant
{
    // Option A: SQL-based (simple)
    return await _repository.ExecuteAsync<ApprovedExample>(@"
        SELECT TOP 3 
            Content,
            GrantType,
            ReviewerRating
        FROM AI_ApprovedContent
        WHERE ProgramTypeCode = @ProgramTypeCode
          AND SectionName = @SectionName
          AND GrantId != @ExcludeGrantId  -- Don't show user's own examples
          AND ReviewerRating >= 4
        ORDER BY ReviewerRating DESC, ApprovalDate DESC
    ", new { 
        ProgramTypeCode = programTypeCode, 
        SectionName = sectionName,
        ExcludeGrantId = excludeGrantId 
    });
    
    // Option B: Vector search (advanced)
    // Use Pinecone to find semantically similar examples
}
```

### Security Checklist

- ✅ User authentication verified
- ✅ Deliverable ownership validated
- ✅ User can only request suggestions for their own deliverables
- ✅ Context built from user's own grant data
- ✅ Examples come from OTHER grants (not user's own)
- ✅ All requests logged with user ID
- ✅ No cross-grant data leakage

---

## Data Architecture

### SQL Server Schema

#### Existing Tables (No Changes)
- `Grants` - Grant master data
- `GrantDeliverables` - Progress reports
- `GrantsRegistration` - User-grant access control
- `AC_User` - User accounts

#### New Tables

**1. DeliverableResponses**
```sql
CREATE TABLE DeliverableResponses (
    ResponseId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DeliverableId UNIQUEIDENTIFIER NOT NULL,
    FormType VARCHAR(50) NOT NULL,  -- 'Performance', 'Accomplishments', 'Challenges'
    QuestionNumber INT NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,  -- 'PerformanceNarrative', etc.
    
    ResponseText NVARCHAR(MAX),  -- The actual narrative content
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Response_Deliverable FOREIGN KEY (DeliverableId) 
        REFERENCES GrantDeliverables(DeliverableId)
);

CREATE INDEX IX_DeliverableResponses_Deliverable 
    ON DeliverableResponses(DeliverableId);
CREATE INDEX IX_DeliverableResponses_Section 
    ON DeliverableResponses(SectionName);
```

**2. AI_UsageLog**
```sql
CREATE TABLE AI_UsageLog (
    LogId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    GrantId UNIQUEIDENTIFIER NOT NULL,
    DeliverableId UNIQUEIDENTIFIER NOT NULL,
    
    FeatureType VARCHAR(50) NOT NULL DEFAULT 'ContentSuggestion',
    SectionName NVARCHAR(100) NOT NULL,
    
    -- AI Provider info
    ProviderName VARCHAR(50) NOT NULL DEFAULT 'OpenAI',
    ModelName VARCHAR(50) NOT NULL,  -- 'gpt-4-turbo'
    
    -- Usage metrics
    PromptTokens INT NULL,
    CompletionTokens INT NULL,
    TotalTokens INT NULL,
    EstimatedCost DECIMAL(10,6) NULL,
    ResponseTimeMs INT NULL,
    
    -- User feedback
    SuggestionAccepted BIT NULL,
    UserAction VARCHAR(50) NULL,  -- 'Accepted', 'Rejected', 'Edited', 'Regenerated'
    UserRating INT NULL,  -- 1-5 scale
    
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_UsageLog_User FOREIGN KEY (UserId) 
        REFERENCES AC_User(UserId)
);

CREATE INDEX IX_AI_UsageLog_User ON AI_UsageLog(UserId);
CREATE INDEX IX_AI_UsageLog_Grant ON AI_UsageLog(GrantId);
CREATE INDEX IX_AI_UsageLog_Date ON AI_UsageLog(CreatedDate);
```

**3. AI_ApprovedContent**
```sql
CREATE TABLE AI_ApprovedContent (
    ContentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrantId UNIQUEIDENTIFIER NOT NULL,
    DeliverableId UNIQUEIDENTIFIER NULL,
    
    ProgramTypeCode INT NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    
    ApprovalDate DATETIME NOT NULL,
    ReviewerRating INT NULL,  -- 1-5 scale (4-5 = high quality)
    
    GrantType NVARCHAR(50) NULL,
    Keywords NVARCHAR(500) NULL,
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_AI_ApprovedContent_Grant FOREIGN KEY (GrantId) 
        REFERENCES Grants(GrantId)
);

CREATE INDEX IX_AI_ApprovedContent_Program 
    ON AI_ApprovedContent(ProgramTypeCode, SectionName);
CREATE INDEX IX_AI_ApprovedContent_Rating 
    ON AI_ApprovedContent(ReviewerRating);
```

### Data Population Strategy

**Initial Seeding:**
1. Identify high-quality approved reports (reviewer rating 4-5)
2. Extract narrative sections
3. Insert into AI_ApprovedContent table
4. Tag with program type, grant type, keywords

**Ongoing:**
- When reports are approved, automatically add to AI_ApprovedContent
- Reviewers can rate quality (1-5 scale)
- Only use 4-5 rated content for suggestions

---

## Vector Database Strategy

### When to Use Vector Database

**Start Without Vector DB (Phase 1):**
- Use SQL queries to find examples by program type, grant type
- Simple, fast to implement
- Good enough for initial launch
- Cost: ~$20-40/month (OpenAI only)

**Add Vector DB Later (Phase 2 - Optional):**
- After 6 months, if you have 1,000+ approved reports
- Enables semantic search (find similar content by meaning)
- Better example matching
- Cost: +$70/month (Pinecone)

### Vector DB Implementation (Optional)

**If using Pinecone:**

**Index Configuration:**
```javascript
{
  name: "bphc-approved-content",
  dimension: 1536,  // text-embedding-3-small
  metric: "cosine"
}
```

**Vector Structure:**
```javascript
{
  id: "ContentId-GUID",
  values: [0.234, -0.567, ..., 0.123],  // 1536 dimensions
  metadata: {
    contentId: "...",
    grantId: "...",
    grantType: "C16",
    programTypeCode: 60,
    sectionName: "PerformanceNarrative",
    reviewerRating: 5,
    approvalDate: "2024-03-15"
  }
}
```

**Search Process:**
1. Convert user's grant context to embedding
2. Search Pinecone for similar approved content
3. Filter by program type, exclude user's grant
4. Return top 3 most similar examples
5. Retrieve full text from SQL Server

---

## Implementation Plan

### Phase 1: Foundation (Weeks 1-2)

**Goal:** Set up core infrastructure

**Tasks:**
1. **Database Setup**
   - Create DeliverableResponses table
   - Create AI_UsageLog table
   - Create AI_ApprovedContent table
   - Add indexes

2. **Security Layer**
   - Implement UserContext service
   - Create access validation logic
   - Test with different user roles

3. **OpenAI Integration**
   - Set up OpenAI API credentials (secure config)
   - Create OpenAI service wrapper
   - Test API connectivity
   - Implement error handling and retries

4. **Data Seeding**
   - Identify 20-30 high-quality approved reports
   - Extract narrative sections
   - Insert into AI_ApprovedContent table

**Deliverables:**
- Database schema deployed
- Security layer functional
- OpenAI integration working
- Initial approved content seeded

**Success Criteria:**
- All database tables created
- Security validations pass
- OpenAI API calls successful
- At least 20 approved examples available

---

### Phase 2: Content Suggestion Feature (Weeks 3-4)

**Goal:** Build and deploy content suggestion feature

**Tasks:**
1. **Backend Services**
   - Implement ContentSuggestionService
   - Build context builder
   - Create example finder (SQL-based)
   - Build prompt templates
   - Implement AI generator
   - Add logging

2. **UI Components**
   - Add "Get AI Suggestion" button to Page1.aspx
   - Create suggestion preview panel (modal or slide-in)
   - Add action buttons (Accept, Reject, Edit, Regenerate)
   - Implement loading indicators
   - Add error message display

3. **API/Handler**
   - Create AJAX handler for suggestion requests
   - Implement security checks
   - Return JSON responses
   - Handle errors gracefully

4. **Testing**
   - Unit tests for services
   - Integration tests
   - Security testing
   - User acceptance testing with 5-10 grantees

**Deliverables:**
- Functional suggestion feature
- UI integrated into Progress Report forms
- API secured and tested
- User feedback collected

**Success Criteria:**
- Suggestions generate in < 5 seconds
- 70%+ acceptance rate in testing
- No security violations
- Positive user feedback (4.0/5.0+)

---

### Phase 3: Optimization & Rollout (Weeks 5-6)

**Goal:** Optimize and deploy to all users

**Tasks:**
1. **Performance Optimization**
   - Implement caching for grant context
   - Optimize SQL queries
   - Add response time monitoring
   - Set timeout limits

2. **User Experience Enhancements**
   - Add user rating system
   - Implement feedback collection
   - Add tooltips and help text
   - Improve error messages

3. **Documentation**
   - User guide (how to use AI suggestions)
   - Admin guide (monitoring, troubleshooting)
   - FAQ document

4. **Training & Rollout**
   - Create training video
   - Conduct user training sessions
   - Gradual rollout (10% → 50% → 100%)
   - Monitor usage and feedback

**Deliverables:**
- Optimized performance
- Complete documentation
- Users trained
- Full production deployment

**Success Criteria:**
- Response time < 3 seconds
- 50%+ of users try feature
- 30%+ regular usage
- High satisfaction scores

---

### Phase 4: Vector DB Enhancement (Weeks 7-8) - Optional

**Goal:** Add semantic search for better suggestions

**Tasks:**
1. **Pinecone Setup**
   - Create Pinecone account
   - Set up index
   - Configure security

2. **Data Migration**
   - Extract all approved content from SQL
   - Generate embeddings for each
   - Upload to Pinecone
   - Verify data integrity

3. **Service Updates**
   - Update example finder to use vector search
   - Implement hybrid approach (vector + SQL filters)
   - Test relevance improvements

4. **Monitoring**
   - Track vector search performance
   - Monitor costs
   - Compare suggestion quality (before/after)

**Deliverables:**
- Pinecone operational
- All approved content vectorized
- Services updated
- Quality metrics improved

**Success Criteria:**
- All data migrated successfully
- Improved suggestion relevance (measured by acceptance rate)
- Response time still < 3 seconds
- Cost within budget ($70/month)

---

## Cost Analysis

### Initial Setup Costs

| Item | Cost | Notes |
|------|------|-------|
| OpenAI API Setup | $0 | Pay-as-you-go |
| Development Time | Internal | 6-8 weeks |
| Testing | Internal | Included |
| **Total Initial** | **$0** | No upfront costs |

### Monthly Operating Costs

#### Scenario 1: 100 Users, 500 Suggestions/Month (Phase 1-2)

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Completions** | 500 suggestions × 1000 tokens | $0.01/1K tokens | $10.00 |
| **SQL Server** | Existing infrastructure | $0 | $0 |
| **Total Monthly** | | | **$10.00** |

#### Scenario 2: 500 Users, 5,000 Suggestions/Month (Phase 3-4)

| Service | Usage | Unit Cost | Monthly Cost |
|---------|-------|-----------|--------------|
| **OpenAI Completions** | 5,000 × 1000 tokens | $0.01/1K tokens | $100.00 |
| **Pinecone** (optional) | 10,000 vectors | $70 | $70.00 |
| **Total Monthly** | | | **$100 - $170** |

### Cost Optimization

1. **Caching** - Cache suggestions for similar contexts (save 20-30%)
2. **Smart Triggering** - Only generate when user clicks button
3. **Model Selection** - Use GPT-3.5-turbo for simpler suggestions (50% cheaper)
4. **Batch Operations** - Process multiple requests efficiently

---

## Risk Management

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **OpenAI API Downtime** | High | Implement retry logic, show graceful error, cache recent suggestions |
| **Slow Response Times** | Medium | Optimize queries, implement caching, set 10-second timeout |
| **Poor Suggestion Quality** | Medium | Iterate on prompts, collect feedback, use better examples |

### Security Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Unauthorized Access** | Critical | Multi-layer validation, audit all requests, fail-closed approach |
| **Data Leakage** | Critical | Exclude user's own examples, validate grant ownership |
| **API Key Exposure** | Critical | Store in secure config, never log keys, rotate regularly |

### Business Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Low User Adoption** | Medium | User training, clear value proposition, gather feedback |
| **Cost Overruns** | Medium | Monitor usage, set budget alerts, implement cost controls |

---

## Success Metrics

### User Adoption

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Feature Awareness** | 80% | Survey after 1 month |
| **Trial Rate** | 50% | Track first-time usage |
| **Regular Usage** | 30% | Weekly active users |
| **Acceptance Rate** | 60% | Track accept/reject |

### Quality

| Metric | Target | Measurement |
|--------|--------|-------------|
| **User Rating** | 4.0/5.0 | In-app ratings |
| **Response Time** | < 5 seconds | Server logs |
| **Error Rate** | < 5% | Error logs |

### Business Impact

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Time Savings** | 40% reduction | Time tracking study |
| **Report Quality** | 20% improvement | Reviewer ratings |
| **User Satisfaction** | 4.0/5.0 | Post-implementation survey |

---

## Appendix: Example Prompt Template

### Content Suggestion Prompt

```
You are an expert grant writer for HRSA Health Center Program reports.

Generate a Performance Narrative for the following grant:

Grant Information:
- Grant Number: {grantNumber}
- Grant Type: {grantType}
- Program: {programName}
- Focus Areas: {focusAreas}
- Reporting Period: {reportingPeriod}

Key Highlights for This Period (provided by the grantee):
{keyPoints}

Previous Report Content (for continuity):
{previousContent}

Examples of High-Quality Approved Reports:

Example 1 (Rating: 5/5):
{example1}

Example 2 (Rating: 5/5):
{example2}

Example 3 (Rating: 4/5):
{example3}

Instructions:
1. Incorporate the grantee's key highlights prominently — these represent the most important achievements this period
2. Maintain consistency with the previous report
3. Address all focus areas mentioned
4. Follow the style and structure of the approved examples
5. Include specific, measurable accomplishments
6. Write 500-1000 words
7. Use professional, clear language
8. Focus on outcomes and impact

Performance Narrative:
```

### Key Points Input — UX Guidelines

- Label: **"Key highlights for this reporting period (optional)"**
- Placeholder: `e.g. 90% improvement in health service delivery, extended to 3 new counties, telehealth adoption up 45%`
- Input type: Multi-line textarea (4 rows)
- Max length: 1000 characters
- Behavior: If left blank, the prompt omits the key points section and falls back to past-report-only context
- Regenerate: Clicking "Regenerate" re-submits with whatever is currently in the key points field

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-04-10 | AI Design Team | Initial content suggestions document |
| 1.1 | 2026-04-15 | Baldev Bhattarai | Add user key points input — user-provided period highlights woven into LLM prompt; add Regenerate UX pattern |

---

**End of Document**
