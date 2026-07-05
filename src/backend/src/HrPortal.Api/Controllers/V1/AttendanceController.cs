using HrPortal.AccessControl.Domain;
using HrPortal.Attendance.Application.Commands;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Application.Queries;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Attendance session check-in/out, dashboard, and history.</summary>
[ApiController]
[Route("api/v1/attendance")]
[Tags("Attendance")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class AttendanceController : ControllerBase
{
    private readonly CheckInCommandHandler _checkInHandler;
    private readonly CheckOutCommandHandler _checkOutHandler;
    private readonly GetAttendanceDashboardQueryHandler _dashboardHandler;
    private readonly GetAttendanceHistoryQueryHandler _historyHandler;

    public AttendanceController(
        CheckInCommandHandler checkInHandler,
        CheckOutCommandHandler checkOutHandler,
        GetAttendanceDashboardQueryHandler dashboardHandler,
        GetAttendanceHistoryQueryHandler historyHandler)
    {
        _checkInHandler = checkInHandler;
        _checkOutHandler = checkOutHandler;
        _dashboardHandler = dashboardHandler;
        _historyHandler = historyHandler;
    }

    /// <summary>Record employee check-in.</summary>
    /// <remarks>Auth: attendance_session.check_in:self</remarks>
    [HttpPost("check-in")]
    [RequirePermission(Permissions.AttendanceSessionCheckInSelf)]
    [ProducesResponseType(typeof(AttendanceSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _checkInHandler.HandleAsync(request, ipAddress, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Record employee check-out.</summary>
    /// <remarks>Auth: attendance_session.check_out:self</remarks>
    [HttpPost("check-out")]
    [RequirePermission(Permissions.AttendanceSessionCheckOutSelf)]
    [ProducesResponseType(typeof(CheckOutResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _checkOutHandler.HandleAsync(request, ipAddress, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get attendance dashboard summary.</summary>
    /// <remarks>Auth: attendance_session.read:self OR attendance_session.read:team</remarks>
    [HttpGet("dashboard")]
    [RequireAnyPermission(
        Permissions.AttendanceSessionReadSelf,
        Permissions.AttendanceSessionReadTeam,
        Permissions.AttendanceSessionReadTenant)]
    [ProducesResponseType(typeof(AttendanceDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardHandler.HandleAsync(employeeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get paginated attendance session history.</summary>
    /// <remarks>Auth: attendance_session.read:self OR attendance_session.read:team</remarks>
    [HttpGet("history")]
    [RequireAnyPermission(
        Permissions.AttendanceSessionReadSelf,
        Permissions.AttendanceSessionReadTeam,
        Permissions.AttendanceSessionReadTenant)]
    [ProducesResponseType(typeof(PagedResult<AttendanceSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] GetAttendanceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _historyHandler.HandleAsync(query, cancellationToken);
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
