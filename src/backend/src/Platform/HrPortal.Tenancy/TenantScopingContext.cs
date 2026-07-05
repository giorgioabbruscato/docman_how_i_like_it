namespace HrPortal.Tenancy;

public static class TenantScopingContext
{
    public static TenantContext ForSeeding(Guid tenantId) =>
        TenantContext.CreateTenantOnly(tenantId, "seed", TenantDeploymentMode.Multi, []);
}
