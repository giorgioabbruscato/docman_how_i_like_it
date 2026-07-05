# TASK 30 — ADMIN DASHBOARD

> Status: **PENDING**

Create platform-level admin dashboard with cross-tenant metrics for platform administrators.

## Goal

Expose aggregate platform metrics (tenant count, usage, license utilization) for users with `IsPlatformAdmin`, with explicit cross-tenant queries that never bypass tenant scoping on business data.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 28 — Calendar integration (recommended sequence — last EPIC 9 task)
- Tasks 00–22 — All feature modules **COMPLETED**
- Tenancy platform — `IsPlatformAdmin` flag

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | Cross-tenant safety |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Platform service template |
| Architecture | `cursor/core/03_architecture.md` | Platform admin scope |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| Domain model | `cursor/memory/domain_model.md` | Platform metrics DTOs |
| API contracts | `cursor/memory/api_contracts.md` | Admin endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- **Platform admin only** — `[RequirePlatformAdmin]` or equivalent; tenant users get `403`
- Cross-tenant aggregate queries use explicit `tenantId` grouping — never disable global query filters on business entities for convenience
- Prefer dedicated read models / raw SQL aggregates over loading full entity graphs
- Metrics: tenant count, active users, storage usage (if tracked), license seats used vs allocated
- Per-tenant breakdown table with drill-down link (tenant slug, employee count, last activity)
- No PII in aggregate logs beyond what platform admin already has access to
- Separate route namespace: `/api/v1/platform/admin/*`

### Memory — source of truth (`cursor/memory/`)

- Document platform admin endpoints in `api_contracts.md`
- Add platform metrics DTOs to `domain_model.md` (read models only)

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`
- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 00–22 and all EPIC 9 predecessors are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Run `npm run build`
3. Update memory files
4. Mark task status **COMPLETED**

## Deliverables

### Backend: platform metrics API

| Method | Path | Access |
|--------|------|--------|
| GET | `/api/v1/platform/admin/dashboard` | Platform admin |
| GET | `/api/v1/platform/admin/tenants` | Platform admin |
| GET | `/api/v1/platform/admin/tenants/{tenantId}/summary` | Platform admin |
| GET | `/api/v1/platform/admin/usage` | Platform admin |

### Dashboard summary DTO

| Field | Type | Notes |
|-------|------|-------|
| TotalTenants | int | Active tenants |
| TotalEmployees | int | Across all tenants |
| ActiveEmployeesLast30Days | int | Had login or activity |
| TotalTimeEntriesLast30Days | int | Aggregate count |
| LicenseSeatsUsed | int | If licensing tracked |
| LicenseSeatsTotal | int | Configured limit |

### Tenant list DTO

| Field | Type | Notes |
|-------|------|-------|
| TenantId | Guid | |
| Slug | string | |
| Name | string | |
| EmployeeCount | int | |
| IsActive | bool | |
| CreatedAt | DateTime | UTC |
| LastActivityAt | DateTime? | UTC |

### Per-tenant summary DTO

| Field | Type | Notes |
|-------|------|-------|
| EmployeeCount | int | |
| ActiveProjects | int | |
| TimeEntriesThisMonth | int | |
| AttendanceSessionsThisMonth | int | |
| LeaveRequestsPending | int | |
| StorageUsedBytes | long? | If Documents module tracks storage |

### Backend implementation

- [ ] Extend Tenancy platform with `IPlatformMetricsService`
- [ ] Aggregate queries with `GROUP BY tenant_id` — bypass filters only on aggregate SQL, not entity loads
- [ ] Integration tests: platform admin gets 200, tenant user gets 403
- [ ] Integration tests: metrics match seeded test data

### Frontend

- [ ] API client: `src/frontend/src/api/platformAdmin.ts`
- [ ] Page: `/admin/dashboard` — KPI cards + tenant table
- [ ] Page: `/admin/tenants/{tenantId}` — per-tenant drill-down
- [ ] Route guard: `IsPlatformAdmin` only
- [ ] KPI cards: total tenants, employees, active users, license utilization
- [ ] Tenant table: sortable by name, employee count, last activity
- [ ] Charts (optional): tenant growth over time, usage trend (Recharts)
- [ ] Add "Platform Admin" section to sidebar (visible only to platform admins)

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Platform/HrPortal.Tenancy/**` | Extend metrics |
| `src/backend/src/HrPortal.Api/Controllers/V1/PlatformAdminController.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/PlatformAdminDashboardTests.cs` | Create |
| `src/frontend/src/api/platformAdmin.ts` | Create |
| `src/frontend/src/pages/admin/*` | Create |
| `src/frontend/src/router/*` | Add admin routes + guard |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] Platform admin sees aggregate KPIs and tenant list
- [ ] Tenant users cannot access admin endpoints or pages
- [ ] Per-tenant drill-down shows module usage summary
- [ ] Cross-tenant queries do not bypass business entity scoping improperly
- [ ] `dotnet test` and `npm run build` green

## Next task

→ End of EPIC 9 — see `99_future_backlog.md` for index
