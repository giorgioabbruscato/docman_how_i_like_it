using HrPortal.Departments.Application;
using HrPortal.Departments.Domain;
using HrPortal.Employees.Application;
using HrPortal.Reporting.Infrastructure.Export;

namespace HrPortal.Reporting.Application.Generators;

internal sealed class DepartmentsReportGenerator : IReportGenerator
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public DepartmentsReportGenerator(
        IDepartmentRepository departmentRepository,
        IEmployeeRepository employeeRepository)
    {
        _departmentRepository = departmentRepository;
        _employeeRepository = employeeRepository;
    }

    public string ReportType => "departments";

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        if (scope.AllowedEmployeeIds is not null)
        {
            var allowedDepartments = employees
                .Where(e => scope.AllowedEmployeeIds.Contains(e.Id) && e.DepartmentId.HasValue)
                .Select(e => e.DepartmentId!.Value)
                .ToHashSet();

            departments = departments.Where(d => allowedDepartments.Contains(d.Id)).ToList();
        }

        if (query.DepartmentId.HasValue)
            departments = departments.Where(d => d.Id == query.DepartmentId).ToList();

        var headcounts = employees
            .Where(e => e.IsActive && e.DepartmentId.HasValue)
            .GroupBy(e => e.DepartmentId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var departmentNames = departments.ToDictionary(d => d.Id, d => d.Name);
        var headers = new[] { "Name", "Code", "Employee Count", "Parent" };
        var rows = new List<IReadOnlyList<object?>>();

        foreach (var department in departments.OrderBy(d => d.Name))
        {
            string? parentName = null;
            if (department.ParentDepartmentId.HasValue
                && departmentNames.TryGetValue(department.ParentDepartmentId.Value, out var name))
            {
                parentName = name;
            }

            headcounts.TryGetValue(department.Id, out var count);

            rows.Add([
                department.Name,
                department.Code,
                count,
                parentName
            ]);
        }

        return ReportFileWriter.Write("Departments Report", "departments-report", query.Format, headers, rows);
    }
}
