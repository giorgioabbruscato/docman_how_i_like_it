using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Current authenticated user context.</summary>
[ApiController]
[Route("api/v1/me")]
[Tags("Access Control")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class MeController : ControllerBase
{
    private readonly IMeService _meService;

    public MeController(IMeService meService) => _meService = meService;

    /// <summary>Get current user profile, permissions, and tenant features.</summary>
    /// <remarks>Auth: Authenticated</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(MeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _meService.GetCurrentAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "UNAUTHORIZED" => Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = result.Error
            }),
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
