# TASK 13 — UNIFIED TENANT CONTEXT

> Status: **PENDING**

Extend `TenantContext` into the single request-scoped identity object combining tenant, user, permissions, and deployment mode.

## Goal

Replace the split `TenantContext` + `UserContext` injection pattern with one unified `TenantContext` record that is the sole source of truth for `tenantId`, `userId`, `mode`, `roles`, and `attributes`.

## Depends on

- Task 12 — Access Control foundation (memberships, permissions)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Multi-tenancy mandatory |
| Guardrails | `cursor/core/02_guardrails.md` | TenantContext, no HTTP in services |
| Architecture | `cursor/core/03_architecture.md` | Tenant resolution, pipeline |
| TDD | `cursor/core/01_tdd.md` | Integration tests with X-Tenant-Id |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Backend scope |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Unified TenantContext, hybrid mode |
| Domain model | `cursor/memory/domain_model.md` | TenantContext contract |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Build & test |
| Module deps | `cursor/memory/module_dependencies.md` | Tenancy + AccessControl wiring |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Business logic must not read HTTP, JWT, or headers — use `TenantContext` only
- `TenantContext` is the single source of truth for tenantId, userId, mode, roles, permissions
- No authorization logic in controllers or services — Policy layer only (tasks 20+)
- Convert TenantContext to record with Mode, Permissions, Attributes, EmployeeId, etc.

### Memory — source of truth (`cursor/memory/`)

- Update `domain_model.md` if TenantContext fields change
- Update `architecture_decisions.md` if factory behavior changes

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — dotnet build + dotnet test

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

### TenantContext record

- [ ] Convert `TenantContext` to a record with fields:
  - `TenantId`, `TenantSlug`, `UserId`, `Email`
  - `Mode` (`TenantDeploymentMode`: Single | Multi)
  - `Roles`, `Permissions`, `RoleSlugs`
  - `EmployeeId?`, `DepartmentId?`, `Attributes`
  - `Features`, `IsPlatformAdmin`, `IsResolved`
- [ ] Factory methods: `Empty`, `CreateTenantOnly()`, `CreateSingleTenantDefault()`
- [ ] Helper: `HasPermission(string permission)`

### Interfaces

- [ ] `ITenantContext` interface mirroring the record contract
- [ ] Clarify `ITenantContextAccessor` — scoped per request, set by middleware

### TenantContextFactory

- [ ] `TenantContextFactory.CreateAsync(tenantContext, userContext)` enriches from:
  - `TenantMembership` + `TenantRole` permissions
  - Legacy Keycloak roles via `LegacyRoleMapper` (fallback)
  - Platform admin flag from `UserProfile`
- [ ] Set `IsResolved = false` when user has no membership and no legacy permissions

### Deprecation path

- [ ] Document that `UserContext` remains for Identity layer only (JWT parsing)
- [ ] Application services will migrate to `TenantContext` only in task 26

### Unit tests

- [ ] Anonymous user → minimal context
- [ ] Legacy Keycloak roles only → permissions mapped, resolved
- [ ] Active membership → permissions from tenant roles
- [ ] Platform admin → elevated permissions
- [ ] No membership + no legacy roles → not resolved

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Tenancy/TenantContext.cs` | Extend to full record |
| `HrPortal.Tenancy/ITenantContextAccessor.cs` | Clarify interface |
| `HrPortal.AccessControl/Infrastructure/TenantContextFactory.cs` | Implement enrichment |
| `tests/HrPortal.UnitTests/Tenancy/TenantContextFactoryTests.cs` | New |

## Acceptance criteria

- [ ] `TenantContext` contains all fields from ADR-012
- [ ] Factory correctly merges membership + legacy role permissions
- [ ] Unit tests cover all resolution permutations
- [ ] `dotnet test` passes

## Next task

→ `14_single_tenant_deployment_mode.md` — Single-tenant deployment mode
