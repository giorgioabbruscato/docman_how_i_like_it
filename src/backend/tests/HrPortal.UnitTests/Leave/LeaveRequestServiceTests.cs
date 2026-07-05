using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Leave.Application.Dtos;
using HrPortal.Leave.Domain;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Leave;

public sealed class LeaveRequestServiceTests
{
    private readonly Mock<ILeaveRequestRepository> _repository = new();
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<INotificationRecipientResolver> _recipientResolver = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly LeaveRequestService _service;

    public LeaveRequestServiceTests()
    {
        _service = new LeaveRequestService(
            _repository.Object,
            _employeeLookup.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            _notificationService.Object,
            _recipientResolver.Object,
            NullLogger<LeaveRequestService>.Instance);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CreateAsync_ReturnsValidationError_WhenTypeInvalid()
    {
        var result = await _service.CreateAsync(new CreateLeaveRequest(
            Guid.NewGuid(),
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 7, 5),
            "InvalidType"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenEmployeeMissing()
    {
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(new CreateLeaveRequest(
            Guid.NewGuid(),
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 7, 5),
            "Annual"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenOverlappingApproved()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(r => r.HasOverlappingApprovedAsync(
                employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(new CreateLeaveRequest(
            employeeId,
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 7, 5),
            "Annual"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenAnnualLimitExceeded()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(r => r.HasOverlappingApprovedAsync(
                employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.GetApprovedAnnualDaysInYearAsync(
                employeeId,
                2025,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(LeaveRequest.MaxAnnualLeaveDays);

        var result = await _service.CreateAsync(new CreateLeaveRequest(
            employeeId,
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 7, 5),
            "Annual"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenValid()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(e => e.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(r => r.HasOverlappingApprovedAsync(
                employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.GetApprovedAnnualDaysInYearAsync(
                employeeId,
                2025,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _service.CreateAsync(new CreateLeaveRequest(
            employeeId,
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 7, 5),
            "Annual"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(employeeId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
