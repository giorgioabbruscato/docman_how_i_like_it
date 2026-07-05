using HrPortal.Api.Infrastructure.OpenApi;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.Audit.Application;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Application.Dtos;
using HrPortal.Tenancy.Domain;
using HrPortal.Workflows.Infrastructure.Seeding;
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
    private readonly IAuditService _auditService;
    private readonly ISystemRoleSeeder _systemRoleSeeder;
    private readonly IWorkflowSeeder _workflowSeeder;

    public TenantsController(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ISystemRoleSeeder systemRoleSeeder,
        IWorkflowSeeder workflowSeeder)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _systemRoleSeeder = systemRoleSeeder;
        _workflowSeeder = workflowSeeder;
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

        await _auditService.LogForTenantAsync(
            tenant.Id,
            new AuditEntry("tenant.created", nameof(Tenant), tenant.Id.ToString()),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemRoleSeeder.SeedAsync(tenant.Id, cancellationToken);

        using (var scope = HttpContext.RequestServices.CreateScope())
        {
            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.Set(TenantScopingContext.ForSeeding(tenant.Id));
            await _workflowSeeder.SeedDefaultsAsync(tenant.Id, cancellationToken);
        }

        return CreatedAtAction(
            nameof(GetAll),
            new { id = tenant.Id },
            new TenantCreatedDto(tenant.Id, tenant.Name, tenant.Slug));
    }
}
