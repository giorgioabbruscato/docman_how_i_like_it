namespace HrPortal.AccessControl.Application;

public sealed record ResourceContext(
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    Guid? TenantId = null);
