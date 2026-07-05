# TASK 07 — MANUAL TIME ENTRY

> Status: **PENDING**

Implement manual time entry with date, project, task, hours, and description.

## Goal

Allow employees to log worked time manually with strict validation: max 24 hours/day, no overlapping intervals, no future dates.

## Depends on

- Task 05 — Time Tracking module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Validation mandatory |
| Guardrails | `cursor/core/02_guardrails.md` | FluentValidation |
| TDD | `cursor/core/01_tdd.md` | Validator + integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Manual entry endpoint |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Maximum 24 hours (1440 minutes) per employee per calendar day (UTC date)
- No overlapping time intervals for same employee
- No future dates — entry date/time must be <= UtcNow
- EmployeeId from `TenantContext.EmployeeId`
- Validate project/task via lookup interfaces
- Audit: `time_entry.manual_created`

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with manual entry endpoint

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

### API endpoint

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| POST | `/api/v1/time-entries/manual` | `time_entry.create:self` | Create manual entry |

### Request body

```json
{
  "date": "2026-07-05",
  "projectId": "uuid",
  "taskId": "uuid",
  "hours": 2.5,
  "description": "Code review",
  "billable": true
}
```

### Business rules

- [ ] Convert `hours` to `WorkedMinutes` (hours × 60, rounded)
- [ ] Derive `StartTime`/`EndTime` from date + hours (start at date 09:00 UTC or store date-only + minutes)
- [ ] Reject if total minutes for employee on that date would exceed 1440
- [ ] Reject if interval overlaps existing entries
- [ ] Reject if date is in the future
- [ ] Reject if hours <= 0 or > 24

### Validation

- [ ] `CreateManualTimeEntryRequestValidator` — all rules above

### Tests

- [ ] Unit: 24h/day limit calculation
- [ ] Unit: overlap detection
- [ ] Unit: future date rejection
- [ ] Integration: happy path manual entry
- [ ] Integration: exceed daily limit rejected
- [ ] Integration: overlap rejected

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/Commands/CreateManualTimeEntryCommand.cs` | Create |
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/Validators/ManualTimeEntryValidators.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TimeEntriesController.cs` | Add manual action |
| `src/backend/tests/HrPortal.UnitTests/TimeTracking/ManualTimeEntryTests.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/ManualTimeEntryEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] Manual entry creates TimeEntry with correct minutes
- [ ] 24h/day, overlap, and future date rules enforced
- [ ] Audit logged
- [ ] `dotnet test` green

## Next task

→ `08_export_worked_hours.md` — Export to Excel, CSV, PDF
