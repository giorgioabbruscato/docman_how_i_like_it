using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/attendance")]
[Authorize(Policy = Policies.Authenticated)]
public sealed class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService) =>
        _attendanceService = attendanceService;

    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckInAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CheckOutAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpGet("reports")]
    [Authorize(Policy = Policies.ManagerOrAbove)]
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
