using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;

namespace HrPortal.Tenancy.Infrastructure;

internal sealed class FeatureGateService : IFeatureGateService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantContext _tenantContext;

    public FeatureGateService(ITenantRepository tenantRepository, TenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<TenantFeatures> GetEffectiveFeaturesAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantContext.Mode == TenantDeploymentMode.Single)
            return TenantFeaturesDefaults.ForPlan(TenantPlan.Enterprise);

        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return TenantFeaturesDefaults.ForPlan(TenantPlan.Free);

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId, cancellationToken);
        return tenant?.GetEffectiveFeatures() ?? TenantFeaturesDefaults.ForPlan(TenantPlan.Free);
    }

    public async Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        var features = await GetEffectiveFeaturesAsync(cancellationToken);
        return featureKey switch
        {
            FeatureKeys.CustomRoles => features.CustomRoles,
            FeatureKeys.AuditLog => features.AuditLog,
            FeatureKeys.AdvancedReports => features.AdvancedReports,
            _ => false
        };
    }

    public async Task<int> GetMaxEmployeesAsync(CancellationToken cancellationToken = default) =>
        (await GetEffectiveFeaturesAsync(cancellationToken)).MaxEmployees;
}
