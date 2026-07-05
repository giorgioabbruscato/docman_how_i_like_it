# GUARDRAILS

## Never

- Put business logic in controllers
- Bypass services and access DB directly from controllers
- Skip validation on incoming DTOs
- Return raw exceptions to clients
- Hardcode secrets, connection strings, or API keys
- Access another module's database tables directly
- Raw `Set<T>()` queries without `ApplyTenantScope()` in repositories
- Read HTTP headers, JWT claims, or `HttpContext` in application services
- Authorization logic (`if role`, `User.IsInRole`, permission checks) in controllers or services
- Expose domain entities directly from API responses
- Commit `obj/`, `bin/`, `node_modules/`, or `.env` files
- Skip migrations when adding new entities

## Always

- Validate DTOs with FluentValidation (backend) or Zod (frontend)
- Use structured logging via Serilog
- Isolate external systems behind interfaces (`IStorageProvider`, `ITenantRepository`, etc.)
- Keep domain pure — no EF, HTTP, or framework dependencies in Domain layer
- Return `Result<T>` from application services, map to HTTP in controllers
- Use RFC 7807 `ProblemDetails` for error responses
- Set `TenantId` via entity factory methods, not in controllers
- Call `ApplyTenantScope(ctx)` on every repository query for `ITenantEntity`
- Set tenant context before any DbContext use outside the HTTP pipeline (seeding, background jobs): `accessor.Set(TenantScopingContext.ForSeeding(tenantId))`
- Platform cross-tenant queries use an explicit `tenantId` parameter (e.g. `IgnoreQueryFilters()` + `Where`), not an unresolved accessor bypass
- Inject and use `TenantContext` (via `ITenantContextAccessor`) as sole request identity in services
- Delegate authorization to Policy layer (`IPolicyEngine.Can`)
- Register new modules in `Program.cs` via `{Module}ServiceCollectionExtensions`
- Add EF configurations in module's `Infrastructure/Persistence/`
- Centralize migrations in `HrPortal.Api/Infrastructure/Persistence/Migrations/`

## Security guardrails

- All business endpoints require authentication unless explicitly public
- Authorization via policy engine + `[RequirePermission]` — legacy role policies (`AdminOnly`, `HrOrAdmin`, `ManagerOrAbove`) acceptable until task 23
- Never log JWT tokens, passwords, or PII in plain text
- CORS origins must come from configuration, not wildcards in production

## Code quality

- Match naming and structure of existing modules (`HrPortal.Employees` is the reference)
- One responsibility per file/class
- Prefer records for DTOs, sealed classes for domain entities
