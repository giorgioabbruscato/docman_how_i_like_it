# MODULE DEPENDENCIES

> Source of truth for cross-module project references and lookup interfaces.

## Project dependency graph

```
Platform (SharedKernel, Tenancy, Identity, AccessControl, Audit, Storage, Notifications)
    ↑
Departments  (no module dependencies)
    ↑
Employees
    ↑
    ├── Projects
    │       └── Tasks
    │               └── TimeTracking
    ├── Leave        (+ Notifications)
    ├── Attendance
    ├── Documents    (+ Storage)
    └── Analytics    (read-only; aggregates via provider interfaces)
            ↑ depends on: Departments, Employees, TimeTracking, Attendance, Leave, Projects, Tasks
    └── Reporting    (export; reuses analytics/time-tracking data sources)
            ↑ depends on: Departments, Employees, TimeTracking, Attendance, Projects
```

The graph is a DAG: no module references another module transitively back to itself.

## Allowed module-to-module references

| Consumer | Provider | Interface | Purpose |
|----------|----------|-----------|---------|
| Employees | Departments | `IDepartmentLookup` | Validate department on create/update |
| Projects | Employees | `IEmployeeLookup` | Validate employee on project member assignment |
| Tasks | Projects | `IProjectLookup` | Validate project on task create/update |
| Tasks | Employees | `IEmployeeLookup` | Validate assignee on task create/update |
| TimeTracking | Employees | `IEmployeeLookup` | Validate employee, team scope, export names |
| TimeTracking | Projects | `IProjectLookup` | Validate project, export names |
| TimeTracking | Tasks | `ITaskLookup` | Validate task, export titles |
| Leave | Employees | `IEmployeeLookup` | Validate employee on leave request |
| Attendance | Employees | `IEmployeeLookup` | Validate employee on clock-in/out |
| Documents | Employees | `IEmployeeLookup` | Validate employee on document upload |
| Analytics | Departments | `IDepartmentLookup` | Enrich department breakdown labels |
| Analytics | Employees | `IEmployeeLookup` | Read scope, names, department batch lookup |
| Analytics | TimeTracking | `ITimeEntryAnalyticsProvider` | Hours KPIs, overtime, active timers |
| Analytics | Attendance | `IAttendanceAnalyticsProvider` | Attendance rate, late check-ins, open sessions |
| Analytics | Leave | `ILeaveAnalyticsProvider` | Leave rate, monthly leave trend |
| Analytics | Projects | `IProjectAnalyticsProvider` | Budget snapshots, member hourly rates |
| Analytics | Tasks | `ITaskAnalyticsProvider` | Task spent hours by project |
| Reporting | Departments | `IDepartmentRepository`, `IDepartmentLookup` | Department report rows |
| Reporting | Employees | `IEmployeeRepository`, `IEmployeeLookup` | Employee report rows, scope |
| Reporting | Attendance | `IAttendanceSessionRepository` | Attendance report rows |
| Reporting | Projects | `IProjectRepository`, `IProjectMemberRepository` | Project report rows |
| Reporting | TimeTracking | `ITimeEntryExportService`, `ITimeEntryAnalyticsProvider` | Worked-hours export, spent hours |

## Lookup interface contracts

| Interface | Provider module | Method |
|-----------|-----------------|--------|
| `IDepartmentLookup` | Departments | `ExistsAndIsActiveAsync`, `GetNameAsync` |
| `IEmployeeLookup` | Employees | `ExistsAndIsActiveAsync`, `GetActiveEmployeeIdsInDepartmentAsync`, `GetFullNameAsync`, `GetEmailAsync`, `GetDepartmentIdsAsync`, `CountActiveEmployeesAsync` |
| `IProjectLookup` | Projects | `ExistsAsync`, `GetNameAsync` |
| `ITaskLookup` | Tasks | `ExistsAsync`, `GetTitleAsync` |
| `ITimeEntryAnalyticsProvider` | TimeTracking | Aggregated minutes, overtime, active timers |
| `IAttendanceAnalyticsProvider` | Attendance | Sessions, late check-ins, present employee-days |
| `ILeaveAnalyticsProvider` | Leave | Approved leave days, monthly trend |
| `IProjectAnalyticsProvider` | Projects | Budget snapshots, member hourly rates |
| `ITaskAnalyticsProvider` | Tasks | Task spent hours by project |

Implementations live in the provider's application service (`IDepartmentService`, `IEmployeeService`) or dedicated lookup class (`ProjectLookup`, `TaskLookup`) and are registered in `{Module}ServiceCollectionExtensions`.

## Platform dependencies (not module-to-module)

| Consumer | Platform service | Interface |
|----------|------------------|-----------|
| All modules | SharedKernel | `IUnitOfWork` |
| All modules | Audit | `IAuditService` |
| HrPortal.Api | AccessControl | `IMeService`, `ITenantRoleService`, `ITenantMembershipService`, `IPolicyEngine` |
| HrPortal.Authorization | AccessControl | `IPolicyEngine`, `IPermissionEvaluator` |
| Leave | Notifications | `INotificationService` |
| Projects | Notifications | `INotificationService` (project/task assignment hooks) |
| Tasks | Notifications | `INotificationService` (task assignment hooks) |
| Documents | Notifications | `INotificationService` (document upload hook) |
| Attendance | Notifications | `INotificationService` (forgotten check-in/out reminders) |
| Projects, Tasks, Leave, Documents, Attendance | AccessControl | `INotificationRecipientResolver` (employee → user mapping) |
| Documents | Storage | `IStorageProvider` |

`HrPortal.AccessControl` must not reference business domain modules (Employees, Leave, etc.).

## Rules

1. Modules never access another module's DbSet, repository, or domain types directly.
2. Cross-module reads use public Application-layer lookup interfaces only.
3. No circular project references between modules (enforced by `ModuleDependencyTests`).
4. Provider modules are registered before consumers in `Program.cs`.

## Registration order (`Program.cs`)

```
AddTenancy → AddHrPortalIdentity → AddHrPortalAccessControl
→ AddHrPortalAuthorization → AddHrPortalStorage → AddHrPortalNotifications → AddHrPortalAudit
→ AddDepartmentsModule → AddEmployeesModule → AddProjectsModule → AddTasksModule
→ AddTimeTrackingModule → AddLeaveModule → AddAttendanceModule → AddAnalyticsModule → AddReportingModule → AddDocumentsModule
```
