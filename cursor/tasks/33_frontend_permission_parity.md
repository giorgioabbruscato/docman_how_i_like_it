# TASK 33 — FRONTEND PERMISSION PARITY

> Status: **COMPLETED**

Align all frontend pages with backend permission keys; remove hardcoded role arrays.

## Goal

Frontend UI gating mirrors backend permissions for defense in depth. Backend remains authoritative; frontend hides unauthorized actions.

## Depends on

- Task 32 — Frontend single-tenant mode + /me loading
- Task 22 — Backend permission mapping

## Deliverables

### Page updates

- [x] `dashboard-page.tsx` — widgets gated by read permissions
- [x] `leave-requests-page.tsx` — create/approve/cancel buttons
- [x] `documents-page.tsx` — upload/delete buttons
- [x] `attendance-page.tsx` — check-in/report views
- [x] `audit-page.tsx` — visible only with `audit.read:tenant` + feature
- [x] `app-layout.tsx` — sidebar nav items
- [x] `employees-page.tsx` — create/deactivate gates
- [x] `departments-page.tsx` — create/deactivate gates

### Cleanup

- [x] Deprecate `auth-roles.ts` exports (keep shim)
- [x] Remove `HR_OR_ADMIN_ROLES`, `MANAGER_OR_ABOVE_ROLES` usage from pages
- [x] Single source: `auth-permissions.ts`
- [x] Pre-fill `employeeId` from auth store for self-scoped forms

### Permission mapping table

| UI Action | Permission |
|-----------|------------|
| Nav: Employees / view list | `employee.read:tenant` OR `employee.read:team` |
| Create employee | `employee.create:tenant` |
| Deactivate employee | `employee.delete:tenant` |
| Nav: Departments / view list | `department.read:tenant` |
| Create/update department | `department.write:tenant` |
| Deactivate department | `department.delete:tenant` |
| Dashboard: Employees widget | `employee.read:tenant` OR `employee.read:team` |
| Dashboard: Leave widget | `leave.read:tenant` OR `leave.read:team` |
| Dashboard: Documents widget | `document.read:tenant` |
| Nav: Leave / create request | `leave.create:self` OR any `leave.read:*` |
| Approve/reject leave | `leave.approve:team` |
| Cancel own leave | `leave.delete:self` |
| Nav: Documents / upload | `document.upload:self` OR `document.read:tenant` |
| View document list | `document.read:tenant` |
| Delete document | `document.delete:tenant` |
| Check-in/out | `attendance.write:self` |
| View records / report | `attendance.read:tenant` OR `attendance.read:team` |
| Nav: Audit | `audit.read:tenant` + `planFeatures.auditLog` |

## Acceptance criteria

- [x] No page uses hardcoded role string arrays for authorization
- [x] UI actions hidden when user lacks permission
- [x] Demo users see correct UI per their tenant role permissions
- [x] `npm run build` succeeds
- [x] Zero `hasAnyRole` / `HR_OR_ADMIN_ROLES` / `MANAGER_OR_ABOVE` usage outside `auth-roles.ts`

## Next task

→ `34_hybrid_architecture_acceptance.md` — Final acceptance
