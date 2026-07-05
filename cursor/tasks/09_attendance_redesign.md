# TASK 09 — ATTENDANCE REDESIGN

> Status: **COMPLETED**

Replace daily `AttendanceRecord` with session-based `AttendanceSession` entity.

## Goal

Redesign the Attendance module for session-based check-in/check-out with GPS, device metadata, and automatic duration calculation.

## Depends on

- `HrPortal.Employees` — `IEmployeeLookup`
- Existing `HrPortal.Attendance` module (to be refactored)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | Migrations required |
| TDD | `cursor/core/01_tdd.md` | Update existing tests |
| Architecture | `cursor/core/03_architecture.md` | Module refactor |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | Replace AttendanceRecord |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Add ADR for Attendance 2.0 |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Introduce `AttendanceSession` — do not extend `AttendanceRecord`
- Deprecate `AttendanceRecord` and old clock-in/out endpoints
- Migration strategy: add new table; **clean break** (no data migration) unless business requires otherwise — document choice in ADR
- Update integration tests that reference old Attendance API
- `WorkedMinutes` computed on check-out: `(CheckOut - CheckIn).TotalMinutes`
- Tenant isolation via `ApplyTenantScope`
- Audit on session create/close

### Memory — source of truth (`cursor/memory/`)

- Replace `AttendanceRecord` section with `AttendanceSession` in `domain_model.md`
- Write ADR-013 (or next available) for Attendance 2.0 in `architecture_decisions.md`
- Update ERD in `domain_model.md`

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Review existing `HrPortal.Attendance` module

### Before completing

1. Run `dotnet test` — update/remove old attendance tests
2. Update memory + ADR
3. Mark task status **COMPLETED**

## Deliverables

### Domain entity: `AttendanceSession`

| Field | Type | Notes |
|-------|------|-------|
| EmployeeId | Guid | FK via IEmployeeLookup |
| CheckIn | DateTime | UTC |
| CheckOut | DateTime? | UTC, null = open session |
| LatitudeCheckIn | double? | GPS |
| LongitudeCheckIn | double? | GPS |
| LatitudeCheckOut | double? | GPS |
| LongitudeCheckOut | double? | GPS |
| AccuracyCheckIn | double? | Meters |
| AccuracyCheckOut | double? | Meters |
| IPAddress | string? | Caller IP |
| Device | string? | User agent device |
| Browser | string? | User agent browser |
| WorkedMinutes | int? | Computed on check-out |
| Status | AttendanceSessionStatus | Open, Closed, AutoClosed |

### Enum: `AttendanceSessionStatus`

- `Open` — checked in, not yet out
- `Closed` — normal check-out
- `AutoClosed` — system closed (future: end-of-day job)

### Migration

- [ ] EF migration: create `attendance_sessions` table
- [ ] Mark old `attendance_records` as deprecated (optional: drop in same migration or follow-up)
- [ ] Update `AttendanceRecordConfiguration` removal

### Permissions (update existing)

| Constant | Value |
|----------|-------|
| `AttendanceSessionReadSelf` | `attendance_session.read:self` |
| `AttendanceSessionReadTeam` | `attendance_session.read:team` |
| `AttendanceSessionCheckInSelf` | `attendance_session.check_in:self` |
| `AttendanceSessionCheckOutSelf` | `attendance_session.check_out:self` |

Keep legacy `attendance.*` permissions mapped during transition or replace in `SystemRoleTemplates`.

### Repository + service

- [ ] `IAttendanceSessionRepository` with `GetOpenSessionAsync(employeeId)`
- [ ] Refactor `AttendanceService` → `AttendanceSessionService`
- [ ] Remove or obsolete old `AttendanceRecord` domain logic

### Tests

- [ ] Domain: WorkedMinutes calculation
- [ ] Domain: one open session per employee rule
- [ ] Update/remove old `AttendanceEndpointTests`

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Attendance/Domain/AttendanceSession.cs` | Create |
| `src/backend/src/Modules/HrPortal.Attendance/Domain/AttendanceRecord.cs` | Deprecate/remove |
| `src/backend/src/Modules/HrPortal.Attendance/Application/*` | Refactor |
| `src/backend/src/Modules/HrPortal.Attendance/Infrastructure/Persistence/*` | Update |
| `src/backend/tests/HrPortal.UnitTests/Attendance/*` | Update |
| `src/backend/tests/HrPortal.IntegrationTests/AttendanceEndpointTests.cs` | Update |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/architecture_decisions.md` | Add ADR |

## Acceptance criteria

- [ ] `AttendanceSession` entity with EF migration
- [ ] Old `AttendanceRecord` deprecated/removed
- [ ] Domain rules tested
- [ ] ADR documented
- [ ] `dotnet test` green (updated tests)

## Next task

→ `10_attendance_check_in.md` — Automatic check-in API
