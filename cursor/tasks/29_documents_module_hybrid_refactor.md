# TASK 29 — DOCUMENTS MODULE HYBRID REFACTOR

> Status: **COMPLETED**

Refactor the Documents module following the Employees reference pattern (task 27).

## Goal

Pure document service, tenant-scoped repository and storage paths, permission-based upload/download/delete with resource scope.

## Depends on

- Task 27 — Employees module (reference implementation)

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Cross-module via interfaces only |
| Guardrails | `cursor/core/02_guardrails.md` | ApplyTenantScope, no auth in services |
| Patterns | `cursor/core/04_patterns.md` | HrPortal.Employees reference module |
| TDD | `cursor/core/01_tdd.md` | Unit + integration tests |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Copy Employees structure |
| Domain model | `cursor/memory/domain_model.md` | Entity rules for this module |
| API contracts | `cursor/memory/api_contracts.md` | Endpoint permissions |
| ADR-012 | `cursor/memory/architecture_decisions.md` | Hybrid compliance |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Per-module checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Task 27 (Employees) is the reference — replicate its hybrid pattern exactly
- Repository: every query uses ApplyTenantScope
- Controller: [RequirePermission] only — register IResourceLoader
- Service: pure orchestration — no auth, TenantContext only
- FluentValidation on request DTOs unchanged

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` if endpoint permissions change

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md` — full per-module checklist
- Cross-tenant isolation tests from task 19 must still pass

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

### Service layer

- [ ] `DocumentService` — TenantContext only, no auth checks
- [ ] `ctx.TenantId` for storage path prefix and entity creation
- [ ] `ctx.UserId` for uploader audit field

### Repository + storage

- [ ] `DocumentRepository` — all queries use `ApplyTenantScope`
- [ ] `FileSystemStorageProvider` — path `{tenantId}/employee/documents/{fileName}`
- [ ] Single mode: use default tenantId in path

### Controller + authorization

- [ ] Permissions: `document.read:*`, `document.upload:self`, `document.delete:tenant`
- [ ] `DocumentResourceLoader` for get/download/delete scope checks
- [ ] Upload: validate employeeId scope via policy engine

### Tests

- [ ] Unit tests with TenantContext fixture
- [ ] Integration: user can upload to own employee; cannot upload to others without permission
- [ ] Cross-tenant document access returns 404

## Files to touch

| File | Action |
|------|--------|
| `HrPortal.Documents/Application/DocumentService.cs` | Refactor |
| `HrPortal.Documents/Infrastructure/Persistence/DocumentRepository.cs` | Verify scope |
| `HrPortal.Storage/Infrastructure/FileSystemStorageProvider.cs` | Verify paths |
| `HrPortal.Api/Controllers/V1/DocumentsController.cs` | Permissions |
| `HrPortal.AccessControl/Infrastructure/ResourceLoaders/DocumentResourceLoader.cs` | Create |
| `tests/HrPortal.IntegrationTests/DocumentsEndpointTests.cs` | Update |

## Acceptance criteria

- [ ] Matches Employees module hybrid pattern
- [ ] File storage tenant-isolated in multi mode
- [ ] `dotnet test` green

## Next task

→ `30_attendance_module_hybrid_refactor.md` — Attendance module hybrid refactor
