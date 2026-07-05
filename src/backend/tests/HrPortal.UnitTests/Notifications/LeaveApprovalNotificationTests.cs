using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Notifications;

public sealed class LeaveApprovalNotificationTests
{
    [Fact]
    public async Task ApproveAsync_NotifiesEmployee_AfterSuccessfulSave()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var leaveRequest = LeaveRequest.Create(
            Guid.NewGuid(),
            employeeId,
            new DateOnly(2025, 8, 1),
            new DateOnly(2025, 8, 5),
            LeaveType.Annual,
            "Vacation");

        var repository = new Mock<ILeaveRequestRepository>();
        repository.Setup(r => r.GetByIdAsync(leaveRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        repository.Setup(r => r.HasOverlappingApprovedAsync(
                employeeId,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                leaveRequest.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository.Setup(r => r.GetApprovedAnnualDaysInYearAsync(
                employeeId,
                2025,
                leaveRequest.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.GetEmailAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("employee@demo.local");

        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver.Setup(r => r.ResolveForEmployeeAsync(employeeId, "employee@demo.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(userId, userId.ToString()));

        var notificationService = new Mock<INotificationService>();
        var tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with { UserId = Guid.NewGuid() };

        var service = new LeaveRequestService(
            repository.Object,
            employeeLookup.Object,
            Mock.Of<IUnitOfWork>(),
            tenantContext,
            Mock.Of<IAuditService>(),
            notificationService.Object,
            recipientResolver.Object,
            NullLogger<LeaveRequestService>.Instance);

        var result = await service.ApproveAsync(leaveRequest.Id);

        result.IsSuccess.Should().BeTrue();
        await Task.Delay(100);
        notificationService.Verify(
            n => n.NotifyLeaveApprovedAsync(
                userId,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
