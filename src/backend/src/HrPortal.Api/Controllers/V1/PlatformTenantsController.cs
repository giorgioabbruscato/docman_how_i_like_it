using HrPortal.Api.Infrastructure.OpenApi;
using HrPortal.Tenancy.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Platform administrator tenant management (no tenant header required).</summary>
[ApiController]
[Route("api/v1/platform/tenants")]
[Tags("Platform")]
[Authorize]
[Produces("application/json")]
public sealed class PlatformTenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public PlatformTenantsController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>List all tenants (platform admin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return Ok(tenants.Select(t => new TenantListItemDto(t.Id, t.Name, t.Slug, t.IsActive)));
    }
}
