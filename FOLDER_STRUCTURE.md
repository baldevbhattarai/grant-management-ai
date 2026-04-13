# Grant Management AI - Folder Structure

## Industry-Standard Project Organization

```
GrantManagementAI/
│
├── README.md                       # Project overview and quick start
├── .gitignore                      # Git ignore rules
├── FOLDER_STRUCTURE.md            # This file
│
├── docs/                           # Documentation
│   ├── AI_Content_Suggestions_Feature.md
│   ├── AI_QA_Chatbot_Feature.md
│   ├── AI_Sample_Application_Implementation.md
│   ├── API_Documentation.md       # (To be created)
│   ├── Architecture.md            # (To be created)
│   └── User_Guide.md              # (To be created)
│
├── database/                       # Database scripts
│   ├── README.md                  # Database setup instructions
│   ├── 00_MasterSetup.sql         # Master setup script
│   ├── schema/                    # Table definitions
│   │   ├── 01_CreateDatabase.sql
│   │   ├── 02_CreateTables.sql
│   │   └── 03_CreateIndexes.sql
│   └── sample-data/               # Sample data scripts
│       ├── 01_SeedUsers.sql
│       ├── 02_SeedGrants.sql
│       ├── 03_SeedReports_2024.sql
│       ├── 04_SeedReportSections_2024_Q1.sql
│       ├── 05_SeedReportSections_2024_Remaining.sql
│       ├── 06_SeedReports_2025_Draft.sql
│       └── 07_SeedApprovedContent.sql
│
├── src/                            # Source code
│   │
│   ├── backend/                    # .NET 8 Web API
│   │   ├── GrantManagement.sln    # Solution file
│   │   │
│   │   ├── GrantManagement.API/   # Web API project
│   │   │   ├── Controllers/
│   │   │   │   ├── GrantsController.cs
│   │   │   │   ├── ReportsController.cs
│   │   │   │   ├── AISuggestionController.cs
│   │   │   │   └── ChatbotController.cs
│   │   │   ├── Middleware/
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── appsettings.Development.json
│   │   │
│   │   ├── GrantManagement.Core/  # Domain layer
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Grant.cs
│   │   │   │   ├── Report.cs
│   │   │   │   ├── ReportSection.cs
│   │   │   │   ├── AIUsageLog.cs
│   │   │   │   ├── AIApprovedContent.cs
│   │   │   │   └── ChatConversation.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── GrantDto.cs
│   │   │   │   ├── ReportDto.cs
│   │   │   │   ├── SuggestionRequestDto.cs
│   │   │   │   ├── SuggestionResponseDto.cs
│   │   │   │   ├── ChatRequestDto.cs
│   │   │   │   └── ChatResponseDto.cs
│   │   │   └── Interfaces/
│   │   │       ├── IGrantRepository.cs
│   │   │       ├── IReportRepository.cs
│   │   │       ├── IContentSuggestionService.cs
│   │   │       └── IChatbotService.cs
│   │   │
│   │   ├── GrantManagement.Infrastructure/  # Data access
│   │   │   ├── Data/
│   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   └── DbInitializer.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── GrantRepository.cs
│   │   │   │   ├── ReportRepository.cs
│   │   │   │   └── AIRepository.cs
│   │   │   └── Migrations/
│   │   │
│   │   └── GrantManagement.Services/  # Business logic
│   │       ├── AI/
│   │       │   ├── OpenAIService.cs
│   │       │   ├── ContentSuggestionService.cs
│   │       │   ├── QuestionClassifierService.cs
│   │       │   ├── VectorSearchService.cs
│   │       │   └── ChatbotService.cs
│   │       ├── Grant/
│   │       │   └── GrantService.cs
│   │       └── Report/
│   │           └── ReportService.cs
│   │
│   └── frontend/                   # Angular 17 application
│       └── grant-management-ui/
│           ├── angular.json
│           ├── package.json
│           ├── tsconfig.json
│           └── src/
│               ├── app/
│               │   ├── core/
│               │   │   ├── services/
│               │   │   │   ├── grant.service.ts
│               │   │   │   ├── report.service.ts
│               │   │   │   ├── ai.service.ts
│               │   │   │   └── chat.service.ts
│               │   │   ├── models/
│               │   │   │   ├── grant.model.ts
│               │   │   │   ├── report.model.ts
│               │   │   │   └── chat.model.ts
│               │   │   └── interceptors/
│               │   │
│               │   ├── features/
│               │   │   ├── dashboard/
│               │   │   │   ├── dashboard.component.ts
│               │   │   │   ├── dashboard.component.html
│               │   │   │   └── dashboard.component.css
│               │   │   ├── grants/
│               │   │   │   ├── grant-list/
│               │   │   │   └── grant-detail/
│               │   │   ├── reports/
│               │   │   │   ├── report-list/
│               │   │   │   ├── report-form/
│               │   │   │   └── report-detail/
│               │   │   └── ai/
│               │   │       ├── ai-suggestion/
│               │   │       │   ├── ai-suggestion.component.ts
│               │   │       │   ├── ai-suggestion.component.html
│               │   │       │   └── ai-suggestion.component.css
│               │   │       └── chat-widget/
│               │   │           ├── chat-widget.component.ts
│               │   │           ├── chat-widget.component.html
│               │   │           └── chat-widget.component.css
│               │   │
│               │   ├── shared/
│               │   │   ├── components/
│               │   │   ├── directives/
│               │   │   └── pipes/
│               │   │
│               │   ├── app.component.ts
│               │   ├── app.component.html
│               │   ├── app.routes.ts
│               │   └── app.config.ts
│               │
│               ├── assets/
│               ├── environments/
│               │   ├── environment.ts
│               │   └── environment.development.ts
│               ├── index.html
│               ├── main.ts
│               └── styles.css
│
├── tests/                          # Test projects
│   ├── GrantManagement.Tests/     # Unit tests
│   │   ├── Services/
│   │   ├── Controllers/
│   │   └── Repositories/
│   └── GrantManagement.IntegrationTests/  # Integration tests
│
└── scripts/                        # Utility scripts
    ├── setup-dev-environment.ps1
    ├── run-backend.ps1
    ├── run-frontend.ps1
    └── deploy.ps1
```

## Folder Descriptions

### Root Level
- **README.md**: Project overview, quick start guide, and basic documentation
- **.gitignore**: Files and folders to exclude from version control
- **FOLDER_STRUCTURE.md**: This file - complete project organization

### docs/
Contains all project documentation:
- Feature specifications
- API documentation
- Architecture diagrams
- User guides
- Implementation plans

### database/
All database-related scripts:
- **schema/**: Table definitions, indexes, constraints
- **sample-data/**: Sample data for development and testing
- **00_MasterSetup.sql**: One-click database setup

### src/backend/
.NET 8 Web API following Clean Architecture:

- **GrantManagement.API**: Web API layer (controllers, middleware)
- **GrantManagement.Core**: Domain layer (entities, DTOs, interfaces)
- **GrantManagement.Infrastructure**: Data access layer (EF Core, repositories)
- **GrantManagement.Services**: Business logic layer (AI services, domain services)

### src/frontend/
Angular 17 application following Angular best practices:

- **core/**: Singleton services, models, interceptors
- **features/**: Feature modules (dashboard, grants, reports, AI)
- **shared/**: Reusable components, directives, pipes
- **environments/**: Environment-specific configuration

### tests/
Test projects:
- **Unit tests**: Test individual components/services
- **Integration tests**: Test API endpoints and database operations

### scripts/
Utility scripts for development and deployment

## Design Principles

### Backend (.NET)
- **Clean Architecture**: Separation of concerns with clear dependencies
- **SOLID Principles**: Single responsibility, open/closed, etc.
- **Repository Pattern**: Abstract data access
- **Dependency Injection**: Loose coupling
- **Async/Await**: Non-blocking operations

### Frontend (Angular)
- **Component-Based**: Reusable, modular components
- **Reactive Programming**: RxJS observables
- **Lazy Loading**: Load features on demand
- **Smart/Dumb Components**: Container and presentation components
- **Service Layer**: Centralized business logic

### Database
- **Normalized Schema**: Reduce redundancy
- **Indexed Queries**: Optimize performance
- **Foreign Keys**: Maintain referential integrity
- **Sample Data**: Realistic test data

## Naming Conventions

### C# (.NET)
- **PascalCase**: Classes, methods, properties
- **camelCase**: Local variables, parameters
- **Interfaces**: Prefix with `I` (e.g., `IGrantRepository`)
- **Async methods**: Suffix with `Async` (e.g., `GetGrantAsync`)

### TypeScript (Angular)
- **PascalCase**: Classes, interfaces, types
- **camelCase**: Variables, functions, properties
- **kebab-case**: File names (e.g., `grant-list.component.ts`)
- **Services**: Suffix with `Service` (e.g., `GrantService`)

### SQL
- **PascalCase**: Tables, columns
- **Prefix**: AI tables with `AI_` (e.g., `AI_UsageLog`)
- **Foreign Keys**: Prefix with `FK_` (e.g., `FK_Grants_Users`)
- **Indexes**: Prefix with `IX_` (e.g., `IX_Grants_UserId`)

## Next Steps

1. ✅ Database setup (Complete)
2. ⏳ Create .NET solution structure
3. ⏳ Implement Entity Framework Core
4. ⏳ Create API controllers
5. ⏳ Implement AI services
6. ⏳ Create Angular application
7. ⏳ Implement UI components
8. ⏳ Integration testing
9. ⏳ Documentation

## Notes

- All code follows industry best practices
- Project structure supports scalability
- Clear separation of concerns
- Easy to test and maintain
- Ready for CI/CD integration
