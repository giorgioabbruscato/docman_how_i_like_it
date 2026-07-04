# TASK 23 — DEPRECATE LEGACY ROLE POLICIES

> Status: **PENDING**

Remove legacy ASP.NET role-based policies and migrate frontend to permission-based authorization.

## Goal

Complete the transition from Keycloak global roles to tenant-scoped permissions. Keep `LegacyRoleMapper` only as a migration shim for users without memberships.

## Depends on

- Task 22 — Controller permission migration

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
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | Migrate to permission-based UI |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Authorization **only** in Policy layer — `IPolicyEngine.Can(ctx, action, resource)`
- No `if (role === ...)` or `User.IsInRole` in controllers or services
- Controllers use declarative `[RequirePermission]` — zero inline auth in action bodies
- Resource-aware checks via IResourceLoader + ScopeResolver
- Deprecate Policies.AdminOnly/HrOrAdmin/ManagerOrAbove — use permissions

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

### Backend cleanup

- [ ] Mark `Policies.AdminOnly`, `HrOrAdmin`, `ManagerOrAbove` as `[Obsolete]`
- [ ] Remove policy registrations from `AuthorizationServiceCollectionExtensions` (or keep shim temporarily)
- [ ] Update `EndpointAuthorizationGuardTests` — verify `[RequirePermission]` or `[Authorize]` on all endpoints
- [ ] Rewrite `AuthorizationPolicyTests` for permission-based matrix
- [ ] Document sunset plan for `LegacyRoleMapper` in ADR-012 addendum

### Keycloak

- [ ] Realm roles remain for bootstrap; permissions derived via mapper until all users have memberships
- [ ] No code changes to Keycloak required in this task

### Frontend migration

- [ ] Create `src/frontend/src/lib/auth-permissions.ts`:
  - `hasPermission(permission: string): boolean`
  - Load permissions from auth store (populated by `/api/v1/me`)
- [ ] Deprecate role-based checks in `auth-roles.ts` (keep shim temporarily)
- [ ] Update `auth-store.ts` to store permissions array from `/me`

### Tests

- [ ] Integration: user with legacy Keycloak role still gets correct permissions
- [ ] Integration: user with membership gets tenant role permissions

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Authorization/Policies.cs` | Obsolete legacy constants |
| `HrPortal.Authorization/AuthorizationServiceCollectionExtensions.cs` | Remove legacy policies |
| `tests/HrPortal.IntegrationTests/AuthorizationPolicyTests.cs` | Rewrite |
| `tests/HrPortal.IntegrationTests/EndpointAuthorizationGuardTests.cs` | Update |
| `frontend/src/lib/auth-permissions.ts` | Create |
| `frontend/src/stores/auth-store.ts` | Permissions from /me |

## Acceptance criteria

- [ ] No production code references `Policies.HrOrAdmin` etc.
- [ ] Frontend uses `hasPermission()` for UI gating
- [ ] Legacy Keycloak users still functional via mapper
- [ ] All tests pass

## Next task

→ `24_feature_plans_platform_admin.md` — Feature plans + platform admin
