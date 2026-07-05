# TASK 04 — TASK BOARD

> Status: **COMPLETED**

Create Kanban board APIs supporting drag-and-drop status updates and grouped task responses.

## Goal

Expose board-oriented endpoints that return tasks grouped by status and allow single-field status updates for drag-and-drop UX.

## Depends on

- Task 03 — Tasks module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Thin controllers |
| Guardrails | `cursor/core/02_guardrails.md` | No auth in services |
| TDD | `cursor/core/01_tdd.md` | Integration tests |
| Patterns | `cursor/core/04_patterns.md` | Query handler pattern |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| API contracts | `cursor/memory/api_contracts.md` | Board endpoints |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Status transitions validated in domain (`ProjectTask.UpdateStatus`)
- Valid statuses: `Todo`, `InProgress`, `Review`, `Done`
- `[RequireAnyPermission("task.update:tenant", "task.update_status:self")]` on status PATCH
- Self scope: assignee can update own task status only (policy engine + resource loader)
- Audit: `task.status_changed` with old/new status in metadata
- Optional optimistic concurrency via `UpdatedAt` check

### Memory — source of truth (`cursor/memory/`)

- Update `api_contracts.md` with board endpoints

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Task 03 is **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update `api_contracts.md`
3. Mark task status **COMPLETED**

## Deliverables

### API endpoints

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/v1/projects/{projectId}/tasks/board` | `task.read:tenant` | Tasks grouped by status |
| PATCH | `/api/v1/tasks/{id}/status` | `task.update:tenant` OR `task.update_status:self` | Update status only |

### Board response shape

```json
{
  "projectId": "uuid",
  "columns": [
    { "status": "Todo", "tasks": [...] },
    { "status": "InProgress", "tasks": [...] },
    { "status": "Review", "tasks": [...] },
    { "status": "Done", "tasks": [...] }
  ]
}
```

### PATCH request body

```json
{ "status": "InProgress" }
```

### Business rules

- [x] Invalid status transition returns `400 Bad Request`
- [x] Task must belong to specified project on board query
- [x] Assignee can PATCH status on own tasks when holding `task.update_status:self`
- [x] HR/admin with `task.update:tenant` can update any task status

### Tests

- [x] Integration: board returns all 4 columns (empty arrays when no tasks)
- [x] Integration: status PATCH updates task
- [x] Integration: self-scope permission for assignee
- [x] Integration: 403 when non-assignee lacks tenant permission
- [x] Unit: domain status transition rules

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Modules/HrPortal.Tasks/Application/Queries/GetTaskBoardQuery.cs` | Create |
| `src/backend/src/Modules/HrPortal.Tasks/Application/Commands/UpdateTaskStatusCommand.cs` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/TasksController.cs` | Add board + status actions |
| `src/backend/tests/HrPortal.IntegrationTests/TaskBoardEndpointTests.cs` | Create |
| `cursor/memory/api_contracts.md` | Update |

## Acceptance criteria

- [x] Board endpoint returns tasks grouped by all 4 statuses
- [x] Drag-and-drop status update works via PATCH
- [x] Permission scoping enforced (self vs tenant)
- [x] Audit logged on status change
- [x] `dotnet test` green

## Next task

→ `05_time_tracking_module.md` — Time Tracking module foundation
