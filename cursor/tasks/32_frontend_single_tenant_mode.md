# TASK 32 — FRONTEND SINGLE-TENANT MODE

> Status: **COMPLETED**

Adapt the frontend for hybrid tenancy: optional tenant header in single mode, permissions from `/me`.

## Goal

OSS single-tenant deployments should work without configuring `VITE_TENANT_ID`. Multi-tenant SaaS keeps current tenant header behavior.

## Depends on

- Task 14 — Single-tenant deployment mode (backend)
- Task 23 — Frontend permission foundation (`auth-permissions.ts`)

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
- VITE_TENANCY_MODE=single: omit X-Tenant-Id header from api-client

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

### Configuration

- [ ] Env var `VITE_TENANCY_MODE=single|multi` (default: `multi` for dev)
- [ ] Env var `VITE_TENANT_ID` — required only in multi mode
- [ ] Document in README and `.env.example`

### API client

- [ ] Update `src/frontend/src/lib/api-client.ts`:
  - Single mode: do not send `X-Tenant-Id` header
  - Multi mode: send `X-Tenant-Id` from env (existing behavior)

### Auth provider

- [ ] On login: call `GET /api/v1/me` to load permissions, employeeId, features
- [ ] Store in `auth-store.ts`: `permissions`, `employeeId`, `features`
- [ ] Update `src/frontend/src/api/me.ts` — typed response

### Navigation

- [ ] `app-layout.tsx` — show nav items based on `hasPermission()` not roles
- [ ] Hide tenant-specific UI when single mode (no tenant switcher needed)

## Files to touch

| File | Action |
|------|--------|
| `frontend/src/lib/api-client.ts` | Conditional tenant header |
| `frontend/src/lib/auth-permissions.ts` | hasPermission from store |
| `frontend/src/stores/auth-store.ts` | Permissions from /me |
| `frontend/src/providers/auth-provider.tsx` | Load /me on auth |
| `frontend/src/api/me.ts` | API client |
| `frontend/.env.example` | VITE_TENANCY_MODE |
| `README.md` | Frontend config section |

## Acceptance criteria

- [ ] Single mode: app works without VITE_TENANT_ID
- [ ] Multi mode: unchanged behavior with tenant header
- [ ] Permissions loaded from /me after login
- [ ] `npm run build` succeeds

## Next task

→ `33_frontend_permission_parity.md` — Frontend permission parity
