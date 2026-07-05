using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Calendar.Application;
using HrPortal.Calendar.Application.Dtos;
using HrPortal.Calendar.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/calendar")]
[Tags("Calendar")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class CalendarController : ControllerBase
{
    private readonly ICalendarQueryService _calendarQueryService;
    private readonly IPublicHolidayService _holidayService;

    public CalendarController(ICalendarQueryService calendarQueryService, IPublicHolidayService holidayService)
    {
        _calendarQueryService = calendarQueryService;
        _holidayService = holidayService;
    }

    [HttpGet("events")]
    [RequireAnyPermission(Permissions.CalendarReadTeam, Permissions.CalendarReadSelf)]
    [ProducesResponseType(typeof(IEnumerable<CalendarEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] GetCalendarEventsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _calendarQueryService.GetEventsAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpGet("holidays")]
    [RequirePermission(Permissions.CalendarManageTenant)]
    [ProducesResponseType(typeof(IEnumerable<PublicHolidayDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHolidays(CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost("holidays")]
    [RequirePermission(Permissions.CalendarManageTenant)]
    [ProducesResponseType(typeof(PublicHolidayDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateHoliday(
        [FromBody] CreatePublicHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _holidayService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetHolidays), result.Value) : MapFailure(result);
    }

    [HttpPut("holidays/{id:guid}")]
    [RequirePermission(Permissions.CalendarManageTenant)]
    [ProducesResponseType(typeof(PublicHolidayDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateHoliday(
        Guid id,
        [FromBody] UpdatePublicHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _holidayService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpDelete("holidays/{id:guid}")]
    [RequirePermission(Permissions.CalendarManageTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteHoliday(Guid id, CancellationToken cancellationToken)
    {
        var result = await _holidayService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails { Detail = result.Error }),
            "FORBIDDEN" => StatusCode(403, new ProblemDetails { Detail = result.Error }),
            _ => BadRequest(new ProblemDetails { Detail = result.Error })
        };
}
