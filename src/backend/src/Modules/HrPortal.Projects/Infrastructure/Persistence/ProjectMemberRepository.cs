using HrPortal.Projects.Application;
using HrPortal.Projects.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Projects.Infrastructure.Persistence;

internal sealed class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public ProjectMemberRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<ProjectMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectMember>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectMember>()
            .ApplyTenantScope(_accessor.Current)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(
        Guid projectId,
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectMember>()
            .ApplyTenantScope(_accessor.Current)
            .AnyAsync(m => m.ProjectId == projectId && m.EmployeeId == employeeId, cancellationToken);

    public async Task AddAsync(ProjectMember member, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ProjectMember>().AddAsync(member, cancellationToken);

    public Task RemoveAsync(ProjectMember member, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ProjectMember>().Remove(member);
        return Task.CompletedTask;
    }
}
