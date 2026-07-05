using HrPortal.Tenancy.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HrPortal.Tenancy.Infrastructure;

public sealed class TenantResolver : ITenantResolver
{
    private readonly TenantResolverOptions _options;

    public TenantResolver(IOptions<TenantResolverOptions> options) =>
        _options = options.Value;

    public Task<string?> ResolveSlugAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (_options.UseSubdomainResolution)
        {
            var host = httpContext.Request.Host.Host;
            var suffix = $".{_options.BaseDomain}";

            if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                var slug = host[..^suffix.Length];
                if (!string.IsNullOrWhiteSpace(slug) && slug != "www")
                    return Task.FromResult<string?>(slug.ToLowerInvariant());
            }
        }

        if (httpContext.Request.Headers.TryGetValue(_options.TenantHeaderName, out var headerValue))
        {
            var value = headerValue.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(value))
                return Task.FromResult<string?>(value.ToLowerInvariant());
        }

        if (_options.Mode == TenantDeploymentMode.Single)
        {
            var defaultSlug = _options.DefaultTenantSlug.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(defaultSlug))
                return Task.FromResult<string?>(defaultSlug);
        }

        return Task.FromResult<string?>(null);
    }
}
