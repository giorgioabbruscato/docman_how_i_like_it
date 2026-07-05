using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Identity;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class TenantContextFactory : ITenantContextFactory
{
    private const string DepartmentIdAttributeKey = "departmentId";

    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPermissionResolver _permissionResolver;

    public TenantContextFactory(
        ITenantMembershipRepository membershipRepository,
        IUserProfileRepository userProfileRepository,
        ITenantRepository tenantRepository,
        IPermissionResolver permissionResolver)
    {
        _membershipRepository = membershipRepository;
        _userProfileRepository = userProfileRepository;
        _tenantRepository = tenantRepository;
        _permissionResolver = permissionResolver;
    }

    public async Task<TenantContext> EnrichAsync(
        TenantContext baseContext,
        UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        if (!userContext.IsAuthenticated)
            return baseContext;

        var profile = await _userProfileRepository.GetByUserIdAsync(userContext.UserId, cancellationToken);
        var isPlatformAdmin = profile?.IsPlatformAdmin ?? false;

        var tenant = await _tenantRepository.GetByIdAsync(baseContext.TenantId, cancellationToken);
        var features = tenant?.GetFeatures() ?? baseContext.Features ?? [];

        var membership = await _membershipRepository.GetActiveByUserIdAsync(
            userContext.UserId,
            cancellationToken);

        IReadOnlyList<string> permissions;
        IReadOnlyList<string> roleSlugs;
        Guid? employeeId = null;
        IReadOnlyDictionary<string, string> attributes = new Dictionary<string, string>();
        Guid? departmentId = null;

        if (membership is not null)
        {
            var roleIds = membership.GetRoleIds();
            permissions = await _permissionResolver.ResolveAsync(roleIds, cancellationToken);
            roleSlugs = await _permissionResolver.ResolveRoleSlugsAsync(roleIds, cancellationToken);
            employeeId = membership.EmployeeId;
            attributes = membership.GetAttributes();
            departmentId = TryParseDepartmentId(attributes);
        }
        else
        {
            permissions = LegacyRoleMapper.Map(userContext.Roles);
            roleSlugs = LegacyRoleMapper.MapRoleSlugs(userContext.Roles);

            if (permissions.Count == 0)
            {
                return baseContext with
                {
                    UserId = userContext.UserId,
                    Email = userContext.Email,
                    Roles = userContext.Roles,
                    RoleSlugs = [],
                    Permissions = [],
                    Features = features,
                    IsPlatformAdmin = isPlatformAdmin,
                    IsResolved = false
                };
            }
        }

        if (isPlatformAdmin)
            permissions = UnionPermissions(permissions, Permissions.PlatformAdmin);

        return baseContext with
        {
            UserId = userContext.UserId,
            Email = userContext.Email,
            Roles = userContext.Roles,
            RoleSlugs = roleSlugs,
            Permissions = permissions,
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            Attributes = attributes,
            Features = features,
            IsPlatformAdmin = isPlatformAdmin,
            IsResolved = true
        };
    }

    private static Guid? TryParseDepartmentId(IReadOnlyDictionary<string, string> attributes)
    {
        if (!attributes.TryGetValue(DepartmentIdAttributeKey, out var value))
            return null;

        return Guid.TryParse(value, out var departmentId) ? departmentId : null;
    }

    private static IReadOnlyList<string> UnionPermissions(
        IReadOnlyList<string> existing,
        IReadOnlyList<string> additional)
    {
        var merged = new HashSet<string>(existing, StringComparer.Ordinal);
        foreach (var permission in additional)
            merged.Add(permission);

        return merged.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }
}
