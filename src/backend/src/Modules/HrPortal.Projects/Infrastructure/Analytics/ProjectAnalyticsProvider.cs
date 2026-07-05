using HrPortal.Projects.Application;
using HrPortal.Projects.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Projects.Infrastructure.Analytics;

internal sealed class ProjectAnalyticsProvider : IProjectAnalyticsProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public ProjectAnalyticsProvider(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<IReadOnlyList<ProjectBudgetSnapshot>> GetBudgetSnapshotsAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Project>()
            .ApplyTenantScope(_accessor.Current)
            .Where(p => !p.IsArchived);

        if (projectId.HasValue)
            query = query.Where(p => p.Id == projectId.Value);

        return await query
            .Select(p => new ProjectBudgetSnapshot(
                p.Id,
                p.Name,
                p.CustomerName,
                p.BudgetHours,
                p.BudgetCost))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectMemberRateRow>> GetMemberHourlyRatesAsync(
        IReadOnlyList<Guid>? projectIds,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ProjectMember>()
            .ApplyTenantScope(_accessor.Current);

        if (projectIds is not null)
            query = query.Where(m => projectIds.Contains(m.ProjectId));

        return await query
            .Select(m => new ProjectMemberRateRow(m.ProjectId, m.EmployeeId, m.HourlyRate))
            .ToListAsync(cancellationToken);
    }
}
