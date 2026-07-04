# TASK 33 — FRONTEND PERMISSION PARITY

> Status: **PENDING**

Align all frontend pages with backend permission keys; remove hardcoded role arrays.

## Goal

Frontend UI gating mirrors backend permissions for defense in depth. Backend remains authoritative; frontend hides unauthorized actions.

## Depends on

- Task 32 — Frontend single-tenant mode + /me loading
- Task 22 — Backend permission mapping

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Frontend layout |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` | API client pattern, auth |
| Master prompt | `cursor/prompts/00_master_prompt.md` | npm run build before complete |
| API contracts | `cursor/memory/api_contracts.md` | Permission strings, /me response |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Single/multi frontend behavior |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | Architecture + API checklist |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | Frontend auth + build |

### Mandatory rules (from `cursor/core/` + ADR-012)

- All API calls through typed clients in `src/frontend/src/api/` — never axios in pages
- Auth state in Zustand (`stores/auth-store.ts`)
- Use `hasPermission()` from `auth-permissions.ts` — not hardcoded role arrays
- Backend is authoritative — UI hiding is defense in depth only
- Replace all hasAnyRole() / HR_OR_ADMIN_ROLES with hasPermission()

### Memory — source of truth (`cursor/memory/`)

- Align permission keys with `api_contracts.md`

### Quality gates (`cursor/evals/`)

- `02_frontend_quality_checks.md` — typed API client, no direct axios, auth state
- `02_frontend_quality_checks.md` — npm run build must pass

### Agent prompts (`cursor/prompts/`)

- `02_frontend_agent_prompt.md`
- `00_master_prompt.md`

### Before starting
1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for this task type
3. Follow `/cursor/prompts/00_master_prompt.md` workflow

- Use `02_frontend_agent_prompt.md` for implementation scope

### Before completing
1. Run quality commands listed in Acceptance criteria
2. Verify against applicable `/cursor/evals/` checklist
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### Page updates

Replace `hasAnyRole()` / role arrays with `hasPermission()`:

- [ ] `dashboard-page.tsx` — widgets gated by read permissions
- [ ] `leave-requests-page.tsx` — create/approve/cancel buttons
- [ ] `documents-page.tsx` — upload/delete buttons
- [ ] `attendance-page.tsx` — check-in/report views
- [ ] `audit-page.tsx` — visible only with `audit.read:tenant` + feature
- [ ] `app-layout.tsx` — sidebar nav items

### Cleanup

- [ ] Deprecate `auth-roles.ts` exports (keep shim calling permission equivalents if needed)
- [ ] Remove `HR_OR_ADMIN_ROLES`, `MANAGER_OR_ABOVE_ROLES` usage
- [ ] Single source: `auth-permissions.ts`

### Permission mapping table

Document frontend action → permission string in task file comments:

| UI Action | Permission |
|-----------|------------|
| View employees | `employee.read:tenant` |
| Create employee | `employee.write:tenant` |
| Approve leave | `leave.approve:team` |
| Upload document | `document.upload:self` |
| View audit log | `audit.read:tenant` |
| Manage roles | `role.manage:tenant` |

## Files to touch

| File | Action |
|------|--------|
| `frontend/src/pages/*.tsx` | Permission gates |
| `frontend/src/components/layout/app-layout.tsx` | Nav permissions |
| `frontend/src/lib/auth-roles.ts` | Deprecate |
| `frontend/src/lib/auth-permissions.ts` | Canonical |

## Acceptance criteria

- [ ] No page uses hardcoded role string arrays for authorization
- [ ] UI actions hidden when user lacks permission
- [ ] Demo users see correct UI per their tenant role permissions
- [ ] `npm run build` succeeds

## Next task

→ `34_hybrid_architecture_acceptance.md` — Final acceptance
