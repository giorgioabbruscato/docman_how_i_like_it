namespace HrPortal.Tasks.Application;

public sealed record ProjectTaskSpentHoursRow(Guid ProjectId, decimal SpentHours);

public interface ITaskAnalyticsProvider
{
    Task<IReadOnlyList<ProjectTaskSpentHoursRow>> GetSpentHoursByProjectAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default);
}
