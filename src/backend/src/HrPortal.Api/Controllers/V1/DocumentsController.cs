using HrPortal.Authorization;
using HrPortal.Documents.Application;
using HrPortal.Documents.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

[ApiController]
[Route("api/v1/documents")]
[Authorize(Policy = Policies.Authenticated)]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService) =>
        _documentService = documentService;

    [HttpGet]
    [Authorize(Policy = Policies.ManagerOrAbove)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _documentService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentRequest request,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = "File is required."
            });

        await using var stream = file.OpenReadStream();
        var result = await _documentService.UploadAsync(
            request,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return MapFailure(result);

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.HrOrAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentService.DeleteAsync(id, cancellationToken);
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
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
