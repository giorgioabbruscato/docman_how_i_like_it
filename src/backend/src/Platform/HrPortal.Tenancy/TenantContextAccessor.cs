namespace HrPortal.Tenancy;

internal sealed class TenantContextAccessor : ITenantContextAccessor
{
    public TenantContext Current { get; private set; } = TenantContext.Empty;

    public void Set(TenantContext context) => Current = context;
}
