# TASK 07 — NGINX

> Status: **COMPLETED**

Reverse proxy for unified access to all services.

## TASK 07.1 — Nginx container

**Goal:** Single entry point on port 80.

**Deliverables:**
- [x] Nginx 1.27 Alpine in `docker-compose.yml`
- [x] `docker/nginx/nginx.conf`
- [x] Depends on frontend, backend, keycloak

## TASK 07.2 — Routing rules

**Goal:** Route requests to appropriate services.

**Expected routing:**

| Path | Target | Purpose |
|------|--------|---------|
| `/` | frontend:80 | React SPA |
| `/api/` | backend:8080 | REST API |
| `/auth/` | keycloak:8080 | Identity provider |
| `/health` | backend:8080 | Health check |

## TASK 07.3 — Frontend nginx

**Goal:** SPA serving with fallback routing.

**Deliverables:**
- [x] `docker/frontend/nginx.conf`
- [x] `try_files $uri /index.html` for client-side routing
- [x] Static asset caching headers

## Acceptance criteria

- [x] `http://localhost` serves frontend
- [x] `http://localhost/api/v1/employees` proxies to backend
- [x] SPA routes work on page refresh (no 404)

## Next task

→ `08_docker_final.md` — Full stack Docker validation
