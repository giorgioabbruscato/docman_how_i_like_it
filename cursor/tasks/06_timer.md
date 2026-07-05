# TASK 06 — TIMER

> Status: **PENDING**

Implement timer functionality with start/stop endpoints.

## Goal

Allow employees to start and stop a running timer. Only one active timer per employee; duration calculated automatically; overlapping sessions prevented.

## Depends on

- Task 05 — Time Tracking module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Result<T> pattern |
| Guardrails | `cursor/core/02_guardrails.md` | UTC only |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Timer endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Only one active timer per employee (`EndTime == null`)
- Starting a timer when one is active returns `409 Conflict`
- Stopping sets `EndTime = DateTime.UtcNow` and computes `WorkedMinutes`
- Prevent overlapping intervals with existing closed entries
- EmployeeId from `TenantContext.EmployeeId` — never from request body
- Audit: `time_entry.timer_started`, `time_entry.timer_stopped`

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with timer endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 05 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| POST | `/api/v1/timer/start` | `time_entry.create:self` | Start timer |
| POST | `/api/v1/timer/stop` | `time_entry.update:self` | Stop active timer |
| GET | `/api/v1/timer/active` | `time_entry.read:self` | Get current active timer |

### POST /timer/start request body

```json
{
  "projectId": "uuid",
  "taskId": "uuid",
  "description": "Working on feature X",
  "billable": true
}
```

### Business rules

- [ ] Creates `TimeEntry` with `StartTime = UtcNow`, `EndTime = null`
- [ ] Rejects if employee already has active timer
- [ ] Rejects if new interval would overlap existing closed entries
- [ ] Stop finds active entry for current employee, sets end time, calculates minutes
- [ ] Stop with no active timer returns `404 Not Found`

### Repository

- [ ] `GetActiveTimerAsync(employeeId)` query
- [ ] `HasOverlappingEntryAsync(employeeId, start, end?)` query

### Tests

- [ ] Integration: start → active GET → stop flow
- [ ] Integration: double start rejected
- [ ] Integration: stop without active timer returns 404
- [ ] Integration: overlap detection
- [ ] Unit: WorkedMinutes calculation on stop

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/Commands/StartTimerCommand.cs` | Create |
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/Commands/StopTimerCommand.cs` | Create |
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/Queries/GetActiveTimerQuery.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TimerController.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/TimerEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] One active timer per employee enforced
- [ ] Duration auto-calculated on stop
- [ ] Overlapping sessions prevented
- [ ] All timestamps UTC
- [ ] `dotnet test` green

## Next task

→ `07_manual_time_entry.md` — Manual time entry with validation rules
