using HrPortal.Departments.Application;
using HrPortal.Departments.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Departments.Infrastructure.Persistence;

internal sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public DepartmentRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .ApplyTenantScope(_accessor.Current)
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> CodeExistsAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = code.ToUpperInvariant();
        var query = _dbContext.Set<Department>()
            .ApplyTenantScope(_accessor.Current)
            .Where(d => d.Code == normalized);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasActiveChildrenAsync(Guid departmentId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>()
            .ApplyTenantScope(_accessor.Current)
            .AnyAsync(d => d.ParentDepartmentId == departmentId && d.IsActive, cancellationToken);

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Department>().AddAsync(department, cancellationToken);

    public Task UpdateAsync(Department department, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<Department>().Update(department);
        return Task.CompletedTask;
    }
}
