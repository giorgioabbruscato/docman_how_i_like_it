using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Reporting.Application;

public sealed class GenerateReportQueryHandler
{
    private readonly ReportGeneratorFactory _factory;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;

    public GenerateReportQueryHandler(
        ReportGeneratorFactory factory,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext)
    {
        _factory = factory;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
    }

    public async Task<Result<(byte[] Content, string ContentType, string FileName)>> HandleAsync(
        string type,
        ReportQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var generatorResult = _factory.Resolve(type);
        if (!generatorResult.IsSuccess)
            return Result.Failure<(byte[], string, string)>(generatorResult.Error!, generatorResult.ErrorCode);

        var effectiveEmployeeId = query.EmployeeId;
        if (_tenantContext.HasPermission("report.generate:self")
            && !_tenantContext.HasPermission("report.generate:team")
            && !_tenantContext.HasPermission("report.generate:tenant"))
        {
            effectiveEmployeeId = _tenantContext.EmployeeId;
        }

        var scopeResult = await ReportGenerateScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            effectiveEmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<(byte[], string, string)>(scopeResult.Error!, scopeResult.ErrorCode);

        try
        {
            var effectiveQuery = effectiveEmployeeId != query.EmployeeId
                ? query with { EmployeeId = effectiveEmployeeId }
                : query;

            var result = await generatorResult.Value!.GenerateAsync(
                effectiveQuery,
                scopeResult.Value!,
                cancellationToken);

            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<(byte[], string, string)>(ex.Message, "BAD_REQUEST");
        }
    }
}
