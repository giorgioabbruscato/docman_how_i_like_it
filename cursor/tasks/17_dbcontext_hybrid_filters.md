# TASK 17 — DBCONTEXT HYBRID FILTERS

> Status: **COMPLETED**

Refactor EF Core global query filters to use mode-aware tenant scoping and eliminate the unsafe unresolved-tenant bypass.

## Goal

Align DbContext global filters with `ApplyTenantScope` rules. Remove `!IsResolved ||` bypass that exposes all tenant data when context is unset.

## Depends on

- Task 16 — ApplyTenantScope helper + TenantScopingRules

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | ITenantEntity on all business entities |
| Guardrails | `cursor/core/02_guardrails.md` | **Must** use ApplyTenantScope in repositories |
| Architecture | `cursor/core/03_architecture.md` | Persistence, global filters |
| TDD | `cursor/core/01_tdd.md` | Integration test conventions |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` |  |
| ADR-002 | `cursor/memory/architecture_decisions.md` | Shared DB multi-tenancy |
| ADR-012 | `cursor/memory/architecture_decisions.md` | ApplyTenantScope rules |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` |  |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | Tenant isolation criteria |

### Mandatory rules (from `cursor/core/` + ADR-012)

- **HIGHEST PRIORITY** — prevent cross-tenant data leaks
- `ApplyTenantScope(query, ctx)`: Single mode = no filter; Multi mode = enforce tenantId
- Remove unsafe `!IsResolved` global filter bypass in multi mode
- No raw `Set<T>()` queries without ApplyTenantScope in repositories
- DbContext outside repositories forbidden (guardrails)

### Memory — source of truth (`cursor/memory/`)

- ADR-012 ApplyTenantScope rules are authoritative

### Quality gates (`cursor/evals/`)

- `00_acceptance_criteria.md` — Tenant A cannot see Tenant B data
- `01_backend_quality_checks.md` — hybrid mode checks (after task 11)

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

## Priority

**HIGH** — Prevents cross-tenant data leaks

## Deliverables

### Global query filters

- [x] Refactor `HrPortalDbContext.SetTenantFilter<TEntity>()`:
  - Use `TenantScopingRules.ShouldApplyTenantFilter(_accessor.Current)`
  - When filter applies: `e.TenantId == ctx.TenantId`
  - When single mode: no filter
  - When seeding context: explicit tenantId filter only

### Insert stamping

- [x] `ApplyTenantIdOnInsert()`:
  - Multi mode: stamp from resolved context
  - Single mode: stamp default tenant if `TenantId == Guid.Empty`

### DbInitializer

- [x] Use `TenantScopingContext.ForSeeding(demoTenantId)` when querying during startup
- [x] Remove reliance on unresolved context disabling filters

### Background jobs (future-proof)

- [x] Document pattern: always set tenant context before DbContext use outside HTTP pipeline

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Api/Infrastructure/Persistence/HrPortalDbContext.cs` | Refactor filters |
| `HrPortal.Api/Infrastructure/Persistence/DbInitializer.cs` | Seeding context |
| `tests/HrPortal.IntegrationTests/TenantIsolationTests.cs` | Extend |

## Acceptance criteria

- [x] Multi mode: no code path returns all tenants' data without explicit context
- [x] Single mode: global filter disabled
- [x] DbInitializer seeds correctly with seeding context
- [x] Existing tenant isolation test still passes
- [x] Regression test: unresolved context in multi mode does not leak data

## Next task

→ `18_repository_tenant_scoping_migration.md` — Repository tenant scoping migration
