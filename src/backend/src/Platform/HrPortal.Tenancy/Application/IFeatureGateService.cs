using HrPortal.Tenancy.Domain;

namespace HrPortal.Tenancy.Application;

/// <summary>
/// Resolves plan-based feature gates for the current tenant. Single-tenant (OSS) deployments are
/// always treated as Enterprise-equivalent — every feature is enabled and limits are unbounded.
/// </summary>
public interface IFeatureGateService
{
    Task<TenantFeatures> GetEffectiveFeaturesAsync(CancellationToken cancellationToken = default);

    Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default);

    Task<int> GetMaxEmployeesAsync(CancellationToken cancellationToken = default);
}
