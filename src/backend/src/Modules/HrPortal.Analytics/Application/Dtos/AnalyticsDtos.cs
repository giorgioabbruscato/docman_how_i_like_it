namespace HrPortal.Analytics.Application.Dtos;

public sealed record AnalyticsQueryParams(
    Guid? DepartmentId,
    Guid? ProjectId,
    Guid? EmployeeId,
    DateOnly? FromDate,
    DateOnly? ToDate);

public sealed record AnalyticsFilter(
    DateOnly From,
    DateOnly To,
    Guid? DepartmentId,
    Guid? ProjectId,
    Guid? EmployeeId,
    IReadOnlyList<Guid>? AllowedEmployeeIds);

public sealed record NamedHoursRow(string Label, Guid? Id, decimal Hours);

public sealed record DateHoursRow(DateOnly Date, decimal Hours);

public sealed record MonthHoursRow(int Year, int Month, decimal Hours);

public sealed record LateCheckInRow(Guid EmployeeId, string EmployeeName, DateTime CheckInTime);

public sealed record EmployeeWorkingDto(
    Guid EmployeeId,
    string EmployeeName,
    Guid? ProjectId,
    string? ProjectName,
    DateTime? CheckInTime);

public sealed record AttendanceTodayDto(Guid EmployeeId, string EmployeeName, DateTime CheckInTime);

public sealed record TopEmployeeDto(Guid EmployeeId, string EmployeeName, decimal Hours);

public sealed record TopProjectDto(Guid ProjectId, string ProjectName, decimal Hours);

public sealed record BudgetUsageDto(
    Guid ProjectId,
    string ProjectName,
    decimal? BudgetHours,
    decimal SpentHours,
    decimal? BudgetCost,
    decimal? ActualCost);

public sealed record LateArrivalDto(Guid EmployeeId, string EmployeeName, DateTime CheckInTime);

public sealed record OvertimeEmployeeDto(Guid EmployeeId, string EmployeeName, decimal OvertimeHours);

public sealed record SupervisorSummaryDto(
    IReadOnlyList<EmployeeWorkingDto> EmployeesWorking,
    IReadOnlyList<AttendanceTodayDto> AttendanceToday,
    IReadOnlyList<TopEmployeeDto> TopEmployees,
    IReadOnlyList<TopProjectDto> TopProjects,
    IReadOnlyList<BudgetUsageDto> BudgetUsage,
    IReadOnlyList<LateArrivalDto> LateArrivals,
    IReadOnlyList<OvertimeEmployeeDto> Overtime,
    decimal TotalWorkedHours,
    decimal AttendanceRate,
    decimal LeaveRate);
