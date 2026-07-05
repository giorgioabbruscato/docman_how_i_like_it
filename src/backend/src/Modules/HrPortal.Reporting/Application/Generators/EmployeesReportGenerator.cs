using HrPortal.Departments.Application;
using HrPortal.Employees.Application;
using HrPortal.Employees.Domain;
using HrPortal.Reporting.Infrastructure.Export;

namespace HrPortal.Reporting.Application.Generators;

internal sealed class EmployeesReportGenerator : IReportGenerator
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentLookup _departmentLookup;

    public EmployeesReportGenerator(
        IEmployeeRepository employeeRepository,
        IDepartmentLookup departmentLookup)
    {
        _employeeRepository = employeeRepository;
        _departmentLookup = departmentLookup;
    }

    public string ReportType => "employees";

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        employees = FilterEmployees(employees, query, scope).ToList();

        var headers = new[] { "Name", "Email", "Department", "Hire Date", "Status" };
        var rows = new List<IReadOnlyList<object?>>();

        foreach (var employee in employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
        {
            string? departmentName = null;
            if (employee.DepartmentId.HasValue)
                departmentName = await _departmentLookup.GetNameAsync(employee.DepartmentId.Value, cancellationToken);

            rows.Add([
                employee.FullName,
                employee.Email,
                departmentName,
                employee.HireDate,
                employee.IsActive ? "Active" : "Inactive"
            ]);
        }

        return ReportFileWriter.Write("Employees Report", "employees-report", query.Format, headers, rows);
    }

    private static IEnumerable<Employee> FilterEmployees(
        IReadOnlyList<Employee> employees,
        ReportQueryParams query,
        ReportGenerateFilter scope)
    {
        IEnumerable<Employee> filtered = employees;

        if (scope.AllowedEmployeeIds is not null)
            filtered = filtered.Where(e => scope.AllowedEmployeeIds.Contains(e.Id));

        if (scope.EmployeeId.HasValue)
            filtered = filtered.Where(e => e.Id == scope.EmployeeId.Value);

        if (query.DepartmentId.HasValue)
            filtered = filtered.Where(e => e.DepartmentId == query.DepartmentId);

        if (query.EmployeeId.HasValue)
            filtered = filtered.Where(e => e.Id == query.EmployeeId);

        return filtered;
    }
}
