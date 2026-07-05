# TASK 02 — PROJECT MEMBERS

> Status: **COMPLETED**

Implement project membership management APIs.

## Goal

Allow assigning and removing employees on projects with duplicate prevention, active-employee validation, audit logging, and permission checks.

## Depends on

- Task 01 — Project CRUD

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Cross-module via interfaces |
| Guardrails | `cursor/core/02_guardrails.md` | IEmployeeLookup only |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Patterns | `cursor/core/04_patterns.md` | Lookup pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | ProjectMember |
| API contracts | `cursor/memory/api_contracts.md` | Members endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | IEmployeeLookup |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Validate employee via `IEmployeeLookup.ExistsAndIsActiveAsync` — never query Employees DbSet
- Prevent duplicate `(ProjectId, EmployeeId)` assignments
- `[RequirePermission("project.manage_members:tenant")]` on mutations
- `[RequirePermission("project.read:tenant")]` on list
- Audit: `project.member.added`, `project.member.removed`
- Return `Result<T>` with clear error codes for duplicate/inactive employee

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with members endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 01 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/projects/{id}/members` | `project.read:tenant` | List project members |
| POST | `/api/v1/projects/{id}/members` | `project.manage_members:tenant` | Add member |
| DELETE | `/api/v1/projects/{id}/members/{memberId}` | `project.manage_members:tenant` | Remove member |

### POST request body

```json
{
  "employeeId": "uuid",
  "role": "Member",
  "hourlyRate": 50.00
}
```

### Business rules

- [ ] Only active employees can be assigned (`IEmployeeLookup`)
- [ ] Duplicate assignment returns `409 Conflict` with ProblemDetails
- [ ] Project must exist and belong to current tenant
- [ ] Member must belong to the specified project on DELETE

### Validation

- [ ] `AddProjectMemberRequestValidator` — employeeId required, role valid, hourlyRate >= 0

### Audit logging

- [ ] `project.member.added` on POST
- [ ] `project.member.removed` on DELETE

### Tests

- [ ] Integration: add member happy path
- [ ] Integration: duplicate assignment rejected
- [ ] Integration: inactive employee rejected
- [ ] Integration: remove member
- [ ] Integration: cross-tenant isolation
- [ ] Unit: domain rules for ProjectMember creation

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/HrPortal.Api/Controllers/V1/ProjectsController.cs` | Add member actions |
| `src/backend/src/Modules/HrPortal.Projects/Application/Commands/AddProjectMemberCommand.cs` | Create |
| `src/backend/src/Modules/HrPortal.Projects/Application/Commands/RemoveProjectMemberCommand.cs` | Create |
| `src/backend/src/Modules/HrPortal.Projects/Application/Queries/GetProjectMembersQuery.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/ProjectMembersEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [ ] All 3 endpoints work in Swagger
- [ ] Duplicate and inactive employee guards enforced
- [ ] Audit entries on add/remove
- [ ] `dotnet test` green

## Next task

→ `03_tasks_module.md` — Tasks module foundation
