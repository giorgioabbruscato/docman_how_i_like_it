# TASK 17 — TIME TRACKING UI

> Status: **COMPLETED**

Create Time Tracking frontend with timer, manual entry, calendar views, and export.

## Goal

Build time tracking pages: start/stop timer, manual entry form, daily/weekly/monthly views, and export download.

## Depends on

- Task 08 — Export worked hours (backend complete)
- Task 06 — Timer (backend complete)
- Task 07 — Manual time entry (backend complete)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| API contracts | `cursor/memory/api_contracts.md` | Time entry endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- API client: `src/frontend/src/api/time-tracking.ts`
- Types: `src/frontend/src/types/time-entry.ts`
- Zustand store for active timer state (`stores/timer-store.ts`) with persist
- React Query for time entry lists and history
- Project/task selectors reuse Projects and Tasks API clients
- Live timer display updates every second when active
- Export triggers file download via blob response
- No direct axios in pages

### Memory — source of truth (`cursor/memory/`)

- Reference `api_contracts.md` for endpoint shapes

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 06–08 backend are **COMPLETED**

### Before completing

1. Run `npm run build`
2. Verify against `02_frontend_quality_checks.md`
3. Mark task status **COMPLETED**

## Deliverables

### API client

- [ ] `src/frontend/src/api/time-tracking.ts`
- [ ] Methods: getTimeEntries, startTimer, stopTimer, getActiveTimer, createManualEntry, exportTimeEntries

### Zustand store

- [ ] `stores/timer-store.ts` — active timer, elapsed seconds, isRunning
- [ ] Sync with backend on mount (GET active timer)
- [ ] Persist timer state across page refresh

### Pages

- [ ] `/time-tracking` — Main page with timer widget + entry list
- [ ] `/time-tracking/manual` — Manual entry form
- [ ] `/time-tracking/calendar` — Calendar view (daily/weekly/monthly toggle)

### Components

- [ ] `TimerWidget` — start/stop button, elapsed time display, project/task selector
- [ ] `ManualEntryForm` — date, project, task, hours, description
- [ ] `TimeEntryList` — paginated list with filters
- [ ] `TimeEntryCalendar` — calendar grid with daily totals
- [ ] `ExportButton` — format selector (CSV/XLSX/PDF) + date range
- [ ] `ProjectTaskSelector` — cascading project → task dropdown

### Views

- [ ] Daily view — entries for selected day
- [ ] Weekly view — 7-day summary with totals
- [ ] Monthly view — month grid with daily hour totals

### Navigation

- [ ] Add "Time Tracking" to sidebar (gated by `time_entry.read:self`)

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/api/time-tracking.ts` | Create |
| `src/frontend/src/types/time-entry.ts` | Create |
| `src/frontend/src/stores/timer-store.ts` | Create |
| `src/frontend/src/pages/time-tracking/*` | Create |
| `src/frontend/src/components/time-tracking/*` | Create |

## Acceptance criteria

- [ ] Start/stop timer works with live elapsed display
- [ ] Manual entry form submits successfully
- [ ] Calendar views show entries correctly
- [ ] Export downloads file in selected format
- [ ] Project/task selectors populated from API
- [ ] `npm run build` passes

## Next task

→ `18_attendance_ui.md` — Attendance frontend
