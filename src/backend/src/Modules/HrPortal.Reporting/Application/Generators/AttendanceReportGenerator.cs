using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Employees.Application;
using HrPortal.Reporting.Infrastructure.Export;

namespace HrPortal.Reporting.Application.Generators;

internal sealed class AttendanceReportGenerator : IReportGenerator
{
    private readonly IAttendanceSessionRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;

    public AttendanceReportGenerator(
        IAttendanceSessionRepository repository,
        IEmployeeLookup employeeLookup)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
    }

    public string ReportType => "attendance";

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default)
    {
        var from = query.FromDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow.Date.AddMonths(-1);
        var to = query.ToDate?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.UtcNow.Date.AddDays(1);

        var filter = new AttendanceSessionReadFilter(scope.AllowedEmployeeIds, scope.EmployeeId);
        var (sessions, _) = await _repository.GetHistoryAsync(filter, from, to, 1, 10_000, cancellationToken);

        if (query.DepartmentId.HasValue)
        {
            var departmentIds = await _employeeLookup.GetDepartmentIdsAsync(
                sessions.Select(s => s.EmployeeId).Distinct().ToList(),
                cancellationToken);

            sessions = sessions
                .Where(s => departmentIds.TryGetValue(s.EmployeeId, out var deptId)
                    && deptId == query.DepartmentId)
                .ToList();
        }

        if (query.ProjectId.HasValue)
            sessions = [];

        var employeeNames = new Dictionary<Guid, string>();
        foreach (var employeeId in sessions.Select(s => s.EmployeeId).Distinct())
        {
            employeeNames[employeeId] = await _employeeLookup.GetFullNameAsync(employeeId, cancellationToken)
                ?? employeeId.ToString();
        }

        var headers = new[] { "Employee", "Date", "Check-in", "Check-out", "Hours", "Status" };
        var rows = new List<IReadOnlyList<object?>>();

        foreach (var session in sessions.OrderBy(s => s.CheckIn))
        {
            var hours = session.WorkedMinutes.HasValue
                ? Math.Round(session.WorkedMinutes.Value / 60m, 2)
                : session.CheckOut.HasValue
                    ? Math.Round(AttendanceSession.CalculateWorkedMinutes(session.CheckIn, session.CheckOut.Value) / 60m, 2)
                    : 0m;

            rows.Add([
                employeeNames[session.EmployeeId],
                DateOnly.FromDateTime(session.CheckIn),
                session.CheckIn,
                session.CheckOut,
                hours,
                session.Status.ToString()
            ]);
        }

        return ReportFileWriter.Write("Attendance Report", "attendance-report", query.Format, headers, rows);
    }
}
