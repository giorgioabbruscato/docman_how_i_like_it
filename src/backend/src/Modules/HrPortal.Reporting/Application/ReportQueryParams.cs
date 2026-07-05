namespace HrPortal.Reporting.Application;

public sealed record ReportQueryParams(
    string Format,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? DepartmentId = null,
    Guid? ProjectId = null,
    Guid? EmployeeId = null);

public sealed record ReportGenerateFilter(
    IReadOnlyList<Guid>? AllowedEmployeeIds,
    Guid? EmployeeId);
