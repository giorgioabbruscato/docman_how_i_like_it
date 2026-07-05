# TASK 31 — DEPARTMENTS MODULE HYBRID REFACTOR

> Status: **COMPLETED**

Refactor the Departments module following the Employees reference pattern (task 27).

## Goal

Pure department service, tenant-scoped repository, permission-based CRUD with tenant scope.

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

- [ ] `DepartmentService` — TenantContext only, no auth checks
- [ ] Hierarchy validation unchanged (parent department lookup)
- [ ] `ctx.TenantId` on create; `ctx.UserId` for audit

### Repository layer

- [ ] `DepartmentRepository` — all queries use `ApplyTenantScope`

### Controller + authorization

- [ ] Permissions: `department.read:tenant`, `department.write:tenant`, `department.delete:tenant`
- [ ] `DepartmentResourceLoader` for get/update/delete

### Tests

- [ ] Unit tests with TenantContext fixture
- [ ] Integration: CRUD with correct permissions
- [ ] Cross-tenant department access returns 404

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Departments/Application/DepartmentService.cs` | Refactor |
| `HrPortal.Departments/Infrastructure/Persistence/DepartmentRepository.cs` | Verify scope |
| `HrPortal.Api/Controllers/V1/DepartmentsController.cs` | Permissions |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/DepartmentResourceLoader.cs` | Create |
| `tests/HrPortal.IntegrationTests/DepartmentsEndpointTests.cs` | Update |

## Acceptance criteria

- [ ] Matches Employees module hybrid pattern
- [ ] All 5 business modules now hybrid-compliant
- [ ] `dotnet test` green

## Next task

→ `32_frontend_single_tenant_mode.md` — Frontend single-tenant mode
