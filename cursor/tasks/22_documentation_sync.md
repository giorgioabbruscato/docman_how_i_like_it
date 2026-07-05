# TASK 22 — DOCUMENTATION SYNC

> Status: **PENDING**

Final audit: verify and update all agent documentation, memory files, evals, and API contracts after Tasks 00–21.

## Goal

Ensure `/cursor/memory/`, `/cursor/evals/`, Swagger, and module dependency docs are complete, consistent, and reflect the full Projects → Reporting feature set.

## Depends on

- Tasks 00–21 — All feature tasks **COMPLETED**

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Memory is source of truth |
| Guardrails | `cursor/core/02_guardrails.md` | Consistency |
| Architecture | `cursor/core/03_architecture.md` | Verify module layout |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Before completing workflow |
| Domain model | `cursor/memory/domain_model.md` | Full entity catalog |
| API contracts | `cursor/memory/api_contracts.md` | All endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Full graph |
| ADRs | `cursor/memory/architecture_decisions.md` | Attendance 2.0, Analytics |
| Acceptance | `cursor/evals/00_acceptance_criteria.md` | Global gates |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Module status table |
| Frontend eval | `cursor/evals/02_frontend_quality_checks.md` | Page status table |

### Mandatory rules

- Documentation-only task — fix docs and evals; fix code **only** if docs reveal discrepancies
- Every entity in code must appear in `domain_model.md`
- Every endpoint in controllers must appear in `api_contracts.md`
- Every module reference must appear in `module_dependencies.md`
- No conflicting guidance across core/memory/evals/prompts
- Swagger must document all new endpoints with request/response examples

### Memory — source of truth (`cursor/memory/`)

This task **updates** all memory files — see Deliverables below.

### Quality gates (`cursor/evals/`)

- Run full validation from `00_acceptance_criteria.md`
- Update module/page status tables in eval files

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`

### Before starting

1. Verify Tasks 00–21 are all **COMPLETED**
2. Run full test suite and build

### Before completing

1. Run validation commands below
2. Mark task status **COMPLETED**

## Deliverables

### `cursor/memory/domain_model.md`

- [ ] Add all new entities: Project, ProjectMember, ProjectTask, TimeEntry, AttendanceSession
- [ ] Remove or mark deprecated: AttendanceRecord
- [ ] Update ERD diagram with new relationships
- [ ] Document new enums: ProjectStatus, ProjectMemberRole, TaskPriority, TaskStatus, AttendanceSessionStatus
- [ ] Document cross-module lookup interfaces: IProjectLookup, ITaskLookup, analytics providers
- [ ] Document notification events (from Task 20)

### `cursor/memory/api_contracts.md`

- [ ] Projects CRUD + members section
- [ ] Tasks CRUD + board + status PATCH
- [ ] Time entries CRUD + timer + manual + export
- [ ] Attendance check-in/out + dashboard + history
- [ ] Analytics supervisor + charts
- [ ] Reports generation
- [ ] All permissions documented per endpoint
- [ ] Request/response JSON examples for each endpoint group

### `cursor/memory/module_dependencies.md`

- [ ] Update dependency graph:

```
Departments → Employees → Projects → Tasks → TimeTracking
                                    ↘         ↗
                              Attendance → Analytics → Reporting
                              Leave ↗
                              Documents
```

- [ ] Document all new lookup interfaces and analytics providers
- [ ] Update `Program.cs` registration order

### `cursor/memory/architecture_decisions.md`

- [ ] Verify ADR for Attendance 2.0 (Task 09)
- [ ] Add ADR for Analytics read-only module pattern (if not written)
- [ ] Add ADR for lightweight CQRS without MediatR (if not written)
- [ ] Add ADR for Reporting module (if separate from Analytics)

### `cursor/evals/01_backend_quality_checks.md`

- [ ] Update module status table:

| Module | Status |
|--------|--------|
| Projects | Complete |
| Tasks | Complete |
| TimeTracking | Complete |
| Analytics | Complete |
| Reporting | Complete |
| Attendance | Updated (2.0) |

### `cursor/evals/02_frontend_quality_checks.md`

- [ ] Update page status table:

| Page | Status |
|------|--------|
| Projects | Functional |
| Time Tracking | Functional |
| Attendance | Functional |
| Analytics | Functional |

### `cursor/evals/00_acceptance_criteria.md`

- [ ] Update endpoint count in Swagger criterion
- [ ] Add acceptance checks for new modules if needed

### Swagger verification

- [ ] All new endpoints appear in Swagger UI
- [ ] Request/response schemas documented
- [ ] Permission requirements visible in endpoint descriptions

### Consistency audit

- [ ] No entity in code missing from domain_model.md
- [ ] No endpoint in controllers missing from api_contracts.md
- [ ] No module in Program.cs missing from module_dependencies.md
- [ ] Permission strings in Permissions.cs match api_contracts.md
- [ ] SystemRoleTemplates include new permissions for admin/hr/manager/employee roles

## Validation commands

```bash
# Full stack smoke test
docker compose up --build -d
curl -f http://localhost:5000/health
curl -f http://localhost:5000/ready

# Backend
cd src/backend
dotnet build --configuration Release
dotnet test --configuration Release

# Frontend
cd src/frontend
npm run build
```

## Acceptance criteria

- [ ] All memory files updated and internally consistent
- [ ] Eval status tables reflect current state
- [ ] `dotnet test` — zero failures
- [ ] `npm run build` — zero TS errors
- [ ] Swagger documents all business endpoints
- [ ] No conflicting docs across cursor/ directories
- [ ] Global acceptance criteria from `00_acceptance_criteria.md` pass (or gaps documented)

## Next task

→ `23_timesheet_approval.md` — EPIC 9 (execute after Task 22 complete)
