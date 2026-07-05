using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application.Queries;

public sealed class GetTimeEntryByIdQueryHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly TenantContext _tenantContext;

    public GetTimeEntryByIdQueryHandler(
        ITimeEntryRepository repository,
        TenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TimeEntryDto>> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
            return Result.Failure<TimeEntryDto>("Time entry not found.", "NOT_FOUND");

        if (!_tenantContext.HasPermission("time_entry.read:tenant")
            && !_tenantContext.HasPermission("time_entry.read:team"))
        {
            var ownershipResult = TimeEntryReadScope.EnsureOwnEntry(_tenantContext, entry.EmployeeId);
            if (!ownershipResult.IsSuccess)
                return Result.Failure<TimeEntryDto>(ownershipResult.Error!, ownershipResult.ErrorCode);
        }

        return Result.Success(TimeEntryMapping.ToDto(entry));
    }
}
