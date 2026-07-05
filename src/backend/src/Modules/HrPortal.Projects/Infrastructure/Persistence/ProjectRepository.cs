using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Projects.Infrastructure.Persistence;

internal sealed class ProjectRepository : IProjectRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public ProjectRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Project>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Project>> GetPagedAsync(
        GetProjectsQuery query,
        CancellationToken cancellationToken = default)
    {
        var projects = _dbContext.Set<Project>()
            .ApplyTenantScope(_accessor.Current)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            projects = projects.Where(p => p.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.CustomerName))
            projects = projects.Where(p => p.CustomerName == query.CustomerName);

        if (query.Status.HasValue)
            projects = projects.Where(p => p.Status == query.Status.Value);

        if (query.IsArchived.HasValue)
            projects = projects.Where(p => p.IsArchived == query.IsArchived.Value);

        var totalCount = await projects.CountAsync(cancellationToken);

        var items = await projects
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Project>()
            .ApplyTenantScope(_accessor.Current)
            .AnyAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Project>().AddAsync(project, cancellationToken);

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<Project>().Update(project);
        return Task.CompletedTask;
    }
}
