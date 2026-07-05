using HrPortal.Tasks.Application;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Tasks.Infrastructure.Persistence;

internal sealed class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public ProjectTaskRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectTask>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<PagedResult<ProjectTask>> GetPagedAsync(
        GetProjectTasksQuery query,
        CancellationToken cancellationToken = default)
    {
        var tasks = _dbContext.Set<ProjectTask>()
            .ApplyTenantScope(_accessor.Current)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            tasks = tasks.Where(t => t.Title.ToLower().Contains(search));
        }

        if (query.ProjectId.HasValue)
            tasks = tasks.Where(t => t.ProjectId == query.ProjectId.Value);

        if (query.Status.HasValue)
            tasks = tasks.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            tasks = tasks.Where(t => t.Priority == query.Priority.Value);

        if (query.AssignedEmployeeId.HasValue)
            tasks = tasks.Where(t => t.AssignedEmployeeId == query.AssignedEmployeeId.Value);

        var totalCount = await tasks.CountAsync(cancellationToken);

        var items = await tasks
            .OrderBy(t => t.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectTask>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectTask>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectTask>()
            .ApplyTenantScope(_accessor.Current)
            .AnyAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectTask>().AddAsync(task, cancellationToken);

    public Task UpdateAsync(ProjectTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ProjectTask>().Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProjectTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ProjectTask>().Remove(task);
        return Task.CompletedTask;
    }
}
