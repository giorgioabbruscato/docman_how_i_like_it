using HrPortal.Authorization;
using HrPortal.Notifications.Application;
using HrPortal.Notifications.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>User notification inbox.</summary>
[ApiController]
[Route("api/v1/notifications")]
[Tags("Notifications")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationInboxService _inboxService;

    public NotificationsController(INotificationInboxService inboxService) =>
        _inboxService = inboxService;

    /// <summary>List notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserNotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _inboxService.GetNotificationsAsync(page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(UserNotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _inboxService.MarkAsReadAsync(id, cancellationToken);
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
