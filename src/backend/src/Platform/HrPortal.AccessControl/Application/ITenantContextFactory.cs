using HrPortal.Identity;
using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Application;

public interface ITenantContextFactory
{
    Task<TenantContext> EnrichAsync(
        TenantContext baseContext,
        UserContext userContext,
        CancellationToken cancellationToken = default);
}
