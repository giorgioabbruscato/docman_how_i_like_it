using HrPortal.SharedKernel.Entities;

namespace HrPortal.Documents.Domain;

public enum DocumentCategory
{
    Contract,
    IdentityDocument,
    Certificate,
    Payslip,
    Other
}

public sealed class Document : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public DocumentCategory Category { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid? UploadedBy { get; private set; }

    private Document() { }

    public static Document Create(
        Guid tenantId,
        Guid employeeId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storagePath,
        DocumentCategory category,
        Guid? uploadedBy = null)
    {
        return new Document
        {
            EmployeeId = employeeId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            Category = category,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            CreatedBy = uploadedBy
        }.Also(d => d.SetTenant(tenantId));
    }
}

internal static class DocumentExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
