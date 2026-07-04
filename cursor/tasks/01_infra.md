# TASK 01 — INFRASTRUCTURE

> Status: **COMPLETED**

Docker and database infrastructure for local development and production-like environments.

## TASK 01.1 — Docker Compose

**Goal:** Multi-service orchestration.

**Deliverables:**
- [x] `docker-compose.yml` with postgres, keycloak, backend, frontend, nginx
- [x] Named volumes: `postgres_data`, `storage_data`
- [x] Health checks on postgres
- [x] Service dependency ordering
- [x] `.env.example` with all required variables
- [x] `.dockerignore`

## TASK 01.2 — PostgreSQL

**Goal:** Persistent database with schema separation.

**Deliverables:**
- [x] PostgreSQL 16 Alpine image
- [x] Configurable via environment variables
- [x] Health check endpoint
- [x] Volume persistence

**Environment variables:**
```
POSTGRES_DB=hrportal
POSTGRES_USER=hrportal
POSTGRES_PASSWORD=hrportal
```

## TASK 01.3 — Backend Dockerfile

**Goal:** Multi-stage .NET 8 build.

**Deliverables:**
- [x] `docker/backend/Dockerfile`
- [x] Multi-stage: restore → build → publish → runtime
- [x] Expose port 8080

## TASK 01.4 — Database migrations

**Goal:** EF Core migrations centralized in API project.

**Commands:**
```bash
cd src/backend
dotnet ef migrations add InitialCreate \
  --project src/HrPortal.Api \
  --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --project src/HrPortal.Api
```

## Acceptance criteria

- [x] `docker compose up -d postgres` starts healthy database
- [x] Backend connects to postgres via connection string
- [x] Migrations apply on startup via `DbInitializer`

## Next task

→ `02_keycloak.md` — Identity provider setup
