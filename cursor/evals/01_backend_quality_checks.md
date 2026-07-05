# BACKEND QUALITY CHECKS

Run these checks before marking any backend task complete.

## Per-module checklist

Each module (`HrPortal.{Module}`) must have:

- [ ] **Domain layer** — sealed entity with factory methods, no framework deps
- [ ] **Application layer** — service interface + implementation
- [ ] **Repository abstraction** — `I{Entity}Repository` in Application
- [ ] **Repository implementation** — in Infrastructure/Persistence
- [ ] **EF configuration** — `{Entity}Configuration.cs` with schema, indexes, constraints
- [ ] **DTOs** — separate request/response records
- [ ] **Validators** — FluentValidation for all request DTOs
- [ ] **DI registration** — `{Module}ServiceCollectionExtensions.cs`
- [ ] **Controller** — thin, in `HrPortal.Api/Controllers/V1/`
- [ ] **Unit tests** — domain rules + service layer
- [ ] **Integration tests** — all endpoints with auth + tenant headers

## Platform services checklist

- [ ] No dependency on business domain modules
- [ ] Interface in Application/Domain, implementation in Infrastructure
- [ ] Registered in `Program.cs`

## Hybrid architecture checklist (ADR-012)

Applies from task 14 onward as hybrid features are implemented:

- [ ] Single mode: business endpoints work without `X-Tenant-Id` header
- [ ] Multi mode: tenant isolation — Tenant A cannot see Tenant B data
- [ ] Every repository query uses `ApplyTenantScope`
- [ ] No HTTP/JWT/`HttpContext` access in application services — `TenantContext` only
- [ ] Endpoint authorization via policy engine (`IPolicyEngine.Can`) — no inline role checks
- [ ] `TenantContext.Mode` correctly set for deployment mode (`Single` \| `Multi`)

## Code quality

- [ ] No `DbContext` usage outside repositories
- [ ] No business logic in controllers
- [ ] All services return `Result<T>`
- [ ] Structured logging on significant operations
- [ ] No hardcoded connection strings or secrets

## Build & test

```bash
cd src/backend
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

## Migration check

When adding entities:

```bash
dotnet ef migrations add {MigrationName} \
  --project src/HrPortal.Api \
  --output-dir Infrastructure/Persistence/Migrations
```

Verify migration applies cleanly:

```bash
dotnet ef database update --project src/HrPortal.Api
```

## Current module status

| Module | Domain | Service | Repository | Tests | Status |
|--------|--------|---------|------------|-------|--------|
| Access Control | ✅ | ✅ | ✅ | ✅ | Complete |
| Employees | ✅ | ✅ | ✅ | ✅ | Reference impl |
| Departments | ✅ | ✅ | ✅ | ✅ | Complete |
| Leave | ✅ | ✅ | ✅ | ✅ | Complete |
| Attendance | ✅ | ✅ | ✅ | ✅ | Complete |
| Documents | ✅ | ✅ | ✅ | ✅ | Complete |
