using HrPortal.AccessControl.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Leave.Infrastructure.Workflows;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using HrPortal.Workflows.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Notifications;

public sealed class LeaveApprovalNotificationTests
{
    [Fact]
    public async Task CompletionHandler_NotifiesEmployee_AfterFinalApproval()
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

        var scopeFactory = CreateScopeFactory(tenantContext.TenantId);

        var handler = new LeaveWorkflowCompletionHandler(
            repository.Object,
            employeeLookup.Object,
            Mock.Of<IUnitOfWork>(),
            tenantContext,
            notificationService.Object,
            recipientResolver.Object,
            scopeFactory,
            NullLogger<LeaveWorkflowCompletionHandler>.Instance);

        var instance = WorkflowInstance.Create(
            tenantContext.TenantId,
            Guid.NewGuid(),
            WorkflowRequestType.Leave,
            leaveRequest.Id,
            employeeId);

        var result = await handler.HandleCompletionAsync(
            instance,
            WorkflowStatus.Approved,
            employeeId,
            null);

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

    private static IServiceScopeFactory CreateScopeFactory(Guid tenantId)
    {
        var tenantAccessor = new Mock<ITenantContextAccessor>();
        tenantAccessor.Setup(a => a.Set(It.IsAny<TenantContext>()));

        var syncService = Mock.Of<ILeaveCalendarSyncService>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(p => p.GetService(typeof(ITenantContextAccessor))).Returns(tenantAccessor.Object);
        serviceProvider.Setup(p => p.GetService(typeof(ILeaveCalendarSyncService))).Returns(syncService);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);

        var factory = new Mock<IServiceScopeFactory>();
        factory.Setup(f => f.CreateScope()).Returns(scope.Object);
        return factory.Object;
    }
}
