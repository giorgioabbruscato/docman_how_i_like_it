using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Employees.Infrastructure.Persistence;

internal sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public EmployeeRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => e.Email == email.ToLowerInvariant());

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => e.IsActive)
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetActiveEmployeeIdsInDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => e.IsActive && e.DepartmentId == departmentId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Guid?>> GetDepartmentIdsByEmployeeIdsAsync(
        IReadOnlyList<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
            return new Dictionary<Guid, Guid?>();

        return await _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => employeeIds.Contains(e.Id))
            .Select(e => new { e.Id, e.DepartmentId })
            .ToDictionaryAsync(e => e.Id, e => e.DepartmentId, cancellationToken);
    }

    public async Task<int> CountActiveEmployeesAsync(
        IReadOnlyList<Guid>? restrictToEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Employee>()
            .ApplyTenantScope(_accessor.Current)
            .Where(e => e.IsActive);

        if (restrictToEmployeeIds is not null)
            query = query.Where(e => restrictToEmployeeIds.Contains(e.Id));

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Employee>().AddAsync(employee, cancellationToken);

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<Employee>().Update(employee);
        return Task.CompletedTask;
    }
}
