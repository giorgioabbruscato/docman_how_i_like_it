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

public sealed class NotificationFailureIsolationTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenNotificationFails()
    {
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var projectRepository = new Mock<IProjectRepository>();
        projectRepository.Setup(r => r.ExistsAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        projectRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Project.Create(tenantId, "Portal", ProjectStatus.Active));

        var memberRepository = new Mock<IProjectMemberRepository>();
        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        employeeLookup.Setup(l => l.GetEmailAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync("dev@demo.local");

        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver.Setup(r => r.ResolveForEmployeeAsync(employeeId, "dev@demo.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(Guid.NewGuid(), "user"));

        var notificationService = new Mock<INotificationService>();
        notificationService.Setup(n => n.NotifyProjectAssignedAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("notification down"));

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
    }
}
