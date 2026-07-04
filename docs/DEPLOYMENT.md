# Production Deployment Checklist

End-to-end checklist for deploying HR Portal to a production environment.

See also [OPERATIONS.md](OPERATIONS.md) for backup, migrations, and logging procedures.

---

## Pre-deploy

### Configuration and secrets

- [ ] Copy [`.env.example`](../.env.example) to `.env` on the target host — **never commit `.env`**
- [ ] Rotate default passwords:
  - `POSTGRES_PASSWORD`
  - `KEYCLOAK_ADMIN_PASSWORD`
  - Keycloak client secrets (`hrportal-api`, `hrportal-web`)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Set `Keycloak__RequireHttpsMetadata=true`
- [ ] Restrict CORS to production origins:
  ```
  Cors__AllowedOrigins__0=https://your-domain.com
  ```
- [ ] Configure frontend build args with production URLs:
  ```
  VITE_API_BASE_URL=https://your-domain.com/api
  VITE_KEYCLOAK_URL=https://auth.your-domain.com
  ```

### Infrastructure

- [ ] TLS certificates provisioned for Nginx (see `docker/nginx/`)
- [ ] DNS records for app and Keycloak hostnames
- [ ] Firewall: expose only 443 (and 80 redirect); do not expose Postgres publicly
- [ ] Named volumes or external storage for `postgres_data` and `storage_data`
- [ ] Backup job configured (see [OPERATIONS.md — Backup](OPERATIONS.md#backup-and-restore))

### Keycloak (production mode)

- [ ] Use `start` instead of `start-dev` in production
- [ ] Import realm via `--import-realm` on first deploy only, or manage realm via Admin API
- [ ] Disable or remove demo users in production
- [ ] Enable HTTPS for Keycloak (`KC_HTTPS_*` or reverse proxy)

### Security hardening

- [ ] Review [task 09 security](../cursor/tasks/09_security.md) items (security headers, upload limits, rate limiting)
- [ ] Swagger UI disabled in Production (default — only enabled in Development)
- [ ] Review uploaded file types and size limits in document module

---

## Deploy

```bash
# On target host
git pull
cp .env.example .env   # first time only; then edit secrets
docker compose build
docker compose up -d
```

### Deploy steps

- [ ] `docker compose up --build -d`
- [ ] Wait for Postgres healthcheck: `docker compose ps postgres` → healthy
- [ ] Keycloak started and realm available
- [ ] Backend logs show migrations applied:
  ```bash
  docker logs hrportal-backend 2>&1 | tail -50
  ```
- [ ] All containers running: `docker compose ps`

### Single-replica migration strategy

If this deploy includes new EF migrations:

1. Scale to one backend instance (or ensure only one starts first).
2. Confirm `MigrateAsync` completed in logs.
3. Scale to desired replica count.

---

## Post-deploy verification

### Automated smoke tests

```bash
curl -f https://your-domain.com/health
curl -f https://your-domain.com/ready
```

### Manual verification

- [ ] Keycloak login page loads at configured URL
- [ ] Admin user can obtain JWT (OIDC flow or direct grant in test only)
- [ ] API call with JWT + `X-Tenant-Id` returns data:
  ```bash
  curl -s https://your-domain.com/api/v1/employees \
    -H "Authorization: Bearer <token>" \
    -H "X-Tenant-Id: demo"
  ```
- [ ] Frontend loads and authenticated routes work
- [ ] Document upload and download work (storage volume writable)
- [ ] Tenant isolation: Tenant A cannot read Tenant B data

### Observability

- [ ] Container logs shipping to central store (if configured)
- [ ] `/ready` monitored by uptime checker
- [ ] Disk usage alert on Postgres and storage volumes

---

## Rollback

If deploy fails after database migration:

1. **Application rollback:** redeploy previous Docker image tag.
2. **Database rollback:** restore from latest `pg_dump` backup (see [OPERATIONS.md](OPERATIONS.md)).
3. **Storage rollback:** restore `storage_data` tar backup if files were affected.
4. Verify:
   ```bash
   curl -f http://localhost:5000/ready
   ```

> Prefer forward-fix migrations over rolling back schema in production.

---

## Post-deploy maintenance

| Task | Frequency |
|------|-----------|
| Postgres backup | Daily |
| Storage backup | Daily |
| Review audit logs | Weekly |
| Rotate secrets | Quarterly or on incident |
| Update dependencies / images | Monthly |

---

## Quick reference

| Resource | Dev URL | Notes |
|----------|---------|-------|
| API | http://localhost:5000 | Swagger at `/swagger` (dev only) |
| Frontend | http://localhost:5173 | Or port 80 via Nginx |
| Keycloak | http://localhost:8080 | Realm `hrportal` |
| Health | `/health`, `/ready` | No auth required |
| OpenAPI | `docs/openapi/hrportal-v1.json` | Committed spec |
| Postman | `docs/postman/HR-Portal.postman_collection.json` | Import + set `accessToken` |
