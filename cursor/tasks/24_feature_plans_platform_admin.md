# TASK 24 — FEATURE PLANS PLATFORM ADMIN

> Status: **COMPLETED**

Implement tenant plans, feature gates, and platform administrator capabilities for SaaS deployments.

## Goal

Support tiered SaaS offerings (Free/Pro/Enterprise) with feature flags and platform-level tenant management. Single-tenant OSS deployments default to Enterprise-equivalent features.

## Depends on

- Task 12 — Access Control foundation
- Task 15 — Request pipeline (platform admin routes)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` |  |
| Guardrails | `cursor/core/02_guardrails.md` | No secrets hardcoded |
| Architecture | `cursor/core/03_architecture.md` | Platform services |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` |  |
| Domain model | `cursor/memory/domain_model.md` | TenantPlan, TenantFeatures |
| API contracts | `cursor/memory/api_contracts.md` | Platform admin endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Platform services |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Feature gates via IFeatureGateService — no inline plan checks in controllers
- Platform admin routes require IsPlatformAdmin + tenant.manage:all permission
- Services return Result<T> for feature limit violations

### Memory — source of truth (`cursor/memory/`)

- Update `domain_model.md`: TenantPlan, TenantFeatures
- Update `api_contracts.md`: /api/v1/platform/tenants endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — platform services checklist

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

### Domain

- [x] `TenantPlan` enum: Free, Pro, Enterprise
- [x] `TenantFeatures` record with defaults per plan:
  - `maxEmployees`, `customRoles`, `auditLog`, `advancedReports`, etc.
- [x] `Tenant.ModulesJson` (module list) + `Tenant.FeaturesJson` (plan feature overrides only — split
      model) merge into effective `TenantFeatures` via `GetEffectiveFeatures()`

### Feature gate service

- [x] `IFeatureGateService`:
  - `IsEnabledAsync(featureKey)`
  - `GetMaxEmployeesAsync()`
  - `GetEffectiveFeaturesAsync()` (also exposed on `/api/v1/me` as `planFeatures`)
- [x] Enforce employee limit on Free plan (e.g. max 20) — `EmployeeService.CreateAsync`
- [x] Block custom role creation when `customRoles` feature disabled — `TenantRoleService.CreateAsync`
- [x] Single-tenant mode: `FeatureGateService` returns Enterprise-equivalent features regardless of plan

### Platform admin

- [x] `PlatformAdmin` Keycloak role + demo user in seed (`platform.owner@demo.local`)
- [x] `UserProfile.IsPlatformAdmin` flag
- [x] Endpoints `/api/v1/platform/tenants`:
  - GET list all tenants
  - POST suspend / reactivate
  - PUT plan
  - PUT features override
- [x] Platform routes excluded from tenant middleware (task 15); platform context sets
      `Permissions = Permissions.PlatformAdmin` so `[RequirePermission(TenantManageAll)]` succeeds
- [x] `POST /api/v1/tenants` kept `[AllowAnonymous]` (self-service SaaS tenant signup — the documented
      alternative to platform-admin-only creation; see `api_contracts.md`)

### Permissions

- [x] `tenant.manage:all`, `billing.manage:all`, `support.access:all`, `system.override:all`

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Tenancy/Domain/TenantPlan.cs` | Create |
| `HrPortal.Tenancy/Domain/TenantFeatures.cs` | Create |
| `HrPortal.AccessControl/Infrastructure/FeatureGateService.cs` | Create |
| `HrPortal.Api/Controllers/V1/PlatformTenantsController.cs` | Create |
| `HrPortal.Employees/Application/EmployeeService.cs` | Enforce limit |
| `docker/keycloak/realm-export.json` | PlatformAdmin role |
| `DbInitializer.cs` | Platform admin demo user |

## Acceptance criteria

- [x] Free plan blocks employee creation at the plan's `maxEmployees` limit (`PLAN_LIMIT_EXCEEDED` → 403)
- [x] Custom roles blocked on Free plan
- [x] Platform admin can list/suspend/reactivate tenants and change plan/feature overrides
- [x] Non-platform-admin gets 403 on platform routes; anonymous gets 401
- [x] Single-tenant mode: all features enabled for default tenant

## Next task

→ `25_audit_enterprise.md` — Audit enterprise
