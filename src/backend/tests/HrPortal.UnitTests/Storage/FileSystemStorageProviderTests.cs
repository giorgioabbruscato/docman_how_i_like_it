using HrPortal.Storage;
using HrPortal.Storage.Infrastructure;

namespace HrPortal.UnitTests.Storage;

public sealed class FileSystemStorageProviderTests : IDisposable
{
    private readonly string _rootPath;
    private readonly FileSystemStorageProvider _provider;

    public FileSystemStorageProviderTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"hrportal-test-{Guid.NewGuid():N}");
        _provider = new FileSystemStorageProvider(
            Microsoft.Extensions.Options.Options.Create(new StorageOptions { RootPath = _rootPath }));
    }

    [Fact]
    public async Task UploadAndDownload_RoundTripsContent()
    {
        var tenantId = Guid.NewGuid();
        var content = "test document content"u8.ToArray();

        await using var uploadStream = new MemoryStream(content);
        var uploadResult = await _provider.UploadAsync(new StorageUploadRequest
        {
            TenantId = tenantId,
            Category = "employee/documents",
            FileName = "contract.pdf",
            Content = uploadStream,
            ContentType = "application/pdf"
        });

        await using var downloadStream = await _provider.DownloadAsync(uploadResult.Path);
        using var reader = new MemoryStream();
        await downloadStream.CopyToAsync(reader);

        reader.ToArray().Should().BeEquivalentTo(content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, recursive: true);
    }
}
