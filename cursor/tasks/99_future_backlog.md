# FUTURE BACKLOG — EPIC 9

> Status: **REFERENCE ONLY** — not executable tasks

Recommended future improvements that would significantly increase platform value. Execute only after Tasks 00–22 are complete and production-stable.

---

## 1. Timesheet Approval

**Description:** Employees submit worked hours; supervisor approves before hours are accounted in analytics and billing.

**Suggested module:** Extend `HrPortal.TimeTracking`

**New entities:** `TimesheetSubmission`, `TimesheetApproval`

**Dependencies:** Tasks 05–08 (Time Tracking), Task 13 (Analytics)

**Permissions:** `timesheet.submit:self`, `timesheet.approve:team`

---

## 2. Team Calendar

**Description:** Shared team view showing leave, permissions, smart working days, and public holidays.

**Suggested module:** New `HrPortal.Calendar` or extend Leave module

**Dependencies:** Leave module, Employees, Departments

**Frontend:** Full-calendar or similar library; month/week/day views

---

## 3. Personal Dashboard

**Description:** Unified home page: worked hours, remaining leave, documents, assigned tasks, and notifications.

**Suggested module:** Frontend-only aggregating existing APIs; optional `HrPortal.Dashboard` read service

**Dependencies:** Tasks 16–20 (all UI modules + notifications)

---

## 4. Geofencing

**Description:** Allow check-in only within configurable geographic areas (office, job site, client location).

**Suggested module:** Extend `HrPortal.Attendance` + new `GeofenceZone` entity in Tenancy or Attendance

**Dependencies:** Task 09–11 (Attendance 2.0)

**New config:** `GeofenceZone` with center lat/lng + radius meters per tenant

---

## 5. Check-in Location Map

**Description:** Display map screenshot/pin of check-in and check-out locations on attendance detail view.

**Suggested module:** Frontend + optional map tile service

**Dependencies:** Task 18 (Attendance UI), Task 09 (GPS fields)

**Note:** Use OpenStreetMap/Leaflet — no Google Maps API key required for OSS

---

## 6. Calendar Integration (Google / Microsoft 365)

**Description:** Sync approved leave and absences with external calendar providers.

**Suggested module:** New `HrPortal.Integrations` platform service

**Dependencies:** Leave module, OAuth2 infrastructure

**Interfaces:** `ICalendarSyncProvider` with Google and M365 implementations

---

## 7. Configurable Approval Workflows

**Description:** Configurable multi-step approval for leave, overtime, and timesheets.

**Suggested module:** New `HrPortal.Workflows` platform service

**Dependencies:** Leave, TimeTracking, Tenancy (tenant config)

**Pattern:** Workflow definition JSON + state machine per request type

---

## 8. Admin Dashboard

**Description:** Platform-level metrics: aggregate per tenant, platform usage, license utilization.

**Suggested module:** Extend Platform admin APIs (Tenancy)

**Dependencies:** All modules, `IsPlatformAdmin` flag

**Scope:** Cross-tenant queries with explicit `tenantId` — never bypass tenant scoping for business data

---

## Priority recommendation

| Priority | Item | Effort | Value |
|----------|------|--------|-------|
| High | Timesheet Approval | Medium | High |
| High | Personal Dashboard | Low | High |
| Medium | Team Calendar | Medium | High |
| Medium | Geofencing | Medium | Medium |
| Medium | Check-in Location Map | Low | Medium |
| Low | Calendar Integration | High | Medium |
| Low | Configurable Workflows | High | High |
| Low | Admin Dashboard | Medium | Medium |

---

## Rules for future implementation

When implementing any backlog item:

1. Read `/cursor/core/` rules and `/cursor/prompts/00_master_prompt.md`
2. Update `/cursor/memory/domain_model.md` and `/cursor/memory/api_contracts.md`
3. Follow `HrPortal.Employees` as reference module
4. Add executable task file in `/cursor/tasks/` before coding
5. Pass `/cursor/evals/` quality gates before marking complete
