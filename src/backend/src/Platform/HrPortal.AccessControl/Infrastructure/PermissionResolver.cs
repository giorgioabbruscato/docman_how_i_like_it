using HrPortal.AccessControl.Application;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class PermissionResolver : IPermissionResolver
{
    private readonly ITenantRoleRepository _roleRepository;

    public PermissionResolver(ITenantRoleRepository roleRepository) =>
        _roleRepository = roleRepository;

    public async Task<IReadOnlyList<string>> ResolveAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetByIdsAsync(roleIds, cancellationToken);
        var permissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in roles)
        {
            foreach (var permission in role.GetPermissions())
                permissions.Add(permission);
        }

        return permissions.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    public async Task<IReadOnlyList<string>> ResolveRoleSlugsAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetByIdsAsync(roleIds, cancellationToken);
        return roles.Select(r => r.Slug).OrderBy(s => s, StringComparer.Ordinal).ToList();
    }
}
