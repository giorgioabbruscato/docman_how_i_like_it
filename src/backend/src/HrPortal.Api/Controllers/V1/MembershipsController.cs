using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Tenant membership management.</summary>
[ApiController]
[Route("api/v1/memberships")]
[Tags("Access Control")]
[Authorize(Policy = Policies.AdminOnly)]
[Produces("application/json")]
public sealed class MembershipsController : ControllerBase
{
    private readonly ITenantMembershipService _membershipService;

    public MembershipsController(ITenantMembershipService membershipService) =>
        _membershipService = membershipService;

    /// <summary>List tenant memberships.</summary>
    /// <remarks>Auth: AdminOnly (interim; target: membership.read:tenant)</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantMembershipDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _membershipService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get membership by ID.</summary>
    /// <remarks>Auth: AdminOnly (interim; target: membership.read:tenant)</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantMembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _membershipService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a tenant membership.</summary>
    /// <remarks>Auth: AdminOnly (interim; target: membership.create:tenant)</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TenantMembershipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _membershipService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update a tenant membership.</summary>
    /// <remarks>Auth: AdminOnly (interim; target: membership.update:tenant)</remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TenantMembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTenantMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _membershipService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Deactivate a membership (soft delete).</summary>
    /// <remarks>Auth: AdminOnly (interim; target: membership.delete:tenant)</remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _membershipService.DeactivateAsync(id, cancellationToken);
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
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
