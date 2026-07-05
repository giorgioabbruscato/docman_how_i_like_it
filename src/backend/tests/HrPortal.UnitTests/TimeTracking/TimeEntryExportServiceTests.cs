using HrPortal.Employees.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.TimeTracking.Infrastructure.Export;
using Moq;

namespace HrPortal.UnitTests.TimeTracking;

public sealed class TimeEntryExportServiceTests
{
    [Fact]
    public async Task MapRowsAsync_MapsHoursAndNames()
    {
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var start = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var entry = TimeEntry.Create(
            Guid.NewGuid(),
            employeeId,
            projectId,
            start,
            start.AddMinutes(90),
            taskId);

        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.GetFullNameAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Jane Doe");

        var projectLookup = new Mock<IProjectLookup>();
        projectLookup.Setup(l => l.GetNameAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Portal Redesign");

        var taskLookup = new Mock<ITaskLookup>();
        taskLookup.Setup(l => l.GetTitleAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("API wiring");

        var rows = await TimeEntryExportService.MapRowsAsync(
            [entry],
            includeEmployeeName: true,
            employeeLookup.Object,
            projectLookup.Object,
            taskLookup.Object,
            CancellationToken.None);

        rows.Should().ContainSingle();
        rows[0].EmployeeName.Should().Be("Jane Doe");
        rows[0].ProjectName.Should().Be("Portal Redesign");
        rows[0].TaskTitle.Should().Be("API wiring");
        rows[0].Hours.Should().Be(1.5m);
        rows[0].Date.Should().Be(new DateOnly(2026, 7, 5));
    }
}
