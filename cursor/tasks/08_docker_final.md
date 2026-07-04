# TASK 08 — DOCKER FINAL

> Status: **IN PROGRESS**

Full stack Docker Compose validation and production readiness.

## TASK 08.1 — Full stack startup

**Goal:** Single command brings up entire system.

```bash
docker compose up --build
```

**Verify:**
- [ ] All 5 containers start (postgres, keycloak, backend, frontend, nginx)
- [ ] Backend applies migrations on startup
- [ ] Keycloak imports realm
- [ ] Demo tenant seeded
- [ ] No manual intervention required

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

- [ ] `postgres_data` — database persists
- [ ] `storage_data` — uploaded files persist

**Test:**
```bash
docker compose down
docker compose up -d
# Verify data still exists
```

## TASK 08.4 — CI pipeline

**Goal:** Automated build and test in GitHub Actions.

**Deliverables:**
- [x] `.github/workflows/ci.yml`
- [ ] Backend build + test
- [ ] Frontend build
- [ ] Docker compose validation

## Acceptance criteria

- [ ] `docker compose up --build` works from clean clone
- [ ] All acceptance criteria pass (`evals/00_acceptance_criteria.md`)
- [ ] CI pipeline green on main branch

## Next task

→ `09_security.md` — Security hardening
