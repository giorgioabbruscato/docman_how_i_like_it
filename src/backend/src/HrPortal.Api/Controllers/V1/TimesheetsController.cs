using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Commands;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Timesheet submission and approval workflow.</summary>
[ApiController]
[Route("api/v1/timesheets")]
[Tags("TimeTracking")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class TimesheetsController : ControllerBase
{
    private readonly GetTimesheetsQueryHandler _getTimesheetsHandler;
    private readonly GetTimesheetByIdQueryHandler _getByIdHandler;
    private readonly CreateTimesheetCommandHandler _createHandler;
    private readonly SubmitTimesheetCommandHandler _submitHandler;
    private readonly ITimesheetApprovalService _approvalService;

    public TimesheetsController(
        GetTimesheetsQueryHandler getTimesheetsHandler,
        GetTimesheetByIdQueryHandler getByIdHandler,
        CreateTimesheetCommandHandler createHandler,
        SubmitTimesheetCommandHandler submitHandler,
        ITimesheetApprovalService approvalService)
    {
        _getTimesheetsHandler = getTimesheetsHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _submitHandler = submitHandler;
        _approvalService = approvalService;
    }

    /// <summary>List timesheets.</summary>
    /// <remarks>Auth: timesheet.read:team OR timesheet.read:self</remarks>
    [HttpGet]
    [RequireAnyPermission(Permissions.TimesheetReadTeam, Permissions.TimesheetReadSelf)]
    [ProducesResponseType(typeof(PagedResult<TimesheetSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetTimesheetsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getTimesheetsHandler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get timesheet by ID.</summary>
    /// <remarks>Auth: timesheet.read:team OR timesheet.read:self</remarks>
    [HttpGet("{id:guid}")]
    [RequireAnyPermission(Permissions.TimesheetReadTeam, Permissions.TimesheetReadSelf)]
    [ProducesResponseType(typeof(TimesheetSubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a draft timesheet for a period.</summary>
    /// <remarks>Auth: timesheet.submit:self</remarks>
    [HttpPost]
    [RequirePermission(Permissions.TimesheetSubmitSelf)]
    [ProducesResponseType(typeof(TimesheetSubmissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTimesheetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Submit a draft timesheet for approval.</summary>
    /// <remarks>Auth: timesheet.submit:self</remarks>
    [HttpPost("{id:guid}/submit")]
    [RequirePermission(Permissions.TimesheetSubmitSelf)]
    [ProducesResponseType(typeof(TimesheetSubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _submitHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Approve a submitted timesheet.</summary>
    /// <remarks>Auth: timesheet.approve:team</remarks>
    [HttpPost("{id:guid}/approve")]
    [RequirePermission(Permissions.TimesheetApproveTeam)]
    [ProducesResponseType(typeof(TimesheetSubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _approvalService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Reject a submitted timesheet.</summary>
    /// <remarks>Auth: timesheet.approve:team</remarks>
    [HttpPost("{id:guid}/reject")]
    [RequirePermission(Permissions.TimesheetApproveTeam)]
    [ProducesResponseType(typeof(TimesheetSubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectTimesheetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _approvalService.RejectAsync(id, request, cancellationToken);
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
