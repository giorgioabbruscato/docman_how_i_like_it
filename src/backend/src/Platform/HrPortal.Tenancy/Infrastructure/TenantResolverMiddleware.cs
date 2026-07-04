using HrPortal.Tenancy.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HrPortal.Tenancy.Infrastructure;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolverMiddleware> _logger;

    public TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantResolver tenantResolver,
        ITenantRepository tenantRepository)
    {
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var slug = await tenantResolver.ResolveSlugAsync(context, context.RequestAborted);

        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("Tenant could not be resolved for path {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant not specified",
                detail = "A valid tenant identifier is required for this request.",
                status = StatusCodes.Status400BadRequest
            });
            return;
        }

        var tenant = await tenantRepository.GetBySlugAsync(slug, context.RequestAborted);

        if (tenant is null || !tenant.IsActive)
        {
            _logger.LogWarning("Tenant '{Slug}' not found or inactive", slug);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Tenant not found",
                detail = $"Tenant '{slug}' does not exist or is inactive.",
                status = StatusCodes.Status404NotFound
            });
            return;
        }

        context.Items[nameof(TenantContext)] = TenantContext.Create(tenant.Id, tenant.Slug);

        await _next(context);
    }

    private static bool IsExcludedPath(PathString path) =>
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/ready") ||
        path.StartsWithSegments("/swagger");
}
