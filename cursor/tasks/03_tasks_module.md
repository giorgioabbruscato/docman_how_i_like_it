# TASK 03 — TASKS MODULE

> Status: **PENDING**

Create the `HrPortal.Tasks` module with `ProjectTask` entity, CRUD, validation, audit, and tenant isolation.

## Goal

Scaffold the Tasks module and implement full CRUD for project tasks. Entity is named `ProjectTask` to avoid C# `Task` keyword collision.

## Depends on

- Task 00 — Projects module foundation (`IProjectLookup`)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | ApplyTenantScope |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Copy Employees |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | Add ProjectTask |
| API contracts | `cursor/memory/api_contracts.md` | Tasks endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Tasks depends on Projects |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Validate `ProjectId` via `IProjectLookup.ExistsAsync` — no direct Projects DbSet access
- Validate `AssignedEmployeeId` via `IEmployeeLookup` when set
- Lightweight CQRS in Application layer
- All CRUD endpoints with `[RequirePermission]`
- Audit on create/update/delete
- Tenant isolation via `ApplyTenantScope`

### Memory — source of truth (`cursor/memory/`)

- Add `ProjectTask` to `domain_model.md`
- Document Tasks CRUD in `api_contracts.md`
- Update `module_dependencies.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 00 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.Tasks` under `src/backend/src/Modules/`
- [ ] Register in `Program.cs` after `AddProjectsModule()`
- [ ] EF schema: `tasks`

### Domain entity: `ProjectTask`

| Field | Type | Notes |
|-------|------|-------|
| ProjectId | Guid | FK via IProjectLookup |
| Title | string | Required, max 300 |
| Description | string? | Optional |
| AssignedEmployeeId | Guid? | FK via IEmployeeLookup |
| Priority | TaskPriority | Low, Medium, High, Critical |
| Status | TaskStatus | Todo, InProgress, Review, Done |
| EstimatedHours | decimal? | >= 0 |
| SpentHours | decimal | >= 0, default 0 |
| DueDate | DateOnly? | Optional |

### Permissions

| Constant | Value |
|----------|-------|
| `TaskReadTenant` | `task.read:tenant` |
| `TaskCreateTenant` | `task.create:tenant` |
| `TaskUpdateTenant` | `task.update:tenant` |
| `TaskDeleteTenant` | `task.delete:tenant` |
| `TaskUpdateStatusSelf` | `task.update_status:self` |

- [ ] `ITaskLookup` for cross-module use (TimeTracking)
- [ ] `TaskResourceLoader` registered

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/tasks` | `task.read:tenant` |
| GET | `/api/v1/tasks/{id}` | `task.read:tenant` |
| POST | `/api/v1/tasks` | `task.create:tenant` |
| PUT | `/api/v1/tasks/{id}` | `task.update:tenant` |
| DELETE | `/api/v1/tasks/{id}` | `task.delete:tenant` |

### List filters

- [ ] `projectId`, `status`, `priority`, `assignedEmployeeId`
- [ ] Pagination: `page`, `pageSize`
- [ ] Search by title

### Tests

- [ ] Domain unit tests
- [ ] Validator tests
- [ ] Integration tests: CRUD + filters + tenant isolation

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Projects/Application/IProjectLookup.cs` | Create |
| `src/backend/src/Modules/HrPortal.Tasks/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TasksController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |

## Acceptance criteria

- [ ] Module builds, migrates, and registers
- [ ] CRUD endpoints documented in Swagger
- [ ] Filters and pagination work
- [ ] Audit on mutations
- [ ] `dotnet test` green

## Next task

→ `04_task_board.md` — Kanban board APIs
