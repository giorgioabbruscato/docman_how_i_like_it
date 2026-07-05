# TASK 18 — ATTENDANCE UI

> Status: **COMPLETED**

Create attendance page with large check-in/check-out buttons, live timer, GPS status, and dashboard summary.

## Goal

Build a mobile-first attendance page with one-tap check-in/out, automatic GPS collection, live session timer, and today's summary.

## Depends on

- Task 12 — Attendance dashboard (backend complete)
- Task 10 — Check-in (backend complete)
- Task 11 — Check-out (backend complete)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| API contracts | `cursor/memory/api_contracts.md` | Attendance endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- API client: `src/frontend/src/api/attendance.ts`
- Collect GPS via `navigator.geolocation.getCurrentPosition` before check-in/out
- Send timezone via `Intl.DateTimeFormat().resolvedOptions().timeZone`
- Detect device/browser from `navigator.userAgent` (or use a lightweight parser)
- Large touch-friendly buttons (min 64px height)
- Live timer updates every second when session is open
- GPS status indicator: granted, denied, unavailable
- Graceful fallback when GPS denied — still allow check-in with warning
- Zustand for attendance session state (`stores/attendance-store.ts`)

### Memory — source of truth (`cursor/memory/`)

- Reference `api_contracts.md`

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 10–12 backend are **COMPLETED**

### Before completing

1. Run `npm run build`
2. Verify against `02_frontend_quality_checks.md`
3. Mark task status **COMPLETED**

## Deliverables

### API client

- [ ] `src/frontend/src/api/attendance.ts`
- [ ] Methods: checkIn, checkOut, getDashboard, getHistory

### Geolocation utility

- [ ] `src/frontend/src/lib/geolocation.ts` — getCurrentPosition wrapper with timeout and error handling

### Pages

- [ ] `/attendance` — Main attendance page

### Components

- [ ] `CheckInButton` — large green button, collects GPS + metadata on click
- [ ] `CheckOutButton` — large red button, same collection
- [ ] `LiveTimer` — elapsed time since check-in, updates every second
- [ ] `GpsStatus` — indicator showing GPS permission/state
- [ ] `TodaySummary` — check-in time, worked hours, status
- [ ] `AttendanceHistory` — recent sessions list from dashboard API
- [ ] `WeeklyMonthlyTotals` — from dashboard response

### State management

- [ ] `stores/attendance-store.ts` — current session, dashboard data
- [ ] React Query: `useAttendanceDashboard()`, `useAttendanceHistory()`

### UX rules

- [ ] Show Check-In when no open session; Check-Out when session open
- [ ] Disable button during API call (loading spinner)
- [ ] Show success toast on check-in/out
- [ ] Mobile-first responsive layout

### Navigation

- [ ] Add "Attendance" to sidebar (gated by `attendance_session.read:self`)
- [ ] Replace old attendance page if it exists

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/api/attendance.ts` | Create/update |
| `src/frontend/src/types/attendance.ts` | Create/update |
| `src/frontend/src/lib/geolocation.ts` | Create |
| `src/frontend/src/stores/attendance-store.ts` | Create |
| `src/frontend/src/pages/attendance/*` | Create/update |
| `src/frontend/src/components/attendance/*` | Create |

## Acceptance criteria

- [ ] Check-in/out buttons work with GPS collection
- [ ] Live timer runs during open session
- [ ] GPS status displayed correctly
- [ ] Today's summary and history shown
- [ ] Works on mobile viewport
- [ ] `npm run build` passes

## Next task

→ `19_analytics_ui.md` — Analytics dashboard frontend
