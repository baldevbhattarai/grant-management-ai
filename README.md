# Grant Management AI

AI-powered features for the BPHC grant management system — **Smart Content Suggestions** and a **Q&A Chatbot** built on .NET 9, Angular 19, SQL Server, and OpenAI GPT-4.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Structure](#project-structure)
3. [Quick Start](#quick-start)
4. [Step-by-Step Setup](#step-by-step-setup)
   - [1. Database](#1-database)
   - [2. Backend API](#2-backend-api)
   - [3. Frontend](#3-frontend)
5. [Running the Application](#running-the-application)
6. [Using the AI Features](#using-the-ai-features)
7. [Running Tests](#running-tests)
8. [Configuration Reference](#configuration-reference)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 9.x | [download](https://dotnet.microsoft.com/download) |
| Node.js | 18.x or 20.x | [download](https://nodejs.org) |
| Angular CLI | 19.x | `npm install -g @angular/cli` |
| SQL Server | 2019+ | Local instance, Windows Auth |
| sqlcmd | any | Included with SQL Server tools |

---

## Project Structure

```
GrantManagementAI/
├── docs/                          # Feature and design documents
├── database/
│   ├── schema/                    # Table and index scripts
│   └── sample-data/               # Seed data (5 users, 5 grants, 25 reports)
├── src/
│   ├── backend/                   # .NET 9 solution
│   │   ├── GrantManagement.sln
│   │   ├── GrantManagement.API/           # Controllers, Program.cs
│   │   ├── GrantManagement.Core/          # Entities, DTOs, interfaces
│   │   ├── GrantManagement.Infrastructure/# EF Core, repositories
│   │   ├── GrantManagement.Services/      # AI services, business logic
│   │   └── GrantManagement.Tests/         # xUnit unit tests
│   └── frontend/
│       └── grant-management-ui/   # Angular 19 application
└── scripts/                       # Dev environment scripts
```

---

## Quick Start

If you have all prerequisites and a local SQL Server already running:

```bash
# 1. Database
sqlcmd -S localhost -E -i database\schema\01_CreateDatabase.sql
sqlcmd -S localhost -E -i database\schema\02_CreateTables.sql
sqlcmd -S localhost -E -i database\schema\03_CreateIndexes.sql
sqlcmd -S localhost -E -i database\sample-data\01_SeedUsers.sql
sqlcmd -S localhost -E -i database\sample-data\02_SeedGrants.sql
sqlcmd -S localhost -E -i database\sample-data\03_SeedReports_2024.sql
sqlcmd -S localhost -E -i database\sample-data\04_SeedReportSections_2024_Q1.sql
sqlcmd -S localhost -E -i database\sample-data\05_SeedReportSections_2024_Remaining.sql
sqlcmd -S localhost -E -i database\sample-data\06_SeedReports_2025_Draft.sql
sqlcmd -S localhost -E -i database\sample-data\07_SeedApprovedContent.sql

# 2. Add your OpenAI key (edit the file, replace REPLACE_WITH_YOUR_KEY)
notepad src\backend\GrantManagement.API\appsettings.Development.json

# 3. Run API (Terminal 1)
cd src\backend
dotnet run --project GrantManagement.API

# 4. Run Angular (Terminal 2)
cd src\frontend\grant-management-ui
npm install
ng serve
```

Open **http://localhost:4200**

---

## Step-by-Step Setup

### 1. Database

The application uses `GrantDB` on your local SQL Server with Windows Authentication.

**Create schema:**
```bash
sqlcmd -S localhost -E -i database\schema\01_CreateDatabase.sql
sqlcmd -S localhost -E -i database\schema\02_CreateTables.sql
sqlcmd -S localhost -E -i database\schema\03_CreateIndexes.sql
```

**Seed sample data** (5 grantee users, 5 grants, 20 approved 2024 reports + 5 draft 2025 reports):
```bash
sqlcmd -S localhost -E -i database\sample-data\01_SeedUsers.sql
sqlcmd -S localhost -E -i database\sample-data\02_SeedGrants.sql
sqlcmd -S localhost -E -i database\sample-data\03_SeedReports_2024.sql
sqlcmd -S localhost -E -i database\sample-data\04_SeedReportSections_2024_Q1.sql
sqlcmd -S localhost -E -i database\sample-data\05_SeedReportSections_2024_Remaining.sql
sqlcmd -S localhost -E -i database\sample-data\06_SeedReports_2025_Draft.sql
sqlcmd -S localhost -E -i database\sample-data\07_SeedApprovedContent.sql
```

**Verify:**
```bash
sqlcmd -S localhost -E -d GrantDB -Q "SELECT COUNT(*) FROM Users; SELECT COUNT(*) FROM Reports; SELECT COUNT(*) FROM ReportSections;"
```
Expected: 5 users · 25 reports · 35 sections.

> **Named instance?** Change `localhost` to `localhost\SQLEXPRESS` (or your instance name) in both the sqlcmd commands and `appsettings.Development.json`.

---

### 2. Backend API

**Configure OpenAI key:**

Open `src\backend\GrantManagement.API\appsettings.Development.json` and set your key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GrantDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4-turbo"
  }
}
```

> The API still starts without a key — suggestion and chatbot endpoints will return an error message instead of calling OpenAI.

**Build and run:**
```bash
cd src\backend
dotnet restore
dotnet build
dotnet run --project GrantManagement.API
```

The API starts on:
- **https://localhost:7143** (HTTPS)
- **http://localhost:5266** (HTTP)

**Swagger UI:** https://localhost:7143/openapi  
**Health check:** https://localhost:7143/health

---

### 3. Frontend

```bash
cd src\frontend\grant-management-ui
npm install
ng serve
```

Opens on **http://localhost:4200**. The dev server proxies nothing — Angular calls the API directly at `https://localhost:7143/api`.

> If you get a browser SSL warning on the first API call, visit `https://localhost:7143/health` directly in your browser and accept the dev certificate, then reload the Angular app.

**Trust the dev certificate (one-time):**
```bash
dotnet dev-certs https --trust
```

---

## Running the Application

Start both servers in separate terminals:

**Terminal 1 — API:**
```bash
cd src\backend
dotnet run --project GrantManagement.API
```

**Terminal 2 — Angular:**
```bash
cd src\frontend\grant-management-ui
ng serve
```

Navigate to **http://localhost:4200**.

### Demo walkthrough

1. **Select a demo user** from the chip selector on the Dashboard (John Smith, Sarah Johnson, etc.)
2. **Click "View Reports"** on any grant card
3. The report list shows 4 Approved 2024 reports and 1 Draft 2025 Q1 report
4. **Click "Fill Report"** on the 2025 Draft
5. On any narrative text field, click **"✨ Get AI Suggestion"**
6. The AI generates a suggestion based on 2024 approved content — Accept, Edit, or Regenerate
7. Click **Save** per section to persist your response
8. Use the **chat bubble** (bottom-right) to ask questions like:
   - *"What did I write about telehealth last quarter?"*
   - *"What challenges did I face in Q1 2024?"*

---

## Using the AI Features

### Content Suggestions

- Available on all **Text** sections of a **Draft** report
- Click **"✨ Get AI Suggestion"** — the panel appears in ~3–5 seconds
- The suggestion is generated using:
  - Your grant's type, program, and focus areas
  - Your most recent **approved** report content for the same section
  - Up to 3 high-rated (≥4/5) approved examples from similar grants
- **Accept** → text populates the field; **Regenerate** → new suggestion; **Dismiss** → close panel
- Token count and estimated cost are shown in the panel header

### Q&A Chatbot

- Click the **robot icon** (bottom-right corner) to open the chat panel
- Select which grant you want to query using the dropdown
- Type a question in natural language — press Enter or click Send
- Answers include **source citations** showing which report period and section the information came from
- Sample questions are shown when the chat is empty

---

## Running Tests

**Backend unit tests (9 tests):**
```bash
cd src\backend
dotnet test GrantManagement.Tests/GrantManagement.Tests.csproj --verbosity normal
```

Expected output:
```
Total tests: 9
     Passed: 9
```

**Full solution build:**
```bash
cd src\backend
dotnet build GrantManagement.sln
```

**Angular build check:**
```bash
cd src\frontend\grant-management-ui
ng build --configuration=development
```

---

## Configuration Reference

### `appsettings.Development.json` (backend)

| Key | Default | Description |
|-----|---------|-------------|
| `ConnectionStrings:DefaultConnection` | `Server=localhost;Database=GrantDB;...` | SQL Server connection string |
| `OpenAI:ApiKey` | *(empty)* | Your OpenAI API key — required for AI features |
| `OpenAI:Model` | `gpt-4-turbo` | OpenAI model to use (`gpt-4o`, `gpt-3.5-turbo`, etc.) |

### `environment.ts` (frontend)

| Key | Value | Description |
|-----|-------|-------------|
| `apiUrl` | `https://localhost:7143/api` | Base URL for all API calls |

---

## Troubleshooting

**"Cannot reach API" on Dashboard**
- Ensure `dotnet run --project GrantManagement.API` is running
- Visit `https://localhost:7143/health` — if you get a certificate warning, accept it and reload the app

**AI suggestion returns "OpenAI API key not configured"**
- Add your key to `appsettings.Development.json` and restart the API

**SQL Server connection fails**
- Verify SQL Server is running: `sqlcmd -S localhost -E -Q "SELECT 1"`
- Named instance: change `Server=localhost` to `Server=localhost\SQLEXPRESS` in the connection string

**Angular Material version error during `npm install`**
- Run: `npm install @angular/material@19 @angular/cdk@19 @angular/animations@19 --legacy-peer-deps`

**`ng serve` port conflict**
- Run on a different port: `ng serve --port 4201`
- Update `src\backend\GrantManagement.API\Program.cs` CORS origin to match

---

## Sample Users (Seed Data)

| Name | Email | Grant Type |
|------|-------|-----------|
| John Smith | john.smith@healthcenter1.org | C16 — Community Health |
| Sarah Johnson | sarah.johnson@communityclinic.org | C17 — Migrant Health |
| Michael Brown | michael.brown@ruralhealth.org | C16 — Community Health |
| Emily Davis | emily.davis@urbancare.org | H80 — School-Based Health |
| David Wilson | david.wilson@coastalhealth.org | C18 — Health Care for Homeless |

---

