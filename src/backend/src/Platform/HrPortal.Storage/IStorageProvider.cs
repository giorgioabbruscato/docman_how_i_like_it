namespace HrPortal.Storage;

public interface IStorageProvider
{
    Task<StorageObject> UploadAsync(StorageUploadRequest request, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
}
