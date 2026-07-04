# TASK 08 — DOCKER FINAL

> Status: **COMPLETED**

Full stack Docker Compose validation and production readiness.

## TASK 08.1 — Full stack startup

**Goal:** Single command brings up entire system.

```bash
docker compose up --build
```

**Verify:**
- [x] All 5 containers start (postgres, keycloak, backend, frontend, nginx)
- [x] Backend applies migrations on startup
- [x] Keycloak imports realm
- [x] Demo tenant seeded
- [x] No manual intervention required

## TASK 08.2 — Inter-service communication

**Goal:** Services communicate via Docker network.

| From | To | Protocol |
|------|----|----------|
| backend | postgres:5432 | TCP |
| backend | keycloak:8080 | HTTP (JWT validation) |
| nginx | frontend:80 | HTTP |
| nginx | backend:8080 | HTTP |
| nginx | keycloak:8080 | HTTP |
| keycloak | postgres:5432 | TCP |

## TASK 08.3 — Volume persistence

**Goal:** Data survives container restarts.

- [x] `postgres_data` — database persists
- [x] `storage_data` — uploaded files persist

**Test:**
```bash
docker compose down
docker compose up -d
# Verify data still exists
```

Automated via `scripts/docker-smoke-test.sh`.

## TASK 08.4 — CI pipeline

**Goal:** Automated build and test in GitHub Actions.

**Deliverables:**
- [x] `.github/workflows/ci.yml`
- [x] Backend build + test
- [x] Frontend build
- [x] Docker compose validation (build + up --wait + smoke test)

## Acceptance criteria

- [x] `docker compose up --build` works from clean clone
- [x] All acceptance criteria pass (`evals/00_acceptance_criteria.md`)
- [x] CI pipeline green on main branch

## Next task

→ `09_security.md` — Security hardening
