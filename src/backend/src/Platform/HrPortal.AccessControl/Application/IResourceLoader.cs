using Microsoft.AspNetCore.Http;

namespace HrPortal.AccessControl.Application;

public interface IResourceLoader
{
    Task<ResourceContext?> LoadAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}

public interface IEndpointResourceLoader
{
    bool CanLoad(HttpContext httpContext);

    Task<ResourceContext?> LoadAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
