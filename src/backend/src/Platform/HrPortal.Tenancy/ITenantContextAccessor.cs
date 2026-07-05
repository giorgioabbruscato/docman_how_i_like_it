namespace HrPortal.Tenancy;

/// <summary>
/// Request-scoped accessor for the current <see cref="TenantContext"/>.
/// Set by middleware at the start of each request; read by services and repositories.
/// </summary>
public interface ITenantContextAccessor
{
    TenantContext Current { get; }
    void Set(TenantContext context);
}
