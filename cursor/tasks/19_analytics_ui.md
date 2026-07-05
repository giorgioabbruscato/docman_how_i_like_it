# TASK 19 — ANALYTICS UI

> Status: **COMPLETED**

Create analytics dashboard with bar, line, and pie charts, KPI cards, filters, and export buttons.

## Goal

Build supervisor analytics dashboard consuming chart and widget APIs from Tasks 14–15.

## Depends on

- Task 15 — Charts API (backend complete)
- Task 14 — Supervisor dashboard (backend complete)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| API contracts | `cursor/memory/api_contracts.md` | Analytics endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- API client: `src/frontend/src/api/analytics.ts`
- Chart library: Recharts (add dependency if not present)
- React Query for all chart/widget data
- Filters in Zustand or URL search params (prefer URL params for shareable links)
- Permission gate: page visible only with `analytics.read:team` or `analytics.read:tenant`
- Date range picker component (reuse or create shared)
- Loading skeletons for chart areas
- Export buttons link to reporting/export endpoints (Task 21) or time entry export

### Memory — source of truth (`cursor/memory/`)

- Reference `api_contracts.md`

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 14–15 backend are **COMPLETED**

### Before completing

1. Run `npm run build`
2. Verify against `02_frontend_quality_checks.md`
3. Mark task status **COMPLETED**

## Deliverables

### API client

- [x] `src/frontend/src/api/analytics.ts`
- [x] Methods: getSupervisorSummary, getChartData (per chart type)

### Types

- [x] `src/frontend/src/types/analytics.ts` — ChartData, DashboardSummary, widget DTOs

### Pages

- [x] `/analytics` — Supervisor dashboard

### Components

- [x] `KpiCards` — total hours, attendance rate, overtime, late arrivals
- [x] `HoursByProjectChart` — bar chart
- [x] `HoursByDepartmentChart` — bar chart
- [x] `HoursByEmployeeChart` — bar chart
- [x] `MonthlyTrendChart` — line chart
- [x] `AttendanceTrendChart` — line chart
- [x] `LeaveTrendChart` — line chart
- [x] `BudgetConsumptionChart` — pie or stacked bar
- [x] `AnalyticsFilters` — department, project, employee, date range pickers
- [x] `EmployeesWorkingNow` — widget list
- [x] `TopEmployeesWidget` / `TopProjectsWidget`
- [x] `ExportButtons` — quick export actions

### Filters

- [x] Department dropdown (from departments API)
- [x] Project dropdown (from projects API)
- [x] Employee dropdown (from employees API)
- [x] Date range picker with presets (today, this week, this month, custom)

### Navigation

- [x] Add "Analytics" to sidebar (gated by `analytics.read:team`)

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/api/analytics.ts` | Create |
| `src/frontend/src/types/analytics.ts` | Create |
| `src/frontend/src/pages/analytics/*` | Create |
| `src/frontend/src/components/analytics/*` | Create |
| `src/frontend/package.json` | Add recharts if needed |

## Acceptance criteria

- [x] All chart types render with real API data
- [x] KPI cards show supervisor summary
- [x] Filters update all charts/widgets
- [x] Responsive grid layout
- [x] Permission-gated page access
- [x] `npm run build` passes

## Next task

→ `20_notifications.md` — Notification triggers
