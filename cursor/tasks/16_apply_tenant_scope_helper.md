# TASK 16 — APPLY TENANT SCOPE HELPER

> Status: **PENDING**

Create the unified `ApplyTenantScope` query helper with mode-aware tenant filtering rules.

## Goal

Provide a single, explicit tenant scoping helper used by all repositories and mirrored by DbContext global filters. Single mode skips filtering; multi mode enforces `tenantId`.

## Depends on

- Task 13 — Unified TenantContext (Mode field)
- Task 14 — Single-tenant deployment mode

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

**HIGH** — Tenant isolation safety (requirement #1 priority)

## Deliverables

### TenantQueryExtensions

- [ ] Create `HrPortal.Tenancy/TenantQueryExtensions.cs`
- [ ] `ApplyTenantScope<TEntity>(query, ctx)` where `TEntity : ITenantEntity`:
  - `ctx.Mode == Single` → return query unchanged
  - `ctx.Mode == Multi` && `!ctx.IsResolved` → throw `TenantNotResolvedException`
  - `ctx.Mode == Multi` && resolved → `Where(e => e.TenantId == ctx.TenantId)`

### Shared helper

- [ ] `TenantScopingRules.ShouldApplyTenantFilter(ctx)` → bool
- [ ] Used by both extensions and DbContext filter expressions

### Seeding / background context

- [ ] `TenantScopingContext.ForSeeding(tenantId)` — explicit context for DbInitializer
- [ ] No silent bypass via `!IsResolved` in multi mode
- [ ] Document usage in guardrails

### Exception

- [ ] `TenantNotResolvedException` in SharedKernel (if not exists)

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Tenancy/TenantQueryExtensions.cs` | Create |
| `HrPortal.Tenancy/TenantScopingRules.cs` | Create |
| `HrPortal.Tenancy/TenantScopingContext.cs` | Create |
| `HrPortal.SharedKernel/Exceptions/TenantNotResolvedException.cs` | Create if missing |
| `cursor/core/02_guardrails.md` | Reference helper |

## Acceptance criteria

- [ ] Single mode: `ApplyTenantScope` is a no-op
- [ ] Multi mode: unresolved context throws
- [ ] Multi mode: resolved context filters by TenantId
- [ ] Unit tests for all three branches

## Next task

→ `17_dbcontext_hybrid_filters.md` — DbContext hybrid filters
