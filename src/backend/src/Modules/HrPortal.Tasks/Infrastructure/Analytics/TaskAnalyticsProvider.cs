using HrPortal.Tasks.Application;
using HrPortal.Tasks.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Tasks.Infrastructure.Analytics;

internal sealed class TaskAnalyticsProvider : ITaskAnalyticsProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public TaskAnalyticsProvider(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<IReadOnlyList<ProjectTaskSpentHoursRow>> GetSpentHoursByProjectAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ProjectTask>()
            .ApplyTenantScope(_accessor.Current);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        var tasks = await query
            .Select(t => new { t.ProjectId, t.SpentHours })
            .ToListAsync(cancellationToken);

        return tasks
            .GroupBy(t => t.ProjectId)
            .Select(g => new ProjectTaskSpentHoursRow(g.Key, g.Sum(t => t.SpentHours)))
            .ToList();
    }
}
