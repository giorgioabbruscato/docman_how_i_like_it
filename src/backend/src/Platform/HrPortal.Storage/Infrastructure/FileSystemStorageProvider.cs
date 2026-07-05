using Microsoft.Extensions.Options;

namespace HrPortal.Storage.Infrastructure;

public sealed class FileSystemStorageProvider : IStorageProvider
{
    private readonly StorageOptions _options;

    public FileSystemStorageProvider(IOptions<StorageOptions> options) =>
        _options = options.Value;

    public async Task<StorageObject> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var relativePath = BuildPath(request.TenantId, request.Category, request.FileName);
        var fullPath = GetFullPath(relativePath);

        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await request.Content.CopyToAsync(fileStream, cancellationToken);

        var fileInfo = new FileInfo(fullPath);

        return new StorageObject
        {
            Path = relativePath,
            ContentType = request.ContentType,
            SizeBytes = fileInfo.Length
        };
    }

    public Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Storage object not found.", path);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default) =>
        Task.FromResult(File.Exists(GetFullPath(path)));

    private string GetFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, normalized));
        var rootFullPath = Path.GetFullPath(_options.RootPath);

        if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");

        return fullPath;
    }

    // Paths are prefixed with tenantId. In single-tenant mode, middleware resolves the default
    // tenant so callers (e.g. DocumentService) always pass a concrete tenantId, not Guid.Empty.
    private static string BuildPath(Guid tenantId, string category, string fileName)
    {
        var safeCategory = SanitizeSegment(category);
        var safeFileName = SanitizeSegment(fileName);
        return $"{tenantId}/{safeCategory}/{safeFileName}";
    }

    private static string SanitizeSegment(string value) =>
        string.Concat(value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
}
