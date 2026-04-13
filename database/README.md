# Grant Management AI - Database Setup

## Overview
This directory contains SQL scripts to set up the GrantDB database with sample data for demonstrating AI features.

## Database Structure

### Tables
1. **Users** - Grantee users (5 sample users)
2. **Grants** - Grant records (5 grants: C16, C17, C18, H80)
3. **Reports** - Progress reports (20 approved 2024 reports + 5 draft 2025 reports)
4. **ReportSections** - Individual report sections with responses
5. **AI_UsageLog** - Tracks AI feature usage
6. **AI_ApprovedContent** - Stores approved content for AI examples
7. **ChatConversations** - Stores chatbot conversation history

### Sample Data
- **2024 Data:** 5 grants with complete Q1-Q4 reports (all approved, high ratings)
- **2025 Data:** 5 draft Q1 reports with empty narrative fields (for AI suggestions)
- **Approved Content:** Extracted from 2024 reports for AI to use as examples

## Setup Instructions

### Option 1: Run Master Script (Recommended)

1. Open SQL Server Management Studio (SSMS)
2. Connect to your localhost SQL Server instance
3. Open `00_MasterSetup.sql`
4. Click Execute (F5)

This will:
- Create GrantDB database
- Create all tables and indexes
- Populate sample data
- Display summary statistics

### Option 2: Run Scripts Individually

Execute scripts in this order:

**Schema:**
1. `schema/01_CreateDatabase.sql`
2. `schema/02_CreateTables.sql`
3. `schema/03_CreateIndexes.sql`

**Sample Data:**
4. `sample-data/01_SeedUsers.sql`
5. `sample-data/02_SeedGrants.sql`
6. `sample-data/03_SeedReports_2024.sql`
7. `sample-data/04_SeedReportSections_2024_Q1.sql`
8. `sample-data/05_SeedReportSections_2024_Remaining.sql`
9. `sample-data/06_SeedReports_2025_Draft.sql`
10. `sample-data/07_SeedApprovedContent.sql`

### Option 3: Command Line (SQLCMD)

```bash
cd database
sqlcmd -S localhost -E -i 00_MasterSetup.sql
```

## Verification

After setup, verify the database:

```sql
USE GrantDB;

-- Check record counts
SELECT 'Users' as TableName, COUNT(*) as Count FROM Users
UNION ALL
SELECT 'Grants', COUNT(*) FROM Grants
UNION ALL
SELECT 'Reports', COUNT(*) FROM Reports
UNION ALL
SELECT 'ReportSections', COUNT(*) FROM ReportSections
UNION ALL
SELECT 'AI_ApprovedContent', COUNT(*) FROM AI_ApprovedContent;

-- Expected results:
-- Users: 5
-- Grants: 5
-- Reports: 25 (20 approved 2024 + 5 draft 2025)
-- ReportSections: 175 (7 sections × 25 reports)
-- AI_ApprovedContent: 20 (4 text sections × 5 grants)
```

## Sample Users

| Email | Name | Organization | Grant Type |
|-------|------|--------------|------------|
| john.smith@healthcenter1.org | John Smith | Community Health Center of Springfield | C16 |
| sarah.johnson@communityclinic.org | Sarah Johnson | Migrant Community Clinic | C17 |
| michael.brown@ruralhealth.org | Michael Brown | Rural Health Services | C16 |
| emily.davis@urbancare.org | Emily Davis | Urban Care Center | H80 |
| david.wilson@coastalhealth.org | David Wilson | Coastal Health Network | C18 |

## Connection String

For .NET application:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=GrantDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## Reset Database

To start fresh:

```sql
USE master;
ALTER DATABASE GrantDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE GrantDB;
```

Then re-run `00_MasterSetup.sql`

## Next Steps

After database setup is complete:
1. Create .NET Web API project
2. Configure Entity Framework Core
3. Implement AI services
4. Create Angular frontend

## Notes

- All 2024 reports are approved with ratings 4-5 (high quality)
- 2025 Q1 reports have empty narrative fields for AI to suggest content
- Approved content is automatically extracted for AI examples
- Database uses UNIQUEIDENTIFIER (GUID) for primary keys
- All dates use DATETIME type
