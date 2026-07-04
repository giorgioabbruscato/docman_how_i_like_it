# TASK 03 — BACKEND MODULES

> Status: **COMPLETED**

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
- [x] Unit tests for domain + service
- [x] Integration tests for all endpoints

### Departments — COMPLETED

- [x] Domain entity with hierarchy support (`ParentDepartmentId`)
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration
- [x] `IDepartmentLookup` for cross-module queries
- [x] Controller with policy-based auth
- [x] Unit tests
- [x] Integration tests

### LeaveManagement — COMPLETED

- [x] Domain: `LeaveRequest` entity with state machine
- [x] Business rules: no overlapping requests, max 25 annual days/year
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration (schema `leave`)
- [x] Controller with policy-based auth
- [x] Unit + integration tests

### Attendance — COMPLETED

- [x] Domain: `AttendanceRecord` entity
- [x] Business rules: one record per employee per day
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration (schema `attendance`)
- [x] Controller with policy-based auth
- [x] Unit + integration tests

### Documents — COMPLETED

- [x] Domain: `Document` entity
- [x] Uses `IStorageProvider` for file operations
- [x] Business rules: max 10 MB, allowed MIME types
- [x] Application service + DTOs + validators
- [x] Repository + EF configuration (schema `documents`)
- [x] Controller with multipart upload + download
- [x] Unit + integration tests

## Acceptance criteria

- [x] All implemented modules pass quality checks (`evals/01_backend_quality_checks.md`)
- [x] `dotnet test` passes
- [x] Cross-module lookups use public interfaces only (`IDepartmentLookup`, `IEmployeeLookup`)

## Next task

→ `04_auth_backend.md` — Authorization policies and enforcement
