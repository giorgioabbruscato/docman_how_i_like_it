# ARCHITECTURE DECISIONS

> Record of significant architectural decisions. Prevents drift and re-litigation.

---

## ADR-001: Modular Monolith over Microservices

**Status:** Accepted  
**Date:** 2026-01

**Context:** HR Portal needs multiple domain modules (Employees, Leave, Attendance, Documents) with potential for independent scaling later.

**Decision:** Build as a modular monolith — independent domain modules in a single deployable ASP.NET Core application.

**Rationale:**
- Simpler deployment and debugging for a self-hosted product
- Modules are already isolated by namespace and project structure
- Can extract to microservices later if needed
- Shared database with schema separation is sufficient for current scale

**Consequences:**
- Single deployment unit
- Centralized migrations in `HrPortal.Api`
- Cross-module communication via application service interfaces

---

## ADR-002: Shared Database Multi-Tenancy

**Status:** Accepted  
**Date:** 2026-01

**Context:** Platform must support multiple organizations (tenants) with data isolation.

**Decision:** Shared PostgreSQL database with `TenantId` column on all business entities + EF Core global query filters.

**Rationale:**
- Cost-effective for self-hosted deployments
- Simpler operations (one database to backup/migrate)
- EF global filters prevent accidental cross-tenant data leaks
- Sufficient isolation for HR data (not financial/regulated)

**Alternatives rejected:**
- Database-per-tenant: too complex for self-hosted
- Schema-per-tenant: migration complexity

**Consequences:**
- Every entity implements `ITenantEntity`
- `TenantId` set automatically on insert via `SaveChangesAsync`
- Tenant resolved from `X-Tenant-Id` header (dev) or subdomain (prod)

---

## ADR-003: Keycloak for Identity

**Status:** Accepted  
**Date:** 2026-01

**Context:** Need authentication and role-based authorization without building identity management.

**Decision:** Delegate all identity to Keycloak. Backend only validates JWT tokens.

**Rationale:**
- Battle-tested OIDC provider
- Self-hosted (aligns with product philosophy)
- Realm export enables reproducible setup
- Backend stays stateless (no session management)

**Consequences:**
- Keycloak is a required infrastructure component
- Roles defined in Keycloak realm, mapped to ASP.NET policies
- Frontend uses OIDC authorization code flow with PKCE

---

## ADR-004: Result Pattern over Exceptions

**Status:** Accepted  
**Date:** 2026-01

**Context:** Need consistent error handling across application services.

**Decision:** Application services return `Result<T>` for expected failures. Exceptions only for truly unexpected errors.

**Rationale:**
- Explicit error handling in controllers
- No try/catch boilerplate for business rules
- Error codes map cleanly to HTTP status codes
- Testable without exception assertions

**Consequences:**
- All service methods return `Result<T>` or `Result`
- Controllers map `ErrorCode` to HTTP status (NOT_FOUND → 404, CONFLICT → 409)
- `GlobalExceptionMiddleware` handles unexpected exceptions only

---

## ADR-005: Filesystem Storage with S3 Abstraction

**Status:** Accepted  
**Date:** 2026-01

**Context:** Documents module needs file storage. Self-hosted deployments may not have S3.

**Decision:** Implement `IStorageProvider` with filesystem backend. S3 implementation can be added later without domain changes.

**Rationale:**
- Works out of the box with Docker volume
- Interface abstraction allows swap to S3/MinIO
- No cloud dependency for self-hosted users

**Storage path:** `{tenantId}/employee/documents/{filename}`

---

## ADR-006: Centralized EF Migrations

**Status:** Accepted  
**Date:** 2026-01

**Context:** Multiple modules share one DbContext. Where should migrations live?

**Decision:** All migrations in `HrPortal.Api/Infrastructure/Persistence/Migrations/`.

**Rationale:**
- Single DbContext (`HrPortalDbContext`) references all module configurations
- One migration history avoids conflicts
- Simpler CI/CD (one `dotnet ef database update`)

**Consequences:**
- Module EF configurations live in module projects
- Migration commands always target `HrPortal.Api` project
- Adding a module requires a new migration

---

## ADR-007: FluentValidation + ValidationFilter

**Status:** Accepted  
**Date:** 2026-01

**Context:** Need consistent request validation across all endpoints.

**Decision:** FluentValidation validators per request DTO, enforced via ASP.NET `ValidationFilter`.

**Rationale:**
- Validators are testable in isolation
- Separation from domain logic
- Automatic 400 response with validation details

**Frontend counterpart:** Zod schemas for client-side validation.

---

## ADR-008: Cursor Agent System

**Status:** Accepted  
**Date:** 2026-07

**Context:** AI-assisted development needs consistent rules, quality gates, and domain knowledge to prevent architectural drift across tasks.

**Decision:** `/cursor/` directory with core rules, evals, tasks, prompts, and memory files.

**Rationale:**
- Deterministic agent behavior across sessions
- TDD enforcement via quality gates
- Domain model and API contracts as single source of truth
- Multi-agent support (backend/frontend/infra specialists)

**Consequences:**
- All agents must read `/cursor/core/` before coding
- Memory files must be updated when domain/API changes
- Task files track execution progress

---

## ADR-009: Secrets via Environment Variables

**Status:** Accepted  
**Date:** 2026-07

**Context:** Task 09 requires no hardcoded secrets in source or Docker defaults.

**Decision:** All secrets (database passwords, Keycloak admin, API client secrets) are supplied via environment variables or `.env` (gitignored). Base `appsettings.json` contains no credentials; `appsettings.Development.json` holds local dev values only.

**Consequences:**
- `docker compose` requires a populated `.env` (from `.env.example`)
- Production startup fails fast if `Database:ConnectionString` or `Cors:AllowedOrigins` are missing
- Keycloak realm export remains dev-only; production uses separate Keycloak configuration

---

## ADR-010: In-Memory JWT Storage for SPA Auth

**Status:** Accepted  
**Date:** 2026-07

**Context:** Task 09.1 requires secure auth without introducing a BFF layer.

**Decision:** Frontend stores JWT access tokens in memory only (Zustand without persist). SSO session continuity relies on Keycloak session cookies via `check-sso` on page load. Production Keycloak/nginx enforce secure cookie flags over HTTPS.

**Consequences:**
- No JWT in `localStorage`
- Page reload re-authenticates silently when Keycloak session is valid
- API client retries once with `keycloak.updateToken()` on 401 before logout
