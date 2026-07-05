using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.Identity;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using Microsoft.Extensions.Logging;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class MeService : IMeService
{
    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPermissionResolver _permissionResolver;
    private readonly IFeatureGateService _featureGateService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly ILogger<MeService> _logger;

    public MeService(
        ITenantMembershipRepository membershipRepository,
        IUserProfileRepository userProfileRepository,
        ITenantRepository tenantRepository,
        IPermissionResolver permissionResolver,
        IFeatureGateService featureGateService,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        UserContext userContext,
        ILogger<MeService> logger)
    {
        _membershipRepository = membershipRepository;
        _userProfileRepository = userProfileRepository;
        _tenantRepository = tenantRepository;
        _permissionResolver = permissionResolver;
        _featureGateService = featureGateService;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<MeDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!_userContext.IsAuthenticated)
            return Result.Failure<MeDto>("User is not authenticated.", "UNAUTHORIZED");

        if (!_tenantContext.IsResolved)
            return Result.Failure<MeDto>("Tenant is not resolved.", "TENANT_NOT_RESOLVED");

        var profile = await EnsureUserProfileAsync(cancellationToken);
        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<MeDto>("Tenant not found.", "NOT_FOUND");

        var membership = await _membershipRepository.GetActiveByUserIdAsync(
            _userContext.UserId,
            cancellationToken);

        IReadOnlyList<string> permissions;
        IReadOnlyList<string> roleSlugs;
        Guid? employeeId = null;

        if (membership is not null)
        {
            var roleIds = membership.GetRoleIds();
            permissions = await _permissionResolver.ResolveAsync(roleIds, cancellationToken);
            roleSlugs = await _permissionResolver.ResolveRoleSlugsAsync(roleIds, cancellationToken);
            employeeId = membership.EmployeeId;
        }
        else
        {
            _logger.LogDebug(
                "No active membership for user {UserId} in tenant {TenantId}; using legacy role mapper",
                _userContext.UserId,
                _tenantContext.TenantId);

            permissions = LegacyRoleMapper.Map(_userContext.Roles);
            roleSlugs = MapLegacyRoleSlugs(_userContext.Roles);
        }

        var planFeatures = await _featureGateService.GetEffectiveFeaturesAsync(cancellationToken);

        return Result.Success(new MeDto(
            _userContext.UserId,
            profile.Email,
            _tenantContext.TenantId,
            _tenantContext.TenantSlug,
            employeeId,
            roleSlugs,
            permissions,
            tenant.GetModules(),
            profile.IsPlatformAdmin,
            planFeatures));
    }

    private async Task<UserProfile> EnsureUserProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);

        if (profile is not null)
        {
            if (!string.Equals(profile.Email, _userContext.Email, StringComparison.OrdinalIgnoreCase))
            {
                profile.UpdateEmail(_userContext.Email);
                await _userProfileRepository.UpdateAsync(profile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return profile;
        }

        profile = UserProfile.Create(_userContext.UserId, _userContext.Email);
        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private static IReadOnlyList<string> MapLegacyRoleSlugs(IReadOnlyList<string> roles)
    {
        var slugs = new List<string>();

        foreach (var role in roles)
        {
            var slug = role.Trim() switch
            {
                Roles.Admin => SystemRoleTemplates.AdminSlug,
                Roles.Hr => SystemRoleTemplates.HrSlug,
                Roles.Manager => SystemRoleTemplates.ManagerSlug,
                Roles.Employee => SystemRoleTemplates.EmployeeSlug,
                _ => null
            };

            if (slug is not null && !slugs.Contains(slug, StringComparer.Ordinal))
                slugs.Add(slug);
        }

        return slugs;
    }
}
