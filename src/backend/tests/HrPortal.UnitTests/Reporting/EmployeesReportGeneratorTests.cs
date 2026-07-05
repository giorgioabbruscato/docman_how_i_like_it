using HrPortal.Departments.Application;
using HrPortal.Departments.Domain;
using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using Moq;

namespace HrPortal.UnitTests.Reporting;

public sealed class EmployeesReportGeneratorTests
{
    [Theory]
    [InlineData("csv")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public async Task GenerateAsync_ProducesNonEmptyBytes_ForEachFormat(string format)
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            "Jane",
            "Doe",
            "jane@demo.local",
            new DateOnly(2024, 1, 1));

        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { employee });

        var departmentLookup = new Mock<IDepartmentLookup>();
        var generator = new EmployeesReportGenerator(repository.Object, departmentLookup.Object);
        var scope = new ReportGenerateFilter(null, null);
        var query = new ReportQueryParams(format);

        var (content, _, _) = await generator.GenerateAsync(query, scope, CancellationToken.None);

        content.Should().NotBeEmpty();
    }
}
