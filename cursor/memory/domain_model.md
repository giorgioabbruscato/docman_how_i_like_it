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
| Plan | string? | Subscription plan (default: `standard`) |
| FeaturesJson | string? | JSON array of enabled module features |
| SuspendedAt | DateTime? | Suspension timestamp |
| CreatedAt | DateTime | Creation timestamp |

**Factory:** `Tenant.Create(name, slug)`  
**Methods:** `Deactivate()`, `Activate()`, `SetPlan()`, `SetFeatures()`, `Suspend()`, `Unsuspend()`, `GetFeatures()`

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

---

### AttendanceRecord — IMPLEMENTED

**Location:** `HrPortal.Attendance.Domain`  
**Schema:** `attendance`

| Field | Type | Description |
|-------|------|-------------|
| EmployeeId | Guid | FK to Employee |
| Date | DateOnly | Work date |
| CheckIn | TimeOnly? | Clock-in time |
| CheckOut | TimeOnly? | Clock-out time |
| Status | AttendanceStatus | Present, Absent, Late, HalfDay |
| Notes | string? | Optional notes |

**Enum:** `AttendanceStatus`: Present, Absent, Late, HalfDay, Remote

**Business rules:**
- One record per employee per date per tenant
- CheckOut must be after CheckIn
- Cannot check out without checking in first

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

## Entity relationship diagram

```
Tenant (platform)
  │
  ├── TenantRole (platform) — PLANNED
  ├── TenantMembership (platform) — PLANNED
  │       └── EmployeeId? → Employee
  │
  ├── Employee (employees) ──→ Department (departments)
  │       │
  │       ├── LeaveRequest (leave)
  │       ├── AttendanceRecord (attendance)
  │       └── Document (documents)
  │
  └── Department (departments)
        └── ParentDepartmentId → Department (self-ref)

UserProfile (platform, global) — PLANNED
```

## Cross-module lookup interfaces

See also: `cursor/memory/module_dependencies.md` for the full dependency graph.

| Interface | Module | Method |
|-----------|--------|--------|
| `IDepartmentLookup` | Departments | `ExistsAndIsActiveAsync(Guid departmentId)` |
| `IEmployeeLookup` | Employees | `ExistsAndIsActiveAsync(Guid employeeId)` |
