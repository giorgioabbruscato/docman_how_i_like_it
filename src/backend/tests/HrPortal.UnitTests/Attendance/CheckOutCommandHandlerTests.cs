using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Commands;
using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Audit.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Attendance;

public sealed class CheckOutCommandHandlerTests
{
    private readonly Mock<IAttendanceSessionRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext;
    private readonly CheckOutCommandHandler _handler;

    public CheckOutCommandHandlerTests()
    {
        _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
        {
            UserId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid()
        };

        _handler = new CheckOutCommandHandler(
            _repository.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            NullLogger<CheckOutCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenNoOpenSession()
    {
        _repository
            .Setup(r => r.GetOpenSessionAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceSession?)null);

        var result = await _handler.HandleAsync(new CheckOutRequest(), "127.0.0.1");

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_Succeeds_AndReturnsWorkedMinutes()
    {
        var checkIn = DateTime.UtcNow.AddHours(-8);
        var session = AttendanceSession.Create(
            _tenantContext.TenantId,
            _tenantContext.EmployeeId!.Value,
            checkIn);

        _repository
            .Setup(r => r.GetOpenSessionAsync(_tenantContext.EmployeeId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var result = await _handler.HandleAsync(new CheckOutRequest(45.46, 9.19, 8), "127.0.0.1");

        result.IsSuccess.Should().BeTrue();
        result.Value!.WorkedMinutes.Should().BeGreaterThan(0);
        result.Value.Status.Should().Be("Closed");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
