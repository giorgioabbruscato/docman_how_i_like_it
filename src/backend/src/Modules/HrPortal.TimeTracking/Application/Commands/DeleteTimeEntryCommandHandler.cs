using HrPortal.Audit.Application;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.TimeTracking.Application.Commands;

public sealed class DeleteTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<DeleteTimeEntryCommandHandler> _logger;

    public DeleteTimeEntryCommandHandler(
        ITimeEntryRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<DeleteTimeEntryCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
            return Result.Failure("Time entry not found.", "NOT_FOUND");

        var ownershipResult = TimeEntryReadScope.EnsureOwnEntry(_tenantContext, entry.EmployeeId);
        if (!ownershipResult.IsSuccess)
            return Result.Failure(ownershipResult.Error!, ownershipResult.ErrorCode);

        await _repository.DeleteAsync(entry, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "time_entry.deleted",
            nameof(TimeEntry),
            entry.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Time entry {TimeEntryId} deleted", entry.Id);
        return Result.Success();
    }
}
