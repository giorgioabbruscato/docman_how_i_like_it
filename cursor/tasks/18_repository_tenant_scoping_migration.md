# TASK 18 — REPOSITORY TENANT SCOPING MIGRATION

> Status: **PENDING**

Migrate all repository implementations to explicitly call `ApplyTenantScope` on every database query.

## Goal

Defense-in-depth: even if global filters fail or are bypassed, repositories enforce tenant scoping via the unified helper. Align with revised guardrails.

## Depends on

- Task 16 — ApplyTenantScope helper
- Task 17 — DbContext hybrid filters

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

**HIGH** — Tenant isolation safety

## Deliverables

### Business module repositories

Inject `ITenantContextAccessor` and call `.ApplyTenantScope(_accessor.Current)` on every query:

- [ ] `EmployeeRepository`
- [ ] `DepartmentRepository`
- [ ] `LeaveRequestRepository`
- [ ] `AttendanceRepository`
- [ ] `DocumentRepository`

### AccessControl repositories

- [ ] `TenantRoleRepository`
- [ ] `TenantMembershipRepository`
- [ ] `UserProfileRepository`

### Exemptions (document, do not scope)

- [ ] `TenantRepository` — platform table, not `ITenantEntity`
- [ ] Cross-tenant platform admin queries — use explicit tenantId parameter, not accessor bypass

### Storage

- [ ] Audit `FileSystemStorageProvider` — tenant path prefix `{tenantId}/...`
- [ ] Single mode: use default tenantId in path

### Guardrail

- [ ] Update `cursor/core/02_guardrails.md`: "Always use ApplyTenantScope in repositories"

## Files to touch

| Module | Repository |
|--------|------------|
| Employees | `Infrastructure/Persistence/EmployeeRepository.cs` |
| Departments | `Infrastructure/Persistence/DepartmentRepository.cs` |
| Leave | `Infrastructure/Persistence/LeaveRequestRepository.cs` |
| Attendance | `Infrastructure/Persistence/AttendanceRepository.cs` |
| Documents | `Infrastructure/Persistence/DocumentRepository.cs` |
| AccessControl | `Infrastructure/Persistence/*Repository.cs` |
| Storage | `HrPortal.Storage/Infrastructure/FileSystemStorageProvider.cs` |

## Acceptance criteria

- [ ] Every repository query method calls `ApplyTenantScope`
- [ ] No manual `Where(e => e.TenantId == ...)` duplication outside helper
- [ ] `dotnet build && dotnet test` pass
- [ ] Static guard test passes (see task 19)

## Next task

→ `19_tenant_isolation_test_suite.md` — Tenant isolation test suite
