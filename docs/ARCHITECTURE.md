# Architecture

## Design principles

| Principle | Implementation |
|-----------|----------------|
| Modular Monolith | Independent domain modules in one deployable |
| Clean Architecture | API → Application → Domain → Infrastructure |
| DDD (light) | Entities, Value Objects, Repositories, Domain Services |
| SOLID | DI everywhere, interfaces at boundaries |
| Multi-tenant | Shared DB + `TenantId` + global query filters |
| Security | Keycloak for auth, policy-based authorization |

## Platform services

These modules have **no dependency** on HR business domains:

| Service | Responsibility |
|---------|----------------|
| SharedKernel | Base types, Result, exceptions, IUnitOfWork |
| Tenancy | Tenant resolution, TenantContext, EF filters |
| Identity | JWT validation, UserContext from claims |
| Authorization | Policies: AdminOnly, HrOrAdmin, ManagerOrAbove |
| Storage | `IStorageProvider` — filesystem now, S3 later |
| Audit | Structured audit trail per tenant |
| Notifications | Channel-agnostic notification abstraction |
| Configuration | Typed options binding |

## Module communication rules

1. Modules **never** access another module's database tables directly
2. Cross-module communication goes through **public application services**
3. Each module owns its EF configurations and repositories
4. Migrations are **centralized** in `HrPortal.Api`

## Request pipeline

```
Request
  → GlobalExceptionMiddleware
  → Serilog request logging
  → CORS
  → Authentication (JWT)
  → TenantResolverMiddleware
  → Authorization (policies)
  → Controller → Application Service → Repository
```

## Tenant resolution

```
Local:     X-Tenant-Id: demo
Production: acme.hrportal.com → slug "acme"
```

Excluded paths: `/health`, `/ready`, `/swagger`

## Persistence

- PostgreSQL with schema separation (`platform`, `employees`, ...)
- `ITenantEntity` on all tenant-scoped entities
- Global query filter applied automatically in `HrPortalDbContext`
- `TenantId` set on insert via `SaveChangesAsync` override

## File storage

```
storage/
  {tenantId}/
    employee/
      documents/
        {filename}
```

Access only through `IStorageProvider` — swap implementation for S3 without domain changes.

## Error handling

All unhandled exceptions become `ProblemDetails`:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource not found",
  "status": 404,
  "detail": "Employee with key '...' was not found.",
  "errorCode": "NOT_FOUND"
}
```

## Adding a new module

1. Create `src/Modules/HrPortal.{ModuleName}/` with Domain, Application, Infrastructure
2. Add EF entity configurations
3. Register in `HrPortal.Api/Program.cs` via `{Module}ServiceCollectionExtensions`
4. Add controller under `Controllers/V1/`
5. Add migration: `dotnet ef migrations add Add{Module}`
6. Add frontend page and API client

## Future modules (roadmap)

Payroll, Asset Management, Expense Reports, Training, Performance Reviews, Recruitment, Help Desk, IT Inventory, Company Announcements, Calendar & Events, Chat/Messaging, Workflow Engine, Plugin System, Public API, Mobile App, White-label, Billing & Licensing.

## Container strategy

One responsibility per container:

| Container | Image |
|-----------|-------|
| frontend | Node build → Nginx |
| backend | .NET 8 ASP.NET |
| postgres | PostgreSQL 16 |
| keycloak | Keycloak 26 |
| nginx | Reverse proxy |

Each service can be updated independently.

## Backup and operations

See [OPERATIONS.md](OPERATIONS.md) for backup/restore procedures (PostgreSQL, storage volume, Keycloak realm).

## CI/CD pipeline

See [DEPLOYMENT.md](DEPLOYMENT.md) for the production deployment checklist. Automated pipeline:

```
Build → Tests → Docker Build → Compose Validation → Deploy
```
