# TASK 14 — SUPERVISOR DASHBOARD

> Status: **COMPLETED**

Create supervisor dashboard API endpoints with widget data and filters.

## Goal

Expose aggregated widget endpoints for supervisors: employees working now, today's attendance, projects, hours, top employees/projects, budget usage, late arrivals, overtime.

## Depends on

- Task 13 — Analytics module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Thin controllers |
| Guardrails | `cursor/core/02_guardrails.md` | Tenant scope |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Dashboard endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- `[RequirePermission("analytics.read:team")]` or `analytics.read:tenant`
- Filters: department, project, employee, date range — all optional query params
- Widget queries delegate to Analytics services from Task 13
- No N+1 queries — use aggregated SQL where possible
- Return DTOs optimized for frontend cards

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with supervisor dashboard section

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 13 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Widget |
|--------|------|------------|--------|
| GET | `/api/v1/analytics/supervisor/summary` | `analytics.read:team` | All widgets in one response |
| GET | `/api/v1/analytics/supervisor/employees-working` | `analytics.read:team` | Active timers + open attendance |
| GET | `/api/v1/analytics/supervisor/attendance-today` | `analytics.read:team` | Today's check-ins |
| GET | `/api/v1/analytics/supervisor/top-employees` | `analytics.read:team` | Top N by hours |
| GET | `/api/v1/analytics/supervisor/top-projects` | `analytics.read:team` | Top N by hours |
| GET | `/api/v1/analytics/supervisor/budget-usage` | `analytics.read:tenant` | Project budget vs spent |
| GET | `/api/v1/analytics/supervisor/late-arrivals` | `analytics.read:team` | Late check-ins today |
| GET | `/api/v1/analytics/supervisor/overtime` | `analytics.read:team` | Overtime in range |

### Shared query parameters

| Param | Type | Description |
|-------|------|-------------|
| `departmentId` | Guid? | Filter by department |
| `projectId` | Guid? | Filter by project |
| `employeeId` | Guid? | Filter by employee |
| `fromDate` | DateOnly? | Range start |
| `toDate` | DateOnly? | Range end |

### Widget: Employees working now

- [ ] Employees with open attendance session OR active timer
- [ ] Include name, project (if timer), check-in time

### Widget: Budget usage

- [ ] Per project: BudgetHours vs SpentHours (from TimeEntry + ProjectTask)
- [ ] BudgetCost vs actual cost (hours × member rates)

### Tests

- [ ] Integration: summary endpoint returns all widget sections
- [ ] Integration: filters reduce result set
- [ ] Integration: 403 for employee role without analytics permission

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Analytics/Application/Queries/SupervisorDashboardQueries.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/AnalyticsController.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/SupervisorDashboardEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] All widget endpoints return valid JSON
- [ ] Filters work across endpoints
- [ ] Permission scoping enforced
- [ ] `dotnet test` green

## Next task

→ `15_charts_api.md` — Chart-ready JSON endpoints
