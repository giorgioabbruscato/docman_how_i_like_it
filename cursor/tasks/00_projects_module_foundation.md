# TASK 00 — PROJECTS MODULE FOUNDATION

> Status: **COMPLETED**

Create the `HrPortal.Projects` module following the modular monolith Clean Architecture pattern.

## Goal

Scaffold the Projects module with domain entities, EF Core configuration, migrations, DI registration, lightweight CQRS structure, permission catalog entries, and unit tests. Full CRUD endpoints are implemented in Task 01.

## Depends on

- Hybrid architecture foundation (ADR-012) — `TenantContext`, `ApplyTenantScope`, policy engine
- `HrPortal.Employees` — reference module structure

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture, multi-tenancy |
| Guardrails | `cursor/core/02_guardrails.md` | ApplyTenantScope, no auth in services |
| TDD | `cursor/core/01_tdd.md` | Domain + validator tests first |
| Architecture | `cursor/core/03_architecture.md` | Layer boundaries |
| Patterns | `cursor/core/04_patterns.md` | Copy `HrPortal.Employees` structure |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Agent workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Module scaffold |
| Domain model | `cursor/memory/domain_model.md` | Add Project + ProjectMember |
| API contracts | `cursor/memory/api_contracts.md` | Stub Projects section |
| Module deps | `cursor/memory/module_dependencies.md` | Add Projects module |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Hybrid tenancy |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Per-module checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Clean Architecture: Api → Application → Domain → Infrastructure
- Every business entity implements `ITenantEntity` via `AuditableEntity` base
- Repository: every query uses `ApplyTenantScope(ctx)`
- Controller: `[RequirePermission]` only — register `IResourceLoader` for get/update/delete
- Service / command handlers: pure orchestration — `TenantContext` only, no HTTP/JWT access
- Return `Result<T>` for all operations
- FluentValidation on all request DTOs
- Cross-module reads via lookup interfaces only — never access another module's DbSet
- Migrations centralized in `HrPortal.Api/Infrastructure/Persistence/Migrations/`
- Lightweight CQRS: `Application/Commands/` + `Application/Queries/` — no MediatR

### Memory — source of truth (`cursor/memory/`)

- Add `Project` and `ProjectMember` entities to `domain_model.md`
- Stub Projects endpoints in `api_contracts.md`
- Add `HrPortal.Projects` to `module_dependencies.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — full per-module checklist
- Hybrid architecture checklist (ApplyTenantScope, policy engine, TenantContext)

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for backend tasks
3. Follow `/cursor/prompts/00_master_prompt.md` workflow
4. Study `HrPortal.Employees` as reference implementation

### Before completing

1. Run `dotnet build && dotnet test` in `src/backend`
2. Verify against `01_backend_quality_checks.md`
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.Projects` project under `src/backend/src/Modules/`
- [ ] Layers: Domain, Application, Infrastructure
- [ ] `{Module}ServiceCollectionExtensions.cs` with DI registration
- [ ] Register module in `HrPortal.Api/Program.cs` after `AddEmployeesModule()`
- [ ] Add project reference in `HrPortal.Api.csproj` and solution

### Domain entities

- [ ] `Project` — sealed entity with factory methods
- [ ] `ProjectMember` — sealed entity with factory methods
- [ ] Enums: `ProjectStatus`, `ProjectMemberRole`

**Project fields** (plus `AuditableEntity` base: Id, TenantId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy):

| Field | Type | Notes |
|-------|------|-------|
| Name | string | Required, max 200 |
| Description | string? | Optional |
| CustomerName | string? | Optional |
| Status | ProjectStatus | Active, OnHold, Completed, Cancelled |
| StartDate | DateOnly? | Optional |
| EndDate | DateOnly? | Must be >= StartDate when both set |
| BudgetHours | decimal? | >= 0 |
| BudgetCost | decimal? | >= 0 |
| IsArchived | bool | Soft archive flag |

**ProjectMember fields**:

| Field | Type | Notes |
|-------|------|-------|
| ProjectId | Guid | FK to Project |
| EmployeeId | Guid | FK validated via `IEmployeeLookup` |
| Role | ProjectMemberRole | Lead, Member, Observer |
| HourlyRate | decimal? | >= 0 |

### Application layer (lightweight CQRS)

- [ ] `Application/Commands/` — CreateProject, UpdateProject, DeleteProject command + handler pairs
- [ ] `Application/Queries/` — GetProjectById, GetProjects query + handler pairs
- [ ] DTOs: request/response records separate from domain entities
- [ ] `IProjectRepository`, `IProjectMemberRepository` interfaces
- [ ] FluentValidation validators for all request DTOs
- [ ] `IProjectLookup` interface for cross-module consumption

### Infrastructure

- [ ] EF configurations in schema `projects`
- [ ] `{Entity}Configuration.cs` with indexes and constraints
- [ ] Repository implementations with `ApplyTenantScope` on every query
- [ ] Unique index on `ProjectMember (ProjectId, EmployeeId)`

### Permissions

Add to `Permissions.cs` and `SystemRoleTemplates`:

| Constant | Value |
|----------|-------|
| `ProjectReadTenant` | `project.read:tenant` |
| `ProjectCreateTenant` | `project.create:tenant` |
| `ProjectUpdateTenant` | `project.update:tenant` |
| `ProjectDeleteTenant` | `project.delete:tenant` |
| `ProjectManageMembersTenant` | `project.manage_members:tenant` |

- [ ] `ProjectResourceLoader` registered in AccessControl

### Migrations

- [ ] EF migration adding `projects` schema tables
- [ ] Migration applies cleanly on fresh database

### Tests

- [ ] Domain unit tests: factory methods, validation rules, archive behavior
- [ ] Validator unit tests for all request DTOs
- [ ] Service/command handler unit tests with mocked repositories + TenantContext fixture

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Projects/**` | Create |
| `src/backend/src/HrPortal.Api/Program.cs` | Register module |
| `src/backend/src/HrPortal.Api/HrPortal.Api.csproj` | Project reference |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/SystemRoleTemplates.cs` | Add permissions to roles |
| `src/backend/src/Platform/HrPortal.AccessControl/Infrastructure/ResourceLoaders/ProjectResourceLoader.cs` | Create |
| `src/backend/tests/HrPortal.UnitTests/Projects/*` | Create |
| `cursor/memory/domain_model.md` | Add entities |
| `cursor/memory/module_dependencies.md` | Add Projects |

## Acceptance criteria

- [ ] Module builds and registers without errors
- [ ] Migration applies cleanly: `dotnet ef database update --project src/backend/src/HrPortal.Api`
- [ ] Domain + validator unit tests pass
- [ ] `dotnet test` green for Projects tests
- [ ] No cross-module DbSet access

## Next task

→ `01_project_crud.md` — Project CRUD APIs with pagination and filtering
