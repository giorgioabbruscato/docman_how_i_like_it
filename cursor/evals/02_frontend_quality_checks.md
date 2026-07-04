# FRONTEND QUALITY CHECKS

Run these checks before marking any frontend task complete.

## Architecture checklist

- [ ] **Typed API client** — all HTTP calls go through `src/frontend/src/api/` modules
- [ ] **No direct axios in pages** — pages import from `@/api/{module}`
- [ ] **Global auth state** — Zustand store in `stores/auth-store.ts`
- [ ] **Route protection** — unauthenticated users redirected to login
- [ ] **Role-based access** — UI elements hidden/disabled based on roles
- [ ] **Reusable layout** — `AppLayout` wraps all authenticated pages
- [ ] **Typed models** — TypeScript interfaces in `types/` mirror backend DTOs

## UI checklist

- [ ] Responsive layout (mobile-friendly)
- [ ] Loading states for async operations
- [ ] Error states with user-friendly messages
- [ ] Form validation before submission
- [ ] Consistent use of shared UI components (`components/ui/`)

## Build & lint

```bash
cd src/frontend
npm run build        # Must pass with zero TS errors
npm run lint         # When configured
npm test             # When test suite exists
```

## API integration checklist

- [ ] `apiClient` sets `Authorization: Bearer {token}` via interceptor
- [ ] `apiClient` sets `X-Tenant-Id` header on every request
- [ ] 401 responses trigger logout
- [ ] Environment variables used for Keycloak and API URLs

## Environment variables

| Variable | Purpose |
|----------|---------|
| `VITE_API_BASE_URL` | Backend API base path |
| `VITE_KEYCLOAK_URL` | Keycloak server URL |
| `VITE_KEYCLOAK_REALM` | Realm name (`hrportal`) |
| `VITE_KEYCLOAK_CLIENT_ID` | OIDC client (`hrportal-web`) |
| `VITE_TENANT_ID` | Default tenant for dev |

## Current page status

| Page | API client | Auth | Tests | Status |
|------|-----------|------|-------|--------|
| Dashboard | — | ✅ | ❌ | Basic |
| Employees | ✅ | ✅ | ❌ | Functional |
| Departments | ✅ | ✅ | ❌ | Functional |

## Anti-patterns to reject

- Fetch/axios calls directly in page components
- Untyped API responses (`any`)
- Auth token stored outside Zustand persist
- Hardcoded API URLs in components
