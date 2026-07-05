using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Tenant role management.</summary>
[ApiController]
[Route("api/v1/roles")]
[Tags("Access Control")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class RolesController : ControllerBase
{
    private readonly ITenantRoleService _roleService;

    public RolesController(ITenantRoleService roleService) => _roleService = roleService;

    /// <summary>List tenant roles.</summary>
    /// <remarks>Auth: role.read:tenant</remarks>
    [HttpGet]
    [RequirePermission(Permissions.RoleReadTenant)]
    [ProducesResponseType(typeof(IEnumerable<TenantRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get role by ID.</summary>
    /// <remarks>Auth: role.read:tenant</remarks>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.RoleReadTenant)]
    [ProducesResponseType(typeof(TenantRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a custom tenant role.</summary>
    /// <remarks>Auth: role.create:tenant</remarks>
    [HttpPost]
    [RequirePermission(Permissions.RoleCreateTenant)]
    [ProducesResponseType(typeof(TenantRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update role permissions.</summary>
    /// <remarks>Auth: role.update:tenant</remarks>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.RoleUpdateTenant)]
    [ProducesResponseType(typeof(TenantRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTenantRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Deactivate a custom role (soft delete).</summary>
    /// <remarks>Auth: role.delete:tenant</remarks>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.RoleDeleteTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
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
            "CONFLICT" => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Error
            }),
            "PLAN_LIMIT_EXCEEDED" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Plan limit exceeded",
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
