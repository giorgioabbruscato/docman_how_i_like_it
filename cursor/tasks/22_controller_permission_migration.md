# TASK 22 — CONTROLLER PERMISSION MIGRATION

> Status: **COMPLETED**

Migrate all V1 API controllers from legacy role policies to `[RequirePermission]` attributes.

## Goal

Replace `Policies.AdminOnly`, `Policies.HrOrAdmin`, `Policies.ManagerOrAbove` with fine-grained permission strings aligned to the permission catalog.

## Depends on

- Task 21 — Authorization handler + resource loading

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

### Endpoint → permission mapping

Document and apply mapping for all controllers:

| Controller | Endpoints | Permission examples |
|------------|-----------|---------------------|
| EmployeesController | GET list | `employee.read:tenant` |
| EmployeesController | GET by id | `employee.read:self` or tenant scope via loader |
| EmployeesController | POST/PUT/DELETE | `employee.write:tenant`, `employee.delete:tenant` |
| DepartmentsController | CRUD | `department.*:tenant` |
| LeaveRequestsController | list | `leave.read:tenant` |
| LeaveRequestsController | create | `leave.write:self` |
| LeaveRequestsController | approve/reject | `leave.approve:team` |
| AttendanceController | check-in | `attendance.write:self` |
| AttendanceController | reports | `attendance.read:tenant` |
| DocumentsController | upload | `document.upload:self` |
| DocumentsController | delete | `document.delete:tenant` |
| MeController | GET | `Authenticated` (any valid membership) |
| RolesController | CRUD | `role.manage:tenant` |
| MembershipsController | CRUD | `membership.manage:tenant` |
| AuditLogsController | GET | `audit.read:tenant` |
| PlatformTenantsController | * | `tenant.manage:all` |
| TenantsController | POST | `tenant.manage:all` or restricted |

### Migration checklist

- [x] `EmployeesController`
- [x] `DepartmentsController`
- [x] `LeaveRequestsController`
- [x] `AttendanceController`
- [x] `DocumentsController`
- [x] `MeController` (kept `Authenticated` — no per-permission gate needed)
- [x] `RolesController`
- [x] `MembershipsController`
- [x] `AuditLogsController` (task 25 — created with `[RequirePermission(AuditReadTenant)]`)
- [x] `PlatformTenantsController` (task 24 — `[RequirePermission(TenantManageAll)]` + platform context fix)
- [x] `TenantsController`

### Swagger

- [x] Update `<remarks>Auth: ...</remarks>` on each action to show permission string
- [x] Update `AuthResponsesOperationFilter` if needed (403 description updated to "insufficient permission")

### Memory

- [x] Update `cursor/memory/api_contracts.md` with permission column per endpoint

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Api/Controllers/V1/*.cs` | Migrate attributes |
| `cursor/memory/api_contracts.md` | Update |
| `tests/HrPortal.IntegrationTests/AuthorizationPolicyTests.cs` | Rewrite for permissions |

## Acceptance criteria

- [x] No controller uses `Policies.HrOrAdmin` / `ManagerOrAbove` / `AdminOnly`
- [x] Every business endpoint has `[RequirePermission]` / `[RequireAnyPermission]` or explicit `[AllowAnonymous]`
- [x] Integration tests verify permission denied → 403 (`AuthorizationPolicyTests.cs`, `EndpointAuthorizationGuardTests.cs`)
- [x] Swagger documents permissions

## Next task

→ `23_deprecate_legacy_role_policies.md` — Deprecate legacy role policies
