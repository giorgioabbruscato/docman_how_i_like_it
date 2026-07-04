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
| Tenancy | Tenant resolution, `TenantContext`, EF global filters |
| Identity | JWT validation from Keycloak, `UserContext` |
| Authorization | Policies: `AdminOnly`, `HrOrAdmin`, `ManagerOrAbove` |
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
  → TenantResolverMiddleware
  → Authorization (policies)
  → Controller → Application Service → Repository → DbContext
```

## Tenant resolution

| Environment | Mechanism |
|-------------|-----------|
| Local dev | `X-Tenant-Id: demo` header |
| Production | Subdomain: `acme.hrportal.com` → slug `acme` |

Excluded paths: `/health`, `/ready`, `/swagger`

## Persistence

- PostgreSQL with schema separation (`platform`, `employees`, `departments`, …)
- `ITenantEntity` on all tenant-scoped entities
- Global query filter applied in `HrPortalDbContext`
- `TenantId` set on insert via `SaveChangesAsync` override

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
