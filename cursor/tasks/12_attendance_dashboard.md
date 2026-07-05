# TASK 12 — ATTENDANCE DASHBOARD

> Status: **COMPLETED**

Create attendance dashboard query endpoints for today, weekly, monthly totals, current session, and history.

## Goal

Provide read-only APIs powering the attendance dashboard UI with employee self-scope and supervisor team scope.

## Depends on

- Task 11 — Attendance check-out

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Read-only queries |
| Guardrails | `cursor/core/02_guardrails.md` | ApplyTenantScope |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Dashboard endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Self scope: queries default to `TenantContext.EmployeeId`
- Team/tenant scope for supervisors via `[RequireAnyPermission]`
- All aggregations tenant-scoped
- No business logic in controllers — query handlers only
- UTC date boundaries for today/week/month calculations

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with dashboard endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 11 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/attendance/dashboard` | `attendance_session.read:self` OR `read:team` | Dashboard summary |
| GET | `/api/v1/attendance/history` | `attendance_session.read:self` OR `read:team` | Recent sessions |

### Dashboard response

```json
{
  "todayCheckIn": "2026-07-05T07:00:00Z",
  "todayCheckOut": null,
  "todayWorkedMinutes": 240,
  "currentSession": { "id": "uuid", "checkIn": "...", "status": "Open" },
  "weeklyTotalMinutes": 1920,
  "monthlyTotalMinutes": 8640
}
```

### History query params

- [ ] `page`, `pageSize` (default 10 recent)
- [ ] `fromDate`, `toDate` optional filters

### Business rules

- [ ] `todayCheckIn`/`todayCheckOut` from today's sessions (UTC date)
- [ ] `currentSession` = open session if exists, else null
- [ ] Weekly total: Monday–Sunday or rolling 7 days (document choice)
- [ ] Monthly total: current calendar month

### Tests

- [ ] Integration: dashboard with open session
- [ ] Integration: dashboard after check-out shows totals
- [ ] Integration: history pagination
- [ ] Integration: self vs team scope

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Attendance/Application/Queries/GetAttendanceDashboardQuery.cs` | Create |
| `src/backend/src/Modules/HrPortal.Attendance/Application/Queries/GetAttendanceHistoryQuery.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/AttendanceController.cs` | Add dashboard actions |
| `src/backend/tests/HrPortal.IntegrationTests/AttendanceDashboardEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] Dashboard returns today, weekly, monthly, current session
- [ ] History endpoint paginated
- [ ] Permission scoping works
- [ ] `dotnet test` green

## Next task

→ `13_analytics_module.md` — Analytics KPI services
