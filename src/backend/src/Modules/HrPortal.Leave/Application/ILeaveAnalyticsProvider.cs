namespace HrPortal.Leave.Application;

public sealed record LeaveDaysByMonthRow(int Year, int Month, int Days);

public interface ILeaveAnalyticsProvider
{
    Task<int> GetApprovedLeaveDaysAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveDaysByMonthRow>> GetMonthlyLeaveTrendAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
}
