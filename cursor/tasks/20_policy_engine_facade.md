# TASK 20 — POLICY ENGINE FACADE

> Status: **COMPLETED**

Introduce `IPolicyEngine` with `Can(ctx, action, resource)` as the single authorization decision point.

## Goal

Replace scattered role checks and split permission evaluators with one policy engine that combines permission matching and ABAC scope resolution.

## Depends on

- Task 12 — Access Control foundation (permission catalog)
- Task 15 — Request pipeline (enriched TenantContext available)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Controllers thin, no business logic |
| Guardrails | `cursor/core/02_guardrails.md` | Policy-based authorization |
| Architecture | `cursor/core/03_architecture.md` | Authorization layer |
| Patterns | `cursor/core/04_patterns.md` | Controller pattern |
| TDD | `cursor/core/01_tdd.md` | Test auth policies |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` |  |
| ADR-003 | `cursor/memory/architecture_decisions.md` | Keycloak JWT — backend validates only |
| ADR-012 | `cursor/memory/architecture_decisions.md` | can(ctx, action, resource) |
| API contracts | `cursor/memory/api_contracts.md` | Permission per endpoint |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` |  |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | 401/403 auth criteria |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Authorization **only** in Policy layer — `IPolicyEngine.Can(ctx, action, resource)`
- No `if (role === ...)` or `User.IsInRole` in controllers or services
- Controllers use declarative `[RequirePermission]` — zero inline auth in action bodies
- Resource-aware checks via IResourceLoader + ScopeResolver

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with permission string per endpoint

### Quality gates (`cursor/evals/`)

- `00_acceptance_criteria.md` — Unauthorized 401, Forbidden 403
- `01_backend_quality_checks.md` — controller thin, no business logic

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

### IPolicyEngine

- [ ] Interface: `bool Can(TenantContext ctx, string action, ResourceContext? resource)`
- [ ] Implementation delegates to:
  1. Permission match against `ctx.Permissions`
  2. Scope resolution via `IScopeResolver`

### ABAC scopes

- [ ] `AccessScope` enum: `Self`, `Department`, `Team`, `Tenant`, `All`
- [ ] Permission format: `{resource}.{action}:{scope}` (e.g. `leave.approve:team`)
- [ ] `IScopeResolver.IsInScope(ctx, scope, resource)`:
  - `Self` → ctx.EmployeeId == resource.EmployeeId
  - `Department` → ctx.DepartmentId == resource.DepartmentId
  - `Team` → self or same department
  - `Tenant` → resource.TenantId == ctx.TenantId
  - `All` → ctx.IsPlatformAdmin

### ResourceContext record

- [ ] `ResourceContext(EmployeeId?, DepartmentId?, TenantId?)`

### Consolidation

- [ ] `IResourceAuthorizationService` wraps or replaces with `IPolicyEngine`
- [ ] `IPermissionEvaluator` delegates to policy engine

### Unit tests

- [ ] Permission denied when not in ctx.Permissions
- [ ] Scope denied when resource out of scope
- [ ] Platform admin bypass for `All` scope
- [ ] Matrix test: each permission × scope combination

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.AccessControl/Application/IPolicyEngine.cs` | Create |
| `HrPortal.AccessControl/Infrastructure/PolicyEngine.cs` | Create |
| `HrPortal.AccessControl/Infrastructure/ScopeResolver.cs` | Create/update |
| `HrPortal.AccessControl/Domain/AccessScope.cs` | Create |
| `HrPortal.AccessControl/Domain/Permissions.cs` | Verify catalog |
| `tests/HrPortal.UnitTests/AccessControl/PolicyEngineTests.cs` | Create |

## Acceptance criteria

- [ ] All authorization decisions flow through `IPolicyEngine.Can()`
- [ ] No direct `ctx.Roles.Contains(...)` in application layer
- [ ] Unit tests cover permission + scope matrix
- [ ] `dotnet test` passes

## Next task

→ `21_authorization_handler_resource_loading.md` — Authorization handler + resource loading
