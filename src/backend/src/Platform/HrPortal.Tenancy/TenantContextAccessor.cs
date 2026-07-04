namespace HrPortal.Tenancy;

public interface ITenantContextAccessor
{
    TenantContext Current { get; }
    void Set(TenantContext context);
}

internal sealed class TenantContextAccessor : ITenantContextAccessor
{
    public TenantContext Current { get; private set; } = TenantContext.Empty;

    public void Set(TenantContext context) => Current = context;
}
