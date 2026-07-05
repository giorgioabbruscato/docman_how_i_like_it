namespace HrPortal.Tenancy;

public static class TenantScopingRules
{
    public static bool ShouldApplyTenantFilter(ITenantContext ctx) =>
        ctx.Mode == TenantDeploymentMode.Multi;
}
