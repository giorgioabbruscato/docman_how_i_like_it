namespace HrPortal.Documents.Application.Dtos;

public sealed record DocumentDto(
    Guid Id,
    Guid EmployeeId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Category,
    DateTime UploadedAt,
    Guid? UploadedBy);

public sealed record UploadDocumentRequest(
    Guid EmployeeId,
    string Category);

public sealed record DocumentDownloadDto(
    Stream Content,
    string FileName,
    string ContentType);
