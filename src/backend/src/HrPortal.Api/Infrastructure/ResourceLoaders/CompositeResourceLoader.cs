using HrPortal.AccessControl.Application;
using Microsoft.AspNetCore.Http;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class CompositeResourceLoader : IResourceLoader
{
    private readonly IReadOnlyList<IEndpointResourceLoader> _loaders;

    public CompositeResourceLoader(IEnumerable<IEndpointResourceLoader> loaders) =>
        _loaders = loaders.ToList();

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        foreach (var loader in _loaders)
        {
            if (!loader.CanLoad(httpContext))
                continue;

            return await loader.LoadAsync(httpContext, cancellationToken);
        }

        return null;
    }
}
