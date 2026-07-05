using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Leave.Application;
using HrPortal.Leave.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Leave request workflow.</summary>
[ApiController]
[Route("api/v1/leave-requests")]
[Tags("Leave")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService) =>
        _leaveRequestService = leaveRequestService;

    /// <summary>List all leave requests.</summary>
    /// <remarks>Auth: leave.read:tenant OR leave.read:team</remarks>
    [HttpGet]
    [RequireAnyPermission(Permissions.LeaveReadTenant, Permissions.LeaveReadTeam)]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get leave request by ID.</summary>
    /// <remarks>Auth: leave.read:tenant OR leave.read:team OR leave.read:self</remarks>
    [HttpGet("{id:guid}")]
    [RequireAnyPermission(Permissions.LeaveReadTenant, Permissions.LeaveReadTeam, Permissions.LeaveReadSelf)]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Submit a new leave request.</summary>
    /// <remarks>Auth: leave.create:self</remarks>
    [HttpPost]
    [RequirePermission(Permissions.LeaveCreateSelf)]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Approve a pending leave request.</summary>
    /// <remarks>Auth: leave.approve:team</remarks>
    [HttpPut("{id:guid}/approve")]
    [RequirePermission(Permissions.LeaveApproveTeam)]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Reject a pending leave request.</summary>
    /// <remarks>Auth: leave.approve:team</remarks>
    [HttpPut("{id:guid}/reject")]
    [RequirePermission(Permissions.LeaveApproveTeam)]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.RejectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Cancel a pending leave request.</summary>
    /// <remarks>Auth: leave.delete:self</remarks>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.LeaveDeleteSelf)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CancelAsync(id, cancellationToken);
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
