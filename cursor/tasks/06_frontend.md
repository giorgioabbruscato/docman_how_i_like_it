# TASK 06 — FRONTEND

> Status: **IN PROGRESS**

React frontend with typed API clients, auth, and business pages.

## TASK 06.1 — Foundation

**Goal:** Core frontend infrastructure.

**Deliverables:**
- [x] Vite + React 18 + TypeScript
- [x] Tailwind CSS
- [x] Zustand auth store with persistence
- [x] Axios API client with auth + tenant interceptors
- [x] App layout with navigation
- [x] Shared UI components (Button, Input, Card, Select)

## TASK 06.2 — Auth integration

**Goal:** Keycloak login flow.

**Deliverables:**
- [x] Auth store (`stores/auth-store.ts`)
- [x] Token injection via API client interceptor
- [x] 401 → auto logout
- [ ] Keycloak JS adapter integration
- [ ] Login/logout pages
- [ ] Protected route wrapper component

## TASK 06.3 — API clients

**Goal:** Typed API modules mirroring backend endpoints.

**Deliverables:**
- [x] `api/employees.ts` — CRUD operations
- [x] `api/departments.ts` — CRUD operations
- [x] `types/employee.ts` — TypeScript interfaces
- [x] `types/department.ts` — TypeScript interfaces
- [ ] `api/leave-requests.ts`
- [ ] `api/attendance.ts`
- [ ] `api/documents.ts`

## TASK 06.4 — Pages

**Goal:** Business UI pages.

**Deliverables:**
- [x] Dashboard page (basic)
- [x] Employees page (list + CRUD)
- [x] Departments page (list + CRUD)
- [ ] Leave requests page
- [ ] Attendance page
- [ ] Documents page
- [ ] Settings/profile page

## TASK 06.5 — UX requirements

- Loading spinners during API calls
- Error toasts/messages on failure
- Form validation before submit
- Confirmation dialogs for destructive actions
- Empty states when no data

## Acceptance criteria

- [ ] All pages use API clients (no direct axios)
- [ ] Auth flow works end-to-end with Keycloak
- [ ] Role-based UI visibility
- [ ] `npm run build` passes with zero errors
- [ ] Frontend quality checks pass (`evals/02_frontend_quality_checks.md`)

## Next task

→ `07_nginx.md` — Reverse proxy configuration
