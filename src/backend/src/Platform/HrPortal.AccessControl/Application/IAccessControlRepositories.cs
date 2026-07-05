using HrPortal.AccessControl.Domain;

namespace HrPortal.AccessControl.Application;

public interface ITenantRoleRepository
{
    Task<TenantRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantRole?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantRole>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsForTenantAsync(Guid tenantId, string slug, CancellationToken cancellationToken = default);
    Task AddAsync(TenantRole role, CancellationToken cancellationToken = default);
    Task UpdateAsync(TenantRole role, CancellationToken cancellationToken = default);
}

public interface ITenantMembershipRepository
{
    Task<TenantMembership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantMembership?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantMembership>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ActiveMembershipExistsAsync(Guid userId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(TenantMembership membership, CancellationToken cancellationToken = default);
    Task UpdateAsync(TenantMembership membership, CancellationToken cancellationToken = default);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default);
}
