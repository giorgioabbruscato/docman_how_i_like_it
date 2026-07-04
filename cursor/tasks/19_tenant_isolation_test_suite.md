# TASK 19 — TENANT ISOLATION TEST SUITE

> Status: **PENDING**

Comprehensive integration and static guard tests for tenant isolation across all modules and deployment modes.

## Goal

Verify cross-tenant data leaks are impossible in multi mode and single mode behaves correctly. Add automated guards to prevent regression.

## Depends on

- Task 16 — ApplyTenantScope
- Task 17 — DbContext filters
- Task 18 — Repository scoping

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
- Static guard test: repositories must call ApplyTenantScope

### Memory — source of truth (`cursor/memory/`)

- ADR-012 ApplyTenantScope rules are authoritative

### Quality gates (`cursor/evals/`)

- `00_acceptance_criteria.md` — Tenant A cannot see Tenant B data
- `01_backend_quality_checks.md` — hybrid mode checks (after task 11)
- `01_backend_quality_checks.md` — integration tests for all modules

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

**HIGH** — Highest priority per architecture requirements

## Deliverables

### Cross-tenant integration tests (multi mode)

For each module, test tenant A cannot access tenant B data:

- [ ] Employees — list, get-by-id, create, update, delete
- [ ] Departments — list, get-by-id, mutations
- [ ] Leave requests — list, get-by-id, approve, cancel
- [ ] Attendance — list, check-in/out
- [ ] Documents — list, get-by-id, upload, download
- [ ] Audit logs — list (if applicable)

### Single-tenant mode tests

- [ ] All data accessible without `X-Tenant-Id`
- [ ] Writes stamp default tenantId

### Negative tests (multi mode)

- [ ] Missing tenant header → 400
- [ ] Invalid tenant slug → 404
- [ ] User without membership → 403

### Regression tests

- [ ] Unresolved tenant context does not return cross-tenant rows
- [ ] `IgnoreQueryFilters()` not used in production code (test-only allowed)

### Static guard test

- [ ] `RepositoryTenantScopeGuardTests` — scan all `*Repository.cs` files:
  - Every `_dbContext.Set<` call chain must include `ApplyTenantScope`
  - Fail build/test if violation found

## Files to touch

| File | Action |
|------|--------|
| `tests/HrPortal.IntegrationTests/TenantIsolationTests.cs` | Expand |
| `tests/HrPortal.IntegrationTests/SingleTenantModeTests.cs` | Create/expand |
| `tests/HrPortal.UnitTests/Security/RepositoryTenantScopeGuardTests.cs` | Create |
| `tests/HrPortal.UnitTests/Security/SqlInjectionGuardTests.cs` | Keep existing |

## Acceptance criteria

- [ ] Cross-tenant tests for all 5 business modules
- [ ] Single + multi mode tests pass
- [ ] Static guard test fails if repository skips `ApplyTenantScope`
- [ ] `dotnet test` green

## Next task

→ `20_policy_engine_facade.md` — Policy engine facade
