# TASK 27 — EMPLOYEES MODULE HYBRID REFACTOR

> Status: **PENDING**

Refactor the Employees module as the reference implementation for hybrid architecture compliance.

## Goal

Bring `HrPortal.Employees` into full compliance: pure services, scoped repositories, permission-only controllers, no inline auth.

## Depends on

- Task 18 — Repository ApplyTenantScope
- Task 22 — Controller permissions
- Task 26 — Service context unification

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

- [ ] `EmployeeService` uses `TenantContext` only (no UserContext, no auth checks)
- [ ] Use `ctx.TenantId` on create; `ctx.UserId` for audit fields
- [ ] No `EnsureTenantResolved()` — rely on middleware
- [ ] Return `Result<T>` for all operations (unchanged pattern)

### Repository layer

- [ ] `EmployeeRepository` — all queries use `ApplyTenantScope`
- [ ] Verify no raw `Set<Employee>()` without scoping

### Controller layer

- [ ] `[RequirePermission]` on all actions (from task 22 mapping)
- [ ] `EmployeeResourceLoader` registered for get/update/delete
- [ ] Zero inline authorization logic

### Tests

- [ ] Unit tests: domain rules + service with TenantContext fixture
- [ ] Integration tests: permission denied scenarios
- [ ] Cross-tenant isolation (covered in task 19, verify employees pass)

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Employees/Application/EmployeeService.cs` | Refactor |
| `HrPortal.Employees/Infrastructure/Persistence/EmployeeRepository.cs` | Verify scope |
| `HrPortal.Api/Controllers/V1/EmployeesController.cs` | Permissions only |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/EmployeeResourceLoader.cs` | Create |
| `tests/HrPortal.UnitTests/Employees/*` | Update |
| `tests/HrPortal.IntegrationTests/EmployeesEndpointTests.cs` | Update |

## Acceptance criteria

- [ ] Employees module passes hybrid architecture checklist from task 11 eval
- [ ] Serves as copy template for tasks 28–31
- [ ] `dotnet test` green for Employees tests

## Next task

→ `28_leave_module_hybrid_refactor.md` — Leave module hybrid refactor
