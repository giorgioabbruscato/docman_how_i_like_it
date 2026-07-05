# TASK 12 — ACCESS CONTROL FOUNDATION

> Status: **COMPLETED**

Create the `HrPortal.AccessControl` platform module with tenant-scoped RBAC entities, permission catalog, and base API endpoints.

## Goal

Introduce a centralized access-control platform module that replaces Keycloak-only role checks as the long-term authorization source. This module owns memberships, tenant roles, and the permission catalog.

## Depends on

- Task 11 — Hybrid architecture documentation (ADR-012)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Platform module rules, ITenantEntity |
| Guardrails | `cursor/core/02_guardrails.md` | Migrations centralized in Api |
| Architecture | `cursor/core/03_architecture.md` | Platform services table |
| Patterns | `cursor/core/04_patterns.md` | Module structure template, DI pattern |
| TDD | `cursor/core/01_tdd.md` | Tests required before completion |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Reference HrPortal.Employees structure |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Memory update on completion |
| Domain model | `cursor/memory/domain_model.md` | Update with new entities |
| API contracts | `cursor/memory/api_contracts.md` | Document /me, /roles, /memberships |
| Module deps | `cursor/memory/module_dependencies.md` | Register AccessControl |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Hybrid + RBAC context |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Platform services checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Clean Architecture: Domain → Application → Infrastructure — no domain deps on EF/HTTP
- Platform module must **not** depend on business modules (Employees, Leave, etc.)
- All migrations in `HrPortal.Api/Infrastructure/Persistence/Migrations/`
- Entities implement `ITenantEntity` where tenant-scoped; use `AuditableEntity` base
- Services return `Result<T>`; controllers stay thin
- Register via `AccessControlServiceCollectionExtensions` + `Program.cs`
- FluentValidation on all request DTOs

### Memory — source of truth (`cursor/memory/`)

- Update `domain_model.md`: TenantRole, TenantMembership, UserProfile, extended Tenant
- Update `api_contracts.md`: GET /me, CRUD /roles, CRUD /memberships
- Update `module_dependencies.md`: AddHrPortalAccessControl dependency chain

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — platform services checklist (no domain deps, registered in Program.cs)
- `01_backend_quality_checks.md` — per-module checklist for new platform module
- `00_acceptance_criteria.md` — auth section (401/403)

### Agent prompts (`cursor/prompts/`)

- `01_backend_agent_prompt.md` — copy Employees module structure for platform module layout
- `00_master_prompt.md` — run dotnet test before marking complete

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

### Platform module structure

- [x] Create `src/backend/src/Platform/HrPortal.AccessControl/` following Clean Architecture
- [x] Add project to `HrPortal.sln`
- [x] Reference from `HrPortal.Api` and `HrPortal.Authorization`
- [x] `AccessControlServiceCollectionExtensions.cs` with `AddHrPortalAccessControl()`

### Domain entities

- [x] `TenantRole` — slug, permissions JSON, isSystem, isActive, tenantId
- [x] `TenantMembership` — userId, tenantId, roleIds JSON, employeeId?, attributesJson, isActive
- [x] `UserProfile` — userId, email, isPlatformAdmin
- [x] Extend `Tenant` entity: `Plan`, `FeaturesJson`, `SuspendedAt?`
- [x] `Permissions.cs` — canonical permission string constants (e.g. `employee.read:tenant`)
- [x] `SystemRoleTemplates.cs` — default roles (admin, hr, manager, employee) with permission sets
- [x] `LegacyRoleMapper.cs` — map Keycloak realm roles → system permissions during migration

### Persistence

- [x] EF configurations for all AccessControl entities
- [x] Migration `AccessControlEnterprise` in `HrPortal.Api/Infrastructure/Persistence/Migrations/`
- [x] Register configurations in `HrPortalDbContext`

### Application services

- [x] `IMeService` — return current user profile + permissions + features for tenant
- [x] `ITenantRoleService` — CRUD tenant roles (gated later by feature flag)
- [x] `ITenantMembershipService` — manage memberships
- [x] Repository interfaces + implementations

### Infrastructure

- [x] `SystemRoleSeeder` — seed system roles per tenant on creation
- [x] Demo seed: memberships linking demo users to demo tenant with roles

### API endpoints

- [x] `GET /api/v1/me` — current user context (permissions, employeeId, features)
- [x] `GET/POST/PUT/DELETE /api/v1/roles` — tenant role management
- [x] `GET/POST/PUT/DELETE /api/v1/memberships` — membership management

### Registration

- [x] Register `AddHrPortalAccessControl()` in `Program.cs`

## Files to touch

| Area | Files |
|------|-------|
| New module | `Platform/HrPortal.AccessControl/**` |
| Tenancy | `HrPortal.Tenancy/Domain/Tenant.cs`, `TenantConfiguration.cs` |
| Api | `Program.cs`, `HrPortalDbContext.cs`, `Controllers/V1/MeController.cs`, `RolesController.cs`, `MembershipsController.cs` |
| Migration | `Migrations/*AccessControlEnterprise*` |
| Seed | `DbInitializer.cs` |
| Memory | `cursor/memory/domain_model.md`, `api_contracts.md` |

## Acceptance criteria

- [x] `dotnet build` succeeds
- [x] Migration applies cleanly
- [x] Demo seed creates system roles + memberships for demo tenant
- [x] `GET /api/v1/me` returns permissions for authenticated demo user
- [x] Module has no dependency on business domain modules (Employees, Leave, etc.)
- [x] Integration tests for `/me` endpoint

## Next task

→ `13_unified_tenant_context.md` — Unified TenantContext record
