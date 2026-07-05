namespace HrPortal.Attendance.Application;

public sealed record AttendanceSessionAnalyticsRow(
    Guid EmployeeId,
    DateTime CheckIn,
    DateTime? CheckOut,
    bool IsOpen);

public sealed record PresentEmployeeDayRow(DateOnly Date, int Count);

public interface IAttendanceAnalyticsProvider
{
    Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetOpenSessionsAsync(
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetSessionsInRangeAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetLateCheckInsAsync(
        DateOnly date,
        TimeOnly lateThreshold,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<int> GetPresentEmployeeDaysAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PresentEmployeeDayRow>> GetDailyPresentCountsAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
}
