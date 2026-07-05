# TASK 10 — ATTENDANCE CHECK IN

> Status: **PENDING**

Implement automatic check-in API collecting GPS, device, browser, timezone, and timestamp.

## Goal

Single-button check-in: frontend sends collected metadata; backend creates an open `AttendanceSession`.

## Depends on

- Task 09 — Attendance redesign

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Thin controllers |
| Guardrails | `cursor/core/02_guardrails.md` | UTC timestamps |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Check-in endpoint |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- EmployeeId from `TenantContext.EmployeeId`
- Reject check-in if employee already has open session (`409 Conflict`)
- Store `CheckIn` as UTC (convert from client timezone if provided)
- Capture IP from `HttpContext` in controller/middleware — pass to service, not read in service
- Audit: `attendance_session.check_in`
- `[RequirePermission("attendance_session.check_in:self")]`

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with check-in endpoint

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 09 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoint

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| POST | `/api/v1/attendance/check-in` | `attendance_session.check_in:self` | Create open session |

### Request body

```json
{
  "latitude": 45.4642,
  "longitude": 9.1900,
  "accuracy": 12.5,
  "timezone": "Europe/Rome",
  "device": "iPhone 15",
  "browser": "Safari 17"
}
```

### Business rules

- [ ] Creates `AttendanceSession` with `Status = Open`, `CheckOut = null`
- [ ] Server sets `CheckIn = DateTime.UtcNow` (client date/time used for validation only)
- [ ] Reject if open session exists for employee
- [ ] IP address captured at API layer and passed to command
- [ ] GPS fields optional (nullable) — check-in allowed without GPS but log warning

### Tests

- [ ] Integration: check-in creates open session
- [ ] Integration: double check-in rejected
- [ ] Integration: 403 without permission
- [ ] Unit: command handler with mocked repository

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Attendance/Application/Commands/CheckInCommand.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/AttendanceController.cs` | Refactor check-in |
| `src/backend/tests/HrPortal.IntegrationTests/AttendanceCheckInEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] Check-in endpoint creates open AttendanceSession
- [ ] GPS, device, browser, IP stored
- [ ] Duplicate check-in prevented
- [ ] Audit logged
- [ ] `dotnet test` green

## Next task

→ `11_attendance_check_out.md` — Automatic check-out API
