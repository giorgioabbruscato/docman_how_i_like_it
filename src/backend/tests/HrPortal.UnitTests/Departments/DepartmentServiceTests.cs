using HrPortal.Audit.Application;
using HrPortal.Departments.Application;
using HrPortal.Departments.Application.Dtos;
using HrPortal.Departments.Domain;
using HrPortal.Identity;
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
    private readonly TenantContext _tenantContext = TenantContext.Create(Guid.NewGuid(), "demo");
    private readonly UserContext _userContext = new() { UserId = Guid.NewGuid(), IsAuthenticated = true };
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(
            _repository.Object,
            _unitOfWork.Object,
            _tenantContext,
            _userContext,
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
}
