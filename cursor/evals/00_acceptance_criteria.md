# GLOBAL ACCEPTANCE CRITERIA

The system is valid only if **all** criteria below pass.

## Infrastructure

- [ ] `docker compose up --build` completes without errors
- [ ] All containers reach healthy/running state
- [ ] PostgreSQL accepts connections on port 5432
- [ ] Keycloak admin console accessible on port 8080

## Backend

- [ ] Backend starts without manual fixes
- [ ] `/health` returns 200
- [ ] `/ready` returns 200
- [ ] Swagger UI accessible at `/swagger`
- [ ] All API endpoints documented in Swagger (27 business endpoints with examples)
- [ ] `dotnet test` passes with zero failures
- [ ] Migrations apply cleanly on fresh database

## Authentication

- [ ] Keycloak realm `hrportal` imported successfully
- [ ] Demo users can obtain JWT tokens
- [ ] Backend validates JWT from Keycloak
- [ ] Unauthorized requests return 401
- [ ] Forbidden requests return 403 (policy violations)

## Multi-tenancy

- [ ] **Multi mode** (default): requests without `X-Tenant-Id` are rejected with 400 (except excluded paths: `/health`, `/ready`, `/swagger`, `/api/v1/tenants`)
- [ ] **Single mode** (OSS): requests without `X-Tenant-Id` auto-resolve to `DefaultTenantSlug` (default: `demo`)
- [ ] Tenant A cannot see Tenant B data (multi mode)
- [ ] Demo tenant seeded on first startup

## API

- [ ] All endpoints return structured JSON responses
- [ ] Errors return RFC 7807 `ProblemDetails` (no stack traces)
- [ ] CRUD operations work for Employees and Departments
- [ ] Soft delete (deactivate) works correctly

## Frontend

- [ ] Frontend builds without TypeScript errors
- [ ] Frontend connects successfully to API
- [ ] Auth flow via Keycloak works end-to-end
- [ ] Protected routes redirect unauthenticated users
- [ ] Employee and Department pages render data

## Data persistence

- [ ] Data survives container restart (PostgreSQL volume)
- [ ] File storage persists in `storage_data` volume

## Security

- [ ] No secrets in committed `appsettings.json` or docker-compose defaults
- [ ] Security headers on API responses (`X-Content-Type-Options`, `X-Frame-Options`)
- [ ] Tenant isolation verified by integration tests
- [ ] All business endpoints require authorization (except health/tenants)
- [ ] CI runs dependency vulnerability scans (`dotnet list package --vulnerable`, `npm audit`, Trivy)

## Validation command

```bash
# Full stack smoke test
docker compose up --build -d
curl -f http://localhost:5000/health
curl -f http://localhost:5000/ready
cd src/backend && dotnet test
cd src/frontend && npm run build
```
