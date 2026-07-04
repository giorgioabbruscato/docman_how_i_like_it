namespace HrPortal.Tenancy.Infrastructure;

public sealed class TenantResolverOptions
{
    public const string SectionName = "Tenancy";

    public string TenantHeaderName { get; set; } = "X-Tenant-Id";
    public bool UseSubdomainResolution { get; set; }
    public string BaseDomain { get; set; } = "hrportal.local";
}
