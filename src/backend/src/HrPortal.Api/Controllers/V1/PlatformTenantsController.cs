using HrPortal.AccessControl.Domain;
using HrPortal.Api.Infrastructure.OpenApi;
using HrPortal.Authorization;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Platform administrator tenant management (no tenant header required).</summary>
[ApiController]
[Route("api/v1/platform/tenants")]
[Tags("Platform")]
[RequirePermission(Permissions.TenantManageAll)]
[Produces("application/json")]
public sealed class PlatformTenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlatformTenantsController(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>List all tenants (platform admin).</summary>
    /// <remarks>Auth: tenant.manage:all</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlatformTenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return Ok(tenants.Select(MapToDto));
    }

    /// <summary>Suspend a tenant, blocking all further access.</summary>
    /// <remarks>Auth: tenant.manage:all</remarks>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
            return NotFound(NotFoundProblem(id));

        tenant.Suspend();
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(tenant));
    }

    /// <summary>Reactivate a previously suspended tenant.</summary>
    /// <remarks>Auth: tenant.manage:all</remarks>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
            return NotFound(NotFoundProblem(id));

        tenant.Unsuspend();
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(tenant));
    }

    /// <summary>Change a tenant's subscription plan.</summary>
    /// <remarks>Auth: tenant.manage:all</remarks>
    [HttpPut("{id:guid}/plan")]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlan(
        Guid id,
        [FromBody] UpdateTenantPlanRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
            return NotFound(NotFoundProblem(id));

        if (!Enum.TryParse<TenantPlan>(request.Plan, ignoreCase: true, out var plan))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = $"Unknown plan '{request.Plan}'. Valid values: Free, Pro, Enterprise."
            });
        }

        tenant.SetPlan(plan);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(tenant));
    }

    /// <summary>Override plan features for a tenant.</summary>
    /// <remarks>Auth: tenant.manage:all</remarks>
    [HttpPut("{id:guid}/features")]
    [ProducesResponseType(typeof(PlatformTenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeatures(
        Guid id,
        [FromBody] UpdateTenantFeaturesRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
            return NotFound(NotFoundProblem(id));

        tenant.SetFeatureOverrides(new TenantFeaturesOverrides(
            request.MaxEmployees,
            request.CustomRoles,
            request.AuditLog,
            request.AdvancedReports));

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToDto(tenant));
    }

    private static ProblemDetails NotFoundProblem(Guid id) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Not found",
        Detail = $"Tenant '{id}' was not found."
    };

    private static PlatformTenantDto MapToDto(Tenant tenant)
    {
        var effective = tenant.GetEffectiveFeatures();
        return new PlatformTenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive,
            tenant.IsSuspended,
            tenant.GetPlan().ToString(),
            tenant.GetModules(),
            effective);
    }
}
