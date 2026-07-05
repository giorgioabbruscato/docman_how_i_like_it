using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Attendance;

public sealed class AttendanceServiceTests
{
    private readonly Mock<IAttendanceRepository> _repository = new();
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly AttendanceService _service;

    public AttendanceServiceTests()
    {
        _service = new AttendanceService(
            _repository.Object,
            _employeeLookup.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            NullLogger<AttendanceService>.Instance);
    }

    [Fact]
    public async Task CheckInAsync_ReturnsNotFound_WhenEmployeeMissing()
    {
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CheckInAsync(new CheckInRequest(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CheckInAsync_Succeeds_WhenValid()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(r => r.GetByEmployeeAndDateAsync(
                employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        var result = await _service.CheckInAsync(new CheckInRequest(employeeId));

        result.IsSuccess.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(employeeId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_ReturnsNotFound_WhenNoRecord()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(r => r.GetByEmployeeAndDateAsync(
                employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        var result = await _service.CheckOutAsync(new CheckOutRequest(employeeId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task GetReportAsync_ReturnsValidationError_WhenEndBeforeStart()
    {
        var result = await _service.GetReportAsync(
            new DateOnly(2025, 7, 10),
            new DateOnly(2025, 7, 1));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}
