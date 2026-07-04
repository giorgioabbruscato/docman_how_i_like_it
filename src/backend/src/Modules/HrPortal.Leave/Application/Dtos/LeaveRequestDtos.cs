namespace HrPortal.Leave.Application.Dtos;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Type,
    string Status,
    string? Reason,
    Guid? ApprovedBy,
    DateTime? ApprovedAt);

public sealed record CreateLeaveRequest(
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Type,
    string? Reason = null);

public sealed record RejectLeaveRequest(string? Reason = null);
