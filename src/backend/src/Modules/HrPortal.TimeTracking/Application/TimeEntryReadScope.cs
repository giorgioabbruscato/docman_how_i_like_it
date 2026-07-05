using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application;

public sealed record TimeEntryReadFilter(
    IReadOnlyList<Guid>? AllowedEmployeeIds,
    Guid? EmployeeId);

public static class TimeEntryReadScope
{
    private const string ReadTenant = "time_entry.read:tenant";
    private const string ReadTeam = "time_entry.read:team";

    public static async Task<Result<TimeEntryReadFilter>> ResolveAsync(
        TenantContext ctx,
        IEmployeeLookup employeeLookup,
        Guid? requestedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (ctx.HasPermission(ReadTenant))
        {
            return Result.Success(new TimeEntryReadFilter(null, requestedEmployeeId));
        }

        if (ctx.HasPermission(ReadTeam))
        {
            if (!ctx.DepartmentId.HasValue)
                return Result.Failure<TimeEntryReadFilter>("Department context is required.", "FORBIDDEN");

            var departmentEmployeeIds = await employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                ctx.DepartmentId.Value,
                cancellationToken);

            if (requestedEmployeeId.HasValue
                && !departmentEmployeeIds.Contains(requestedEmployeeId.Value))
            {
                return Result.Failure<TimeEntryReadFilter>("Employee not found.", "NOT_FOUND");
            }

            return Result.Success(new TimeEntryReadFilter(departmentEmployeeIds, requestedEmployeeId));
        }

        if (!ctx.EmployeeId.HasValue)
            return Result.Failure<TimeEntryReadFilter>("Employee context is required.", "FORBIDDEN");

        return Result.Success(new TimeEntryReadFilter(null, ctx.EmployeeId));
    }

    public static Result EnsureEmployeeContext(TenantContext ctx)
    {
        if (!ctx.EmployeeId.HasValue)
            return Result.Failure("Employee context is required.", "FORBIDDEN");

        return Result.Success();
    }

    public static Result EnsureOwnEntry(TenantContext ctx, Guid entryEmployeeId)
    {
        if (!ctx.EmployeeId.HasValue)
            return Result.Failure("Employee context is required.", "FORBIDDEN");

        if (ctx.EmployeeId.Value != entryEmployeeId)
            return Result.Failure("Time entry not found.", "NOT_FOUND");

        return Result.Success();
    }
}
