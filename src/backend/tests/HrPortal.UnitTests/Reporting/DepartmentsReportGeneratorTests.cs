using HrPortal.Departments.Application;
using HrPortal.Departments.Domain;
using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using Moq;

namespace HrPortal.UnitTests.Reporting;

public sealed class DepartmentsReportGeneratorTests
{
    [Theory]
    [InlineData("csv")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public async Task GenerateAsync_ProducesNonEmptyBytes_ForEachFormat(string format)
    {
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");
        var employee = Employee.Create(
            Guid.NewGuid(),
            "Jane",
            "Doe",
            "jane@demo.local",
            new DateOnly(2024, 1, 1),
            departmentId: department.Id);

        var departmentRepository = new Mock<IDepartmentRepository>();
        departmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Department> { department });

        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { employee });

        var generator = new DepartmentsReportGenerator(departmentRepository.Object, employeeRepository.Object);
        var scope = new ReportGenerateFilter(null, null);
        var query = new ReportQueryParams(format);

        var (content, _, _) = await generator.GenerateAsync(query, scope, CancellationToken.None);

        content.Should().NotBeEmpty();
    }
}
