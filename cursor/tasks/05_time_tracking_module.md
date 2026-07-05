# TASK 05 — TIME TRACKING MODULE

> Status: **COMPLETED**

Create the `HrPortal.TimeTracking` module with `TimeEntry` entity and CRUD APIs.

## Goal

Scaffold Time Tracking with full CRUD, validation, audit logging, permissions, and tenant isolation.

## Depends on

- Task 00 — Projects (`IProjectLookup`)
- Task 03 — Tasks (`ITaskLookup`)
- `HrPortal.Employees` — `IEmployeeLookup`

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | UTC timestamps |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Module template |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | Add TimeEntry |
| API contracts | `cursor/memory/api_contracts.md` | Time entry endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Add TimeTracking |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- All timestamps stored as UTC (`DateTime.UtcNow`, `DateTimeOffset` normalized to UTC)
- Validate ProjectId via `IProjectLookup`, TaskId via `ITaskLookup` (optional), EmployeeId via `IEmployeeLookup`
- Self scope: employees CRUD own entries; supervisors read team/tenant scope
- `WorkedMinutes` computed from StartTime/EndTime when both set
- Lightweight CQRS structure
- Audit on create/update/delete

### Memory — source of truth (`cursor/memory/`)

- Add `TimeEntry` to `domain_model.md`
- Document endpoints in `api_contracts.md`
- Update `module_dependencies.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 00 and 03 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.TimeTracking` under `src/backend/src/Modules/`
- [ ] Register in `Program.cs` after `AddTasksModule()`
- [ ] EF schema: `time_tracking`

### Domain entity: `TimeEntry`

| Field | Type | Notes |
|-------|------|-------|
| EmployeeId | Guid | FK via IEmployeeLookup |
| ProjectId | Guid | FK via IProjectLookup |
| TaskId | Guid? | Optional FK via ITaskLookup |
| StartTime | DateTime | UTC |
| EndTime | DateTime? | UTC, null = running timer |
| WorkedMinutes | int | Computed when EndTime set |
| Description | string? | Optional |
| Billable | bool | Default true |
| CreatedAt | DateTime | UTC (may use AuditableEntity) |

Note: if `TimeEntry` inherits `AuditableEntity`, `CreatedAt` comes from base — do not duplicate.

### Permissions

| Constant | Value |
|----------|-------|
| `TimeEntryReadSelf` | `time_entry.read:self` |
| `TimeEntryReadTeam` | `time_entry.read:team` |
| `TimeEntryReadTenant` | `time_entry.read:tenant` |
| `TimeEntryCreateSelf` | `time_entry.create:self` |
| `TimeEntryUpdateSelf` | `time_entry.update:self` |
| `TimeEntryDeleteSelf` | `time_entry.delete:self` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/time-entries` | `time_entry.read:self` OR `read:team` OR `read:tenant` |
| GET | `/api/v1/time-entries/{id}` | Same as list |
| POST | `/api/v1/time-entries` | `time_entry.create:self` |
| PUT | `/api/v1/time-entries/{id}` | `time_entry.update:self` |
| DELETE | `/api/v1/time-entries/{id}` | `time_entry.delete:self` |

### List filters

- [ ] `employeeId`, `projectId`, `taskId`, `fromDate`, `toDate`, `billable`
- [ ] Pagination: `page`, `pageSize`
- [ ] Self scope auto-filters to `ctx.EmployeeId`

### Tests

- [ ] Domain: WorkedMinutes calculation
- [ ] Integration: CRUD + scope filtering
- [ ] Integration: tenant isolation

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.TimeTracking/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TimeEntriesController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |

## Acceptance criteria

- [ ] Module builds, migrates, registers
- [ ] CRUD with filters and pagination
- [ ] UTC timestamps enforced
- [ ] Permission scoping (self/team/tenant) works
- [ ] `dotnet test` green

## Next task

→ `06_timer.md` — Timer start/stop functionality
