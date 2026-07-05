using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HrPortal.Authorization.Infrastructure;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPolicyEngine _policyEngine;
    private readonly IResourceLoader _resourceLoader;
    private readonly IAuditService _auditService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
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
        PermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var tenantContext = _tenantContextAccessor.Current;
        var resource = await _resourceLoader.LoadAsync(httpContext, httpContext.RequestAborted);
        var allowed = _policyEngine.Can(tenantContext, requirement.Permission, resource);

        await _auditService.LogAccessDecisionAsync(
            new AccessDecisionEntry(
                tenantContext.UserId,
                requirement.Permission,
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
