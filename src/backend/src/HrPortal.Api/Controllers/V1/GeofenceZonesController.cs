using HrPortal.AccessControl.Domain;
using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/geofence-zones")]
[Tags("Attendance")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class GeofenceZonesController : ControllerBase
{
    private readonly IGeofenceService _geofenceService;

    public GeofenceZonesController(IGeofenceService geofenceService) =>
        _geofenceService = geofenceService;

    [HttpGet]
    [RequirePermission(Permissions.GeofenceReadTenant)]
    [ProducesResponseType(typeof(IEnumerable<GeofenceZoneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetZones(CancellationToken cancellationToken)
    {
        var result = await _geofenceService.GetZonesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.GeofenceManageTenant)]
    [ProducesResponseType(typeof(GeofenceZoneDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGeofenceZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _geofenceService.CreateZoneAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetZones), result.Value)
            : MapFailure(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.GeofenceManageTenant)]
    [ProducesResponseType(typeof(GeofenceZoneDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGeofenceZoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _geofenceService.UpdateZoneAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.GeofenceManageTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _geofenceService.DeleteZoneAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    [HttpGet("settings")]
    [RequirePermission(Permissions.GeofenceReadTenant)]
    [ProducesResponseType(typeof(GeofenceSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _geofenceService.GetSettingsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPut("settings")]
    [RequirePermission(Permissions.GeofenceManageTenant)]
    [ProducesResponseType(typeof(GeofenceSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateGeofenceSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _geofenceService.UpdateSettingsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails { Detail = result.Error }),
            "GEOFENCE_VIOLATION" => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geofence violation",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            }),
            _ => BadRequest(new ProblemDetails { Detail = result.Error })
        };
}
