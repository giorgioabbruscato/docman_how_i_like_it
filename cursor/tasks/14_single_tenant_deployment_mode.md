# TASK 14 — SINGLE-TENANT DEPLOYMENT MODE

> Status: **PENDING**

Add `TenantDeploymentMode` configuration so the platform supports OSS single-tenant deployments without requiring `X-Tenant-Id` on every request.

## Goal

Implement hybrid deployment mode selection via configuration. Single-tenant mode auto-resolves the default tenant; multi-tenant mode preserves existing header/subdomain resolution.

## Depends on

- Task 13 — Unified TenantContext (Mode field)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Multi-tenancy mandatory |
| Guardrails | `cursor/core/02_guardrails.md` | TenantContext, no HTTP in services |
| Architecture | `cursor/core/03_architecture.md` | Tenant resolution, pipeline |
| TDD | `cursor/core/01_tdd.md` | Integration tests with X-Tenant-Id |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Backend scope |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Unified TenantContext, hybrid mode |
| Domain model | `cursor/memory/domain_model.md` | TenantContext contract |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Build & test |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | Multi-tenancy section — update for hybrid |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Business logic must not read HTTP, JWT, or headers — use `TenantContext` only
- `TenantContext` is the single source of truth for tenantId, userId, mode, roles, permissions
- No authorization logic in controllers or services — Policy layer only (tasks 20+)
- Single mode: auto-resolve DefaultTenantSlug — no X-Tenant-Id required
- Multi mode: preserve header/subdomain resolution — 400 if missing
- Configuration via IOptions<TenantResolverOptions> — no hardcoded values

### Memory — source of truth (`cursor/memory/`)

- Consult ADR-012 in `architecture_decisions.md` before changing TenantContext

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — dotnet build + dotnet test
- `00_acceptance_criteria.md` — update multi-tenancy criteria for single mode

### Agent prompts (`cursor/prompts/`)

- `01_backend_agent_prompt.md`
- `00_master_prompt.md`

### Before starting
1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for this task type
3. Follow `/cursor/prompts/00_master_prompt.md` workflow

- Use `01_backend_agent_prompt.md` for implementation scope

### Before completing
1. Run quality commands listed in Acceptance criteria
2. Verify against applicable `/cursor/evals/` checklist
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### Configuration

- [ ] `TenantDeploymentMode` enum: `Single = 0`, `Multi = 1`
- [ ] Extend `TenantResolverOptions`:
  - `Mode` (default: `Multi` for backward compat)
  - `DefaultTenantSlug` (default: `"demo"`)
- [ ] `appsettings.json` — document both modes
- [ ] `appsettings.Development.json` — `Multi` mode with header resolution
- [ ] Add `appsettings.SingleTenant.json` or env profile for OSS: `Mode: Single`
- [ ] Docker Compose: `TENANCY__MODE=Single` env var for OSS profile

### Resolution logic

- [ ] In single mode: when header/subdomain absent, resolve `DefaultTenantSlug` automatically
- [ ] In single mode: `TenantContext.Mode = Single`
- [ ] In multi mode: require header/subdomain (existing behavior); 400 if missing
- [ ] Set `TenantContext.Mode` from options on every resolved context

### Frontend alignment (stub — full work in task 32)

- [ ] Document expected `VITE_TENANCY_MODE` env var in memory/api_contracts

### Integration tests

- [ ] Single mode: requests work without `X-Tenant-Id` header
- [ ] Single mode: data scoped to default tenant on write
- [ ] Multi mode: missing tenant header → 400
- [ ] Multi mode: invalid tenant slug → 404

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Tenancy/TenantDeploymentMode.cs` | Create enum |
| `HrPortal.Tenancy/Infrastructure/TenantResolverOptions.cs` | Add Mode, DefaultTenantSlug |
| `HrPortal.Tenancy/Infrastructure/TenantResolver.cs` | Single-mode auto-resolve |
| `HrPortal.Api/appsettings.json` | Tenancy section |
| `docker-compose.yml` | Optional OSS profile env |
| `tests/HrPortal.IntegrationTests/SingleTenantModeTests.cs` | New |

## Acceptance criteria

- [ ] Single-tenant Docker profile runs without tenant header
- [ ] Multi-tenant mode unchanged for existing tests
- [ ] `TenantContext.Mode` correctly set in both modes
- [ ] Integration tests pass for both modes

## Next task

→ `15_request_pipeline_integration.md` — Request pipeline integration
