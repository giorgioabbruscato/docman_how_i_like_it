using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Reporting.Application;

public static class ReportGenerateScope
{
    private const string GenerateTenant = "report.generate:tenant";
    private const string GenerateTeam = "report.generate:team";
    private const string GenerateSelf = "report.generate:self";

    public static async Task<Result<ReportGenerateFilter>> ResolveAsync(
        TenantContext ctx,
        IEmployeeLookup employeeLookup,
        Guid? requestedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (ctx.HasPermission(GenerateTenant))
            return Result.Success(new ReportGenerateFilter(null, requestedEmployeeId));

        if (ctx.HasPermission(GenerateTeam))
        {
            if (!ctx.DepartmentId.HasValue)
                return Result.Failure<ReportGenerateFilter>("Department context is required.", "FORBIDDEN");

            var departmentEmployeeIds = await employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                ctx.DepartmentId.Value,
                cancellationToken);

            if (requestedEmployeeId.HasValue
                && !departmentEmployeeIds.Contains(requestedEmployeeId.Value))
            {
                return Result.Failure<ReportGenerateFilter>("Employee not found.", "NOT_FOUND");
            }

            return Result.Success(new ReportGenerateFilter(departmentEmployeeIds, requestedEmployeeId));
        }

        if (ctx.HasPermission(GenerateSelf))
        {
            if (!ctx.EmployeeId.HasValue)
                return Result.Failure<ReportGenerateFilter>("Employee context is required.", "FORBIDDEN");

            return Result.Success(new ReportGenerateFilter(null, ctx.EmployeeId));
        }

        return Result.Failure<ReportGenerateFilter>("Report generation access is required.", "FORBIDDEN");
    }
}
