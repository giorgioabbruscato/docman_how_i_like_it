using HrPortal.AccessControl.Domain;
using HrPortal.Api.Infrastructure.OpenApi;
using HrPortal.Authorization;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Platform administrator cross-tenant metrics (no tenant header required).</summary>
[ApiController]
[Route("api/v1/platform/admin")]
[Tags("Platform")]
[RequirePermission(Permissions.TenantManageAll)]
[Produces("application/json")]
public sealed class PlatformAdminController : ControllerBase
{
    private readonly IPlatformMetricsService _metricsService;

    public PlatformAdminController(IPlatformMetricsService metricsService) =>
        _metricsService = metricsService;

    /// <summary>Aggregate platform KPIs.</summary>
    /// <remarks>Auth: tenant.manage:all (platform admin)</remarks>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PlatformDashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var summary = await _metricsService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>Per-tenant metrics for the platform admin table.</summary>
    /// <remarks>Auth: tenant.manage:all (platform admin)</remarks>
    [HttpGet("tenants")]
    [ProducesResponseType(typeof(IEnumerable<PlatformTenantMetricsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenants(CancellationToken cancellationToken)
    {
        var tenants = await _metricsService.GetTenantsAsync(cancellationToken);
        return Ok(tenants);
    }

    /// <summary>Per-tenant module usage drill-down.</summary>
    /// <remarks>Auth: tenant.manage:all (platform admin)</remarks>
    [HttpGet("tenants/{tenantId:guid}/summary")]
    [ProducesResponseType(typeof(PlatformTenantSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantSummary(Guid tenantId, CancellationToken cancellationToken)
    {
        var summary = await _metricsService.GetTenantSummaryAsync(tenantId, cancellationToken);
        if (summary is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = $"Tenant '{tenantId}' was not found."
            });
        }

        return Ok(summary);
    }

    /// <summary>Platform usage trends (tenant growth and time entries).</summary>
    /// <remarks>Auth: tenant.manage:all (platform admin)</remarks>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(PlatformUsageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var usage = await _metricsService.GetUsageAsync(cancellationToken);
        return Ok(usage);
    }
}
