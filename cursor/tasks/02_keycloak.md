# TASK 02 — KEYCLOAK

> Status: **COMPLETED**

Identity provider setup with realm import and OIDC configuration.

## TASK 02.1 — Keycloak container

**Goal:** Run Keycloak 26 with PostgreSQL backend.

**Deliverables:**
- [x] Keycloak service in `docker-compose.yml`
- [x] Shared PostgreSQL database
- [x] `start-dev --import-realm` command
- [x] Admin credentials via environment variables

## TASK 02.2 — Realm export

**Goal:** Pre-configured realm with clients, roles, and demo users.

**Deliverables:**
- [x] `docker/keycloak/realm-export.json`
- [x] Realm: `hrportal`
- [x] Client: `hrportal-web` (public, PKCE)
- [x] Client: `hrportal-api` (bearer-only)
- [x] Roles: `admin`, `hr`, `manager`, `employee`

**Demo users:**

| User | Password | Role |
|------|----------|------|
| admin@demo.local | admin123 | admin |
| hr@demo.local | hr123 | hr |

## TASK 02.3 — Backend JWT validation

**Goal:** Validate Keycloak-issued JWT tokens.

**Deliverables:**
- [x] `HrPortal.Identity` — JWT bearer authentication
- [x] Authority: `Keycloak__Authority` config
- [x] Audience: `Keycloak__Audience` config
- [x] Role claims mapped from Keycloak realm roles

## TASK 02.4 — Frontend OIDC

**Goal:** Keycloak login flow in React app.

**Configuration:**
```
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_REALM=hrportal
VITE_KEYCLOAK_CLIENT_ID=hrportal-web
```

## Acceptance criteria

- [x] Keycloak admin console accessible at `http://localhost:8080`
- [x] Realm imported on first startup
- [x] Demo users can login and obtain tokens
- [x] Backend rejects invalid/expired tokens

## Next task

→ `03_backend.md` — Business module implementation
