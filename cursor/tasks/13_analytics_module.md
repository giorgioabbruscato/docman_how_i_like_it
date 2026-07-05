# TASK 13 — ANALYTICS MODULE

> Status: **COMPLETED**

Create the read-only `HrPortal.Analytics` module with KPI aggregation services.

## Goal

Build analytics services computing KPIs across TimeTracking, Attendance, Leave, Projects, and Employees modules.

## Depends on

- Task 05 — Time Tracking module
- Task 09 — Attendance redesign
- Existing: Leave, Employees, Departments, Projects modules

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | No cross-module DbSet |
| Guardrails | `cursor/core/02_guardrails.md` | Read-only module |
| Architecture | `cursor/core/03_architecture.md` | Module boundaries |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Module deps | `cursor/memory/module_dependencies.md` | Analytics dependencies |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- **Read-only module** — no writes, no new business entities (query DTOs only)
- Cross-module data via public lookup interfaces and read repositories exposed for analytics OR dedicated `IAnalyticsDataProvider` per source module
- Never query another module's DbSet directly — use interfaces defined in Application layer
- All queries tenant-scoped
- Return DTOs, not domain entities
- `[RequirePermission("analytics.read:team")]` or `analytics.read:tenant`

### Memory — source of truth (`cursor/memory/`)

- Add Analytics section to `module_dependencies.md`
- Document KPI DTOs in `domain_model.md` (as query models, not entities)

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Time Tracking and Attendance tasks are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.Analytics` under `src/backend/src/Modules/`
- [ ] Register in `Program.cs`
- [ ] No EF entities — query services only

### KPI services

| KPI | Description |
|-----|-------------|
| TotalWorkedHours | Sum of TimeEntry minutes in range |
| HoursPerEmployee | Breakdown by employee |
| HoursPerDepartment | Via employee → department join |
| HoursPerProject | Breakdown by project |
| HoursPerCustomer | Via project customer name |
| MonthlyTrend | Hours per month |
| DailyTrend | Hours per day |
| AverageHoursPerDay | Mean daily hours in range |
| AttendanceRate | Present sessions / expected workdays |
| LeaveRate | Approved leave days / workdays |
| OvertimeHours | Hours beyond configured threshold (8h/day default) |
| LateCheckIns | Sessions where check-in after configured time |

### Permissions

| Constant | Value |
|----------|-------|
| `AnalyticsReadTeam` | `analytics.read:team` |
| `AnalyticsReadTenant` | `analytics.read:tenant` |

### Cross-module interfaces

- [ ] `ITimeEntryAnalyticsProvider` (TimeTracking exposes)
- [ ] `IAttendanceAnalyticsProvider` (Attendance exposes)
- [ ] `ILeaveAnalyticsProvider` (Leave exposes)
- [ ] Or consolidated lookup pattern per guardrails

### Tests

- [ ] Unit: KPI calculation with mocked providers
- [ ] Integration: KPI endpoint returns expected shape

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Analytics/**` | Create |
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/ITimeEntryAnalyticsProvider.cs` | Create |
| `src/backend/src/Modules/HrPortal.Attendance/Application/IAttendanceAnalyticsProvider.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `cursor/memory/module_dependencies.md` | Update |
| `src/backend/tests/HrPortal.UnitTests/Analytics/*` | Create |

## Acceptance criteria

- [ ] Analytics module registered
- [ ] All 12 KPIs computable via service methods
- [ ] No direct cross-module DbSet access
- [ ] Unit tests for calculation logic
- [ ] `dotnet test` green

## Next task

→ `14_supervisor_dashboard.md` — Supervisor dashboard widget endpoints
