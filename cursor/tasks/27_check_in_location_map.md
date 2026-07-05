# TASK 27 — CHECK-IN LOCATION MAP

> Status: **PENDING**

Display check-in and check-out locations on a map in the attendance detail and history views.

## Goal

Show a map pin (and optional screenshot-style static view) for GPS coordinates captured during attendance check-in and check-out, using OpenStreetMap without requiring a Google Maps API key.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 26 — Geofencing (recommended sequence)
- Task 18 — Attendance UI
- Task 09 — Attendance redesign (GPS fields on `AttendanceSession`)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | API client pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | Page structure |
| Domain model | `cursor/memory/domain_model.md` | AttendanceSession GPS fields |
| API contracts | `cursor/memory/api_contracts.md` | Attendance endpoints |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + frontend eval)

- Use Leaflet + OpenStreetMap tiles — no Google Maps API key
- Add `leaflet` and `@types/leaflet` dependencies
- Reusable `AttendanceLocationMap` component
- Show check-in pin (green) and check-out pin (red) when coordinates exist
- Fit map bounds to include both pins when both present
- Graceful fallback when coordinates missing: "Location not recorded" message
- Optional: overlay geofence zones from Task 26 API (read-only circles)
- No backend changes required unless attendance DTO lacks lat/lng — verify and extend DTO only if needed
- Lazy-load map component (code split) to avoid bundle bloat on main attendance page

### Memory — source of truth (`cursor/memory/`)

- Verify `AttendanceSession` GPS fields documented in `domain_model.md`
- Update `api_contracts.md` only if DTO extended

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 09, 18, and 22 are **COMPLETED**
3. Confirm attendance API returns check-in/out latitude and longitude

### Before completing

1. Run `npm run build`
2. Verify against `02_frontend_quality_checks.md`
3. Mark task status **COMPLETED**

## Deliverables

### Component: `AttendanceLocationMap`

- [ ] Props: `checkInLat`, `checkInLng`, `checkOutLat?`, `checkOutLng?`, `geofenceZones?`
- [ ] OpenStreetMap tile layer (`https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png`)
- [ ] Markers with popups showing timestamp and coordinates
- [ ] Auto-fit bounds when two pins present
- [ ] Single pin centered with default zoom 15
- [ ] Accessible: aria-label on map container, text fallback for screen readers

### Integration points

- [ ] Attendance session detail drawer/modal — map section below session info
- [ ] Attendance history list — expand row or detail link shows map
- [ ] Optional geofence zone circles (from Task 26 zones API) as read-only overlay

### Types

- [ ] Extend `src/frontend/src/types/attendance.ts` if GPS fields missing from DTO

### Styling

- [ ] Import Leaflet CSS in component or global styles
- [ ] Fixed height map container (e.g. 240px mobile, 320px desktop)
- [ ] Responsive width 100%

## Files to touch

| File | Action |
|------|--------|
| `src/frontend/src/components/attendance/AttendanceLocationMap.tsx` | Create |
| `src/frontend/src/pages/attendance/*` | Update detail/history views |
| `src/frontend/src/types/attendance.ts` | Update if needed |
| `src/frontend/package.json` | Add leaflet dependencies |
| `cursor/memory/api_contracts.md` | Update if DTO extended |

## Acceptance criteria

- [ ] Map renders check-in location when coordinates exist
- [ ] Both check-in and check-out pins shown when both exist
- [ ] Missing coordinates show friendly fallback (no broken map)
- [ ] OpenStreetMap tiles load without API key
- [ ] Map lazy-loaded on detail view only
- [ ] `npm run build` passes

## Next task

→ `29_approval_workflows.md` — Configurable multi-step approval engine
