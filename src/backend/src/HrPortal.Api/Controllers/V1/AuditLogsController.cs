using HrPortal.AccessControl.Domain;
using HrPortal.Audit.Application;
using HrPortal.Authorization;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Enterprise audit log querying.</summary>
[ApiController]
[Route("api/v1/audit-logs")]
[Tags("Audit")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditQueryService _auditQueryService;
    private readonly IFeatureGateService _featureGateService;

    public AuditLogsController(IAuditQueryService auditQueryService, IFeatureGateService featureGateService)
    {
        _auditQueryService = auditQueryService;
        _featureGateService = featureGateService;
    }

    /// <summary>List audit log entries for the current tenant.</summary>
    /// <remarks>
    /// Auth: audit.read:tenant. Gated by the tenant's Enterprise-tier "auditLog" plan feature — Free/Pro
    /// tenants without the feature enabled get 403 PLAN_LIMIT_EXCEEDED even if they hold the permission.
    /// </remarks>
    [HttpGet]
    [RequirePermission(Permissions.AuditReadTenant)]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? actorUserId,
        [FromQuery] string? action,
        [FromQuery] string? decision,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!await _featureGateService.IsEnabledAsync(FeatureKeys.AuditLog, cancellationToken))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Plan limit exceeded",
                Detail = "Audit log access is not available on the current plan. Upgrade to Enterprise."
            });
        }

        var result = await _auditQueryService.QueryAsync(
            new AuditLogQuery(from, to, actorUserId, action, decision, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = result.Error
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
