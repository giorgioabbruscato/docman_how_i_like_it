using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application;

public sealed record TimesheetReadFilter(
    IReadOnlyList<Guid>? AllowedEmployeeIds,
    Guid? EmployeeId);

public static class TimesheetReadScope
{
    private const string ReadTeam = "timesheet.read:team";

    public static async Task<Result<TimesheetReadFilter>> ResolveAsync(
        TenantContext ctx,
        IEmployeeLookup employeeLookup,
        Guid? requestedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (ctx.HasPermission(ReadTeam))
        {
            if (!ctx.DepartmentId.HasValue)
                return Result.Success(new TimesheetReadFilter(null, requestedEmployeeId));

            var departmentEmployeeIds = await employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                ctx.DepartmentId.Value,
                cancellationToken);

            if (requestedEmployeeId.HasValue
                && !departmentEmployeeIds.Contains(requestedEmployeeId.Value))
            {
                return Result.Failure<TimesheetReadFilter>("Employee not found.", "NOT_FOUND");
            }

            return Result.Success(new TimesheetReadFilter(departmentEmployeeIds, requestedEmployeeId));
        }

        if (!ctx.EmployeeId.HasValue)
            return Result.Failure<TimesheetReadFilter>("Employee context is required.", "FORBIDDEN");

        return Result.Success(new TimesheetReadFilter(null, ctx.EmployeeId));
    }

    public static Result EnsureEmployeeContext(TenantContext ctx)
    {
        if (!ctx.EmployeeId.HasValue)
            return Result.Failure("Employee context is required.", "FORBIDDEN");

        return Result.Success();
    }

    public static Result EnsureOwnSubmission(TenantContext ctx, Guid submissionEmployeeId)
    {
        if (!ctx.EmployeeId.HasValue)
            return Result.Failure("Employee context is required.", "FORBIDDEN");

        if (ctx.EmployeeId.Value != submissionEmployeeId)
            return Result.Failure("Timesheet not found.", "NOT_FOUND");

        return Result.Success();
    }
}
