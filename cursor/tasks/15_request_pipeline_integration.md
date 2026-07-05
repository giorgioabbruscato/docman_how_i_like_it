# TASK 15 — REQUEST PIPELINE INTEGRATION

> Status: **COMPLETED**

Wire the unified request context middleware and replace the legacy tenant-only middleware pipeline.

## Goal

Integrate AccessControl + enriched TenantContext into the HTTP pipeline. After auth, resolve tenant, enrich context with membership/permissions, validate user↔tenant binding, then authorize.

## Depends on

- Task 13 — Unified TenantContext
- Task 14 — Single-tenant deployment mode
- Task 12 — Access Control module registered

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
| Architecture | `cursor/core/03_architecture.md` | Request pipeline order |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Business logic must not read HTTP, JWT, or headers — use `TenantContext` only
- `TenantContext` is the single source of truth for tenantId, userId, mode, roles, permissions
- No authorization logic in controllers or services — Policy layer only (tasks 20+)
- Pipeline: Authentication → RequestContextMiddleware → Authorization → Controllers
- Membership validation in multi mode — 403 without tenant access
- Platform routes excluded per architecture doc

### Memory — source of truth (`cursor/memory/`)

- Consult ADR-012 in `architecture_decisions.md` before changing TenantContext

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

### Middleware

- [x] Create `RequestContextMiddleware` (or extend `TenantResolverMiddleware`):
  1. Resolve tenant slug (respecting single/multi mode)
  2. Load tenant from DB; reject inactive/suspended
  3. Set base `TenantContext` on accessor
  4. If authenticated: enrich via `TenantContextFactory`
  5. Validate membership (multi mode): 403 if no access
  6. Platform admin routes: validate `IsPlatformAdmin`
- [x] `MembershipMiddleware` logic merged or chained as needed

### Pipeline order (Program.cs)

```
Authentication
→ RequestContextMiddleware
→ Authorization
→ Controllers
```

- [x] Remove standalone `TenantResolverMiddleware` if superseded
- [x] Register `AddHrPortalAccessControl()` before pipeline wiring

### Excluded paths (no tenant required)

- [x] `/health`, `/ready`, `/swagger`
- [x] `/api/v1/tenants` (registration/listing)
- [x] `/api/v1/platform/*` (platform admin — tenant optional)

### HttpContext.Items

- [x] Store enriched `TenantContext` in `HttpContext.Items` for debugging/tests

### DI

- [x] `services.AddScoped<TenantContext>(sp => accessor.Current)` remains valid
- [x] Scoped `TenantContext` reflects enriched context after middleware

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.AccessControl/Infrastructure/RequestContextMiddleware.cs` | Create/replace |
| `HrPortal.Api/Program.cs` | Pipeline wiring |
| `HrPortal.Tenancy/TenancyServiceCollectionExtensions.cs` | DI updates |
| `tests/HrPortal.IntegrationTests/Infrastructure/HrPortalWebApplicationFactory.cs` | Pipeline compat |

## Acceptance criteria

- [x] Pipeline order matches ADR-012 diagram
- [x] Authenticated user without tenant membership gets 403 in multi mode
- [x] Platform admin can access `/api/v1/platform/tenants`
- [x] Existing integration tests pass (update test setup if needed)
- [x] `dotnet build && dotnet test` green

## Next task

→ `16_apply_tenant_scope_helper.md` — ApplyTenantScope helper
