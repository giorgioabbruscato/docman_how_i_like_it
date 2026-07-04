# TASK 05 — MODULE INTEGRATION

> Status: **IN PROGRESS**

Cross-module communication patterns and shared abstractions.

## Rules

1. Modules **never** access another module's DbSet or tables
2. Cross-module data needs use **public application interfaces**
3. Each module exposes only what other modules need via lookup interfaces

## Implemented cross-module interfaces

| Interface | Provider | Consumer | Purpose |
|-----------|----------|----------|---------|
| `IDepartmentLookup` | Departments | Employees | Validate department exists |
| `IUnitOfWork` | Api/Infrastructure | All modules | Transaction boundary |

## Patterns for cross-module communication

### Lookup interface (read-only)

When Module A needs to validate/reference data from Module B:

```csharp
// In HrPortal.Departments
public interface IDepartmentLookup
{
    Task<bool> ExistsAsync(Guid departmentId, CancellationToken ct);
}

// In HrPortal.Employees — inject IDepartmentLookup in service
```

### Application event (future)

For side effects that don't need synchronous response:

```csharp
// Future: domain events for audit, notifications
public record EmployeeCreatedEvent(Guid EmployeeId, Guid TenantId);
```

## Planned cross-module dependencies

| Consumer | Provider | Interface |
|----------|----------|-----------|
| Leave | Employees | `IEmployeeLookup` |
| Attendance | Employees | `IEmployeeLookup` |
| Documents | Employees | `IEmployeeLookup` |
| Documents | Storage | `IStorageProvider` (platform) |
| Leave | Notifications | `INotificationService` (platform) |

## Module registration order in Program.cs

```csharp
builder.Services.AddTenancy();
builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddStorage(builder.Configuration);
builder.Services.AddAudit();
builder.Services.AddDepartments();    // before Employees (lookup dependency)
builder.Services.AddEmployees();
// builder.Services.AddLeave();
// builder.Services.AddAttendance();
// builder.Services.AddDocuments();
```

## Acceptance criteria

- [x] `IDepartmentLookup` implemented and used by Employees
- [ ] `IEmployeeLookup` for upcoming modules
- [ ] No circular dependencies between modules
- [ ] Module dependency graph documented in memory

## Next task

→ `06_frontend.md` — Frontend pages and API integration
