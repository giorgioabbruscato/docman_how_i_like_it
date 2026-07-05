# TASK 29 — APPROVAL WORKFLOWS

> Status: **PENDING**

Create a configurable multi-step approval workflow engine for leave, overtime, and timesheets.

## Goal

Introduce a platform workflow service with JSON-defined approval chains and state machines, generalizing simple approval flows from Leave and Task 23 timesheets.

## Depends on

- Task 22 — Documentation sync **COMPLETED**
- Task 27 — Check-in location map (recommended sequence)
- Task 23 — Timesheet approval (simple flow to migrate optionally)
- `HrPortal.Leave` — leave request approvals
- `HrPortal.TimeTracking` — timesheet/overtime approvals
- Tenancy — tenant workflow configuration

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Clean Architecture |
| Guardrails | `cursor/core/02_guardrails.md` | Tenant scope |
| TDD | `cursor/core/01_tdd.md` | Tests required |
| Patterns | `cursor/core/04_patterns.md` | Platform service template |
| Architecture | `cursor/core/03_architecture.md` | Platform vs module |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Domain model | `cursor/memory/domain_model.md` | Workflow entities |
| API contracts | `cursor/memory/api_contracts.md` | Workflow endpoints |
| Module deps | `cursor/memory/module_dependencies.md` | Add Workflows |
| ADRs | `cursor/memory/architecture_decisions.md` | Workflow engine ADR |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- New `HrPortal.Workflows` platform service under `src/backend/src/Platform/`
- Workflow definition stored as JSON per tenant + request type
- State machine: Pending → InProgress (step N) → Approved | Rejected | Cancelled
- `IWorkflowEngine` interface — domain modules invoke engine, not duplicate logic
- Request types: `LeaveRequest`, `TimesheetSubmission`, `OvertimeRequest` (define overtime as TimeTracking extension or reuse timesheet)
- Approver resolution: fixed role, direct manager (`IEmployeeLookup` manager chain), or named employee
- Task 23 simple timesheet approval may coexist — document optional migration path to workflow engine
- No hardcoded approval chains in domain modules after this task
- Audit every state transition

### Memory — source of truth (`cursor/memory/`)

- Add `WorkflowDefinition`, `WorkflowInstance`, `WorkflowStep`, `WorkflowAction` to `domain_model.md`
- Document endpoints in `api_contracts.md`
- Update `module_dependencies.md`
- Add ADR for workflow JSON schema

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Verify Tasks 23, Leave, TimeTracking, and 22 are **COMPLETED**

### Before completing

1. Run `dotnet test`
2. Update memory files and ADR
3. Mark task status **COMPLETED**

## Deliverables

### Domain entities

#### `WorkflowDefinition`

| Field | Type | Notes |
|-------|------|-------|
| RequestType | WorkflowRequestType | Leave, Timesheet, Overtime |
| Name | string | Display name |
| StepsJson | string | JSON array of step definitions |
| IsActive | bool | Only one active per request type per tenant |
| Version | int | Increment on update |

#### `WorkflowInstance`

| Field | Type | Notes |
|-------|------|-------|
| RequestType | WorkflowRequestType | |
| RequestId | Guid | FK to domain entity (leave, timesheet, etc.) |
| EmployeeId | Guid | Requester |
| Status | WorkflowStatus | Pending, InProgress, Approved, Rejected, Cancelled |
| CurrentStepIndex | int | 0-based |
| StartedAt | DateTime | UTC |
| CompletedAt | DateTime? | UTC |

#### `WorkflowAction`

| Field | Type | Notes |
|-------|------|-------|
| WorkflowInstanceId | Guid | FK |
| StepIndex | int | Which step |
| ActorEmployeeId | Guid | Who acted |
| Action | WorkflowActionType | Approve, Reject, Delegate, Cancel |
| Comment | string? | |
| ActionAt | DateTime | UTC |

**Enums:** `WorkflowRequestType`, `WorkflowStatus`, `WorkflowActionType`

### Workflow step JSON schema (example)

```json
{
  "steps": [
    { "name": "Direct Manager", "approverType": "DirectManager" },
    { "name": "HR", "approverType": "Role", "role": "hr.approve:tenant" }
  ]
}
```

### Permissions

| Constant | Value |
|----------|-------|
| `WorkflowManageTenant` | `workflow.manage:tenant` |
| `WorkflowReadTeam` | `workflow.read:team` |
| `WorkflowActTeam` | `workflow.act:team` |

### API endpoints

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/workflows/definitions` | `workflow.manage:tenant` |
| POST | `/api/v1/workflows/definitions` | `workflow.manage:tenant` |
| PUT | `/api/v1/workflows/definitions/{id}` | `workflow.manage:tenant` |
| GET | `/api/v1/workflows/instances` | `workflow.read:team` |
| GET | `/api/v1/workflows/instances/{id}` | `workflow.read:team` |
| POST | `/api/v1/workflows/instances/{id}/approve` | `workflow.act:team` |
| POST | `/api/v1/workflows/instances/{id}/reject` | `workflow.act:team` |
| GET | `/api/v1/workflows/pending` | `workflow.act:team` | Current user's pending actions |

### Engine integration

- [ ] `IWorkflowEngine.StartWorkflow(requestType, requestId, employeeId)`
- [ ] `IWorkflowEngine.ProcessAction(instanceId, action, actorId, comment?)`
- [ ] Hook Leave module: on leave submit → start workflow (replace or wrap existing approve)
- [ ] Hook TimeTracking: optional migration from Task 23 simple approval
- [ ] Notifications on step assignment (integrate with Notifications module)

### Tests

- [ ] Unit: state machine transitions (approve advances step, reject terminates)
- [ ] Unit: multi-step chain completes on final approval
- [ ] Integration: leave request through 2-step workflow
- [ ] Integration: unauthorized actor cannot approve
- [ ] Integration: tenant isolation on definitions

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Platform/HrPortal.Workflows/**` | Create |
| `src/backend/src/HrPortal.Api/Controllers/V1/WorkflowsController.cs` | Create |
| `src/backend/src/Modules/HrPortal.Leave/**` | Integrate engine |
| `src/backend/src/Modules/HrPortal.TimeTracking/**` | Optional migrate Task 23 |
| `src/backend/src/Platform/HrPortal.AccessControl/Domain/Permissions.cs` | Add constants |
| `src/backend/tests/HrPortal.UnitTests/WorkflowStateMachineTests.cs` | Create |
| `src/backend/tests/HrPortal.IntegrationTests/WorkflowLeaveApprovalTests.cs` | Create |
| `cursor/memory/domain_model.md` | Update |
| `cursor/memory/api_contracts.md` | Update |
| `cursor/memory/module_dependencies.md` | Update |
| `cursor/memory/architecture_decisions.md` | Add workflow ADR |

## Acceptance criteria

- [ ] Tenant admins can define multi-step workflows per request type
- [ ] Leave requests follow configured workflow
- [ ] Pending actions visible to assigned approvers
- [ ] State transitions audited
- [ ] Optional migration path from Task 23 documented
- [ ] `dotnet test` green

## Next task

→ `28_calendar_integration.md` — External calendar sync
