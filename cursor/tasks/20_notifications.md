# TASK 20 — NOTIFICATIONS

> Status: **PENDING**

Implement user notifications for key HR events using the existing `INotificationService`.

## Goal

Notify users when: check-in forgotten, check-out forgotten, project assigned, task assigned, leave approved, document uploaded.

## Depends on

- Task 02 — Project members
- Task 03 — Tasks module
- Task 11 — Attendance check-out
- Existing: Leave module, Documents module

## Rules & references

Read before starting:

| Source | Path | Notes |
|--------|------|-------|
| Global rules | `cursor/core/00_rules.md` | Interface abstraction |
| Guardrails | `cursor/core/02_guardrails.md` | No PII in logs |
| Architecture | `cursor/core/03_architecture.md` | Platform service |
| Master prompt | `cursor/prompts/00_master_prompt.md` | Workflow |
| Backend prompt | `cursor/prompts/01_backend_agent_prompt.md` | Scope |
| Module deps | `cursor/memory/module_dependencies.md` | Notifications platform |
| Backend eval | `cursor/evals/01_backend_quality_checks.md` | Checklist |

### Mandatory rules (from `cursor/core/` + ADR-012)

- Use existing `INotificationService` from `HrPortal.Notifications` — do not bypass
- Notifications are side effects — call **after** successful mutation, never before
- Notification failures must not roll back the business transaction (fire-and-forget with logging)
- Extend `INotificationService` if needed with typed methods (e.g. `NotifyProjectAssignedAsync`)
- No direct email/SMS — abstract behind interface (current impl is logging)
- Forgotten check-in/out: implement as scheduled check OR on-next-login check (document choice in ADR)

### Memory — source of truth (`cursor/memory/`)

- Document notification events in `domain_model.md` or `architecture_decisions.md`
- Update `module_dependencies.md` if new consumer relationships added

### Quality gates (`cursor/evals/`)

- `01_backend_quality_checks.md`

### Agent prompts (`cursor/prompts/`)

- `00_master_prompt.md`
- `01_backend_agent_prompt.md`

### Before starting

1. Read this task file and listed references
2. Review `HrPortal.Notifications/INotificationService.cs`

### Before completing

1. Run `dotnet test`
2. Update memory files
3. Mark task status **COMPLETED**

## Deliverables

### Notification triggers

| Event | Trigger point | Recipient |
|-------|---------------|-----------|
| Check-in forgotten | Scheduled job or login hook — no check-in by configured hour | Employee |
| Check-out forgotten | Scheduled job — open session past configured hour | Employee |
| Project assigned | After `AddProjectMemberCommand` succeeds | Assigned employee |
| Task assigned | After task create/update with AssignedEmployeeId | Assigned employee |
| Leave approved | After leave approval in LeaveService | Requesting employee |
| Document uploaded | After document upload in DocumentsService | Document owner |

### Service extensions

- [ ] Extend `INotificationService` with typed notification methods
- [ ] `NotificationPayload` record with: type, recipientUserId, title, body, metadata JSON
- [ ] Update `LoggingNotificationService` to log structured notification events

### Forgotten attendance detection

- [ ] `IAttendanceReminderService` in Attendance module
- [ ] Configurable thresholds via `IOptions<AttendanceReminderOptions>`:
  - `CheckInReminderHour` (default 10:00 local)
  - `CheckOutReminderHour` (default 18:00 local)
- [ ] Implementation: hosted service (`BackgroundService`) or check on `GET /me` — document choice

### Integration points

- [ ] Hook in `AddProjectMemberCommandHandler`
- [ ] Hook in `CreateTaskCommandHandler` / `UpdateTaskCommandHandler`
- [ ] Hook in `LeaveRequestService.ApproveAsync`
- [ ] Hook in `DocumentService.UploadAsync`

### Tests

- [ ] Unit: notification called after successful project member add
- [ ] Unit: notification called after task assignment
- [ ] Unit: notification called after leave approval
- [ ] Unit: forgotten check-in detection logic
- [ ] Unit: notification failure does not affect business result

## Files to touch

| File | Action |
|------|--------|
| `src/backend/src/Platform/HrPortal.Notifications/INotificationService.cs` | Extend |
| `src/backend/src/Platform/HrPortal.Notifications/Infrastructure/LoggingNotificationService.cs` | Update |
| `src/backend/src/Modules/HrPortal.Attendance/Application/AttendanceReminderService.cs` | Create |
| `src/backend/src/Modules/HrPortal.Projects/Application/Commands/AddProjectMemberCommand.cs` | Add hook |
| `src/backend/src/Modules/HrPortal.Tasks/Application/Commands/*` | Add hook |
| `src/backend/src/Modules/HrPortal.Leave/Application/LeaveRequestService.cs` | Add hook |
| `src/backend/src/Modules/HrPortal.Documents/Application/DocumentService.cs` | Add hook |
| `src/backend/tests/HrPortal.UnitTests/Notifications/*` | Create |

## Acceptance criteria

- [ ] All 6 notification triggers implemented
- [ ] Notifications use INotificationService interface
- [ ] Failures logged but don't break mutations
- [ ] Forgotten check-in/out detection works
- [ ] `dotnet test` green

## Next task

→ `21_reporting.md` — Report generation
