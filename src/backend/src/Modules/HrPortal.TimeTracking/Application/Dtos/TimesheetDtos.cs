namespace HrPortal.TimeTracking.Application.Dtos;

public sealed record TimesheetSubmissionDto(
    Guid Id,
    Guid EmployeeId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int TotalWorkedMinutes,
    string Status,
    string? Notes,
    DateTime? SubmittedAt,
    IReadOnlyList<Guid> TimeEntryIds,
    TimesheetApprovalDto? LatestApproval);

public sealed record TimesheetApprovalDto(
    Guid Id,
    Guid DecidedBy,
    string Decision,
    string? Comment,
    DateTime DecidedAt);

public sealed record CreateTimesheetRequest(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string? Notes = null);

public sealed record RejectTimesheetRequest(string? Comment = null);

public sealed record GetTimesheetsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? EmployeeId = null,
    string? Status = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null);
