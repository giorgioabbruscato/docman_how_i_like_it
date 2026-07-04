namespace HrPortal.Attendance.Application.Dtos;

public sealed record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    DateOnly Date,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string Status,
    string? Notes);

public sealed record CheckInRequest(
    Guid EmployeeId,
    DateOnly? Date = null,
    TimeOnly? Time = null);

public sealed record CheckOutRequest(
    Guid EmployeeId,
    DateOnly? Date = null,
    TimeOnly? Time = null);

public sealed record AttendanceReportDto(
    DateOnly From,
    DateOnly To,
    int TotalRecords,
    int PresentCount,
    int AbsentCount,
    int LateCount,
    int HalfDayCount,
    int RemoteCount);
