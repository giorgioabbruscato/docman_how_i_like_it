using HrPortal.Audit.Application;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Application.Dtos;
using HrPortal.Tenancy.Domain;
using HrPortal.SharedKernel.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public TenantsController(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return Ok(tenants.Select(t => new { t.Id, t.Name, t.Slug, t.IsActive }));
    }

    [HttpPost]
    [AllowAnonymous]
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
        return CreatedAtAction(nameof(GetAll), new { id = tenant.Id }, new { tenant.Id, tenant.Name, tenant.Slug });
    }
}
