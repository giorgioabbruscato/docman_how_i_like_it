using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.TimeTracking.Application.Commands;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Time entry CRUD, manual entry, and export.</summary>
[ApiController]
[Route("api/v1/time-entries")]
[Tags("Time Tracking")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class TimeEntriesController : ControllerBase
{
    private readonly GetTimeEntriesQueryHandler _getEntriesHandler;
    private readonly GetTimeEntryByIdQueryHandler _getEntryByIdHandler;
    private readonly CreateTimeEntryCommandHandler _createEntryHandler;
    private readonly UpdateTimeEntryCommandHandler _updateEntryHandler;
    private readonly DeleteTimeEntryCommandHandler _deleteEntryHandler;
    private readonly CreateManualTimeEntryCommandHandler _createManualEntryHandler;
    private readonly ExportTimeEntriesQueryHandler _exportHandler;

    public TimeEntriesController(
        GetTimeEntriesQueryHandler getEntriesHandler,
        GetTimeEntryByIdQueryHandler getEntryByIdHandler,
        CreateTimeEntryCommandHandler createEntryHandler,
        UpdateTimeEntryCommandHandler updateEntryHandler,
        DeleteTimeEntryCommandHandler deleteEntryHandler,
        CreateManualTimeEntryCommandHandler createManualEntryHandler,
        ExportTimeEntriesQueryHandler exportHandler)
    {
        _getEntriesHandler = getEntriesHandler;
        _getEntryByIdHandler = getEntryByIdHandler;
        _createEntryHandler = createEntryHandler;
        _updateEntryHandler = updateEntryHandler;
        _deleteEntryHandler = deleteEntryHandler;
        _createManualEntryHandler = createManualEntryHandler;
        _exportHandler = exportHandler;
    }

    /// <summary>List time entries with pagination and filters.</summary>
    [HttpGet]
    [RequireAnyPermission(Permissions.TimeEntryReadSelf, Permissions.TimeEntryReadTeam, Permissions.TimeEntryReadTenant)]
    [ProducesResponseType(typeof(PagedResult<TimeEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetTimeEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getEntriesHandler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Export time entries to CSV, XLSX, or PDF.</summary>
    [HttpGet("export")]
    [RequireAnyPermission(Permissions.TimeEntryReadSelf, Permissions.TimeEntryReadTeam, Permissions.TimeEntryReadTenant)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Export(
        [FromQuery] ExportTimeEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _exportHandler.HandleAsync(query, cancellationToken);
        if (!result.IsSuccess)
            return MapFailure(result);

        var (content, contentType, fileName) = result.Value!;
        return File(content, contentType, fileName);
    }

    /// <summary>Get time entry by ID.</summary>
    [HttpGet("{id:guid}")]
    [RequireAnyPermission(Permissions.TimeEntryReadSelf, Permissions.TimeEntryReadTeam, Permissions.TimeEntryReadTenant)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getEntryByIdHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a time entry.</summary>
    [HttpPost]
    [RequirePermission(Permissions.TimeEntryCreateSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createEntryHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Create a manual time entry.</summary>
    [HttpPost("manual")]
    [RequirePermission(Permissions.TimeEntryCreateSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateManual(
        [FromBody] CreateManualTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createManualEntryHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update a time entry.</summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.TimeEntryUpdateSelf)]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateEntryHandler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Delete a time entry.</summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.TimeEntryDeleteSelf)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteEntryHandler.HandleAsync(id, cancellationToken);
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
