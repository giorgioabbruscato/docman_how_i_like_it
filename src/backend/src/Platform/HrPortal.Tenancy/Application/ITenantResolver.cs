using Microsoft.AspNetCore.Http;

namespace HrPortal.Tenancy.Application;

public interface ITenantResolver
{
    Task<string?> ResolveSlugAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
