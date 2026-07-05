# TASK 01 — PROJECT CRUD

> Status: **PENDING**

Implement full CRUD APIs for Projects with pagination, filtering, search, tenant isolation, audit logging, and FluentValidation.

## Goal

Expose OpenAPI-documented endpoints for project management. Controllers are thin; business logic lives in command/query handlers.

## Depends on

- Task 00 — Projects module foundation

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Thin controllers |
| Guardrails | `cursor/core/02_guardrails.md` | Audit via IAuditService |
| TDD | `cursor/core/01_tdd.md` | Integration tests required |
| Patterns | `cursor/core/04_patterns.md` | Controller pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Implementation scope |
| Domain model | `cursor/memory/domain_model.md` | Project entity |
| API contracts | `cursor/memory/api_contracts.md` | Document all endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Per-module checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Controllers delegate to command/query handlers — zero business logic
- `[RequirePermission]` on every action — no inline authorization
- `IAuditService.LogAsync` on create, update, delete
- Return `Result<T>` from handlers; map to HTTP in controller
- FluentValidation runs before handler execution
- Pagination via `PagedResult<T>` pattern (match Employees)
- All list queries use `ApplyTenantScope`
- Errors return RFC 7807 `ProblemDetails`

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with full Projects CRUD section

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — controller, integration tests, hybrid checklist

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 00 is **COMPLETED**
3. Follow `/cursor/prompts/00_master_prompt.md` workflow

### Before completing

1. Run `dotnet test` — all Projects integration tests pass
2. Verify Swagger documents all endpoints
3. Update `api_contracts.md`
4. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/projects` | `project.read:tenant` | Paginated list |
| GET | `/api/v1/projects/{id}` | `project.read:tenant` | Get by ID |
| POST | `/api/v1/projects` | `project.create:tenant` | Create project |
| PUT | `/api/v1/projects/{id}` | `project.update:tenant` | Update project |
| DELETE | `/api/v1/projects/{id}` | `project.delete:tenant` | Soft delete / archive |

### Query parameters (GET list)

| Param | Type | Description |
|-------|------|-------------|
| `page` | int | Page number (default 1) |
| `pageSize` | int | Items per page (default 20, max 100) |
| `search` | string? | Search by name (case-insensitive contains) |
| `customerName` | string? | Filter by exact customer name |
| `status` | ProjectStatus? | Filter by status |
| `isArchived` | bool? | Filter archived/active |

### Audit logging

- [ ] `project.created` on POST
- [ ] `project.updated` on PUT
- [ ] `project.deleted` on DELETE
- [ ] Include entity ID and relevant metadata in audit entry

### Controller

- [ ] `ProjectsController` in `HrPortal.Api/Controllers/V1/`
- [ ] `[RequirePermission]` on all actions
- [ ] `ProjectResourceLoader` used for get/update/delete authorization

### Validation

- [ ] `CreateProjectRequestValidator` — name required, date range, budget >= 0
- [ ] `UpdateProjectRequestValidator` — same rules
- [ ] `GetProjectsQueryValidator` — page/pageSize bounds

### Tests

- [ ] Integration tests: CRUD happy path with auth + tenant header
- [ ] Integration tests: pagination and each filter
- [ ] Integration tests: 401 without token, 403 without permission
- [ ] Integration tests: cross-tenant isolation (Tenant A cannot see Tenant B projects)
- [ ] Unit tests: query handler filter logic

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/HrPortal.Api/Controllers/V1/ProjectsController.cs` | Create |
| `src/backend/src/Modules/HrPortal.Projects/Application/Commands/*` | Extend |
| `src/backend/src/Modules/HrPortal.Projects/Application/Queries/*` | Extend |
| `src/backend/src/Modules/HrPortal.Projects/Application/Validators/*` | Extend |
| `src/backend/tests/HrPortal.IntegrationTests/ProjectsEndpointTests.cs` | Create |
| `src/backend/tests/HrPortal.UnitTests/Projects/*` | Extend |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] All 5 endpoints work in Swagger
- [ ] Pagination, search, customer, status, and archive filters work
- [ ] Audit entries written on mutations
- [ ] `dotnet test` green
- [ ] Tenant isolation verified by integration test

## Next task

→ `02_project_members.md` — Project membership management
