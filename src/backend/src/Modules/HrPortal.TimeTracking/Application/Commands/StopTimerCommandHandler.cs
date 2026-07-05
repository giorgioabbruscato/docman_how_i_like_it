using HrPortal.Audit.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.TimeTracking.Application.Commands;

public sealed class StopTimerCommandHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<StopTimerCommandHandler> _logger;

    public StopTimerCommandHandler(
        ITimeEntryRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<StopTimerCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<TimeEntryDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var contextResult = TimeEntryReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<TimeEntryDto>(contextResult.Error!, contextResult.ErrorCode);

        var employeeId = _tenantContext.EmployeeId!.Value;
        var entry = await _repository.GetActiveTimerAsync(employeeId, cancellationToken);
        if (entry is null)
            return Result.Failure<TimeEntryDto>("No active timer found.", "NOT_FOUND");

        entry.Stop(DateTime.UtcNow);

        await _repository.UpdateAsync(entry, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "time_entry.timer_stopped",
            nameof(TimeEntry),
            entry.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Timer stopped for employee {EmployeeId}", employeeId);
        return Result.Success(TimeEntryMapping.ToDto(entry));
    }
}
