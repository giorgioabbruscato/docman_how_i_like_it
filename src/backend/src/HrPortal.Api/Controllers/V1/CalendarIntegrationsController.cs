using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Integrations.Application;
using HrPortal.Integrations.Application.Dtos;
using HrPortal.Integrations.Domain;
using HrPortal.Integrations.Infrastructure;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>External calendar OAuth and leave sync.</summary>
[ApiController]
[Route("api/v1/integrations/calendar")]
[Tags("Calendar Integrations")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class CalendarIntegrationsController : ControllerBase
{
    private readonly ICalendarConnectionService _connectionService;
    private readonly ICalendarSyncService _syncService;
    private readonly CalendarOAuthStateProtector _stateProtector;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public CalendarIntegrationsController(
        ICalendarConnectionService connectionService,
        ICalendarSyncService syncService,
        CalendarOAuthStateProtector stateProtector,
        ITenantContextAccessor tenantContextAccessor)
    {
        _connectionService = connectionService;
        _syncService = syncService;
        _stateProtector = stateProtector;
        _tenantContextAccessor = tenantContextAccessor;
    }

    /// <summary>List supported calendar providers.</summary>
    [HttpGet("providers")]
    [Authorize(Policy = Policies.Authenticated)]
    [ProducesResponseType(typeof(IEnumerable<CalendarProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders(CancellationToken cancellationToken)
    {
        var result = await _connectionService.GetProvidersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get OAuth authorization URL for a provider.</summary>
    [HttpGet("connect/{provider}")]
    [Authorize(Policy = Policies.Authenticated)]
    [RequirePermission(Permissions.CalendarConnectSelf)]
    [ProducesResponseType(typeof(CalendarConnectResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Connect(
        string provider,
        [FromQuery] string redirectUri,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CalendarProvider>(provider, ignoreCase: true, out var calendarProvider))
            return BadRequest(new ProblemDetails { Title = "Invalid provider", Detail = $"Unknown provider: {provider}" });

        var result = await _connectionService.GetConnectUrlAsync(calendarProvider, redirectUri, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>OAuth callback — exchanges code for tokens.</summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        var oauthState = _stateProtector.Unprotect(state);
        if (oauthState is null)
            return BadRequest(new ProblemDetails { Title = "Invalid OAuth state" });

        _tenantContextAccessor.Set(TenantScopingContext.ForSeeding(oauthState.TenantId) with
        {
            EmployeeId = oauthState.EmployeeId
        });

        var result = await _connectionService.HandleCallbackAsync(code, state, cancellationToken);
        if (!result.IsSuccess)
            return MapFailure(result);

        var callback = result.Value!;
        var separator = callback.RedirectUri.Contains('?') ? '&' : '?';
        var redirectUrl = callback.Success
            ? $"{callback.RedirectUri}{separator}success=true"
            : $"{callback.RedirectUri}{separator}success=false&error={Uri.EscapeDataString(callback.Error ?? "Connection failed")}";

        return Redirect(redirectUrl);
    }

    /// <summary>List current employee calendar connections.</summary>
    [HttpGet("connections")]
    [Authorize(Policy = Policies.Authenticated)]
    [RequirePermission(Permissions.CalendarConnectSelf)]
    [ProducesResponseType(typeof(IEnumerable<CalendarConnectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConnections(CancellationToken cancellationToken)
    {
        var result = await _connectionService.GetConnectionsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Disconnect a calendar connection.</summary>
    [HttpDelete("connections/{id:guid}")]
    [Authorize(Policy = Policies.Authenticated)]
    [RequirePermission(Permissions.CalendarConnectSelf)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Disconnect(Guid id, CancellationToken cancellationToken)
    {
        var result = await _connectionService.DisconnectAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    /// <summary>Manually trigger leave sync to external calendars.</summary>
    [HttpPost("sync/{leaveRequestId:guid}")]
    [Authorize(Policy = Policies.Authenticated)]
    [RequirePermission(Permissions.CalendarSyncManageTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SyncLeave(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncLeaveRequestAsync(leaveRequestId, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    /// <summary>View calendar sync log for tenant admins.</summary>
    [HttpGet("sync-log")]
    [Authorize(Policy = Policies.Authenticated)]
    [RequirePermission(Permissions.CalendarSyncManageTenant)]
    [ProducesResponseType(typeof(IEnumerable<CalendarSyncLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSyncLog(
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var result = await _syncService.GetSyncLogAsync(limit, cancellationToken);
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
            "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
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
