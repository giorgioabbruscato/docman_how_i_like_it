using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Employees.Application;
using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using Moq;

namespace HrPortal.UnitTests.Reporting;

public sealed class AttendanceReportGeneratorTests
{
    [Theory]
    [InlineData("csv")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public async Task GenerateAsync_ProducesNonEmptyBytes_ForEachFormat(string format)
    {
        var employeeId = Guid.NewGuid();
        var session = AttendanceSession.Create(
            Guid.NewGuid(),
            employeeId,
            DateTime.UtcNow.AddHours(-8));
        session.Close(DateTime.UtcNow);

        var repository = new Mock<IAttendanceSessionRepository>();
        repository.Setup(r => r.GetHistoryAsync(
                It.IsAny<AttendanceSessionReadFilter>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AttendanceSession> { session }, 1));

        var employeeLookup = new Mock<IEmployeeLookup>();
        employeeLookup.Setup(l => l.GetFullNameAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Jane Doe");
        employeeLookup.Setup(l => l.GetDepartmentIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Guid?>());

        var generator = new AttendanceReportGenerator(repository.Object, employeeLookup.Object);
        var scope = new ReportGenerateFilter(null, null);
        var query = new ReportQueryParams(format);

        var (content, _, _) = await generator.GenerateAsync(query, scope, CancellationToken.None);

        content.Should().NotBeEmpty();
    }
}
