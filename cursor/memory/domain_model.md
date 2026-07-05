# DOMAIN MODEL

> Source of truth for all domain entities. Update this file when adding or modifying entities.

## Base types (SharedKernel)

### AuditableEntity

All business entities inherit from `AuditableEntity`:

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| TenantId | Guid | Multi-tenant isolation |
| CreatedAt | DateTime | UTC creation timestamp |
| CreatedBy | Guid? | User who created |
| UpdatedAt | DateTime? | UTC last update |
| UpdatedBy | Guid? | User who last updated |

### ITenantEntity

Interface requiring `TenantId` — enables EF global query filters.

---

## Platform entities

### Tenant

**Location:** `HrPortal.Tenancy.Domain`  
**Schema:** `platform`

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| Name | string | Display name |
| Slug | string | URL-safe identifier (lowercase) |
| IsActive | bool | Active flag |
| Plan | string? | `TenantPlan` enum as string: `Free` \| `Pro` \| `Enterprise` |
| ModulesJson | string? | JSON array of enabled module keys (e.g. `employees`, `leave`) — the OSS "which modules are on" list |
| FeaturesJson | string? | JSON `TenantFeaturesOverrides` — partial per-tenant overrides layered on top of plan defaults; `null` field = inherit plan default |
| IsSuspended | bool | Platform-admin suspension flag (blocks all tenant-scoped access with `404`) |
| SuspendedAt | DateTime? | Suspension timestamp |
| CreatedAt | DateTime | Creation timestamp |

**Factory:** `Tenant.Create(name, slug)`  
**Methods:** `Deactivate()`, `Activate()`, `Suspend()`, `Unsuspend()`,
`GetPlan()`/`SetPlan(TenantPlan)`, `GetModules()`/`SetModules(IReadOnlyList<string>)`,
`GetFeatureOverrides()`/`SetFeatureOverrides(TenantFeaturesOverrides)`,
`GetEffectiveFeatures()` — merges plan defaults (`TenantFeaturesDefaults.ForPlan`) with overrides

#### TenantPlan / TenantFeatures — IMPLEMENTED (Task 24)

**Location:** `HrPortal.Tenancy.Domain`

- `TenantPlan` enum: `Free`, `Pro`, `Enterprise`
- `TenantFeatures` record: `MaxEmployees` (int), `CustomRoles` (bool), `AuditLog` (bool), `AdvancedReports` (bool)
- `TenantFeaturesOverrides` record: same shape, all fields nullable (partial override)
- `TenantFeaturesDefaults.ForPlan(plan)`: Free → 20/false/false/false; Pro → 200/true/true/false;
  Enterprise → unlimited/true/true/true
- `IFeatureGateService` (`HrPortal.Tenancy.Application`/`Infrastructure`) resolves effective features per
  request; in `TenantDeploymentMode.Single` (OSS) it always returns Enterprise-equivalent features
  regardless of the persisted plan

---

### Access Control entities — IMPLEMENTED

**Module:** `HrPortal.AccessControl.Domain`  
**Schema:** `platform`

#### TenantRole

| Field | Type | Description |
|-------|------|-------------|
| Slug | string | URL-safe role identifier (e.g. `admin`, `hr`, `manager`, `employee`) |
| PermissionsJson | string | JSON array of permission strings |
| IsSystem | bool | System-defined role (cannot be deleted) |
| IsActive | bool | Active flag |

**Factory:** `TenantRole.Create(tenantId, slug, permissions, isSystem)`  
**Methods:** `UpdatePermissions(...)`, `Deactivate()`

#### TenantMembership

| Field | Type | Description |
|-------|------|-------------|
| UserId | Guid | Keycloak user ID |
| RoleIdsJson | string | JSON array of TenantRole IDs |
| EmployeeId | Guid? | Optional link to Employee record |
| AttributesJson | string? | JSON key-value attributes for ABAC |
| IsActive | bool | Active membership flag |

**Factory:** `TenantMembership.Create(tenantId, userId, roleIds, employeeId?, attributes?)`  
**Methods:** `UpdateRoles(...)`, `Deactivate()`

#### UserProfile

| Field | Type | Description |
|-------|------|-------------|
| UserId | Guid | Keycloak user ID (unique) |
| Email | string | User email (lowercase) |
| IsPlatformAdmin | bool | Platform-level admin flag |

**Factory:** `UserProfile.Create(userId, email, isPlatformAdmin?)`

#### Permissions catalog

Canonical permission strings in `Permissions.cs`:

- Format: `{resource}.{action}:{scope}` (e.g. `employee.read:tenant`, `leave.approve:team`)
- Scopes: `Self`, `Department`, `Team`, `Tenant`, `All`

System role templates in `SystemRoleTemplates.cs`: `admin`, `hr`, `manager`, `employee` with default permission sets.

Legacy Keycloak realm roles mapped via `LegacyRoleMapper` during migration period.

---

### AuditLog — IMPLEMENTED (extended Task 25)

**Location:** `HrPortal.Audit.Domain`  
**Schema:** `platform`

| Field | Type | Description |
|-------|------|-------------|
| UserId | Guid | Actor's user ID |
| ActorEmail | string? | Actor's email at time of action |
| Action | string | Business action or permission string (e.g. `employee.read:tenant`) |
| Entity | string | Entity type name |
| EntityId | string? | Affected entity ID |
| TargetId | string? | Resource target ID for access-decision entries (may differ from EntityId) |
| Scope | string? | ABAC scope of the decision: `Self`, `Department`, `Team`, `Tenant`, `All` |
| Decision | string? | `Allow` \| `Deny` — set only for access-decision entries, `null` for business-mutation entries |
| IpAddress | string? | Caller IP for access-decision entries |
| Metadata | string? | Optional JSON metadata |
| Timestamp | DateTime | UTC event time |

**Factory:** `AuditLog.Create(tenantId, userId, action, entity, entityId?, targetId?, scope?, decision?, ipAddress?, actorEmail?, metadata?)`  
**Immutability:** No update/delete methods exist; `HrPortalDbContext.SaveChanges` throws if an `AuditLog` entry is modified or deleted after creation.

**Written by:** `IAuditService`
- `LogAsync` / `LogForTenantAsync` — business mutations (e.g. `employee.created`), saved with the caller's unit-of-work
- `LogAccessDecisionAsync` — one row per permission check from `PermissionAuthorizationHandler` /
  `PermissionAnyAuthorizationHandler`, saved **immediately** (`saveImmediately: true`) so read-only (GET)
  requests are captured even though they have no other pending `SaveChanges` call

**Read by:** `IAuditQueryService.QueryAsync(AuditLogQuery)` — filtered (date range, actor, action, decision), paginated (`PagedResult<AuditLogDto>`), tenant-scoped. Exposed via `GET /api/v1/audit-logs` (`audit.read:tenant` permission + `auditLog` plan feature).

---

### TenantContext — IMPLEMENTED

**Location:** `HrPortal.Tenancy`  
**Type:** Request-scoped record implementing `ITenantContext` (not a database entity)

Sole identity object for application services. Enriched per request by `TenantContextFactory` (AccessControl). See ADR-012 for full field list.

| Field | Type | Description |
|-------|------|-------------|
| TenantId | Guid | Resolved tenant primary key |
| TenantSlug | string | URL-safe tenant identifier |
| UserId | Guid? | Authenticated user (Keycloak sub) |
| Email | string? | User email |
| Mode | TenantDeploymentMode | `Single` or `Multi` |
| Roles | IReadOnlyList\<string\> | Legacy Keycloak realm roles |
| RoleSlugs | IReadOnlyList\<string\> | Tenant role slugs from membership |
| Permissions | IReadOnlyList\<string\> | Resolved permission strings |
| EmployeeId | Guid? | Linked employee record |
| DepartmentId | Guid? | Linked department (ABAC scope) |
| Attributes | IReadOnlyDictionary\<string, string\> | Membership attributes |
| Features | IReadOnlyList\<string\> | Tenant feature flags |
| IsPlatformAdmin | bool | Platform-level admin flag |
| IsResolved | bool | Tenant successfully resolved |

**Factory methods:** `Empty`, `CreateTenantOnly()`, `CreateSingleTenantDefault()`  
**Helper:** `HasPermission(string permission)`

---

## Business entities

### Employee — IMPLEMENTED

**Location:** `HrPortal.Employees.Domain`  
**Schema:** `employees`

| Field | Type | Description |
|-------|------|-------------|
| FirstName | string | Given name |
| LastName | string | Family name |
| Email | string | Work email (lowercase) |
| JobTitle | string? | Position title |
| DepartmentId | Guid? | FK to Department |
| HireDate | DateOnly | Employment start date |
| IsActive | bool | Active employee flag |

**Computed:** `FullName` → `"{FirstName} {LastName}"`

**Factory:** `Employee.Create(tenantId, firstName, lastName, email, hireDate, jobTitle?, departmentId?, createdBy?)`  
**Methods:** `Update(...)`, `Deactivate(updatedBy)`

**Business rules:**
- Email stored lowercase
- Deactivation is soft delete (IsActive = false)
- DepartmentId validated via `IDepartmentLookup`

**Cross-module interface:** `IEmployeeLookup.ExistsAndIsActiveAsync(employeeId)`

---

### Department — IMPLEMENTED

**Location:** `HrPortal.Departments.Domain`  
**Schema:** `departments`

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Department name |
| Code | string | Short code (uppercase) |
| Description | string? | Optional description |
| ParentDepartmentId | Guid? | Self-referencing FK for hierarchy |
| IsActive | bool | Active flag |

**Factory:** `Department.Create(tenantId, name, code, description?, parentDepartmentId?, createdBy?)`  
**Methods:** `Update(...)`, `Deactivate(updatedBy)`

**Business rules:**
- Code stored uppercase
- Supports hierarchical structure via ParentDepartmentId
- Deactivation is soft delete

**Cross-module interface:** `IDepartmentLookup.ExistsAndIsActiveAsync(departmentId)`

---

### LeaveRequest — IMPLEMENTED

**Location:** `HrPortal.Leave.Domain`  
**Schema:** `leave`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | FK to Employee |
| StartDate | DateOnly | Leave start |
| EndDate | DateOnly | Leave end |
| Type | LeaveType | Annual, Sick, Personal, etc. |
| Status | LeaveStatus | Pending, Approved, Rejected, Cancelled |
| Reason | string? | Optional reason |
| ApprovedBy | Guid? | Manager who approved |
| ApprovedAt | DateTime? | Approval timestamp |

**Enums:**
- `LeaveType`: Annual, Sick, Personal, Maternity, Paternity, Unpaid
- `LeaveStatus`: Pending, Approved, Rejected, Cancelled

**Business rules:**
- No overlapping approved requests for same employee
- EndDate must be >= StartDate
- Only Pending requests can be approved/rejected
- Employee can cancel own Pending requests

- Max 25 annual leave days per employee per year (Annual type only)

**Notifications:** On approval, `NotifyLeaveApprovedAsync` fires fire-and-forget to the requesting employee's user (via `INotificationRecipientResolver`).

---

### Notification events — IMPLEMENTED (Task 20)

Platform service: `INotificationService` (`HrPortal.Notifications`). Current implementation logs structured events (no email/SMS).

| Event | Type string | Trigger | Recipient |
|-------|-------------|---------|-----------|
| Project assigned | `project.assigned` | After `AddProjectMemberCommand` succeeds | Assigned employee's user |
| Task assigned | `task.assigned` | Task create/update when assignee set/changed | Assigned employee's user |
| Leave approved | `leave.approved` | After leave approval | Requesting employee's user |
| Document uploaded | `document.uploaded` | After document upload | Document owner's user |
| Forgotten check-in | `attendance.forgotten_check_in` | Hourly reminder job | Employee's user |
| Forgotten check-out | `attendance.forgotten_check_out` | Hourly reminder job | Employee's user |
| Timesheet submitted | `timesheet.submitted` | After timesheet submit | Department managers |
| Timesheet approved | `timesheet.approved` | After timesheet approve | Submitting employee's user |
| Timesheet rejected | `timesheet.rejected` | After timesheet reject | Submitting employee's user |

Recipient resolution: `INotificationRecipientResolver` maps `EmployeeId` → `UserId` via active tenant membership; email used as log fallback when no membership. Dispatches persist a `UserNotification` row for inbox APIs (Task 25).

---

### AttendanceSession — IMPLEMENTED

**Location:** `HrPortal.Attendance.Domain`
**Schema:** `attendance`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | FK to Employee |
| CheckIn | DateTime | UTC check-in timestamp |
| CheckOut | DateTime? | UTC check-out timestamp (null = open) |
| LatitudeCheckIn | double? | GPS latitude at check-in |
| LongitudeCheckIn | double? | GPS longitude at check-in |
| LatitudeCheckOut | double? | GPS latitude at check-out |
| LongitudeCheckOut | double? | GPS longitude at check-out |
| AccuracyCheckIn | double? | GPS accuracy (meters) at check-in |
| AccuracyCheckOut | double? | GPS accuracy (meters) at check-out |
| IPAddress | string? | Caller IP address |
| Device | string? | Device metadata |
| Browser | string? | Browser metadata |
| WorkedMinutes | int? | Computed on check-out |
| Status | AttendanceSessionStatus | Open, Closed, AutoClosed |
| MatchedGeofenceZoneId | Guid? | Zone matched at check-in when geofencing enabled |
| GpsUnavailableAtCheckIn | bool | True when GPS missing but allowed by policy |

**Enum:** `AttendanceSessionStatus`: Open, Closed, AutoClosed

**Business rules:**
- One open session per employee per tenant (partial unique index)
- CheckOut must be after CheckIn
- WorkedMinutes = (CheckOut - CheckIn).TotalMinutes on close
- Cannot close an already-closed session

### GeofenceZone — IMPLEMENTED (Task 26)

**Location:** `HrPortal.Attendance.Domain`  
**Schema:** `attendance`

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Zone label |
| Latitude | double | Center latitude |
| Longitude | double | Center longitude |
| RadiusMeters | double | Haversine radius |
| IsActive | bool | Active flag |
| Description | string? | Optional notes |

### GeofenceSettings — IMPLEMENTED (Task 26)

Tenant singleton (`attendance` schema): `GeofencingEnabled` (default false), `AllowCheckInWithoutGps` (default true).

---

### Document — IMPLEMENTED

**Location:** `HrPortal.Documents.Domain`  
**Schema:** `documents`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | FK to Employee |
| FileName | string | Original file name |
| ContentType | string | MIME type |
| SizeBytes | long | File size |
| StoragePath | string | Path in IStorageProvider |
| Category | DocumentCategory | Contract, ID, Certificate, etc. |
| UploadedAt | DateTime | Upload timestamp |
| UploadedBy | Guid? | User who uploaded |

**Enum:** `DocumentCategory`: Contract, IdentityDocument, Certificate, Payslip, Other

**Business rules:**
- Max file size: 10 MB
- Allowed MIME types: PDF, JPEG, PNG, DOCX
- Storage path: `{tenantId}/employee/documents/{filename}`
- Uses `IStorageProvider` — never direct filesystem access

---

### Project — IMPLEMENTED

**Location:** `HrPortal.Projects.Domain`  
**Schema:** `projects`

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Required, max 200 |
| Description | string? | Optional |
| CustomerName | string? | Optional |
| Status | ProjectStatus | Active, OnHold, Completed, Cancelled |
| StartDate | DateOnly? | Optional |
| EndDate | DateOnly? | Must be >= StartDate when both set |
| BudgetHours | decimal? | >= 0 |
| BudgetCost | decimal? | >= 0 |
| IsArchived | bool | Soft archive flag |

**Factory:** `Project.Create(tenantId, name, status, ...)`  
**Methods:** `Update(...)`, `Archive(updatedBy)`

**Enum:** `ProjectStatus`: Active, OnHold, Completed, Cancelled

---

### ProjectMember — IMPLEMENTED

**Location:** `HrPortal.Projects.Domain`  
**Schema:** `projects`

| Field | Type | Description |
|-------|------|-------------|
| ProjectId | Guid | FK to Project |
| EmployeeId | Guid | Validated via `IEmployeeLookup` |
| Role | ProjectMemberRole | Lead, Member, Observer |
| HourlyRate | decimal? | >= 0 |

**Factory:** `ProjectMember.Create(tenantId, projectId, employeeId, role, hourlyRate?, createdBy?)`

**Enum:** `ProjectMemberRole`: Lead, Member, Observer

**Business rules:**
- Unique `(ProjectId, EmployeeId)` per tenant
- Only active employees can be assigned (via `IEmployeeLookup`)

---

### ProjectTask — IMPLEMENTED

**Location:** `HrPortal.Tasks.Domain`  
**Schema:** `tasks`

| Field | Type | Description |
|-------|------|-------------|
| ProjectId | Guid | Validated via `IProjectLookup` |
| Title | string | Required, max 300 |
| Description | string? | Optional |
| AssignedEmployeeId | Guid? | Validated via `IEmployeeLookup` when set |
| Priority | TaskPriority | Low, Medium, High, Critical |
| Status | TaskStatus | Todo, InProgress, Review, Done |
| EstimatedHours | decimal? | >= 0 |
| SpentHours | decimal | >= 0, default 0 |
| DueDate | DateOnly? | Optional |

**Factory:** `ProjectTask.Create(tenantId, projectId, title, priority, status?, ...)`  
**Methods:** `Update(...)`, `UpdateStatus(newStatus, updatedBy)`

**Enums:**
- `TaskPriority`: Low, Medium, High, Critical
- `TaskStatus`: Todo, InProgress, Review, Done

**Business rules:**
- ProjectId validated via `IProjectLookup.ExistsAsync`
- AssignedEmployeeId validated via `IEmployeeLookup.ExistsAndIsActiveAsync` when set
- Delete is hard delete (no soft-delete flag)
- Status transitions: free Kanban — any move between valid statuses allowed; no-op (same status) rejected

**Cross-module interface:** `ITaskLookup.ExistsAsync(taskId)`, `GetTitleAsync(taskId)`

---

### TimeEntry — IMPLEMENTED

**Location:** `HrPortal.TimeTracking.Domain`  
**Schema:** `time_tracking`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | From `TenantContext.EmployeeId` on create |
| ProjectId | Guid | Validated via `IProjectLookup` |
| TaskId | Guid? | Optional, validated via `ITaskLookup` |
| StartTime | DateTime | UTC |
| EndTime | DateTime? | UTC; null = active timer |
| WorkedMinutes | int | Computed when EndTime set |
| Description | string? | Max 1000 |
| Billable | bool | Default true |

**Factory:** `TimeEntry.Create(...)`, `TimeEntry.StartTimer(...)`  
**Methods:** `Stop(utcNow)`, `Update(...)`, `CalculateWorkedMinutes(start, end)`

**Business rules:**
- One active timer per employee (`EndTime == null`)
- No overlapping intervals for same employee
- Manual entries: max 1440 minutes/day (UTC date), start at 09:00 UTC on given date
- Read scope: self / team (department) / tenant via permissions

### TimesheetSubmission — IMPLEMENTED (Task 23)

**Location:** `HrPortal.TimeTracking.Domain`  
**Schema:** `time_tracking`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | Submitting employee |
| PeriodStart | DateOnly | Inclusive period start |
| PeriodEnd | DateOnly | Inclusive period end |
| TotalWorkedMinutes | int | Snapshot sum at create |
| Status | TimesheetStatus | Draft → Submitted → Approved/Rejected |
| Notes | string? | Optional employee notes |
| SubmittedAt | DateTime? | UTC submit timestamp |

**Enums:** `TimesheetStatus` (Draft, Submitted, Approved, Rejected), `ApprovalDecision` (Approved, Rejected)

**Related entities:** `TimesheetSubmissionEntry` (join to `TimeEntry`), `TimesheetApproval` (audit of decision)

**Business rules:** Only draft can submit; only submitted can approve/reject. Analytics and export count hours only from entries linked to **Approved** timesheets.

---

### UserNotification — IMPLEMENTED (Task 25)

**Location:** `HrPortal.Notifications.Domain`  
**Schema:** `platform`

| Field | Type | Description |
|-------|------|-------------|
| RecipientUserId | Guid | Inbox owner |
| Type | string | Event type key |
| Title | string | Display title |
| Body | string? | Optional body |
| MetadataJson | string? | JSON payload |
| IsRead | bool | Read flag |
| CreatedAt | DateTime | UTC dispatch time |

Persisted by `LoggingNotificationService` on every `DispatchAsync`.

---

### PublicHoliday — IMPLEMENTED (Task 24)

**Location:** `HrPortal.Calendar.Domain`  
**Schema:** `calendar`

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Holiday name |
| Date | DateOnly | Calendar date |
| IsRecurring | bool | Repeats yearly |
| CountryCode | string? | Optional ISO country |

### SmartWorkingSchedule — IMPLEMENTED (Task 24)

Tenant weekday configuration for remote-work calendar events (`calendar` schema).

---

## Entity relationship diagram

```
Tenant (platform)
  │   Plan, ModulesJson, FeaturesJson (overrides) → GetEffectiveFeatures()
  │
  ├── TenantRole (platform)
  ├── TenantMembership (platform)
  │       └── EmployeeId? → Employee
  ├── AuditLog (platform) — business mutations + access decisions (immutable)
  │
  ├── Employee (employees) ──→ Department (departments)
  │       │
  │       ├── LeaveRequest (leave)
  │       ├── AttendanceSession (attendance)
  │       ├── Document (documents)
  │       └── ProjectMember (projects) ──→ Project (projects)
  │               └── ProjectTask (tasks) ──→ Project (via IProjectLookup)
  │       └── TimeEntry (time_tracking) ──→ Project, Task?, Employee
  │
  ├── Project (projects)
  │       └── ProjectTask (tasks)
  │       └── TimeEntry (time_tracking)
  │
  └── Department (departments)
        └── ParentDepartmentId → Department (self-ref)

UserProfile (platform, global) — UserId (Keycloak sub, unique), Email, IsPlatformAdmin
```

## Cross-module lookup interfaces

See also: `cursor/memory/module_dependencies.md` for the full dependency graph.

| Interface | Module | Method |
|-----------|--------|--------|
| `IDepartmentLookup` | Departments | `ExistsAndIsActiveAsync(Guid departmentId)` |
| `IEmployeeLookup` | Employees | `ExistsAndIsActiveAsync`, `GetActiveEmployeeIdsInDepartmentAsync`, `GetFullNameAsync` |
| `IProjectLookup` | Projects | `ExistsAsync`, `GetNameAsync` |
| `ITaskLookup` | Tasks | `ExistsAsync`, `GetTitleAsync` |

---

## Analytics query models (read-only, not entities) — IMPLEMENTED (Tasks 13–15)

**Module:** `HrPortal.Analytics.Application.Dtos`  
No EF entities — KPI/chart/supervisor DTOs only.

| DTO | Purpose |
|-----|---------|
| `AnalyticsQueryParams` | Shared filters: departmentId, projectId, employeeId, fromDate, toDate |
| `AnalyticsFilter` | Resolved scope + date range + allowed employee IDs |
| `NamedHoursRow`, `DateHoursRow`, `MonthHoursRow` | KPI breakdown rows |
| `SupervisorSummaryDto` | All supervisor widget sections in one response |
| `EmployeeWorkingDto`, `AttendanceTodayDto`, `TopEmployeeDto`, `TopProjectDto` | Supervisor widgets |
| `BudgetUsageDto`, `LateArrivalDto`, `OvertimeEmployeeDto` | Supervisor widgets |
| `ChartResponseDto` / `ChartDatasetDto` | `{ labels, datasets }` chart JSON |

**Options:** `AnalyticsOptions` — `DailyStandardMinutes` (480), `LateCheckInTime` (09:00 UTC), Mon–Fri workdays.

**Permissions:** `analytics.read:team` (Manager), `analytics.read:tenant` (HR/Admin).

**Feature gate:** `FeatureKeys.AdvancedReports` (Enterprise plan; mirrored from audit log pattern).
