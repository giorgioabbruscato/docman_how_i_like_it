using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Notifications;
using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Commands;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Notifications;

public sealed class AddProjectMemberNotificationTests
{
    [Fact]
    public async Task HandleAsync_NotifiesAssignee_AfterSuccessfulSave()
    {
        var tenantId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var project = Project.Create(tenantId, "Portal Redesign", ProjectStatus.Active);

        var projectRepository = new Mock<IProjectRepository>();
        projectRepository.Setup(r => r.ExistsAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        projectRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var memberRepository = new Mock<IProjectMemberRepository>();
        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        employeeLookup.Setup(l => l.GetEmailAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync("dev@demo.local");

        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver.Setup(r => r.ResolveForEmployeeAsync(employeeId, "dev@demo.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(userId, userId.ToString()));

        var notificationService = new Mock<INotificationService>();

        var handler = new AddProjectMemberCommandHandler(
            projectRepository.Object,
            memberRepository.Object,
            employeeLookup.Object,
            Mock.Of<IUnitOfWork>(),
            TenantContext.CreateTenantOnly(tenantId, "demo") with { UserId = Guid.NewGuid() },
            Mock.Of<IAuditService>(),
            notificationService.Object,
            recipientResolver.Object,
            NullLogger<AddProjectMemberCommandHandler>.Instance);

        var result = await handler.HandleAsync(projectId, new AddProjectMemberRequest(employeeId, ProjectMemberRole.Member));

        result.IsSuccess.Should().BeTrue();
        notificationService.Verify(
            n => n.NotifyProjectAssignedAsync(userId, "Portal Redesign", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
