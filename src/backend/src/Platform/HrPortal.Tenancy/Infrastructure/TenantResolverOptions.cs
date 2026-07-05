namespace HrPortal.Tenancy.Infrastructure;

public sealed class TenantResolverOptions
{
    public const string SectionName = "Tenancy";

    public TenantDeploymentMode Mode { get; set; } = TenantDeploymentMode.Multi;
    public string DefaultTenantSlug { get; set; } = "demo";
    public string TenantHeaderName { get; set; } = "X-Tenant-Id";
    public bool UseSubdomainResolution { get; set; }
    public string BaseDomain { get; set; } = "hrportal.local";
}
