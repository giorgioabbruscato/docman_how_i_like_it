using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Attendance check-in/out and reporting.</summary>
[ApiController]
[Route("api/v1/attendance")]
[Tags("Attendance")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService) =>
        _attendanceService = attendanceService;

    /// <summary>List attendance records.</summary>
    /// <remarks>Auth: ManagerOrAbove</remarks>
    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    [ProducesResponseType(typeof(IEnumerable<AttendanceRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Record employee check-in.</summary>
    /// <remarks>Auth: Authenticated</remarks>
    [HttpPost("check-in")]
    [ProducesResponseType(typeof(AttendanceRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckInAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Record employee check-out.</summary>
    /// <remarks>Auth: Authenticated</remarks>
    [HttpPost("check-out")]
    [ProducesResponseType(typeof(AttendanceRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckOutAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get attendance report for a date range.</summary>
    /// <remarks>Auth: ManagerOrAbove. Query params: from, to (DateOnly).</remarks>
    [HttpGet("reports")]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    [ProducesResponseType(typeof(AttendanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetReportAsync(from, to, cancellationToken);
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
