using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Identity;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrPortal.AccessControl.Infrastructure;

public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestContextMiddleware> _logger;

    public RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantResolver tenantResolver,
        ITenantRepository tenantRepository,
        ITenantContextAccessor tenantContextAccessor,
        ITenantContextFactory tenantContextFactory,
        IUserProfileRepository userProfileRepository,
        UserContext userContext,
        IOptions<TenantResolverOptions> options)
    {
        if (IsFullyExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (IsPlatformPath(context.Request.Path))
        {
            await HandlePlatformPathAsync(
                context,
                tenantContextAccessor,
                userProfileRepository,
                userContext,
                options.Value);
            return;
        }

        await HandleTenantPathAsync(
            context,
            tenantResolver,
            tenantRepository,
            tenantContextAccessor,
            tenantContextFactory,
            userContext,
            options.Value);
    }

    private async Task HandleTenantPathAsync(
        HttpContext context,
        ITenantResolver tenantResolver,
        ITenantRepository tenantRepository,
        ITenantContextAccessor tenantContextAccessor,
        ITenantContextFactory tenantContextFactory,
        UserContext userContext,
        TenantResolverOptions options)
    {
        var slug = await tenantResolver.ResolveSlugAsync(context, context.RequestAborted);

        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("Tenant could not be resolved for path {Path}", context.Request.Path);
            await WriteJsonErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Tenant not specified",
                "A valid tenant identifier is required for this request.");
            return;
        }

        var tenant = await tenantRepository.GetBySlugAsync(slug, context.RequestAborted);

        if (tenant is null || !tenant.IsActive || tenant.IsSuspended)
        {
            _logger.LogWarning("Tenant '{Slug}' not found, inactive, or suspended", slug);
            await WriteJsonErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                "Tenant not found",
                $"Tenant '{slug}' does not exist, is inactive, or is suspended.");
            return;
        }

        var tenantContext = TenantContext.CreateTenantOnly(
            tenant.Id,
            tenant.Slug,
            options.Mode,
            tenant.GetModules());

        SetContext(context, tenantContextAccessor, tenantContext);

        if (userContext.IsAuthenticated)
        {
            tenantContext = await tenantContextFactory.EnrichAsync(
                tenantContext,
                userContext,
                context.RequestAborted);

            if (options.Mode == TenantDeploymentMode.Multi &&
                !tenantContext.IsResolved)
            {
                _logger.LogWarning(
                    "User {UserId} denied access to tenant '{Slug}'",
                    userContext.UserId,
                    slug);
                await WriteJsonErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Access denied",
                    "You do not have access to this tenant.");
                return;
            }

            SetContext(context, tenantContextAccessor, tenantContext);
        }

        await _next(context);
    }

    private async Task HandlePlatformPathAsync(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        IUserProfileRepository userProfileRepository,
        UserContext userContext,
        TenantResolverOptions options)
    {
        if (userContext.IsAuthenticated)
        {
            var profile = await userProfileRepository.GetByUserIdAsync(
                userContext.UserId,
                context.RequestAborted);

            if (profile is null || !profile.IsPlatformAdmin)
            {
                _logger.LogWarning(
                    "User {UserId} denied platform access for path {Path}",
                    userContext.UserId,
                    context.Request.Path);
                await WriteJsonErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Access denied",
                    "Platform administrator access is required.");
                return;
            }

            var platformContext = TenantContext.Empty with
            {
                UserId = userContext.UserId,
                Email = userContext.Email,
                Mode = options.Mode,
                Permissions = Permissions.PlatformAdmin,
                IsPlatformAdmin = true,
                IsResolved = true
            };

            SetContext(context, tenantContextAccessor, platformContext);
        }

        await _next(context);
    }

    private static void SetContext(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        TenantContext tenantContext)
    {
        tenantContextAccessor.Set(tenantContext);
        context.Items[nameof(TenantContext)] = tenantContext;
    }

    private static async Task WriteJsonErrorAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            title,
            detail,
            status = statusCode
        });
    }

    private static bool IsFullyExcludedPath(PathString path) =>
        path.StartsWithSegments("/health") ||
        path.StartsWithSegments("/ready") ||
        path.StartsWithSegments("/swagger") ||
        path.StartsWithSegments("/api/v1/tenants") ||
        path.StartsWithSegments("/api/v1/integrations/calendar/callback");

    private static bool IsPlatformPath(PathString path) =>
        path.StartsWithSegments("/api/v1/platform");
}
