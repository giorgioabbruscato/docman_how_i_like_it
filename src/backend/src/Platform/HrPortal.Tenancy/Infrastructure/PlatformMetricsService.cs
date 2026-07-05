using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Application.Dtos;
using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Tenancy.Infrastructure;

internal sealed class PlatformMetricsService : IPlatformMetricsService
{
    private readonly DbContext _dbContext;

    public PlatformMetricsService(DbContext dbContext) => _dbContext = dbContext;

    private bool IsSqlite =>
        _dbContext.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

    private string CountExpression => IsSqlite ? "COUNT(*)" : "COUNT(*)::int";

    private string TenantsTable => IsSqlite ? "tenants" : "platform.tenants";

    private string EmployeesTable => IsSqlite ? "employees" : "employees.employees";

    private string TimeEntriesTable => IsSqlite ? "time_entries" : "time_tracking.time_entries";

    private string AttendanceSessionsTable => IsSqlite ? "attendance_sessions" : "attendance.attendance_sessions";

    private string LeaveRequestsTable => IsSqlite ? "leave_requests" : "leave.leave_requests";

    private string ProjectsTable => IsSqlite ? "projects" : "projects.projects";

    private string AuditLogsTable => IsSqlite ? "audit_logs" : "platform.audit_logs";

    public async Task<PlatformDashboardSummaryDto> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var since30Days = DateTime.UtcNow.AddDays(-30);

        var totalTenants = await _dbContext.Set<Tenant>()
            .CountAsync(t => t.IsActive, cancellationToken);

        var employeeCounts = await QueryEmployeeCountsByTenantAsync(cancellationToken);
        var totalEmployees = employeeCounts.Sum(x => x.Count);

        var activeEmployeesLast30Days = await CountActiveEmployeesSinceAsync(since30Days, cancellationToken);

        var totalTimeEntriesLast30Days = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + TimeEntriesTable + " WHERE \"CreatedAt\" >= {0}",
            cancellationToken,
            since30Days);

        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        var licenseSeatsUsed = totalEmployees;
        var licenseSeatsTotal = tenants.Sum(t => t.GetEffectiveFeatures().MaxEmployees);

        return new PlatformDashboardSummaryDto(
            totalTenants,
            totalEmployees,
            activeEmployeesLast30Days,
            totalTimeEntriesLast30Days,
            licenseSeatsUsed,
            licenseSeatsTotal);
    }

    public async Task<IReadOnlyList<PlatformTenantMetricsDto>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants = await _dbContext.Set<Tenant>()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var employeeCounts = await QueryEmployeeCountsByTenantAsync(cancellationToken);
        var employeeCountByTenant = employeeCounts.ToDictionary(x => x.TenantId, x => x.Count);

        var lastActivityByTenant = await QueryLastActivityByTenantAsync(cancellationToken);

        return tenants
            .Select(t => new PlatformTenantMetricsDto(
                t.Id,
                t.Slug,
                t.Name,
                employeeCountByTenant.GetValueOrDefault(t.Id),
                t.IsActive,
                t.CreatedAt,
                lastActivityByTenant.GetValueOrDefault(t.Id)))
            .ToList();
    }

    public async Task<PlatformTenantSummaryDto?> GetTenantSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
            return null;

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeFlag = IsSqlite ? "1" : "TRUE";
        var archivedFlag = IsSqlite ? "0" : "FALSE";

        var employeeCount = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + EmployeesTable +
            " WHERE \"TenantId\" = {0} AND \"IsActive\" = " + activeFlag,
            cancellationToken,
            tenantId);

        var activeProjects = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + ProjectsTable +
            " WHERE \"TenantId\" = {0} AND \"Status\" = 'Active' AND \"IsArchived\" = " + archivedFlag,
            cancellationToken,
            tenantId);

        var timeEntriesThisMonth = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + TimeEntriesTable +
            " WHERE \"TenantId\" = {0} AND \"CreatedAt\" >= {1}",
            cancellationToken,
            tenantId,
            monthStart);

        var attendanceSessionsThisMonth = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + AttendanceSessionsTable +
            " WHERE \"TenantId\" = {0} AND \"CreatedAt\" >= {1}",
            cancellationToken,
            tenantId,
            monthStart);

        var leaveRequestsPending = await ScalarCountAsync(
            "SELECT " + CountExpression + " AS \"Value\" FROM " + LeaveRequestsTable +
            " WHERE \"TenantId\" = {0} AND \"Status\" = 'Pending'",
            cancellationToken,
            tenantId);

        return new PlatformTenantSummaryDto(
            tenant.Id,
            tenant.Slug,
            tenant.Name,
            employeeCount,
            activeProjects,
            timeEntriesThisMonth,
            attendanceSessionsThisMonth,
            leaveRequestsPending,
            StorageUsedBytes: null);
    }

    public async Task<PlatformUsageDto> GetUsageAsync(CancellationToken cancellationToken = default)
    {
        var since12Months = DateTime.UtcNow.AddMonths(-12);

        var tenantGrowth = await _dbContext.Database
            .SqlQueryRaw<UsageTrendRow>(BuildMonthlyTrendSql(TenantsTable), since12Months)
            .ToListAsync(cancellationToken);

        var timeEntriesByMonth = await _dbContext.Database
            .SqlQueryRaw<UsageTrendRow>(BuildMonthlyTrendSql(TimeEntriesTable), since12Months)
            .ToListAsync(cancellationToken);

        return new PlatformUsageDto(
            tenantGrowth.Select(MapTrendPoint).ToList(),
            timeEntriesByMonth.Select(MapTrendPoint).ToList());
    }

    private string BuildMonthlyTrendSql(string tableName)
    {
        if (IsSqlite)
        {
            return
                "SELECT strftime('%Y-%m', \"CreatedAt\") AS \"Period\", " +
                CountExpression + " AS \"Count\" " +
                "FROM " + tableName + " " +
                "WHERE \"CreatedAt\" >= {0} " +
                "GROUP BY strftime('%Y-%m', \"CreatedAt\") " +
                "ORDER BY strftime('%Y-%m', \"CreatedAt\")";
        }

        return
            "SELECT TO_CHAR(DATE_TRUNC('month', \"CreatedAt\"), 'YYYY-MM') AS \"Period\", " +
            CountExpression + " AS \"Count\" " +
            "FROM " + tableName + " " +
            "WHERE \"CreatedAt\" >= {0} " +
            "GROUP BY DATE_TRUNC('month', \"CreatedAt\") " +
            "ORDER BY DATE_TRUNC('month', \"CreatedAt\")";
    }

    private async Task<List<TenantCountRow>> QueryEmployeeCountsByTenantAsync(
        CancellationToken cancellationToken)
    {
        var sql =
            "SELECT \"TenantId\", " + CountExpression + " AS \"Count\" " +
            "FROM " + EmployeesTable + " " +
            "WHERE \"IsActive\" = " + (IsSqlite ? "1" : "TRUE") + " " +
            "GROUP BY \"TenantId\"";

        return await _dbContext.Database
            .SqlQueryRaw<TenantCountRow>(sql)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, DateTime?>> QueryLastActivityByTenantAsync(
        CancellationToken cancellationToken)
    {
        var sentinel = IsSqlite ? "'0001-01-01'" : "TIMESTAMPTZ '0001-01-01'";
        var lastActivityExpression = IsSqlite
            ? "MAX(COALESCE(e.last_activity, " + sentinel + "), COALESCE(te.last_activity, " + sentinel + "), COALESCE(a.last_activity, " + sentinel + "), COALESCE(al.last_activity, " + sentinel + "))"
            : "GREATEST(COALESCE(e.last_activity, " + sentinel + "), COALESCE(te.last_activity, " + sentinel + "), COALESCE(a.last_activity, " + sentinel + "), COALESCE(al.last_activity, " + sentinel + "))";

        var sql =
            "SELECT t.\"Id\" AS \"TenantId\", " +
            "NULLIF(" + lastActivityExpression + ", " + sentinel + ") AS \"LastActivityAt\" " +
            "FROM " + TenantsTable + " t " +
            "LEFT JOIN (SELECT \"TenantId\", MAX(COALESCE(\"UpdatedAt\", \"CreatedAt\")) AS last_activity FROM " + EmployeesTable + " GROUP BY \"TenantId\") e ON e.\"TenantId\" = t.\"Id\" " +
            "LEFT JOIN (SELECT \"TenantId\", MAX(\"CreatedAt\") AS last_activity FROM " + TimeEntriesTable + " GROUP BY \"TenantId\") te ON te.\"TenantId\" = t.\"Id\" " +
            "LEFT JOIN (SELECT \"TenantId\", MAX(\"CreatedAt\") AS last_activity FROM " + AttendanceSessionsTable + " GROUP BY \"TenantId\") a ON a.\"TenantId\" = t.\"Id\" " +
            "LEFT JOIN (SELECT \"TenantId\", MAX(\"Timestamp\") AS last_activity FROM " + AuditLogsTable + " GROUP BY \"TenantId\") al ON al.\"TenantId\" = t.\"Id\"";

        var rows = await _dbContext.Database
            .SqlQueryRaw<TenantActivityRow>(sql)
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.TenantId,
            r => r.LastActivityAt);
    }

    private async Task<int> CountActiveEmployeesSinceAsync(
        DateTime since,
        CancellationToken cancellationToken)
    {
        var activeFlag = IsSqlite ? "1" : "TRUE";
        var countCast = IsSqlite ? "" : "::int";
        var sql =
            "SELECT COUNT(DISTINCT e.\"Id\")" + countCast + " AS \"Value\" " +
            "FROM " + EmployeesTable + " e " +
            "WHERE e.\"IsActive\" = " + activeFlag + " " +
            "AND (COALESCE(e.\"UpdatedAt\", e.\"CreatedAt\") >= {0} " +
            "OR EXISTS (SELECT 1 FROM " + TimeEntriesTable + " te WHERE te.\"TenantId\" = e.\"TenantId\" AND te.\"EmployeeId\" = e.\"Id\" AND te.\"CreatedAt\" >= {0}) " +
            "OR EXISTS (SELECT 1 FROM " + AttendanceSessionsTable + " a WHERE a.\"TenantId\" = e.\"TenantId\" AND a.\"EmployeeId\" = e.\"Id\" AND a.\"CreatedAt\" >= {0}) " +
            "OR EXISTS (SELECT 1 FROM " + AuditLogsTable + " al WHERE al.\"TenantId\" = e.\"TenantId\" AND al.\"Timestamp\" >= {0}))";

        return await ScalarCountAsync(sql, cancellationToken, since);
    }

    private async Task<int> ScalarCountAsync(
        string sql,
        CancellationToken cancellationToken,
        params object[] parameters)
    {
        var rows = await _dbContext.Database
            .SqlQueryRaw<ScalarRow>(sql, parameters)
            .ToListAsync(cancellationToken);

        return rows.FirstOrDefault()?.Value ?? 0;
    }

    private static PlatformUsageTrendPointDto MapTrendPoint(UsageTrendRow row) =>
        new(row.Period, row.Count);

    private sealed record TenantCountRow(Guid TenantId, int Count);

    private sealed record TenantActivityRow(Guid TenantId, DateTime? LastActivityAt);

    private sealed record UsageTrendRow(string Period, int Count);

    private sealed record ScalarRow(int Value);
}
