# TASK 23 — TIMESHEET APPROVAL

> Status: **COMPLETED**

Extend Time Tracking with timesheet submission and supervisor approval before hours count in analytics and billing.

## Goal

Employees submit worked hours for a period; supervisors approve or reject before entries are accounted in analytics and billing exports.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Tasks 05–08 — Time Tracking module (CRUD, timer, manual entry, export)
- Task 13 — Analytics module (approved hours feed analytics)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | UTC timestamps |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Module template |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| Domain model | `cursor/memory/domain_model.md` | Add entities |
| API contracts | `cursor/memory/api_contracts.md` | Timesheet endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Update TimeTracking |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- All timestamps stored as UTC
- Validate EmployeeId via `IEmployeeLookup`
- Self scope: employees submit own timesheets; supervisors approve team scope
- Only **Approved** submissions feed analytics and billing exports
- Lightweight CQRS structure
- Audit on submit/approve/reject
- Task 29 (Workflows) may generalize this later — keep approval logic replaceable behind an interface

### Memory — source of truth (`cursor/memory/`)

- Add `TimesheetSubmission`, `TimesheetApproval` to `domain_model.md`
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
2. Verify Tasks 05–08, 13, and 22 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Run `npm run build`
3. Update memory files
4. Mark task status **COMPLETED**

## Deliverables

### Domain entities

#### `TimesheetSubmission`

| Field | Type | Notes |
|-------|------|-------|
| EmployeeId | Guid | FK via IEmployeeLookup |
| PeriodStart | DateOnly | Submission period start |
| PeriodEnd | DateOnly | Submission period end |
| Status | TimesheetStatus | Draft, Submitted, Approved, Rejected |
| SubmittedAt | DateTime? | UTC when submitted |
| TotalWorkedMinutes | int | Sum of linked time entries |
| Notes | string? | Employee notes |

#### `TimesheetApproval`

| Field | Type | Notes |
|-------|------|-------|
| TimesheetSubmissionId | Guid | FK |
| ApproverEmployeeId | Guid | Supervisor who acted |
| Decision | ApprovalDecision | Approved, Rejected |
| DecisionAt | DateTime | UTC |
| Comment | string? | Optional rejection reason |

**Enums:** `TimesheetStatus` (Draft, Submitted, Approved, Rejected), `ApprovalDecision` (Approved, Rejected)

### Permissions

| Constant | Value |
|----------|-------|
| `TimesheetSubmitSelf` | `timesheet.submit:self` |
| `TimesheetReadSelf` | `timesheet.read:self` |
| `TimesheetReadTeam` | `timesheet.read:team` |
| `TimesheetApproveTeam` | `timesheet.approve:team` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/timesheets` | `timesheet.read:self` OR `read:team` |
| GET | `/api/v1/timesheets/{id}` | Same as list |
| POST | `/api/v1/timesheets` | `timesheet.submit:self` |
| POST | `/api/v1/timesheets/{id}/submit` | `timesheet.submit:self` |
| POST | `/api/v1/timesheets/{id}/approve` | `timesheet.approve:team` |
| POST | `/api/v1/timesheets/{id}/reject` | `timesheet.approve:team` |

### List filters

- [ ] `employeeId`, `status`, `fromDate`, `toDate`
- [ ] Pagination: `page`, `pageSize`
- [ ] Self scope auto-filters to `ctx.EmployeeId`

### Backend

- [ ] Extend `HrPortal.TimeTracking` with submission/approval commands and queries
- [ ] Link submission to `TimeEntry` records in period (snapshot or FK list)
- [ ] Analytics queries exclude non-approved hours (or filter by approved submission)
- [ ] Integration tests: submit, approve, reject, scope self/team, tenant isolation

### Frontend

- [ ] API client: `src/frontend/src/api/timesheets.ts`
- [ ] Employee page: `/time-tracking/timesheets` — create/submit timesheet for period
- [ ] Supervisor page: `/time-tracking/timesheets/approvals` — pending queue with approve/reject
- [ ] Permission gates: `timesheet.submit:self`, `timesheet.approve:team`
- [ ] React Query for list and detail

### Tests

- [ ] Domain: status transitions (Draft → Submitted → Approved/Rejected)
- [ ] Integration: CRUD + approval flow + scope filtering
- [ ] Integration: tenant isolation
- [ ] Integration: analytics excludes unapproved hours

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.TimeTracking/**` | Extend |
| `src/backend/src/HrPortal.Api/Controllers/V1/TimesheetsController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/tests/HrPortal.IntegrationTests/TimesheetApprovalEndpointTests.cs` | Create |
| `src/frontend/src/api/timesheets.ts` | Create |
| `src/frontend/src/pages/time-tracking/timesheets/*` | Create |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |

## Acceptance criteria

- [ ] Employees can submit timesheets for a date range
- [ ] Supervisors can approve/reject team submissions
- [ ] Only approved hours appear in analytics/export
- [ ] Permission scoping (self/team) works
- [ ] UI pages functional with permission gates
- [ ] `dotnet test` and `npm run build` green

## Next task

→ `25_personal_dashboard.md` — Unified employee home page
