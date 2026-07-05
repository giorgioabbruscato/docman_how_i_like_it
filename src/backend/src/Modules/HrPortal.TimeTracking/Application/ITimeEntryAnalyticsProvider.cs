namespace HrPortal.TimeTracking.Application;

public sealed record MinutesByGuidRow(Guid Id, int Minutes);
public sealed record MinutesByDateRow(DateOnly Date, int Minutes);
public sealed record MinutesByMonthRow(int Year, int Month, int Minutes);
public sealed record ActiveTimerAnalyticsRow(Guid EmployeeId, Guid ProjectId, DateTime StartTime);

public interface ITimeEntryAnalyticsProvider
{
    Task<int> GetTotalMinutesAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MinutesByGuidRow>> GetMinutesByEmployeeAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MinutesByGuidRow>> GetMinutesByProjectAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MinutesByDateRow>> GetMinutesByDayAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MinutesByMonthRow>> GetMinutesByMonthAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<int> GetOvertimeMinutesAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        int dailyStandardMinutes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActiveTimerAnalyticsRow>> GetActiveTimersAsync(
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
}
