# EPIC 9 — FUTURE IMPROVEMENTS INDEX

> Status: **INDEX** — not executable directly; run individual task files below

Execute EPIC 9 only after Task 22 is **COMPLETED** and the platform is production-stable.

## Execution order

```
22 (complete) → 23 → 25 → 24 → 26 → 27 → 29 → 28 → 30
```

## Task files

| # | Task file | Description | Priority |
|---|-----------|-------------|----------|
| 23 | [`23_timesheet_approval.md`](23_timesheet_approval.md) | Timesheet submission and supervisor approval | High |
| 25 | [`25_personal_dashboard.md`](25_personal_dashboard.md) | Unified employee home page | High |
| 24 | [`24_team_calendar.md`](24_team_calendar.md) | Shared team calendar (leave, holidays) | Medium |
| 26 | [`26_geofencing.md`](26_geofencing.md) | Geographic check-in zones | Medium |
| 27 | [`27_check_in_location_map.md`](27_check_in_location_map.md) | Map display on attendance detail | Medium |
| 29 | [`29_approval_workflows.md`](29_approval_workflows.md) | Configurable multi-step approval engine | Low |
| 28 | [`28_calendar_integration.md`](28_calendar_integration.md) | Google / Microsoft 365 calendar sync | Low |
| 30 | [`30_admin_dashboard.md`](30_admin_dashboard.md) | Platform admin cross-tenant metrics | Low |

## Priority summary

| Priority | Item | Effort | Value |
|----------|------|--------|-------|
| High | Timesheet Approval (23) | Medium | High |
| High | Personal Dashboard (25) | Low | High |
| Medium | Team Calendar (24) | Medium | High |
| Medium | Geofencing (26) | Medium | Medium |
| Medium | Check-in Location Map (27) | Low | Medium |
| Low | Calendar Integration (28) | High | Medium |
| Low | Configurable Workflows (29) | High | High |
| Low | Admin Dashboard (30) | Medium | Medium |

## Rules for implementation

When implementing any EPIC 9 task:

1. Read `/cursor/core/` rules and `/cursor/prompts/00_master_prompt.md`
2. Update `/cursor/memory/domain_model.md` and `/cursor/memory/api_contracts.md`
3. Follow `HrPortal.Employees` as reference module
4. Mark the task file **COMPLETED** only after quality gates pass
5. Pass `/cursor/evals/` before marking complete
