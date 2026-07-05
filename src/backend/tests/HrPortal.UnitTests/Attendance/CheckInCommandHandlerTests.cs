using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Commands;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Attendance;

public sealed class CheckInCommandHandlerTests
{
    private readonly Mock<IAttendanceSessionRepository> _repository = new();
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext;
    private readonly CheckInCommandHandler _handler;

    public CheckInCommandHandlerTests()
    {
        _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
        {
            UserId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid()
        };

        _handler = new CheckInCommandHandler(
            _repository.Object,
            _employeeLookup.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            NullLogger<CheckInCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_ReturnsForbidden_WhenEmployeeContextMissing()
    {
        var context = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo");
        var handler = new CheckInCommandHandler(
            _repository.Object,
            _employeeLookup.Object,
            _unitOfWork.Object,
            context,
            _auditService.Object,
            NullLogger<CheckInCommandHandler>.Instance);

        var result = await handler.HandleAsync(new CheckInRequest(), null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenEmployeeInactive()
    {
        _employeeLookup
            .Setup(e => e.ExistsAndIsActiveAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CheckInRequest(), "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_ReturnsConflict_WhenOpenSessionExists()
    {
        _employeeLookup
            .Setup(e => e.ExistsAndIsActiveAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository
            .Setup(r => r.GetOpenSessionAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AttendanceSession.Create(
                _tenantContext.TenantId,
                _tenantContext.EmployeeId!.Value,
                DateTime.UtcNow));

        var result = await _handler.HandleAsync(new CheckInRequest(), "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenValid()
    {
        _employeeLookup
            .Setup(e => e.ExistsAndIsActiveAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository
            .Setup(r => r.GetOpenSessionAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceSession?)null);

        var result = await _handler.HandleAsync(
            new CheckInRequest(45.4642, 9.19, 12.5, "Europe/Rome", "iPhone", "Safari"),
            "127.0.0.1");

        result.IsSuccess.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(_tenantContext.EmployeeId!.Value);
        result.Value.Status.Should().Be("Open");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
