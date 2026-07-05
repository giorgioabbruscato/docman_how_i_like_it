namespace HrPortal.TimeTracking.Application.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record TimeEntryDto(
    Guid Id,
    Guid EmployeeId,
    Guid ProjectId,
    Guid? TaskId,
    DateTime StartTime,
    DateTime? EndTime,
    int WorkedMinutes,
    string? Description,
    bool Billable);

public sealed record CreateTimeEntryRequest(
    Guid ProjectId,
    DateTime StartTime,
    DateTime EndTime,
    Guid? TaskId = null,
    string? Description = null,
    bool Billable = true);

public sealed record UpdateTimeEntryRequest(
    Guid ProjectId,
    DateTime StartTime,
    DateTime EndTime,
    Guid? TaskId = null,
    string? Description = null,
    bool Billable = true);

public sealed record GetTimeEntriesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? EmployeeId = null,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    bool? Billable = null);

public sealed record StartTimerRequest(
    Guid ProjectId,
    Guid? TaskId = null,
    string? Description = null,
    bool Billable = true);

public sealed record CreateManualTimeEntryRequest(
    DateOnly Date,
    Guid ProjectId,
    decimal Hours,
    Guid? TaskId = null,
    string? Description = null,
    bool Billable = true);

public sealed record ExportTimeEntriesQuery(
    string Format,
    Guid? EmployeeId = null,
    Guid? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int? Month = null,
    int? Year = null);

public sealed record TimeEntryExportRow(
    DateOnly Date,
    string? EmployeeName,
    string ProjectName,
    string? TaskTitle,
    decimal Hours,
    string? Description,
    bool Billable);
