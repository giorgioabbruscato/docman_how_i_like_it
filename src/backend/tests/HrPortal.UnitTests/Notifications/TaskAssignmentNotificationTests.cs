using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Notifications;
using HrPortal.Projects.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tasks.Application;
using HrPortal.Tasks.Application.Commands;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.UnitTests.Notifications;

public sealed class TaskAssignmentNotificationTests
{
    [Fact]
    public async Task UpdateHandleAsync_Notifies_WhenAssigneeChanges()
    {
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = ProjectTask.Create(
            Guid.NewGuid(),
            projectId,
            "Implement API",
            TaskPriority.Medium,
            DomainTaskStatus.Todo);

        var repository = new Mock<IProjectTaskRepository>();
        repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var projectLookup = new Mock<IProjectLookup>();
        projectLookup.Setup(l => l.ExistsAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        projectLookup.Setup(l => l.GetNameAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync("Portal");

        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.ExistsAndIsActiveAsync(assigneeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        employeeLookup.Setup(l => l.GetEmailAsync(assigneeId, It.IsAny<CancellationToken>())).ReturnsAsync("dev@demo.local");

        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver.Setup(r => r.ResolveForEmployeeAsync(assigneeId, "dev@demo.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationRecipient(userId, userId.ToString()));

        var notificationService = new Mock<INotificationService>();

        var handler = new UpdateProjectTaskCommandHandler(
            repository.Object,
            projectLookup.Object,
            employeeLookup.Object,
            Mock.Of<IUnitOfWork>(),
            TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with { UserId = Guid.NewGuid() },
            Mock.Of<IAuditService>(),
            notificationService.Object,
            recipientResolver.Object,
            NullLogger<UpdateProjectTaskCommandHandler>.Instance);

        var result = await handler.HandleAsync(task.Id, new UpdateProjectTaskRequest(
            projectId,
            "Implement API",
            TaskPriority.Medium,
            DomainTaskStatus.Todo,
            AssignedEmployeeId: assigneeId));

        result.IsSuccess.Should().BeTrue();
        notificationService.Verify(
            n => n.NotifyTaskAssignedAsync(userId, "Implement API", "Portal", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
