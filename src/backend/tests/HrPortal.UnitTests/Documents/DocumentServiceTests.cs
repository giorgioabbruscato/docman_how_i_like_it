using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Documents.Application;
using HrPortal.Documents.Application.Dtos;
using HrPortal.Employees.Application;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Storage;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Documents;

public sealed class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repository = new();
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IStorageProvider> _storageProvider = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<INotificationRecipientResolver> _recipientResolver = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _storageProvider
            .Setup(s => s.UploadAsync(It.IsAny<StorageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageObject
            {
                Path = $"{_tenantContext.TenantId}/employee/documents/test.pdf",
                ContentType = "application/pdf",
                SizeBytes = 100
            });

        _service = new DocumentService(
            _repository.Object,
            _employeeLookup.Object,
            _storageProvider.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            _notificationService.Object,
            _recipientResolver.Object,
            NullLogger<DocumentService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_ReturnsValidationError_WhenCategoryInvalid()
    {
        await using var stream = new MemoryStream("content"u8.ToArray());

        var result = await _service.UploadAsync(
            new UploadDocumentRequest(Guid.NewGuid(), "InvalidCategory"),
            stream,
            "file.pdf",
            "application/pdf",
            100);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UploadAsync_ReturnsNotFound_WhenEmployeeMissing()
    {
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await using var stream = new MemoryStream("content"u8.ToArray());

        var result = await _service.UploadAsync(
            new UploadDocumentRequest(Guid.NewGuid(), "Contract"),
            stream,
            "file.pdf",
            "application/pdf",
            100);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task UploadAsync_ReturnsValidationError_WhenFileInvalid()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await using var stream = new MemoryStream("content"u8.ToArray());

        var result = await _service.UploadAsync(
            new UploadDocumentRequest(employeeId, "Contract"),
            stream,
            "file.pdf",
            "application/zip",
            100);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UploadAsync_Succeeds_WhenValid()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _employeeLookup.Setup(e => e.GetEmailAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("employee@demo.local");
        _recipientResolver.Setup(r => r.ResolveForEmployeeAsync(
                employeeId,
                "employee@demo.local",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(null, "employee@demo.local"));

        await using var stream = new MemoryStream("content"u8.ToArray());

        var result = await _service.UploadAsync(
            new UploadDocumentRequest(employeeId, "Contract"),
            stream,
            "file.pdf",
            "application/pdf",
            100);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(employeeId);
        _storageProvider.Verify(s => s.UploadAsync(
            It.Is<StorageUploadRequest>(r => r.TenantId == _tenantContext.TenantId),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HrPortal.Documents.Domain.Document?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }
}
