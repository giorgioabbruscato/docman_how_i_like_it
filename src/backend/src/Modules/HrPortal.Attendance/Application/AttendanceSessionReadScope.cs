using HrPortal.Attendance.Application.Dtos;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Attendance.Application;

public static class AttendanceSessionReadScope
{
    private const string ReadTenant = "attendance_session.read:tenant";
    private const string ReadTeam = "attendance_session.read:team";

    public static async Task<Result<AttendanceSessionReadFilter>> ResolveAsync(
        TenantContext ctx,
        IEmployeeLookup employeeLookup,
        Guid? requestedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (ctx.HasPermission(ReadTenant))
        {
            return Result.Success(new AttendanceSessionReadFilter(null, requestedEmployeeId));
        }

        if (ctx.HasPermission(ReadTeam))
        {
            if (!ctx.DepartmentId.HasValue)
                return Result.Failure<AttendanceSessionReadFilter>("Department context is required.", "FORBIDDEN");

            var departmentEmployeeIds = await employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                ctx.DepartmentId.Value,
                cancellationToken);

            if (requestedEmployeeId.HasValue
                && !departmentEmployeeIds.Contains(requestedEmployeeId.Value))
            {
                return Result.Failure<AttendanceSessionReadFilter>("Employee not found.", "NOT_FOUND");
            }

            return Result.Success(new AttendanceSessionReadFilter(departmentEmployeeIds, requestedEmployeeId));
        }

        if (!ctx.EmployeeId.HasValue)
            return Result.Failure<AttendanceSessionReadFilter>("Employee context is required.", "FORBIDDEN");

        if (requestedEmployeeId.HasValue && requestedEmployeeId.Value != ctx.EmployeeId.Value)
            return Result.Failure<AttendanceSessionReadFilter>("Employee not found.", "NOT_FOUND");

        return Result.Success(new AttendanceSessionReadFilter(null, ctx.EmployeeId));
    }

    public static Result EnsureEmployeeContext(TenantContext ctx)
    {
        if (!ctx.EmployeeId.HasValue)
            return Result.Failure("Employee context is required.", "FORBIDDEN");

        return Result.Success();
    }
}
