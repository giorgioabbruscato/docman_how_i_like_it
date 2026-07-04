# TASK 34 — HYBRID ARCHITECTURE ACCEPTANCE

> Status: **PENDING**

Final validation of the hybrid tenancy architecture against all 10 hard requirements.

## Goal

Verify the complete refactor satisfies isolation safety, layer boundaries, centralized auth, hybrid deployment, and extensibility. Update documentation and mark epic complete.

## Depends on

- Tasks 11–33 — All prior hybrid architecture tasks

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | All mandatory rules |
| Guardrails | `cursor/core/02_guardrails.md` | Full guardrail compliance |
| Architecture | `cursor/core/03_architecture.md` | Hybrid pipeline |
| TDD | `cursor/core/01_tdd.md` | Full test suite |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Complete workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` |  |
| Frontend prompt | `cursor/prompts/02_frontend_agent_prompt.md` |  |
| All memory | `cursor/memory/` | Final sync required |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Full checklist |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | Full checklist |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | All global criteria + hybrid |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Verify implementation matches ADR |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Verify all 10 hard requirements from hybrid architecture plan
- dotnet test + npm run build must pass with zero failures
- Docker smoke: Single mode (no header) + Multi mode (isolation)
- Mark tasks 11–34 COMPLETED after verification

### Memory — source of truth (`cursor/memory/`)

- Final sync: architecture_decisions.md, domain_model.md, api_contracts.md, module_dependencies.md
- Update README.md deployment section for OSS vs SaaS

### Quality gates (`cursor/evals/`)

- `00_acceptance_criteria.md` — full global validation
- `01_backend_quality_checks.md` — hybrid section complete
- `02_frontend_quality_checks.md` — permission-based UI

### Agent prompts (`cursor/prompts/`)

- All prompts in `cursor/prompts/` — verify agent docs match implemented system

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

### Hard requirements checklist

| # | Requirement | Verification |
|---|-------------|--------------|
| 1 | TenantContext abstraction — no HTTP/JWT in services | Code review + grep |
| 2 | Business logic purity — framework/request/tenant agnostic services | Code review |
| 3 | Hybrid strategy — Single skips filter, Multi enforces tenantId | Integration tests |
| 4 | DB entities have id, tenant_id, timestamps | domain_model.md audit |
| 5 | Centralized `can(ctx, action, resource)` — no inline auth | Grep + tests |
| 6 | Layer separation — Controller/Service/Repository/Policy | Architecture review |
| 7 | Tenant isolation — all queries via ApplyTenantScope | Static guard test |
| 8 | Single-tenant as special case of multi — no duplicated paths | Code review |
| 9 | Consistent refactor — reduced duplication | Review |
| 10 | Priority order respected — isolation first | Task order audit |

- [ ] Document pass/fail for each requirement in this task file upon completion

### Build and test

- [ ] `cd src/backend && dotnet build --configuration Release`
- [ ] `cd src/backend && dotnet test --configuration Release`
- [ ] `cd src/frontend && npm run build`

### Smoke tests

- [ ] Docker Compose — Single mode: no tenant header, full CRUD works
- [ ] Docker Compose — Multi mode: demo + second tenant, isolation verified
- [ ] Login as employee/manager/hr/admin — permissions match UI and API

### Documentation

- [ ] Update `README.md` — OSS single-tenant vs SaaS multi-tenant deployment sections
- [ ] Final sync: `cursor/memory/architecture_decisions.md`, `domain_model.md`, `api_contracts.md`, `module_dependencies.md`
- [ ] Mark tasks 11–34 as COMPLETED

### Eval

- [ ] Pass all checks in `cursor/evals/01_backend_quality_checks.md` hybrid section
- [ ] Pass frontend build check

## Files to touch

| File | Action |
|------|--------|
| `README.md` | Deployment modes |
| `cursor/memory/*` | Final sync |
| `cursor/tasks/11_*.md` … `33_*.md` | Mark completed |
| This file | Pass/fail checklist |

## Acceptance criteria

- [ ] All 10 hard requirements verified pass
- [ ] Full test suite green
- [ ] Smoke tests pass in both deployment modes
- [ ] Documentation accurate and complete
- [ ] Epic complete — ready for feature development on hybrid foundation

## Next task

None — hybrid architecture epic complete. Future features should follow patterns established in tasks 11–34.
