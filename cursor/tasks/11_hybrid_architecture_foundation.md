# TASK 11 — HYBRID ARCHITECTURE FOUNDATION

> Status: **PENDING**

Document the hybrid single/multi-tenant architecture before any implementation begins.

## Goal

Establish ADR-012 and update all agent-facing documentation so subsequent tasks (12–34) share a single source of truth for the hybrid tenancy model, layer boundaries, and authorization approach.

## Depends on

- Task 10 (Documentation) — completed baseline

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture, multi-tenancy baseline |
| Guardrails | `cursor/core/02_guardrails.md` | Revise tenant/auth rules in this task |
| Architecture | `cursor/core/03_architecture.md` | Update pipeline + tenancy section |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Agent workflow; update for hybrid modes |
| ADR history | `cursor/memory/architecture_decisions.md` | Add ADR-012 |
| Domain model | `cursor/memory/domain_model.md` | Stub AccessControl entities |
| Module deps | `cursor/memory/module_dependencies.md` | Add HrPortal.AccessControl |
| API contracts | `cursor/memory/api_contracts.md` | Planned endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Add hybrid checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Documentation-only task — **no application code**
- Single-tenant is a special case of multi-tenant — document, do not fork architecture
- Guardrails must require `ApplyTenantScope()` in repositories (replace old 'never filter manually' rule)
- Authorization belongs in Policy layer only — update guardrails accordingly
- All agent docs must stay consistent — no conflicting guidance across core/memory/evals

### Memory — source of truth (`cursor/memory/`)

- Write ADR-012 in `architecture_decisions.md`
- Extend `domain_model.md` with AccessControl entity stubs
- Extend `module_dependencies.md` with AccessControl platform service
- Update `api_contracts.md` for /me, /roles, /memberships when defined

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — add hybrid mode section (single/multi, ApplyTenantScope, policy engine)
- `00_acceptance_criteria.md` — multi-tenancy section will evolve in task 14/19

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md` — mention Single | Multi deployment modes

### Before starting
1. Read this task file and listed `cursor/core/` + `cursor/memory/` references
2. Check `/cursor/evals/` quality gates for this task type
3. Follow `/cursor/prompts/00_master_prompt.md` workflow


### Before completing
1. Run quality commands listed in Acceptance criteria
2. Verify against applicable `/cursor/evals/` checklist
3. Update `/cursor/memory/` if domain model or API contracts changed
4. Mark task status **COMPLETED** in this file

## Deliverables

### ADR-012: Hybrid Single/Multi-Tenancy

- [ ] Write ADR in `cursor/memory/architecture_decisions.md`
- [ ] Document decision: single-tenant is a special case of multi-tenant, not a separate architecture
- [ ] Document `TenantDeploymentMode` (`Single` | `Multi`) and config surface
- [ ] Document unified `TenantContext` as sole request-scoped identity object
- [ ] Document `ApplyTenantScope` rules per mode
- [ ] Document policy engine `can(ctx, action, resource)` layering

### Memory updates

- [ ] Update `cursor/memory/domain_model.md` — add AccessControl entities (stub section if not yet implemented)
- [ ] Update `cursor/memory/module_dependencies.md` — add `HrPortal.AccessControl` to platform services
- [ ] Update `cursor/memory/api_contracts.md` if new endpoints are planned (me, roles, memberships)

### Core rules updates

- [ ] Update `cursor/prompts/00_master_prompt.md` — mention hybrid deployment modes
- [ ] Update `cursor/core/03_architecture.md` — new request pipeline diagram (Auth → RequestContext → Authorization)
- [ ] Revise `cursor/core/02_guardrails.md`:
  - Repositories **must** call `ApplyTenantScope()` on every query
  - Forbid raw `Set<T>()` queries without tenant scoping helper
  - Services must not read HTTP/JWT directly; use `TenantContext` only
  - Authorization logic only in Policy layer, not controllers or services

### Eval updates

- [ ] Add hybrid mode checklist to `cursor/evals/01_backend_quality_checks.md`:
  - Single-tenant mode works without `X-Tenant-Id`
  - Multi-tenant mode enforces tenant isolation
  - All repositories use `ApplyTenantScope`
  - Policy engine used for all endpoint authorization

### Task chain

- [ ] Update `cursor/tasks/10_docs.md` — Next task → `11_hybrid_architecture_foundation.md`

## Files to touch

| File | Action |
|------|--------|
| `cursor/memory/architecture_decisions.md` | Add ADR-012 |
| `cursor/memory/domain_model.md` | Extend |
| `cursor/memory/module_dependencies.md` | Extend |
| `cursor/prompts/00_master_prompt.md` | Update |
| `cursor/core/03_architecture.md` | Update pipeline + tenancy section |
| `cursor/core/02_guardrails.md` | Revise tenant/auth guardrails |
| `cursor/evals/01_backend_quality_checks.md` | Add hybrid checks |
| `cursor/tasks/10_docs.md` | Next task link |

## Acceptance criteria

- [ ] ADR-012 accepted and complete
- [ ] No conflicting guidance between guardrails and hybrid requirements
- [ ] Agent system docs accurately describe target architecture from plan
- [ ] No application code changed in this task (documentation only)

## Next task

→ `12_access_control_foundation.md` — Access Control platform module
