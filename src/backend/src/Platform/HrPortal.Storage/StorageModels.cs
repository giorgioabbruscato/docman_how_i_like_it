namespace HrPortal.Storage;

public sealed class StorageObject
{
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public long SizeBytes { get; init; }
}

public sealed class StorageUploadRequest
{
    public required Guid TenantId { get; init; }
    public required string Category { get; init; }
    public required string FileName { get; init; }
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
}
