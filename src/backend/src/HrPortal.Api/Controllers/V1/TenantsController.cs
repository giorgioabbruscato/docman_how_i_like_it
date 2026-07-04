using HrPortal.Api.Infrastructure.OpenApi;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using HrPortal.SharedKernel.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Tenant registration and listing (no tenant header required).</summary>
[ApiController]
[Route("api/v1/tenants")]
[Tags("Tenants")]
[Produces("application/json")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantsController(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>List all tenants.</summary>
    /// <remarks>Auth: None</remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TenantListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return Ok(tenants.Select(t => new TenantListItemDto(t.Id, t.Name, t.Slug, t.IsActive)));
    }

    /// <summary>Create a new tenant.</summary>
    /// <remarks>Auth: None</remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantCreatedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(request.Name, request.Slug);
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(
            nameof(GetAll),
            new { id = tenant.Id },
            new TenantCreatedDto(tenant.Id, tenant.Name, tenant.Slug));
    }
}

public sealed record CreateTenantRequest(string Name, string Slug);
