# TASK 03 — BACKEND MODULES

> Status: **IN PROGRESS**

Business domain modules following Clean Architecture.

## Module requirements

Each module must follow:

- Clean Architecture (Domain → Application → Infrastructure)
- Service layer required (`I{Entity}Service`)
- Repository abstraction required (`I{Entity}Repository`)
- DTO validation required (FluentValidation)
- Unit + integration tests required
- EF configuration with schema separation
- Controller in `HrPortal.Api/Controllers/V1/`

## Reference implementation

Use `HrPortal.Employees` as the canonical template. Copy structure exactly.

## Modules

### Employees — COMPLETED

- [x] Domain entity with factory methods
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration
- [x] Controller with policy-based auth
- [ ] Unit tests for domain + service
- [ ] Integration tests for all endpoints

### Departments — COMPLETED

- [x] Domain entity with hierarchy support (`ParentDepartmentId`)
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration
- [x] `IDepartmentLookup` for cross-module queries
- [x] Controller with policy-based auth
- [ ] Unit tests
- [ ] Integration tests

### LeaveManagement — PLANNED

**Domain:** `LeaveRequest` entity
- Fields: EmployeeId, StartDate, EndDate, Type, Status, Reason
- States: Pending → Approved/Rejected/Cancelled
- Business rules: no overlapping requests, max days per year

**Endpoints:**
```
GET    /api/v1/leave-requests
GET    /api/v1/leave-requests/{id}
POST   /api/v1/leave-requests
PUT    /api/v1/leave-requests/{id}/approve
PUT    /api/v1/leave-requests/{id}/reject
DELETE /api/v1/leave-requests/{id}
```

### Attendance — PLANNED

**Domain:** `AttendanceRecord` entity
- Fields: EmployeeId, Date, CheckIn, CheckOut, Status
- Business rules: one record per employee per day

**Endpoints:**
```
GET    /api/v1/attendance
POST   /api/v1/attendance/check-in
POST   /api/v1/attendance/check-out
GET    /api/v1/attendance/reports
```

### Documents — PLANNED

**Domain:** `Document` entity
- Fields: EmployeeId, FileName, ContentType, Size, StoragePath
- Uses `IStorageProvider` for file operations
- Business rules: max file size, allowed MIME types

**Endpoints:**
```
GET    /api/v1/documents
GET    /api/v1/documents/{id}
POST   /api/v1/documents          (multipart upload)
GET    /api/v1/documents/{id}/download
DELETE /api/v1/documents/{id}
```

## Adding a new module — step by step

1. Create `src/Modules/HrPortal.{Module}/` with Domain, Application, Infrastructure
2. Define entity in Domain with factory methods
3. Create DTOs and validators in Application
4. Create service interface + implementation
5. Create repository interface + EF implementation
6. Add EF configuration with schema name
7. Create `{Module}ServiceCollectionExtensions.cs`
8. Register in `Program.cs`
9. Add controller in `HrPortal.Api/Controllers/V1/`
10. Add migration
11. Write unit + integration tests
12. Update `/cursor/memory/domain_model.md` and `/cursor/memory/api_contracts.md`

## Acceptance criteria

- [ ] All implemented modules pass quality checks (`evals/01_backend_quality_checks.md`)
- [ ] `dotnet test` passes
- [ ] Cross-module lookups use public interfaces only

## Next task

→ `04_auth_backend.md` — Authorization policies and enforcement
