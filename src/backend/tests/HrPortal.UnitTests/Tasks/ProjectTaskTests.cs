using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Exceptions;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.UnitTests.Tasks;

public sealed class ProjectTaskTests
{
    [Fact]
    public void Create_SetsDefaults()
    {
        var tenantId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var task = ProjectTask.Create(tenantId, projectId, "Implement login", TaskPriority.High);

        task.TenantId.Should().Be(tenantId);
        task.ProjectId.Should().Be(projectId);
        task.Title.Should().Be("Implement login");
        task.Priority.Should().Be(TaskPriority.High);
        task.Status.Should().Be(DomainTaskStatus.Todo);
        task.SpentHours.Should().Be(0);
    }

    [Fact]
    public void Create_Throws_WhenTitleEmpty()
    {
        var act = () => ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "  ", TaskPriority.Low);
        act.Should().Throw<DomainException>().WithMessage("*title*");
    }

    [Fact]
    public void Create_Throws_WhenTitleTooLong()
    {
        var act = () => ProjectTask.Create(
            Guid.NewGuid(), Guid.NewGuid(), new string('x', 301), TaskPriority.Low);
        act.Should().Throw<DomainException>().WithMessage("*300*");
    }

    [Fact]
    public void Create_Throws_WhenEstimatedHoursNegative()
    {
        var act = () => ProjectTask.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Task", TaskPriority.Low, estimatedHours: -1);
        act.Should().Throw<DomainException>().WithMessage("*Estimated hours*");
    }

    [Fact]
    public void Update_Throws_WhenSpentHoursNegative()
    {
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task", TaskPriority.Low);

        var act = () => task.Update(
            task.ProjectId,
            "Task",
            TaskPriority.Low,
            DomainTaskStatus.Todo,
            null,
            null,
            null,
            -0.5m,
            null,
            Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*Spent hours*");
    }

    [Fact]
    public void Update_UpdatesFields()
    {
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task", TaskPriority.Low);

        task.Update(
            task.ProjectId,
            "Updated Task",
            TaskPriority.Critical,
            DomainTaskStatus.InProgress,
            "Details",
            null,
            10m,
            2m,
            new DateOnly(2025, 12, 31),
            Guid.NewGuid());

        task.Title.Should().Be("Updated Task");
        task.Priority.Should().Be(TaskPriority.Critical);
        task.Status.Should().Be(DomainTaskStatus.InProgress);
        task.SpentHours.Should().Be(2m);
    }

    [Theory]
    [InlineData(DomainTaskStatus.Todo, DomainTaskStatus.InProgress)]
    [InlineData(DomainTaskStatus.Done, DomainTaskStatus.Todo)]
    [InlineData(DomainTaskStatus.Review, DomainTaskStatus.InProgress)]
    public void UpdateStatus_AllowsFreeKanbanTransitions(DomainTaskStatus from, DomainTaskStatus to)
    {
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task", TaskPriority.Low, from);

        task.UpdateStatus(to, Guid.NewGuid());

        task.Status.Should().Be(to);
    }

    [Fact]
    public void UpdateStatus_Throws_WhenSameStatus()
    {
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Task", TaskPriority.Low);

        var act = () => task.UpdateStatus(DomainTaskStatus.Todo, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("*already in the requested status*")
            .Where(ex => ex.ErrorCode == "INVALID_TRANSITION");
    }
}
