# TASK 08 — EXPORT WORKED HOURS

> Status: **COMPLETED**

Implement export functionality for worked hours in Excel, CSV, and PDF formats.

## Goal

Allow employees to export their own time entries and supervisors to export team/tenant entries with flexible filters.

## Depends on

- Task 07 — Manual time entry (complete TimeEntry data model)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Interface abstraction |
| Guardrails | `cursor/core/02_guardrails.md` | No PII in logs |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Export endpoint |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Employee with `time_entry.read:self` exports **only own** entries
- Supervisor with `time_entry.read:team` or `time_entry.read:tenant` exports broader scope
- Export logic behind `ITimeEntryExportService` interface
- Approved NuGet packages (add during execution):
  - Excel: `ClosedXML`
  - CSV: `CsvHelper` or built-in
  - PDF: `QuestPDF`
- Return `FileContentResult` with correct MIME types
- No stack traces in error responses

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with export endpoint

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 07 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoint

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/time-entries/export` | `time_entry.read:self` OR `read:team` OR `read:tenant` | Export file |

### Query parameters

| Param | Type | Description |
|-------|------|-------------|
| `format` | string | `csv`, `xlsx`, `pdf` (required) |
| `employeeId` | Guid? | Filter by employee (supervisor only) |
| `projectId` | Guid? | Filter by project |
| `fromDate` | DateOnly? | Start of date range |
| `toDate` | DateOnly? | End of date range |
| `month` | int? | Filter by month (1-12) |
| `year` | int? | Filter by year |

### Export columns

- [ ] Date
- [ ] Employee name (supervisor exports)
- [ ] Project name
- [ ] Task title (if linked)
- [ ] Hours (WorkedMinutes / 60)
- [ ] Description
- [ ] Billable flag

### Permissions

| Constant | Value |
|----------|-------|
| `TimeEntryExportTeam` | `time_entry.export:team` |
| `TimeEntryExportTenant` | `time_entry.export:tenant` |

### Tests

- [ ] Integration: CSV export returns valid content-type
- [ ] Integration: self scope limits to own entries
- [ ] Integration: supervisor export includes team entries
- [ ] Integration: date range filter applied
- [ ] Unit: export service row mapping

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.TimeTracking/Application/ITimeEntryExportService.cs` | Create |
| `src/backend/src/Modules/HrPortal.TimeTracking/Infrastructure/Export/TimeEntryExportService.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TimeEntriesController.cs` | Add export action |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add export constants |
| `src/backend/tests/HrPortal.IntegrationTests/TimeEntryExportEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] CSV, XLSX, and PDF exports work
- [ ] Filters: employee, project, date range, month, year
- [ ] Permission scoping enforced (self vs supervisor)
- [ ] `dotnet test` green

## Next task

→ `09_attendance_redesign.md` — Attendance 2.0 session-based model
