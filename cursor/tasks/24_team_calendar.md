# TASK 24 — TEAM CALENDAR

> Status: **PENDING**

Create a shared team calendar showing leave, permissions, smart working days, and public holidays.

## Goal

Provide supervisors and employees a unified team calendar view aggregating absence and schedule data from Leave, Employees, and Departments modules.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 25 — Personal dashboard (recommended sequence)
- `HrPortal.Leave` — approved leave requests
- `HrPortal.Employees` — `IEmployeeLookup`
- `HrPortal.Departments` — department filtering

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | Tenant scope |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Module template |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| Domain model | `cursor/memory/domain_model.md` | CalendarEvent DTO |
| API contracts | `cursor/memory/api_contracts.md` | Calendar endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Add Calendar |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- New `HrPortal.Calendar` module — read-only aggregation, no duplicate leave data
- Calendar events sourced via provider interfaces (`ILeaveCalendarProvider`, etc.)
- Tenant public holidays stored per tenant (new `PublicHoliday` entity or tenant config)
- Smart working days: tenant-configurable weekdays or per-employee flag (document choice in ADR)
- Filters: department, employee, date range
- Permission: `calendar.read:team` for team view; `calendar.read:self` for own events only
- Use FullCalendar (or equivalent) — month/week/day views

### Memory — source of truth (`cursor/memory/`)

- Add `CalendarEvent`, `PublicHoliday` to `domain_model.md`
- Document endpoints in `api_contracts.md`
- Update `module_dependencies.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`
- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Leave, Employees, Departments modules and Task 22 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Run `npm run build`
3. Update memory files
4. Mark task status **COMPLETED**

## Deliverables

### Module scaffold

- [ ] Create `HrPortal.Calendar` under `src/backend/src/Modules/`
- [ ] Register in `Program.cs` after Leave module
- [ ] EF schema: `calendar` (PublicHoliday table only; events are aggregated DTOs)

### Domain: `PublicHoliday`

| Field | Type | Notes |
|-------|------|-------|
| Name | string | Holiday name |
| Date | DateOnly | Holiday date |
| IsRecurring | bool | Repeats yearly |
| CountryCode | string? | Optional ISO country |

### Calendar event DTO (not persisted)

| Field | Type | Notes |
|-------|------|-------|
| Id | string | Composite key (source + id) |
| Title | string | Display title |
| Start | DateOnly | Event start |
| End | DateOnly | Event end (inclusive) |
| Type | CalendarEventType | Leave, Permission, SmartWorking, PublicHoliday |
| EmployeeId | Guid? | Null for public holidays |
| EmployeeName | string? | Display |
| DepartmentId | Guid? | For filtering |
| Color | string? | Frontend hint |

**Enum:** `CalendarEventType` — Leave, Permission, SmartWorking, PublicHoliday

### Permissions

| Constant | Value |
|----------|-------|
| `CalendarReadSelf` | `calendar.read:self` |
| `CalendarReadTeam` | `calendar.read:team` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/calendar/events` | `calendar.read:self` OR `read:team` |
| GET | `/api/v1/calendar/holidays` | `calendar.read:team` |
| GET | `/api/v1/calendar/holidays/{id}` | `calendar.read:team` |
| POST | `/api/v1/calendar/holidays` | `calendar.manage:tenant` |
| PUT | `/api/v1/calendar/holidays/{id}` | `calendar.manage:tenant` |
| DELETE | `/api/v1/calendar/holidays/{id}` | `calendar.manage:tenant` |

### Query parameters (events)

- [ ] `fromDate`, `toDate` (required range, max 366 days)
- [ ] `departmentId`, `employeeId` (optional filters)
- [ ] Self scope returns only own events unless `read:team`

### Backend providers

- [ ] `ILeaveCalendarProvider` — approved leave from Leave module
- [ ] Aggregate permission/smart-working events (extend Leave types or tenant config — document in ADR)
- [ ] Merge public holidays from `PublicHoliday` table

### Frontend

- [ ] API client: `src/frontend/src/api/calendar.ts`
- [ ] Page: `/calendar/team` — FullCalendar with month/week/day toggle
- [ ] Filters: department dropdown, employee multi-select
- [ ] Event color coding by type
- [ ] Click event → detail popover (employee, dates, type, status)
- [ ] Admin: `/calendar/holidays` — CRUD public holidays (gated by `calendar.manage:tenant`)
- [ ] Add "Calendar" to sidebar (gated by `calendar.read:team`)

### Tests

- [ ] Integration: events endpoint aggregates leave + holidays
- [ ] Integration: department filter reduces events
- [ ] Integration: self vs team scope
- [ ] Integration: tenant isolation on holidays CRUD

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Calendar/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/CalendarController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/tests/HrPortal.IntegrationTests/TeamCalendarEndpointTests.cs` | Create |
| `src/frontend/src/api/calendar.ts` | Create |
| `src/frontend/src/pages/calendar/*` | Create |
| `src/frontend/package.json` | Add `@fullcalendar/react` if needed |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |
| `cursor/memory/architecture_decisions.md` | Add ADR for smart working source |

## Acceptance criteria

- [ ] Team calendar shows leave, holidays, and configured absence types
- [ ] Month/week/day views work with filters
- [ ] Public holidays CRUD for tenant admins
- [ ] Permission scoping enforced
- [ ] `dotnet test` and `npm run build` green

## Next task

→ `26_geofencing.md` — Geographic check-in zones
