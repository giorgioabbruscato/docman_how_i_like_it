using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Documents.Application.Dtos;
using HrPortal.Documents.Domain;
using HrPortal.Employees.Application;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Storage;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Documents.Application;

public interface IDocumentService
{
    Task<Result<IReadOnlyList<DocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DocumentDto>> UploadAsync(
        UploadDocumentRequest request,
        Stream fileStream,
        string fileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken = default);
    Task<Result<DocumentDownloadDto>> DownloadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IStorageProvider _storageProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository repository,
        IEmployeeLookup employeeLookup,
        IStorageProvider storageProvider,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _storageProvider = storageProvider;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(documents.Select(MapToDto).ToList() as IReadOnlyList<DocumentDto>);
    }

    public async Task<Result<DocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return Result.Failure<DocumentDto>("Document not found.", "NOT_FOUND");

        return Result.Success(MapToDto(document));
    }

    public async Task<Result<DocumentDto>> UploadAsync(
        UploadDocumentRequest request,
        Stream fileStream,
        string fileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DocumentCategory>(request.Category, true, out var category))
            return Result.Failure<DocumentDto>("Invalid document category.", "VALIDATION_ERROR");

        if (!await _employeeLookup.ExistsAndIsActiveAsync(request.EmployeeId, cancellationToken))
            return Result.Failure<DocumentDto>("Employee not found or inactive.", "NOT_FOUND");

        var fileValidation = DocumentUploadRules.Validate(contentType, sizeBytes);
        if (!fileValidation.IsSuccess)
            return Result.Failure<DocumentDto>(fileValidation.Error!, "VALIDATION_ERROR");

        var storageFileName = $"{Guid.NewGuid():N}_{fileName}";
        var storageResult = await _storageProvider.UploadAsync(new StorageUploadRequest
        {
            TenantId = _tenantContext.TenantId,
            Category = "employee/documents",
            FileName = storageFileName,
            Content = fileStream,
            ContentType = contentType
        }, cancellationToken);

        var document = Document.Create(
            _tenantContext.TenantId,
            request.EmployeeId,
            fileName,
            contentType,
            storageResult.SizeBytes,
            storageResult.Path,
            category,
            _tenantContext.UserId);

        await _repository.AddAsync(document, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "document.uploaded",
            nameof(Document),
            document.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var email = await _employeeLookup.GetEmailAsync(request.EmployeeId, cancellationToken)
            ?? request.EmployeeId.ToString();
        var recipient = await _recipientResolver.ResolveForEmployeeAsync(request.EmployeeId, email, cancellationToken);
        if (recipient.UserId.HasValue)
        {
            await NotificationHelper.TryNotifyAsync(
                _logger,
                ct => _notificationService.NotifyDocumentUploadedAsync(
                    recipient.UserId.Value,
                    document.FileName,
                    ct),
                cancellationToken);
        }

        _logger.LogInformation("Document {DocumentId} uploaded for employee {EmployeeId}", document.Id, request.EmployeeId);

        return Result.Success(MapToDto(document));
    }

    public async Task<Result<DocumentDownloadDto>> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return Result.Failure<DocumentDownloadDto>("Document not found.", "NOT_FOUND");

        var stream = await _storageProvider.DownloadAsync(document.StoragePath, cancellationToken);
        return Result.Success(new DocumentDownloadDto(stream, document.FileName, document.ContentType));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return Result.Failure("Document not found.", "NOT_FOUND");

        await _storageProvider.DeleteAsync(document.StoragePath, cancellationToken);
        await _repository.DeleteAsync(document, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "document.deleted",
            nameof(Document),
            document.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Document {DocumentId} deleted", id);

        return Result.Success();
    }

    private static DocumentDto MapToDto(Document document) =>
        new(
            document.Id,
            document.EmployeeId,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.Category.ToString(),
            document.UploadedAt,
            document.UploadedBy);
}
