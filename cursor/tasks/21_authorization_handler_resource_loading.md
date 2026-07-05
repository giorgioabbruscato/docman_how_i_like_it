# TASK 21 — AUTHORIZATION HANDLER RESOURCE LOADING

> Status: **COMPLETED**

Wire ASP.NET authorization to the policy engine with resource-aware entity loading.

## Goal

Enforce authorization at the policy layer via `IAuthorizationHandler`. Controllers remain declarative (`[RequirePermission]`); zero inline auth logic in action bodies.

## Depends on

- Task 20 — Policy engine facade

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

### Authorization infrastructure

- [ ] `PermissionRequirement` — holds permission string
- [ ] `PermissionAuthorizationHandler` — calls `IPolicyEngine.Can(ctx, permission, resource)`
- [ ] `[RequirePermission("leave.approve:team")]` attribute → policy registration
- [ ] Dynamic policy registration: `Policies.PermissionPrefix + permission`

### Resource loading

- [ ] `IResourceLoader` interface: `Task<ResourceContext?> LoadAsync(HttpContext, CancellationToken)`
- [ ] Per-module loaders (or keyed by entity type):
  - `LeaveRequestResourceLoader` — from route `{id}`
  - `EmployeeResourceLoader`
  - `DocumentResourceLoader`
  - etc.
- [ ] List/create endpoints: resource context null (permission-only check)
- [ ] Get/update/delete: load entity, build ResourceContext

### Audit integration

- [ ] `LogAccessDecisionAsync` on every permission check (allow + deny)
- [ ] Include: actor, permission, resource, decision, IP address

### Controller rules

- [ ] No `if (User.IsInRole(...))` in controller actions
- [ ] No manual permission checks in services

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Authorization/PermissionRequirement.cs` | Create/update |
| `HrPortal.Authorization/Infrastructure/PermissionAuthorizationHandler.cs` | Create |
| `HrPortal.Authorization/RequirePermissionAttribute.cs` | Create |
| `HrPortal.Authorization/AuthorizationServiceCollectionExtensions.cs` | Register handler |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/*` | Create |
| `HrPortal.Audit/Infrastructure/AuditService.cs` | LogAccessDecision |

## Acceptance criteria

- [ ] Handler invokes policy engine with loaded resource
- [ ] Denied requests return 403 with ProblemDetails
- [ ] Access decisions logged to audit trail
- [ ] Unit tests for handler with mock policy engine + loader

## Next task

→ `22_controller_permission_migration.md` — Controller permission migration
