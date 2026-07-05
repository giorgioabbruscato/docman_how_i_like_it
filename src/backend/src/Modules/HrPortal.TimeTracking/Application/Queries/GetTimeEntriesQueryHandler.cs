using HrPortal.Employees.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application.Queries;

public sealed class GetTimeEntriesQueryHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;

    public GetTimeEntriesQueryHandler(
        ITimeEntryRepository repository,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<TimeEntryDto>>> HandleAsync(
        GetTimeEntriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await TimeEntryReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            query.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<PagedResult<TimeEntryDto>>(scopeResult.Error!, scopeResult.ErrorCode);

        var filter = scopeResult.Value!;
        var effectiveQuery = filter.EmployeeId.HasValue
            ? query with { EmployeeId = filter.EmployeeId }
            : query;

        var page = await _repository.GetPagedAsync(
            effectiveQuery,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var items = page.Items.Select(TimeEntryMapping.ToDto).ToList();
        return Result.Success(new PagedResult<TimeEntryDto>(items, page.TotalCount, page.Page, page.PageSize));
    }
}
