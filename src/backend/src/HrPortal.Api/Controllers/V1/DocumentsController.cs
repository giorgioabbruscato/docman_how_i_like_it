using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Documents.Application;
using HrPortal.Documents.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Employee document upload and download.</summary>
[ApiController]
[Route("api/v1/documents")]
[Tags("Documents")]
[Authorize(Policy = Policies.Authenticated)]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService) =>
        _documentService = documentService;

    /// <summary>List all documents.</summary>
    /// <remarks>Auth: document.read:tenant</remarks>
    [HttpGet]
    [RequirePermission(Permissions.DocumentReadTenant)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _documentService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get document metadata by ID.</summary>
    /// <remarks>Auth: document.read:tenant OR document.read:self</remarks>
    [HttpGet("{id:guid}")]
    [RequireAnyPermission(Permissions.DocumentReadTenant, Permissions.DocumentReadSelf)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Upload a document for an employee.</summary>
    /// <remarks>Auth: document.upload:self. Form fields: employeeId, category, file.</remarks>
    [HttpPost]
    [RequirePermission(Permissions.DocumentUploadSelf)]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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

    /// <summary>Download document file content.</summary>
    /// <remarks>Auth: document.read:tenant OR document.read:self</remarks>
    [HttpGet("{id:guid}/download")]
    [RequireAnyPermission(Permissions.DocumentReadTenant, Permissions.DocumentReadSelf)]
    [Produces("application/octet-stream", "application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return MapFailure(result);

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>Delete a document.</summary>
    /// <remarks>Auth: document.delete:tenant</remarks>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.DocumentDeleteTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
