# TASK 28 — LEAVE MODULE HYBRID REFACTOR

> Status: **COMPLETED**

Refactor the Leave module following the Employees reference pattern (task 27).

## Goal

Pure leave service orchestration, tenant-scoped repository, permission-based controller, resource-aware authorization for approve/reject/cancel.

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

- [ ] `LeaveRequestService` — TenantContext only, no auth checks
- [ ] Business rules unchanged: overlap check, annual leave limits, domain exceptions
- [ ] `ctx.UserId` for approve/reject/cancel actor

### Repository layer

- [ ] `LeaveRequestRepository` — all queries use `ApplyTenantScope`

### Controller + authorization

- [ ] Permissions: `leave.read:*`, `leave.write:self`, `leave.approve:team`, `leave.delete:self`
- [ ] `LeaveRequestResourceLoader` — load employeeId/departmentId for scope checks
- [ ] Approve/reject: policy engine validates team/tenant scope against leave request's employee

### Tests

- [ ] Unit tests updated for TenantContext fixture
- [ ] Integration: employee can cancel own leave; manager can approve team leave
- [ ] Integration: employee cannot approve another employee's leave

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Leave/Application/LeaveRequestService.cs` | Refactor |
| `HrPortal.Leave/Infrastructure/Persistence/LeaveRequestRepository.cs` | Verify scope |
| `HrPortal.Api/Controllers/V1/LeaveRequestsController.cs` | Permissions |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/LeaveRequestResourceLoader.cs` | Create |
| `tests/HrPortal.UnitTests/Leave/*` | Update |
| `tests/HrPortal.IntegrationTests/LeaveRequestsEndpointTests.cs` | Update |

## Acceptance criteria

- [ ] Matches Employees module hybrid pattern
- [ ] Resource-scoped approve/reject enforced by policy engine
- [ ] `dotnet test` green

## Next task

→ `29_documents_module_hybrid_refactor.md` — Documents module hybrid refactor
