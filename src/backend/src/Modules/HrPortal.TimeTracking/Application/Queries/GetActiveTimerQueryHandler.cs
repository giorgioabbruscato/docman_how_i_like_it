using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application.Queries;

public sealed class GetActiveTimerQueryHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly TenantContext _tenantContext;

    public GetActiveTimerQueryHandler(
        ITimeEntryRepository repository,
        TenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TimeEntryDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var contextResult = TimeEntryReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<TimeEntryDto>(contextResult.Error!, contextResult.ErrorCode);

        var entry = await _repository.GetActiveTimerAsync(_tenantContext.EmployeeId!.Value, cancellationToken);
        if (entry is null)
            return Result.Failure<TimeEntryDto>("No active timer found.", "NOT_FOUND");

        return Result.Success(TimeEntryMapping.ToDto(entry));
    }
}
