using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HrPortal.Authorization.Infrastructure;

internal sealed class PermissionAnyAuthorizationHandler : AuthorizationHandler<PermissionAnyRequirement>
{
    private readonly IPolicyEngine _policyEngine;
    private readonly IResourceLoader _resourceLoader;
    private readonly IAuditService _auditService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAnyAuthorizationHandler(
        IPolicyEngine policyEngine,
        IResourceLoader resourceLoader,
        IAuditService auditService,
        ITenantContextAccessor tenantContextAccessor,
        IHttpContextAccessor httpContextAccessor)
    {
        _policyEngine = policyEngine;
        _resourceLoader = resourceLoader;
        _auditService = auditService;
        _tenantContextAccessor = tenantContextAccessor;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAnyRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var tenantContext = _tenantContextAccessor.Current;
        var resource = await _resourceLoader.LoadAsync(httpContext, httpContext.RequestAborted);

        string? matchedPermission = null;
        foreach (var permission in requirement.Permissions)
        {
            if (_policyEngine.Can(tenantContext, permission, resource))
            {
                matchedPermission = permission;
                break;
            }
        }

        var allowed = matchedPermission is not null;
        var permissionLabel = matchedPermission ??
            string.Join(Policies.PermissionAnySeparator, requirement.Permissions);

        await _auditService.LogAccessDecisionAsync(
            new AccessDecisionEntry(
                tenantContext.UserId,
                permissionLabel,
                allowed,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                resource?.EmployeeId,
                resource?.DepartmentId,
                resource?.TenantId),
            httpContext.RequestAborted);

        if (allowed)
            context.Succeed(requirement);
    }
}
