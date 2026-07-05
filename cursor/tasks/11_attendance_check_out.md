# TASK 11 — ATTENDANCE CHECK OUT

> Status: **COMPLETED**

Implement automatic check-out API that closes the open session and calculates worked time.

## Goal

Frontend sends GPS and metadata on check-out; backend closes `AttendanceSession`, computes `WorkedMinutes`, and stores checkout location.

## Depends on

- Task 10 — Attendance check-in

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Result<T> |
| Guardrails | `cursor/core/02_guardrails.md` | UTC |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Check-out endpoint |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Find open session for `TenantContext.EmployeeId`
- No open session → `404 Not Found`
- Set `CheckOut = DateTime.UtcNow`, `Status = Closed`
- `WorkedMinutes = (int)(CheckOut - CheckIn).TotalMinutes`
- CheckOut must be after CheckIn (domain guard)
- Audit: `attendance_session.check_out`
- `[RequirePermission("attendance_session.check_out:self")]`

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 10 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoint

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| POST | `/api/v1/attendance/check-out` | `attendance_session.check_out:self` | Close open session |

### Request body

```json
{
  "latitude": 45.4642,
  "longitude": 9.1900,
  "accuracy": 8.0,
  "device": "iPhone 15",
  "browser": "Safari 17"
}
```

### Response

```json
{
  "sessionId": "uuid",
  "checkIn": "2026-07-05T07:00:00Z",
  "checkOut": "2026-07-05T16:00:00Z",
  "workedMinutes": 540,
  "status": "Closed"
}
```

### Business rules

- [ ] Updates checkout GPS fields
- [ ] Computes and persists WorkedMinutes
- [ ] Returns session summary DTO

### Tests

- [ ] Integration: check-in → check-out flow
- [ ] Integration: check-out without open session returns 404
- [ ] Integration: WorkedMinutes calculated correctly
- [ ] Unit: domain Close() method

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Attendance/Application/Commands/CheckOutCommand.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/AttendanceController.cs` | Add check-out |
| `src/backend/tests/HrPortal.IntegrationTests/AttendanceCheckOutEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] Check-out closes session and calculates duration
- [ ] Checkout location stored
- [ ] Audit logged
- [ ] `dotnet test` green

## Next task

→ `12_attendance_dashboard.md` — Attendance dashboard queries
