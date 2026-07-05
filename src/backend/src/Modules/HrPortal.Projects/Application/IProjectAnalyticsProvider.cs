namespace HrPortal.Projects.Application;

public sealed record ProjectBudgetSnapshot(
    Guid ProjectId,
    string Name,
    string? CustomerName,
    decimal? BudgetHours,
    decimal? BudgetCost);

public sealed record ProjectMemberRateRow(Guid ProjectId, Guid EmployeeId, decimal? HourlyRate);

public interface IProjectAnalyticsProvider
{
    Task<IReadOnlyList<ProjectBudgetSnapshot>> GetBudgetSnapshotsAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectMemberRateRow>> GetMemberHourlyRatesAsync(
        IReadOnlyList<Guid>? projectIds,
        CancellationToken cancellationToken = default);
}
