using HrPortal.Employees.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application.Queries;

public sealed class GetTimesheetsQueryHandler
{
    private readonly ITimesheetRepository _repository;
    private readonly TenantContext _tenantContext;
    private readonly IEmployeeLookup _employeeLookup;

    public GetTimesheetsQueryHandler(
        ITimesheetRepository repository,
        TenantContext tenantContext,
        IEmployeeLookup employeeLookup)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _employeeLookup = employeeLookup;
    }

    public async Task<Result<PagedResult<TimesheetSubmissionDto>>> HandleAsync(
        GetTimesheetsQuery query,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await TimesheetReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            query.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<PagedResult<TimesheetSubmissionDto>>(scopeResult.Error!, scopeResult.ErrorCode);

        var filter = scopeResult.Value!;
        var effectiveQuery = filter.EmployeeId.HasValue
            ? query with { EmployeeId = filter.EmployeeId }
            : query;

        var page = await _repository.GetPagedAsync(effectiveQuery, filter.AllowedEmployeeIds, cancellationToken);

        var dtos = new List<TimesheetSubmissionDto>();
        foreach (var submission in page.Items)
        {
            var approval = await _repository.GetLatestApprovalAsync(submission.Id, cancellationToken);
            dtos.Add(TimesheetMapping.ToDto(submission, approval));
        }

        return Result.Success(new PagedResult<TimesheetSubmissionDto>(
            dtos, page.TotalCount, page.Page, page.PageSize));
    }
}

public sealed class GetTimesheetByIdQueryHandler
{
    private readonly ITimesheetRepository _repository;
    private readonly TenantContext _tenantContext;
    private readonly IEmployeeLookup _employeeLookup;

    public GetTimesheetByIdQueryHandler(
        ITimesheetRepository repository,
        TenantContext tenantContext,
        IEmployeeLookup employeeLookup)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _employeeLookup = employeeLookup;
    }

    public async Task<Result<TimesheetSubmissionDto>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdWithEntriesAsync(id, cancellationToken);
        if (submission is null)
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");

        var scopeResult = await TimesheetReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            submission.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<TimesheetSubmissionDto>(scopeResult.Error!, scopeResult.ErrorCode);

        var filter = scopeResult.Value!;
        if (filter.AllowedEmployeeIds is not null
            && !filter.AllowedEmployeeIds.Contains(submission.EmployeeId))
        {
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");
        }

        if (filter.EmployeeId.HasValue && filter.EmployeeId.Value != submission.EmployeeId)
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");

        var approval = await _repository.GetLatestApprovalAsync(id, cancellationToken);
        return Result.Success(TimesheetMapping.ToDto(submission, approval));
    }
}
