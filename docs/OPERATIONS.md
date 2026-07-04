# Operations Guide

Operator runbook for backup, restore, identity, database migrations, and observability.

For architecture context see [ARCHITECTURE.md](ARCHITECTURE.md). For production rollout see [DEPLOYMENT.md](DEPLOYMENT.md).

## Services and volumes

| Service | Container | Persistent data |
|---------|-----------|-----------------|
| PostgreSQL | `hrportal-postgres` | Volume `postgres_data` |
| Keycloak | `hrportal-keycloak` | Realm config in git + Postgres |
| Backend | `hrportal-backend` | Volume `storage_data` → `/app/storage` |
| Frontend | `hrportal-frontend` | Stateless |
| Nginx | `hrportal-nginx` | Stateless |

Environment variables are documented in [`.env.example`](../.env.example).

---

## Backup and restore

### PostgreSQL

**Backup** (while stack is running):

```bash
docker exec hrportal-postgres pg_dump \
  -U hrportal \
  -d hrportal \
  --format=custom \
  --file=/tmp/hrportal-$(date +%Y%m%d).dump

docker cp hrportal-postgres:/tmp/hrportal-$(date +%Y%m%d).dump ./backups/
```

**Restore** (requires stopping writers to avoid corruption):

```bash
docker compose stop backend keycloak

docker cp ./backups/hrportal-20250704.dump hrportal-postgres:/tmp/restore.dump

docker exec hrportal-postgres pg_restore \
  -U hrportal \
  -d hrportal \
  --clean \
  --if-exists \
  /tmp/restore.dump

docker compose start keycloak backend
```

**Verify:**

```bash
curl -f http://localhost:5000/ready
curl -s http://localhost:5000/api/v1/tenants | jq .
```

### File storage (uploaded documents)

```bash
docker run --rm \
  -v docman_how_i_like_it_storage_data:/data \
  -v "$(pwd)/backups":/backup \
  alpine tar czf /backup/storage-$(date +%Y%m%d).tar.gz -C /data .
```

**Restore:**

```bash
docker run --rm \
  -v docman_how_i_like_it_storage_data:/data \
  -v "$(pwd)/backups":/backup \
  alpine sh -c "rm -rf /data/* && tar xzf /backup/storage-20250704.tar.gz -C /data"
```

> Volume name prefix may differ. Run `docker volume ls | grep storage` to confirm.

### Keycloak realm (configuration)

The committed realm export at [`docker/keycloak/realm-export.json`](../docker/keycloak/realm-export.json) is the source of truth for clients, roles, and demo users. Back up Postgres for Keycloak runtime state; export the realm JSON after admin changes (see below).

### Backup schedule (recommended)

| Asset | Frequency | Retention |
|-------|-----------|-----------|
| PostgreSQL | Daily | 30 days |
| Storage volume | Daily | 30 days |
| Realm JSON | After identity changes | Version control |

---

## Keycloak realm export and import

### Automatic import (local Docker)

On container start, Keycloak imports the mounted realm file:

```yaml
command: start-dev --import-realm
volumes:
  - ./docker/keycloak/realm-export.json:/opt/keycloak/data/import/realm-export.json:ro
```

Admin console: http://localhost:8080 (credentials from `.env`).

### Export updated realm

1. Make changes in Keycloak Admin Console (clients, roles, users).
2. Export realm `hrportal`:
   - **UI:** Realm settings → Action → Partial export (or full export).
   - **CLI** inside container:
     ```bash
     docker exec hrportal-keycloak /opt/keycloak/bin/kc.sh export \
       --dir /tmp/export --realm hrportal --users realm_file
     docker cp hrportal-keycloak:/tmp/export/hrportal-realm.json ./docker/keycloak/realm-export.json
     ```
3. Review diff, redact secrets if needed, commit to git.
4. Restart Keycloak: `docker compose restart keycloak`.

### Re-import behaviour

With `--import-realm`, Keycloak imports on startup. If the realm already exists, behaviour depends on Keycloak version and import strategy — treat production realm updates as a controlled change: export first, test in staging, then apply.

**Local dev:** after pulling changes to demo users in `realm-export.json`, Keycloak does not add new users to an existing realm. Either reset volumes (`docker compose down -v` then `docker compose up --build -d`) or create the user manually in the Keycloak Admin Console. The backend seeds the matching `Employee` record on startup when missing.

### Client secret rotation

1. Rotate secret in Keycloak Admin Console for client `hrportal-api` or `hrportal-web`.
2. Update backend/frontend environment variables.
3. Export realm JSON and commit (or store secrets in a vault, not git).

---

## Database migrations

Migrations live in:

```
src/backend/src/HrPortal.Api/Infrastructure/Persistence/Migrations/
```

### Development (manual)

```bash
cd src/backend

# Add migration after model changes
dotnet ef migrations add <MigrationName> \
  --project src/HrPortal.Api \
  --output-dir Infrastructure/Persistence/Migrations

# Apply to local database
dotnet ef database update --project src/HrPortal.Api
```

### Docker / default startup

`DbInitializer.InitializeAsync()` runs on every backend start and calls `Database.MigrateAsync()` when migrations exist. A demo tenant is seeded if none exists.

### Production recommendations

1. **Single-instance migration:** Scale backend to one replica, deploy, wait for migrations in logs, then scale up.
2. **Avoid concurrent MigrateAsync** from multiple replicas on first deploy of a new migration.
3. **Rollback:** EF Core does not support safe down-migrations in production. Ship a forward-fix migration instead of `database update <PreviousMigration>`.
4. **Verification:**
   ```bash
   docker logs hrportal-backend 2>&1 | grep -i migrat
   dotnet ef migrations list --project src/backend/src/HrPortal.Api
   ```

### CI note

Integration tests use an isolated SQLite database. Add a CI migration smoke step against ephemeral Postgres if you need to enforce clean applies on fresh databases.

---

## Monitoring and logging

### Structured logging (Serilog)

Configured in `Program.cs`:

- Reads `appsettings.json` / environment-specific overrides
- Enriches from log context
- **Console sink only** (default)
- HTTP request logging via `UseSerilogRequestLogging()`

**Log levels** (`appsettings.json`):

| Namespace | Level |
|-----------|-------|
| Default | Information |
| Microsoft | Warning |
| System | Warning |

Development overrides (`appsettings.Development.json`): Default = Debug.

**View logs:**

```bash
docker logs -f hrportal-backend
docker logs -f hrportal-keycloak
docker logs -f hrportal-postgres
```

### Health checks

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | All registered checks |
| `GET /ready` | PostgreSQL connectivity (`ready` tag) |

```bash
curl -f http://localhost:5000/health
curl -f http://localhost:5000/ready
```

Use `/ready` for load-balancer and orchestrator readiness probes.

### Business audit trail

The `HrPortal.Audit` module records tenant-scoped write operations. This is **not** infrastructure APM — it is an application audit log stored in PostgreSQL.

### Planned extensions

| Capability | Suggested approach |
|------------|-------------------|
| Centralized logs | Serilog sink → Seq, Elasticsearch, or Loki |
| Metrics | OpenTelemetry + Prometheus |
| Alerting | Health-check monitor on `/ready` + log error rate |
| Log retention | Configure at sink; redact PII in production |

Example future Serilog config (not enabled by default):

```json
"WriteTo": [
  { "Name": "Console" },
  { "Name": "Seq", "Args": { "serverUrl": "http://seq:5341" } }
]
```

---

## API documentation artifacts

| Artifact | Path |
|----------|------|
| OpenAPI spec | [`docs/openapi/hrportal-v1.json`](openapi/hrportal-v1.json) |
| Postman collection | [`docs/postman/HR-Portal.postman_collection.json`](postman/HR-Portal.postman_collection.json) |
| Swagger UI (dev) | http://localhost:5000/swagger |

**Regenerate OpenAPI:**

```bash
./scripts/export-openapi.sh
python3 scripts/generate-postman-collection.py
```
