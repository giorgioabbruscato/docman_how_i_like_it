using HrPortal.Authorization;
using HrPortal.Leave.Application;
using HrPortal.Leave.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/leave-requests")]
[Authorize(Policy = Policies.Authenticated)]
public sealed class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService) =>
        _leaveRequestService = leaveRequestService;

    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.RejectAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpDelete("{id:guid}")]
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
