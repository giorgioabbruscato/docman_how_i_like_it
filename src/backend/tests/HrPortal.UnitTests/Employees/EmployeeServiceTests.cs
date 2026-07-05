using HrPortal.Audit.Application;
using HrPortal.Departments.Application;
using HrPortal.Employees.Application;
using HrPortal.Employees.Application.Dtos;
using HrPortal.Employees.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace HrPortal.UnitTests.Employees;

public sealed class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repository = new();
    private readonly Mock<IDepartmentLookup> _departmentLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<IFeatureGateService> _featureGateService = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _featureGateService
            .Setup(f => f.GetMaxEmployeesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(int.MaxValue);
        _repository
            .Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _service = new EmployeeService(
            _repository.Object,
            _departmentLookup.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            _featureGateService.Object,
        NullLogger<EmployeeService>.Instance);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenEmailExists()
    {
        _repository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(new CreateEmployeeRequest(
            "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 1)));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenDepartmentInvalid()
    {
        var departmentId = Guid.NewGuid();
        _repository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _departmentLookup.Setup(d => d.ExistsAndIsActiveAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(new CreateEmployeeRequest(
            "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 1), null, departmentId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CreateAsync_ReturnsPlanLimitExceeded_WhenAtCapacity()
    {
        _featureGateService
            .Setup(f => f.GetMaxEmployeesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(20);
        _repository
            .Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(20);

        var result = await _service.CreateAsync(new CreateEmployeeRequest(
            "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 1)));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAN_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenValid()
    {
        _repository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(new CreateEmployeeRequest(
            "Mario", "Rossi", "mario@demo.local", new DateOnly(2024, 1, 1)));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("mario@demo.local");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
