using HrPortal.Audit.Application;
using HrPortal.Departments.Application;
using HrPortal.Departments.Application.Dtos;
using HrPortal.Departments.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace HrPortal.UnitTests.Departments;

public sealed class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(
            _repository.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
        NullLogger<DepartmentService>.Instance);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task ExistsAndIsActiveAsync_ReturnsTrue_WhenActive()
    {
        var id = Guid.NewGuid();
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        var exists = await _service.ExistsAndIsActiveAsync(id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenCodeExists()
    {
        _repository.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(new CreateDepartmentRequest("Engineering", "ENG"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenValid()
    {
        _repository.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(new CreateDepartmentRequest("Engineering", "ENG", "Dev team"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Engineering");
        result.Value.Code.Should().Be("ENG");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var result = await _service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateDepartmentRequest("Engineering", "ENG"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task DeactivateAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var result = await _service.DeactivateAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }
}
