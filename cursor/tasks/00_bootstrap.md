# TASK 00 — BOOTSTRAP

> Status: **COMPLETED**

Foundation setup for the HR Portal modular monolith.

## TASK 00.1 — Solution structure

**Goal:** Create backend solution with Clean Architecture layout.

**Deliverables:**
- [x] `src/backend/HrPortal.sln`
- [x] `src/backend/Directory.Build.props` (shared MSBuild properties)
- [x] `src/backend/src/HrPortal.Api/` — API host project
- [x] `src/backend/src/Platform/` — platform services
- [x] `src/backend/src/Modules/` — domain modules directory
- [x] `src/backend/tests/HrPortal.UnitTests/`
- [x] `src/backend/tests/HrPortal.IntegrationTests/`

## TASK 00.2 — SharedKernel

**Goal:** Base types used across all modules.

**Deliverables:**
- [x] `AuditableEntity` base class with tenant support
- [x] `ITenantEntity` interface
- [x] `Result<T>` pattern for service responses
- [x] `IUnitOfWork` abstraction
- [x] Domain exceptions (`NotFoundException`, etc.)

## TASK 00.3 — Platform services

**Goal:** Cross-cutting infrastructure with no domain dependencies.

**Deliverables:**
- [x] `HrPortal.Tenancy` — tenant resolution, context, middleware
- [x] `HrPortal.Identity` — JWT validation
- [x] `HrPortal.Authorization` — policy definitions
- [x] `HrPortal.Storage` — `IStorageProvider` (filesystem)
- [x] `HrPortal.Audit` — audit logging abstraction
- [x] `HrPortal.Notifications` — notification abstraction
- [x] `HrPortal.Configuration` — typed options

## TASK 00.4 — API host

**Goal:** ASP.NET Core host with middleware pipeline.

**Deliverables:**
- [x] `Program.cs` with DI composition
- [x] `HrPortalDbContext` with global tenant filters
- [x] `GlobalExceptionMiddleware` → ProblemDetails
- [x] `ValidationFilter` for FluentValidation
- [x] `DbInitializer` for seed data
- [x] Health checks: `/health`, `/ready`
- [x] Swagger in Development
- [x] Serilog structured logging

## TASK 00.5 — Frontend scaffold

**Goal:** React + TypeScript + Vite foundation.

**Deliverables:**
- [x] `src/frontend/` with Vite + React 18 + TypeScript
- [x] Tailwind CSS setup
- [x] Zustand auth store
- [x] Axios API client with interceptors
- [x] App layout component
- [x] Basic routing structure

## TASK 00.6 — Documentation

**Goal:** Project documentation baseline.

**Deliverables:**
- [x] `README.md` with quick start
- [x] `docs/ARCHITECTURE.md`
- [x] `.env.example`

## Verification

```bash
cd src/backend && dotnet build
cd src/frontend && npm install && npm run build
```

## Next task

→ `01_infra.md` — Docker infrastructure setup
