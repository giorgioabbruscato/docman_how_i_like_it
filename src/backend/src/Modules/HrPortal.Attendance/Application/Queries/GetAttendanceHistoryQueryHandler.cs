using HrPortal.Attendance.Application.Dtos;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Attendance.Application.Queries;

public sealed class GetAttendanceHistoryQueryHandler
{
    private readonly IAttendanceSessionRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;

    public GetAttendanceHistoryQueryHandler(
        IAttendanceSessionRepository repository,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<AttendanceSessionDto>>> HandleAsync(
        GetAttendanceHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await AttendanceSessionReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            query.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<PagedResult<AttendanceSessionDto>>(scopeResult.Error!, scopeResult.ErrorCode);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var (items, total) = await _repository.GetHistoryAsync(
            scopeResult.Value!,
            query.FromDate,
            query.ToDate,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(AttendanceSessionMapping.ToDto).ToList();
        return Result.Success(new PagedResult<AttendanceSessionDto>(dtos, total, page, pageSize));
    }
}
