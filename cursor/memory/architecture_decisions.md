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

---

## ADR-012: Hybrid Single/Multi-Tenancy

**Status:** Accepted  
**Date:** 2026-07

**Context:** The platform must support both OSS single-tenant deployments (one organization, no `X-Tenant-Id` header) and SaaS multi-tenant deployments (many organizations, strict isolation). A separate architecture per mode would duplicate entities, repositories, and authorization paths.

**Decision:** Single-tenant is a **special case of multi-tenant**, not a separate architecture. One codebase, one data model (`ITenantEntity` on all business entities), one `TenantContext`, one `ApplyTenantScope` helper, and one policy engine. Deployment mode is selected via configuration only.

### TenantDeploymentMode

```csharp
enum TenantDeploymentMode { Single = 0, Multi = 1 }
```

Configuration surface (`IOptions<TenantResolverOptions>`):

| Setting | Default | Description |
|---------|---------|-------------|
| `Mode` | `Multi` | Backward-compatible default for existing deployments |
| `DefaultTenantSlug` | `"demo"` | Auto-resolved tenant in Single mode |

Environment variables: `TENANCY__MODE`, `TENANCY__DEFAULTTENANTSLUG`  
Frontend counterpart (task 32): `VITE_TENANCY_MODE`

### Mode behavior

| Aspect | Single | Multi |
|--------|--------|-------|
| Tenant resolution | Auto-resolve `DefaultTenantSlug` when header/subdomain absent | Require `X-Tenant-Id` header or subdomain; `400` if missing |
| `TenantContext.Mode` | `Single` | `Multi` |
| `ApplyTenantScope` | No-op (no filter applied) | `Where(e => e.TenantId == ctx.TenantId)`; throw `TenantNotResolvedException` if `!ctx.IsResolved` |
| DbContext global filter | Aligned with `TenantScopingRules` (task 17) | No `!IsResolved` bypass — prevents cross-tenant leaks |

### Unified TenantContext

`TenantContext` is the **sole request-scoped identity object** in the application layer. It replaces the split `TenantContext` + `UserContext` injection pattern in services.

| Field | Type | Description |
|-------|------|-------------|
| `TenantId` | Guid | Resolved tenant primary key |
| `TenantSlug` | string | URL-safe tenant identifier |
| `UserId` | Guid? | Authenticated user (Keycloak sub) |
| `Email` | string? | User email |
| `Mode` | `TenantDeploymentMode` | Current deployment mode |
| `Roles` | IReadOnlyList\<string\> | Legacy Keycloak realm roles |
| `RoleSlugs` | IReadOnlyList\<string\> | Tenant role slugs from membership |
| `Permissions` | IReadOnlyList\<string\> | Resolved permission strings |
| `EmployeeId` | Guid? | Linked employee record |
| `DepartmentId` | Guid? | Linked department (ABAC scope) |
| `Attributes` | IReadOnlyDictionary\<string, string\> | Membership attributes |
| `Features` | IReadOnlyList\<string\> | Tenant feature flags |
| `IsPlatformAdmin` | bool | Platform-level admin flag |
| `IsResolved` | bool | Tenant (and membership, if required) successfully resolved |

Factory methods: `Empty`, `CreateTenantOnly()`, `CreateSingleTenantDefault()`  
Helper: `HasPermission(string permission)`

`UserContext` remains in the Identity layer for JWT parsing only — **not** injected into application services.

### Request pipeline

```
Request
  → GlobalExceptionMiddleware
  → Serilog request logging
  → CORS
  → Authentication (JWT / Keycloak)
  → RequestContextMiddleware          ← replaces TenantResolverMiddleware
  → Authorization (PolicyEngine)
  → Controller → Application Service → Repository → DbContext
```

`RequestContextMiddleware` (in `HrPortal.AccessControl`, task 15):

1. Resolve tenant slug (mode-aware)
2. Load tenant from DB; reject inactive/suspended tenants
3. Set base `TenantContext` on `ITenantContextAccessor`
4. If authenticated: enrich via `TenantContextFactory` (membership + legacy Keycloak roles via `LegacyRoleMapper`)
5. Validate membership in Multi mode → `403` if no access
6. Validate `IsPlatformAdmin` for `/api/v1/platform/*` routes

Excluded paths (no tenant required): `/health`, `/ready`, `/swagger`, `/api/v1/tenants`, `/api/v1/platform/*`

### ApplyTenantScope

Every repository query on `ITenantEntity` **must** call `ApplyTenantScope(ctx)`:

```csharp
_dbContext.Set<Employee>()
    .ApplyTenantScope(_accessor.Current)
    .Where(...)
```

Rules:

| Mode | Behavior |
|------|----------|
| `Single` | Return query unchanged (no-op) |
| `Multi` + `!ctx.IsResolved` | Throw `TenantNotResolvedException` |
| `Multi` + resolved | Filter `Where(e => e.TenantId == ctx.TenantId)` |

DbContext global filters must mirror `TenantScopingRules.ShouldApplyTenantFilter(ctx)` (task 17). The current `!IsResolved ||` bypass in `HrPortalDbContext` is technical debt to be removed.

Seeding/background jobs use explicit `TenantScopingContext.ForSeeding(tenantId)` — no silent bypass.

### Policy engine

Authorization belongs in the **Policy layer only**. Application services and controllers must not contain authorization logic.

```csharp
bool IPolicyEngine.Can(TenantContext ctx, string action, ResourceContext? resource)
```

- Single authorization decision point (task 20)
- Permission format: `{resource}.{action}:{scope}` (e.g. `employee.read:tenant`, `leave.approve:team`)
- ABAC scopes: `Self`, `Department`, `Team`, `Tenant`, `All`
- Controllers use declarative `[RequirePermission("employee.read:tenant")]` — zero inline `if (role...)` checks
- `ResourceContext` carries `EmployeeId?`, `DepartmentId?`, `TenantId?` for scope resolution

### Relationship to prior ADRs

- **ADR-002** (Shared DB multi-tenancy): Confirmed. ADR-012 adds mode-aware scoping on top of `TenantId` columns and global filters.
- **ADR-003** (Keycloak identity): Keycloak remains the identity provider. Realm roles are mapped to permissions via `LegacyRoleMapper` during migration to tenant-scoped RBAC (`HrPortal.AccessControl`, task 12). Legacy ASP.NET role policies (`ManagerOrAbove`, etc.) are deprecated in task 23.

**Rationale:**
- One architecture reduces drift between OSS and SaaS deployments
- Explicit `ApplyTenantScope` in repositories makes tenant filtering auditable (static guard test, task 19)
- Centralized policy engine enables ABAC without scattering role checks
- Unified `TenantContext` keeps services framework-agnostic

**Consequences:**
- New platform module `HrPortal.AccessControl` owns memberships, roles, and permission catalog (task 12)
- All repositories migrated to `ApplyTenantScope` (task 18)
- Legacy role policies sunset after controller migration (tasks 22–23)
- `00_acceptance_criteria.md` multi-tenancy section evolves in tasks 14/19/34

### Addendum (task 23): Legacy policy deprecation + `LegacyRoleMapper` sunset plan

**Status:** Completed (backend) — `LegacyRoleMapper` retained as a compatibility shim.

All 10 V1 controllers now authorize exclusively via `[RequirePermission]` / `[RequireAnyPermission]` against
permission strings from `Permissions.cs` (task 22). As a direct consequence:

- `Policies.AdminOnly`, `Policies.HrOrAdmin`, and `Policies.ManagerOrAbove` are marked `[Obsolete]` in
  `Policies.cs`. Only `Policies.Authenticated` remains a live ASP.NET authorization policy.
- Their DI registrations were removed from `AuthorizationServiceCollectionExtensions` — only the
  `Authenticated` policy, `PermissionPolicyProvider`, `PermissionAuthorizationHandler`, and
  `PermissionAnyAuthorizationHandler` are registered.
- Zero production references to the obsolete constants remain (`grep` gate is safe to add to CI); the
  `[Obsolete]` attribute itself acts as a compile-time trip wire against regressions.

**`LegacyRoleMapper` sunset plan:** `MeService` and `TenantContextFactory` still fall back to
`LegacyRoleMapper.Map(ctx.Roles)` when a user has **no active `TenantMembership`** row (e.g. Keycloak-only
users provisioned before task 12's membership model, or environments still relying on realm-role JWT
claims). This mapper translates the four legacy realm roles (`Admin`, `HR`, `Manager`, `Employee`) into
their equivalent permission sets from `SystemRoleTemplates`, so authorization behavior is identical whether
a user resolves permissions via a real membership or via the legacy shim.

The mapper should be removed once both are true:
1. Every tenant's users have been backfilled into `TenantMembership` + `TenantRole` rows (one-time data
   migration, tracked separately — not part of tasks 22–25), and
2. Keycloak realm roles are no longer relied upon for authorization anywhere in the codebase (they may still
   exist for display purposes, e.g. `auth-roles.ts` on the frontend).

Until then, `LegacyRoleMapper` is intentionally kept as documented technical debt rather than deleted, since
removing it early would silently lock out any un-migrated user.

**Frontend parity (task 23):** The SPA mirrors this shift — `auth-permissions.ts` (`hasPermission` /
`hasAnyPermission`) replaces role-string checks against `/api/v1/me`'s `permissions` array for all page and
navigation gating (`app-layout.tsx`, `dashboard-page.tsx`, `attendance-page.tsx`, `documents-page.tsx`,
`leave-requests-page.tsx`). `auth-roles.ts` is marked `@deprecated` and now only backs the raw-role display on
the settings page; it is not used for any access-control decision.

---

## ADR-013: Attendance 2.0 — Session Model

**Status:** Accepted
**Date:** 2026-07

**Context:** The legacy `AttendanceRecord` model stored daily `DateOnly` + `TimeOnly?` clock times. This could not support GPS metadata, device/browser capture, open-session semantics, or dashboard aggregations (weekly/monthly totals, current session).

**Decision:** Replace `AttendanceRecord` with session-based `AttendanceSession`:

- `CheckIn`/`CheckOut` as UTC `DateTime`; `WorkedMinutes` computed on close
- One **open** session per employee (partial unique index + repository guard)
- Permissions renamed: `attendance.*` → `attendance_session.*` (clean break)
- Command handlers (`CheckInCommandHandler`, `CheckOutCommandHandler`) follow TimeTracking timer pattern
- Read APIs: `GET /dashboard`, `GET /history` with `AttendanceSessionReadScope` (self/team/tenant)

**Migration:** Clean break — create `attendance_sessions`, drop `attendance_records` (no data migration).

**Weekly boundary:** Calendar week Monday 00:00 UTC – Sunday 23:59 UTC (documented to avoid ambiguity with rolling 7-day windows).

**Consequences:**
- Frontend attendance page deferred to task 18 (API contracts updated)
- `AttendanceResourceLoader` removed (self-scope via `TenantContext.EmployeeId`)
- Audit actions: `attendance_session.check_in`, `attendance_session.check_out`

---

## ADR-014: Attendance Reminders via BackgroundService

**Status:** Accepted  
**Date:** 2026-07

**Context:** Users may forget to check in or check out. Reminders must fire even when the user never opens the SPA (login hook would miss offline employees).

**Decision:** Run `AttendanceReminderHostedService` (`BackgroundService`) hourly. For each tenant it sets `ITenantContextAccessor` via `TenantScopingContext.ForSeeding`, then `IAttendanceReminderService` evaluates:

- **Check-in:** after configurable `CheckInReminderHour` (default 10:00 local), active employees with no session today
- **Check-out:** after configurable `CheckOutReminderHour` (default 18:00 local), employees with an open session started today

Dedup via in-process `ConcurrentDictionary` keyed by `tenantId:employeeId:date:type`. Notifications use `INotificationService` typed methods; recipient resolved via `INotificationRecipientResolver` (employee → user membership).

**Alternatives rejected:** Login/`GET /me` hook — misses users who never sign in.

**Consequences:**
- Options bound from `AttendanceReminders` configuration section
- Notification failures logged via `NotificationHelper`; never affect attendance data
- MVP dedup resets on process restart (acceptable for logging-only notification backend)

---

## ADR-015: Timesheet Approval Gates Analytics

**Status:** Accepted  
**Date:** 2026-07

**Decision:** Hours from `TimeEntry` rows count toward analytics KPIs and worked-hours export only when linked to a `TimesheetSubmission` with `Status = Approved`. Unapproved or missing timesheets yield zero counted hours for that employee/period.

**Rationale:** Supervisors must sign off before billing/analytics use logged time; mirrors leave approval trust model.

---

## ADR-016: Calendar Aggregation Module

**Status:** Accepted  
**Date:** 2026-07

**Decision:** Introduce `HrPortal.Calendar` as a read-only aggregation module. Leave events via `ILeaveCalendarProvider` (implemented in Leave), holidays and smart-working schedules owned in Calendar schema. No direct Leave DbSet access from Calendar.

---

## ADR-017: Attendance Geofencing

**Status:** Accepted  
**Date:** 2026-07

**Decision:** Optional tenant geofence zones (Haversine radius). Defaults: geofencing off, allow check-in without GPS. When enabled with active zones, out-of-range check-ins return `GEOFENCE_VIOLATION`. `AttendanceSession` stores `MatchedGeofenceZoneId` and `GpsUnavailableAtCheckIn`.
