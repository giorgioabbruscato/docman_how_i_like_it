using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Tenancy.Infrastructure.Persistence;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly DbContext _dbContext;

    public TenantRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Tenant>().ToListAsync(cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Tenant>().AddAsync(tenant, cancellationToken);

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<Tenant>().Update(tenant);
        return Task.CompletedTask;
    }
}
