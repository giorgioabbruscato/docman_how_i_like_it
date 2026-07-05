# TASK 15 — CHARTS API

> Status: **COMPLETED**

Create analytics chart endpoints returning JSON ready for frontend chart libraries.

## Goal

Expose chart-specific endpoints with `{ labels, datasets }` shape compatible with Recharts/Chart.js.

## Depends on

- Task 13 — Analytics module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | DTO separation |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Chart endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Consistent chart DTO shape across all endpoints
- `[RequirePermission("analytics.read:team")]` or `analytics.read:tenant`
- Date range filters on all chart endpoints
- No raw SQL in controllers — analytics query handlers only

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with chart endpoints and response shapes

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

### Standard chart response shape

```json
{
  "labels": ["Jan", "Feb", "Mar"],
  "datasets": [
    { "label": "Hours", "data": [120, 150, 180] }
  ]
}
```

### API endpoints

| Method | Path | Chart type | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/analytics/charts/hours-by-project` | Bar | Hours grouped by project |
| GET | `/api/v1/analytics/charts/hours-by-department` | Bar | Hours grouped by department |
| GET | `/api/v1/analytics/charts/hours-by-employee` | Bar | Hours grouped by employee |
| GET | `/api/v1/analytics/charts/hours-by-month` | Line | Monthly hours trend |
| GET | `/api/v1/analytics/charts/attendance-trend` | Line | Daily attendance rate |
| GET | `/api/v1/analytics/charts/leave-trend` | Line | Monthly leave days |
| GET | `/api/v1/analytics/charts/budget-consumption` | Pie/Bar | Budget used vs remaining per project |

### Query parameters (all endpoints)

- [ ] `fromDate`, `toDate`
- [ ] `departmentId`, `projectId`, `employeeId` (optional filters)

### Tests

- [ ] Integration: each chart endpoint returns labels + datasets
- [ ] Integration: empty data returns empty arrays (not null)
- [ ] Unit: chart DTO mapping from KPI service results

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Analytics/Application/Dtos/ChartDtos.cs` | Create |
| `src/backend/src/Modules/HrPortal.Analytics/Application/Queries/ChartQueries.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/AnalyticsController.cs` | Add chart actions |
| `src/backend/tests/HrPortal.IntegrationTests/ChartsEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] All 7 chart endpoints documented in Swagger
- [ ] Consistent JSON shape for frontend consumption
- [ ] Filters applied correctly
- [ ] `dotnet test` green

## Next task

→ `16_projects_ui.md` — Projects frontend pages
