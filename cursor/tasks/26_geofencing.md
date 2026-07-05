# TASK 26 — GEOFENCING

> Status: **COMPLETED**

Add configurable geographic zones and enforce check-in only within allowed areas.

## Goal

Allow tenants to define geofence zones (office, job site, client location) and reject check-ins when GPS coordinates fall outside configured areas.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 24 — Team calendar (recommended sequence)
- Tasks 09–11 — Attendance 2.0 (session-based check-in/out with GPS fields)
- Task 18 — Attendance UI (for admin zone management page)

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
| Domain model | `cursor/memory/domain_model.md` | GeofenceZone |
| API contracts | `cursor/memory/api_contracts.md` | Geofence endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | UI checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Extend `HrPortal.Attendance` — do not create a separate geofencing module
- `GeofenceZone` entity per tenant with center lat/lng + radius in meters
- Haversine distance for point-in-circle validation
- Tenant config flag: `GeofencingEnabled` (disable enforcement when false or no zones defined)
- Check-in with GPS denied: configurable policy — reject or allow with warning (document default: allow with audit flag)
- Check-in outside all zones: return `400` with clear error code `GEOFENCE_VIOLATION`
- Store which zone matched on `AttendanceSession` (optional `GeofenceZoneId`)

### Memory — source of truth (`cursor/memory/`)

- Add `GeofenceZone` to `domain_model.md`
- Document endpoints and check-in validation in `api_contracts.md`
- Add ADR for geofencing policy defaults

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`
- `02_frontend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`
- `02_frontend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 09–11, 18, and 22 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Run `npm run build`
3. Update memory files
4. Mark task status **COMPLETED**

## Deliverables

### Domain entity: `GeofenceZone`

| Field | Type | Notes |
|-------|------|-------|
| Name | string | Zone label (e.g. "HQ Milan") |
| Latitude | double | Center latitude |
| Longitude | double | Center longitude |
| RadiusMeters | int | Allowed radius |
| IsActive | bool | Soft disable |
| Description | string? | Optional |

### Permissions

| Constant | Value |
|----------|-------|
| `GeofenceReadTenant` | `geofence.read:tenant` |
| `GeofenceManageTenant` | `geofence.manage:tenant` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/geofence-zones` | `geofence.read:tenant` |
| GET | `/api/v1/geofence-zones/{id}` | `geofence.read:tenant` |
| POST | `/api/v1/geofence-zones` | `geofence.manage:tenant` |
| PUT | `/api/v1/geofence-zones/{id}` | `geofence.manage:tenant` |
| DELETE | `/api/v1/geofence-zones/{id}` | `geofence.manage:tenant` |
| GET | `/api/v1/geofence-zones/settings` | `geofence.read:tenant` |
| PUT | `/api/v1/geofence-zones/settings` | `geofence.manage:tenant` |

### Settings DTO

| Field | Type | Notes |
|-------|------|-------|
| GeofencingEnabled | bool | Master toggle |
| AllowCheckInWithoutGps | bool | When GPS unavailable |

### Check-in validation

- [ ] Extend check-in command to validate lat/lng against active zones when enabled
- [ ] Return `GEOFENCE_VIOLATION` when outside all zones
- [ ] Optional: set `MatchedGeofenceZoneId` on session when inside a zone

### Backend

- [ ] `IGeofenceValidator` service with Haversine distance
- [ ] Unit tests for distance calculation (known coordinates)
- [ ] Integration tests: check-in inside zone, outside zone, geofencing disabled

### Frontend

- [ ] API client: `src/frontend/src/api/geofence.ts`
- [ ] Admin page: `/settings/geofencing` — list/create/edit zones on map preview
- [ ] Settings toggle: enable geofencing, allow check-in without GPS
- [ ] Attendance check-in page: show geofence violation error message
- [ ] Map preview for zone drawing (Leaflet circle overlay — reuse Task 27 component if available, or inline simple preview)

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Attendance/**` | Extend |
| `src/backend/src/HrPortal.Api/Controllers/V1/GeofenceZonesController.cs` | Create |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/tests/HrPortal.UnitTests/GeofenceValidatorTests.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/GeofenceCheckInTests.cs` | Create |
| `src/frontend/src/api/geofence.ts` | Create |
| `src/frontend/src/pages/settings/geofencing/*` | Create |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/architecture_decisions.md` | Add geofencing ADR |

## Acceptance criteria

- [ ] Tenant admins can CRUD geofence zones
- [ ] Check-in rejected when outside zones (when enabled)
- [ ] Check-in allowed when geofencing disabled or no zones
- [ ] Haversine validation covered by unit tests
- [ ] Admin UI for zone management
- [ ] `dotnet test` and `npm run build` green

## Next task

→ `27_check_in_location_map.md` — Map display on attendance detail
