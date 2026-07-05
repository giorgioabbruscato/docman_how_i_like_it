# TASK 25 — PERSONAL DASHBOARD

> Status: **PENDING**

Create a unified employee home page aggregating worked hours, leave balance, documents, tasks, and notifications.

## Goal

Build a personal dashboard as the authenticated home page, composing existing APIs into widget cards with permission-aware visibility.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 16 — Projects UI
- Task 17 — Time Tracking UI
- Task 18 — Attendance UI
- Task 19 — Analytics UI (optional widgets for self-scope data)
- Task 20 — Notifications

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| API contracts | `cursor/memory/api_contracts.md` | Existing endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- Frontend-first: reuse existing API clients where possible
- Optional backend read service (`HrPortal.Dashboard`) only if aggregation requires N+1 calls — prefer parallel React Query fetches first
- React Query for all widget data (`useQueries` or parallel hooks)
- Permission gates per widget — hide widgets user cannot access
- Loading skeletons per widget (independent failure — one widget error must not break page)
- Responsive grid: 1 column mobile, 2–3 columns desktop
- Route: `/dashboard` as authenticated home; redirect `/` → `/dashboard` when logged in

### Memory — source of truth (`cursor/memory/`)

- Document optional dashboard read endpoint in `api_contracts.md` if added
- No new domain entities unless read service added

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 16–20 and 22 are **COMPLETED**

### Before completing

1. Run `npm run build`
2. Verify against `02_frontend_quality_checks.md`
3. Update `api_contracts.md` if new read endpoint added
4. Mark task status **COMPLETED**

## Deliverables

### Widgets

| Widget | Data source | Permission gate |
|--------|-------------|-----------------|
| Worked hours this week | Time entries API / attendance dashboard | `time_entry.read:self` |
| Remaining leave balance | Leave requests API | `leave.read:self` |
| Recent documents | Documents API | `document.read:self` |
| Assigned tasks | Tasks API | `task.read:self` |
| Notifications | Notifications API | authenticated |
| Today's attendance | Attendance dashboard API | `attendance_session.read:self` |
| Quick actions | — | check-in, start timer, request leave links |

### Pages

- [ ] `/dashboard` — Personal dashboard home
- [ ] Redirect authenticated `/` to `/dashboard`

### Components

- [ ] `DashboardGrid` — responsive widget layout
- [ ] `WorkedHoursWidget` — weekly summary with link to time tracking
- [ ] `LeaveBalanceWidget` — remaining days by leave type
- [ ] `DocumentsWidget` — last 5 documents
- [ ] `TasksWidget` — assigned tasks with status badges
- [ ] `NotificationsWidget` — unread count + recent items
- [ ] `AttendanceTodayWidget` — check-in status + quick action
- [ ] `QuickActionsBar` — shortcuts to attendance, timer, leave request
- [ ] `WidgetSkeleton` — shared loading state
- [ ] `WidgetErrorBoundary` — per-widget error fallback

### API client (optional)

- [ ] `src/frontend/src/api/dashboard.ts` — only if backend read service added
- [ ] Otherwise compose existing clients: `timeEntries`, `leave`, `documents`, `tasks`, `notifications`, `attendance`

### Navigation

- [ ] Add "Dashboard" as first sidebar item (all authenticated users)
- [ ] Set as default post-login route

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/pages/dashboard/*` | Create |
| `src/frontend/src/components/dashboard/*` | Create |
| `src/frontend/src/api/dashboard.ts` | Create (optional) |
| `src/frontend/src/router/*` | Update default route |
| `src/backend/src/Modules/HrPortal.Dashboard/**` | Create (optional read service) |
| `cursor/memory/api_contracts.md` | Update if read service added |

## Acceptance criteria

- [ ] Dashboard loads all accessible widgets in parallel
- [ ] Widgets hidden when user lacks permission
- [ ] Failed widget shows error state without breaking page
- [ ] Responsive layout on mobile and desktop
- [ ] Quick actions navigate to correct modules
- [ ] `npm run build` passes

## Next task

→ `24_team_calendar.md` — Shared team calendar view
