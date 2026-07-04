# TASK 30 — ATTENDANCE MODULE HYBRID REFACTOR

> Status: **PENDING**

Refactor the Attendance module following the Employees reference pattern (task 27).

## Goal

Pure attendance service, tenant-scoped repository, permission-based check-in/out and reporting with self/department/tenant scopes.

## Depends on

- Task 27 — Employees module (reference implementation)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Cross-module via interfaces only |
| Guardrails | `cursor/core/02_guardrails.md` | ApplyTenantScope, no auth in services |
| Patterns | `cursor/core/04_patterns.md` | HrPortal.Employees reference module |
| TDD | `cursor/core/01_tdd.md` | Unit + integration tests |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Copy Employees structure |
| Domain model | `cursor/memory/domain_model.md` | Entity rules for this module |
| API contracts | `cursor/memory/api_contracts.md` | Endpoint permissions |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Hybrid compliance |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Per-module checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Task 27 (Employees) is the reference — replicate its hybrid pattern exactly
- Repository: every query uses ApplyTenantScope
- Controller: [RequirePermission] only — register IResourceLoader
- Service: pure orchestration — no auth, TenantContext only
- FluentValidation on request DTOs unchanged

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` if endpoint permissions change

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — full per-module checklist
- Cross-tenant isolation tests from task 19 must still pass

### Agent prompts (`cursor/prompts/`)

- `01_backend_agent_prompt.md`
- `00_master_prompt.md`

### Before starting
1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for this task type
3. Follow `/cursor/prompts/00_master_prompt.md` workflow

- Use `01_backend_agent_prompt.md` for implementation scope

### Before completing
1. Run quality commands listed in Acceptance criteria
2. Verify against applicable `/cursor/evals/` checklist
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### Service layer

- [ ] `AttendanceService` — TenantContext only, no auth checks
- [ ] Check-in/out uses `ctx.UserId` and request `employeeId` (authorization at policy layer)
- [ ] Reports use tenant-scoped queries only

### Repository layer

- [ ] `AttendanceRepository` — all queries use `ApplyTenantScope`

### Controller + authorization

- [ ] Permissions: `attendance.read:*`, `attendance.write:self`
- [ ] Check-in/out: policy validates employeeId against self scope (or elevated scope)
- [ ] Reports endpoint: `attendance.read:tenant` or `attendance.read:department`

### Tests

- [ ] Unit tests with TenantContext fixture
- [ ] Integration: employee can check in for self only
- [ ] Integration: manager can view department attendance reports

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Attendance/Application/AttendanceService.cs` | Refactor |
| `HrPortal.Attendance/Infrastructure/Persistence/AttendanceRepository.cs` | Verify scope |
| `HrPortal.Api/Controllers/V1/AttendanceController.cs` | Permissions |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/AttendanceResourceLoader.cs` | Create |
| `tests/HrPortal.IntegrationTests/AttendanceEndpointTests.cs` | Update |

## Acceptance criteria

- [ ] Matches Employees module hybrid pattern
- [ ] Check-in for another employee denied without permission
- [ ] `dotnet test` green

## Next task

→ `31_departments_module_hybrid_refactor.md` — Departments module hybrid refactor
