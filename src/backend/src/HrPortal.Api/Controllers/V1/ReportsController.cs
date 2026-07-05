using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Reporting.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Generate downloadable reports in CSV, XLSX, or PDF format.</summary>
[ApiController]
[Route("api/v1/reports")]
[Tags("Reports")]
[Authorize(Policy = Policies.Authenticated)]
public sealed class ReportsController : ControllerBase
{
    private readonly GenerateReportQueryHandler _handler;

    public ReportsController(GenerateReportQueryHandler handler) =>
        _handler = handler;

    /// <summary>Generate a report file for the given type.</summary>
    [HttpGet("{type}")]
    [RequireAnyPermission(
        Permissions.ReportGenerateSelf,
        Permissions.ReportGenerateTeam,
        Permissions.ReportGenerateTenant)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate(
        string type,
        [FromQuery] ReportQueryParams query,
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(type, query, cancellationToken);
        if (!result.IsSuccess)
            return MapFailure(result);

        var (content, contentType, fileName) = result.Value!;
        return File(content, contentType, fileName);
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
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
