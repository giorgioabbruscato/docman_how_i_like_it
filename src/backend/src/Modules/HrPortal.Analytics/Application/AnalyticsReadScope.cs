using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Analytics.Application;

public sealed record AnalyticsReadFilter(
    IReadOnlyList<Guid>? AllowedEmployeeIds,
    Guid? EmployeeId);

public static class AnalyticsReadScope
{
    private const string ReadTenant = "analytics.read:tenant";
    private const string ReadTeam = "analytics.read:team";

    public static async Task<Result<AnalyticsReadFilter>> ResolveAsync(
        TenantContext ctx,
        IEmployeeLookup employeeLookup,
        Guid? requestedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (ctx.HasPermission(ReadTenant))
            return Result.Success(new AnalyticsReadFilter(null, requestedEmployeeId));

        if (ctx.HasPermission(ReadTeam))
        {
            if (!ctx.DepartmentId.HasValue)
                return Result.Failure<AnalyticsReadFilter>("Department context is required.", "FORBIDDEN");

            var departmentEmployeeIds = await employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                ctx.DepartmentId.Value,
                cancellationToken);

            if (requestedEmployeeId.HasValue
                && !departmentEmployeeIds.Contains(requestedEmployeeId.Value))
            {
                return Result.Failure<AnalyticsReadFilter>("Employee not found.", "NOT_FOUND");
            }

            return Result.Success(new AnalyticsReadFilter(departmentEmployeeIds, requestedEmployeeId));
        }

        return Result.Failure<AnalyticsReadFilter>("Analytics access is required.", "FORBIDDEN");
    }
}
