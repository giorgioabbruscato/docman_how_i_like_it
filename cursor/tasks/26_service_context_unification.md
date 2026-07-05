# TASK 26 — SERVICE CONTEXT UNIFICATION

> Status: **COMPLETED**

Remove dual `TenantContext` + `UserContext` injection from all application services.

## Goal

Application services receive unified `TenantContext` only. `UserContext` remains in Identity infrastructure for JWT parsing but is not injected into business services.

## Depends on

- Task 13 — Unified TenantContext
- Task 15 — Pipeline enrichment (ctx.UserId populated)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Business logic in Application/Domain only |
| Guardrails | `cursor/core/02_guardrails.md` | No UserContext in services — TenantContext only |
| Patterns | `cursor/core/04_patterns.md` | Service layer, Result<T> |
| TDD | `cursor/core/01_tdd.md` | Service unit tests with mocks |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Reference Employees |
| ADR-004 | `cursor/memory/architecture_decisions.md` | Result pattern |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Service purity |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Per-module checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Services receive `TenantContext` only — remove `UserContext` injection
- Zero authorization logic in services — enforced at Policy layer
- Use `ctx.TenantId` / `ctx.UserId` for tenant stamp and audit fields
- All services return `Result<T>` — map to HTTP in controllers only
- No EnsureTenantResolved() duplication — middleware guarantees resolution

### Memory — source of truth (`cursor/memory/`)

- No domain/API memory changes unless endpoints change

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — no DbContext outside repositories, Result<T>

### Agent prompts (`cursor/prompts/`)

- `01_backend_agent_prompt.md` — TDD workflow
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

### Service migration

Remove `UserContext` constructor injection; use `_tenantContext.UserId` for audit fields:

- [x] `EmployeeService`
- [x] `DepartmentService`
- [x] `LeaveRequestService`
- [x] `AttendanceService`
- [x] `DocumentService`
- [x] `AuditService`

### Cleanup

- [x] Remove duplicated `EnsureTenantResolved()` private methods — middleware guarantees resolution in multi mode; single mode uses default tenant
- [x] Replace `_tenantContext.TenantId` with ctx from accessor consistently
- [x] Update service unit tests to build `TenantContext` fixtures (not UserContext)

### DI

- [x] Verify scoped `TenantContext` is enriched before service resolution
- [x] Document: services must never inject `IHttpContextAccessor`

## Files to touch

| File | Action |
|------|--------|
| `Modules/*/Application/*Service.cs` | Remove UserContext |
| `HrPortal.Audit/Infrastructure/AuditService.cs` | Use TenantContext |
| `tests/HrPortal.UnitTests/**/*ServiceTests.cs` | Update fixtures |

## Acceptance criteria

- [x] No application service injects `UserContext`
- [x] Audit fields (`CreatedBy`, `UpdatedBy`) use `TenantContext.UserId`
- [x] All unit tests pass with TenantContext fixtures
- [x] `dotnet test` green

## Next task

→ `27_employees_module_hybrid_refactor.md` — Employees module (reference implementation)
