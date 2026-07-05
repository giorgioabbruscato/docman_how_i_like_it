using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.TimeTracking.Application.Commands;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Timer start/stop operations.</summary>
[ApiController]
[Route("api/v1/timer")]
[Tags("Time Tracking")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class TimerController : ControllerBase
{
    private readonly StartTimerCommandHandler _startTimerHandler;
    private readonly StopTimerCommandHandler _stopTimerHandler;
    private readonly GetActiveTimerQueryHandler _getActiveTimerHandler;

    public TimerController(
        StartTimerCommandHandler startTimerHandler,
        StopTimerCommandHandler stopTimerHandler,
        GetActiveTimerQueryHandler getActiveTimerHandler)
    {
        _startTimerHandler = startTimerHandler;
        _stopTimerHandler = stopTimerHandler;
        _getActiveTimerHandler = getActiveTimerHandler;
    }

    /// <summary>Start a running timer.</summary>
    [HttpPost("start")]
    [RequirePermission(Permissions.TimeEntryCreateSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(
        [FromBody] StartTimerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _startTimerHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : MapFailure(result);
    }

    /// <summary>Stop the active timer.</summary>
    [HttpPost("stop")]
    [RequirePermission(Permissions.TimeEntryUpdateSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Stop(CancellationToken cancellationToken)
    {
        var result = await _stopTimerHandler.HandleAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get the active timer for the current employee.</summary>
    [HttpGet("active")]
    [RequirePermission(Permissions.TimeEntryReadSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _getActiveTimerHandler.HandleAsync(cancellationToken);
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
            "CONFLICT" => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Error
            }),
            "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
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
