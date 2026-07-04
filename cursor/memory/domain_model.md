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
| CreatedAt | DateTime | Creation timestamp |

**Factory:** `Tenant.Create(name, slug)`  
**Methods:** `Deactivate()`, `Activate()`

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
  ├── Employee (employees) ──→ Department (departments)
  │       │
  │       ├── LeaveRequest (leave)
  │       ├── AttendanceRecord (attendance)
  │       └── Document (documents)
  │
  └── Department (departments)
        └── ParentDepartmentId → Department (self-ref)
```

## Cross-module lookup interfaces

| Interface | Module | Method |
|-----------|--------|--------|
| `IDepartmentLookup` | Departments | `ExistsAndIsActiveAsync(Guid departmentId)` |
| `IEmployeeLookup` | Employees | `ExistsAndIsActiveAsync(Guid employeeId)` |
