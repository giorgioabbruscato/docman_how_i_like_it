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
- [ ] `dotnet test` passes with zero failures
- [ ] Migrations apply cleanly on fresh database

## Authentication

- [ ] Keycloak realm `hrportal` imported successfully
- [ ] Demo users can obtain JWT tokens
- [ ] Backend validates JWT from Keycloak
- [ ] Unauthorized requests return 401
- [ ] Forbidden requests return 403 (policy violations)

## Multi-tenancy

- [ ] Requests without `X-Tenant-Id` are rejected (except excluded paths)
- [ ] Tenant A cannot see Tenant B data
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

## Validation command

```bash
# Full stack smoke test
docker compose up --build -d
curl -f http://localhost:5000/health
curl -f http://localhost:5000/ready
cd src/backend && dotnet test
cd src/frontend && npm run build
```
