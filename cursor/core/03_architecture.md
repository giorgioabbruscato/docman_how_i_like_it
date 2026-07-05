# ARCHITECTURE MODEL

## Backend — Clean Architecture

```
Api → Application → Domain → Infrastructure
```

| Layer | Responsibility | Dependencies |
|-------|----------------|--------------|
| **Domain** | Pure business logic, entities, value objects | SharedKernel only |
| **Application** | Orchestration, DTOs, validators, service interfaces | Domain, SharedKernel |
| **Infrastructure** | EF Core, repositories, external integrations | Application, Domain |
| **Api** | HTTP transport, middleware, DI composition | Application, Infrastructure |

## Platform services (no domain dependencies)

| Service | Responsibility |
|---------|----------------|
| SharedKernel | Base entities, `Result<T>`, exceptions, `IUnitOfWork` |
| Tenancy | Tenant resolution, `TenantContext`, `ApplyTenantScope`, EF global filters |
| Identity | JWT validation from Keycloak, `UserContext` (Identity layer only) |
| AccessControl | Memberships, tenant roles, permission catalog, `TenantContextFactory`, `IPolicyEngine` |
| Authorization | ASP.NET authorization handlers, `[RequirePermission]` attribute |
| Storage | `IStorageProvider` — filesystem now, S3 later |
| Audit | Structured audit trail per tenant |
| Notifications | Channel-agnostic notification abstraction |
| Configuration | Typed options binding |

## Module communication rules

1. Modules **never** access another module's database tables directly
2. Cross-module lookups use **public application interfaces** (e.g. `IDepartmentLookup`)
3. Each module owns its EF configurations and repositories
4. Migrations are **centralized** in `HrPortal.Api`

## Request pipeline

```
Request
  → GlobalExceptionMiddleware
  → Serilog request logging
  → CORS
  → Authentication (JWT / Keycloak)
  → RequestContextMiddleware
  → Authorization (PolicyEngine)
  → Controller → Application Service → Repository → DbContext
```

`RequestContextMiddleware` (in `HrPortal.AccessControl`) replaces the legacy `TenantResolverMiddleware`. It resolves tenant (mode-aware), enriches `TenantContext` with membership/permissions, and validates user↔tenant binding.

Excluded paths: `/health`, `/ready`, `/swagger`, `/api/v1/tenants`, `/api/v1/platform/*`

## Authorization layers

```
Identity (JWT parsing) → AccessControl (membership + permissions) → PolicyEngine (decision)
```

| Layer | Responsibility |
|-------|----------------|
| **Identity** | Validate JWT, extract `UserContext` (sub, email, realm roles) |
| **AccessControl** | Resolve `TenantMembership`, load `TenantRole` permissions, `LegacyRoleMapper` fallback |
| **PolicyEngine** | `IPolicyEngine.Can(ctx, action, resource)` — single authorization decision point |

Controllers use `[RequirePermission]` — no inline role checks. Application services receive `TenantContext` only.

## Hybrid tenancy (ADR-012)

Single-tenant is a special case of multi-tenant — same entities, same `ApplyTenantScope`, same policy engine. Mode selected via configuration.

| Aspect | Single | Multi |
|--------|--------|-------|
| Tenant resolution | Auto-resolve `DefaultTenantSlug` when header/subdomain absent | Require `X-Tenant-Id` or subdomain; `400` if missing |
| `TenantContext.Mode` | `Single` | `Multi` |
| `ApplyTenantScope` | No-op | Filter by `TenantId`; throw if unresolved |
| `X-Tenant-Id` header | Optional | Required on business endpoints |

Configuration (`IOptions<TenantResolverOptions>`):

- `Mode`: `Single` \| `Multi` (default: `Multi`)
- `DefaultTenantSlug`: default `"demo"`

## Tenant resolution

| Environment | Mechanism |
|-------------|-----------|
| Local dev (Multi) | `X-Tenant-Id: demo` header |
| Production (Multi) | Subdomain: `acme.hrportal.com` → slug `acme` |
| Single mode | Auto-resolve `DefaultTenantSlug` — no header required |

## Persistence

- PostgreSQL with schema separation (`platform`, `employees`, `departments`, …)
- `ITenantEntity` on all tenant-scoped entities
- Repositories **must** call `ApplyTenantScope(ctx)` on every query
- DbContext global filters aligned with `TenantScopingRules` (task 17)
- `TenantId` set on insert via `SaveChangesAsync` override
- Seeding uses explicit `TenantScopingContext.ForSeeding(tenantId)`

## Frontend architecture

```
Pages → API clients → apiClient (axios) → Backend
       ↓
    Stores (Zustand) — auth state, global UI state
       ↓
    Components — reusable, no direct API calls
```

## Container strategy

One responsibility per container: frontend, backend, postgres, keycloak, nginx.

See also: `/docs/ARCHITECTURE.md` for extended documentation.
