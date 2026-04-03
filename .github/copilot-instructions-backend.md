# Backend Copilot Instructions

You are an expert .NET 10 Architect. Generate code that strictly adheres to Clean Architecture principles and the specific patterns defined below.

## Tech Stack
- Framework: .NET 10 (C# 14)
- Architecture: Clean Architecture
- API Pattern: Classic Controllers (`ControllerBase`)
- Database & ORM: PostgreSQL + Entity Framework Core
- Key Patterns: CQRS (MediatR), FluentValidation, Manual Projections

## Hard Rules & Constraints

### 1. Clean Architecture Strictness
- Domain Layer: Contains purely business logic, entities, and domain events. NO external dependencies (no EF Core, no database logic).
- Application Layer: Contains MediatR Commands/Queries, Handlers, and FluentValidation rules. References Domain, but not Infrastructure.
- Infrastructure Layer: Contains EF Core DbContext, PostgreSQL configurations, and external service integrations.
- Presentation Layer (API): Contains Classic Controllers. Controllers must be thin and ONLY map HTTP requests to MediatR calls and return appropriate HTTP status codes.

### 2. API & Controllers
- Use Classic Controllers. Do NOT generate Minimal APIs (`app.MapGet`, etc.).
- Controllers must not contain business logic. Inject `ISender` (MediatR) and dispatch requests immediately.

### 3. Data Access & ORM
- Use Entity Framework Core configured for PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- Do not use generic repository patterns. The `DbContext` and `DbSet` are already repositories/unit of work.
- Use compiled queries or `AsNoTracking()` for read-only operations.

### 4. CQRS & MediatR
- Strictly separate Commands (state-changing) and Queries (data-fetching).
- Each Handler must have its own dedicated Request and Response records.

### 5. Validation & Mapping
- ALWAYS use FluentValidation in the Application Layer. Do not use Data Annotations on domain entities or DTOs.
- DO NOT use AutoMapper or Mapster.
- Use Manual Projections (e.g., `.Select(x => new Dto { ... })` in EF Core queries) to avoid fetching unused columns from the database.