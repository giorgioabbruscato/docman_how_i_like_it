using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.AccessControl.Infrastructure.Persistence;

internal sealed class TenantRoleRepository : ITenantRoleRepository
{
    private readonly DbContext _dbContext;

    public TenantRoleRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<TenantRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantRole>().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<TenantRole?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantRole>()
            .FirstOrDefaultAsync(r => r.Slug == TenantRole.NormalizeSlug(slug), cancellationToken);

    public async Task<IReadOnlyList<TenantRole>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantRole>()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Slug)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantRole>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _dbContext.Set<TenantRole>()
            .Where(r => ids.Contains(r.Id) && r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TenantRole>()
            .Where(r => r.Slug == TenantRole.NormalizeSlug(slug));

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsForTenantAsync(
        Guid tenantId,
        string slug,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantRole>()
            .IgnoreQueryFilters()
            .AnyAsync(
                r => r.TenantId == tenantId && r.Slug == TenantRole.NormalizeSlug(slug),
                cancellationToken);

    public async Task AddAsync(TenantRole role, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantRole>().AddAsync(role, cancellationToken);

    public Task UpdateAsync(TenantRole role, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TenantRole>().Update(role);
        return Task.CompletedTask;
    }
}

internal sealed class TenantMembershipRepository : ITenantMembershipRepository
{
    private readonly DbContext _dbContext;

    public TenantMembershipRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<TenantMembership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantMembership>().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<TenantMembership?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantMembership>()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive, cancellationToken);

    public async Task<IReadOnlyList<TenantMembership>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantMembership>()
            .Where(m => m.IsActive)
            .OrderBy(m => m.UserId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ActiveMembershipExistsAsync(
        Guid userId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TenantMembership>()
            .Where(m => m.UserId == userId && m.IsActive);

        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(TenantMembership membership, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<TenantMembership>().AddAsync(membership, cancellationToken);

    public Task UpdateAsync(TenantMembership membership, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TenantMembership>().Update(membership);
        return Task.CompletedTask;
    }
}

internal sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly DbContext _dbContext;

    public UserProfileRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<UserProfile>().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<UserProfile>().AddAsync(profile, cancellationToken);

    public Task UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<UserProfile>().Update(profile);
        return Task.CompletedTask;
    }
}
