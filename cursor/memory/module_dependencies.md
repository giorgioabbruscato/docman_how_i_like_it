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
    ├── Leave        (+ Notifications)
    ├── Attendance
    └── Documents    (+ Storage)
```

The graph is a DAG: no module references another module transitively back to itself.

## Allowed module-to-module references

| Consumer | Provider | Interface | Purpose |
|----------|----------|-----------|---------|
| Employees | Departments | `IDepartmentLookup` | Validate department on create/update |
| Leave | Employees | `IEmployeeLookup` | Validate employee on leave request |
| Attendance | Employees | `IEmployeeLookup` | Validate employee on clock-in/out |
| Documents | Employees | `IEmployeeLookup` | Validate employee on document upload |

## Lookup interface contracts

| Interface | Provider module | Method |
|-----------|-----------------|--------|
| `IDepartmentLookup` | Departments | `ExistsAndIsActiveAsync(Guid departmentId)` |
| `IEmployeeLookup` | Employees | `ExistsAndIsActiveAsync(Guid employeeId)` |

Implementations live in the provider's application service (`IDepartmentService`, `IEmployeeService`) and are registered as the lookup interface in `{Module}ServiceCollectionExtensions`.

## Platform dependencies (not module-to-module)

| Consumer | Platform service | Interface |
|----------|------------------|-----------|
| All modules | SharedKernel | `IUnitOfWork` |
| All modules | Audit | `IAuditService` |
| HrPortal.Api | AccessControl | `IMeService`, `ITenantRoleService`, `ITenantMembershipService`, `IPolicyEngine` |
| HrPortal.Authorization | AccessControl | `IPolicyEngine`, `IPermissionEvaluator` |
| Leave | Notifications | `INotificationService` |
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
→ AddDepartmentsModule → AddEmployeesModule
→ AddLeaveModule → AddAttendanceModule → AddDocumentsModule
```
