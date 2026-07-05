using HrPortal.Audit.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.TimeTracking.Application.Commands;

public sealed class CreateManualTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly IProjectLookup _projectLookup;
    private readonly ITaskLookup _taskLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateManualTimeEntryCommandHandler> _logger;

    public CreateManualTimeEntryCommandHandler(
        ITimeEntryRepository repository,
        IProjectLookup projectLookup,
        ITaskLookup taskLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CreateManualTimeEntryCommandHandler> logger)
    {
        _repository = repository;
        _projectLookup = projectLookup;
        _taskLookup = taskLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<TimeEntryDto>> HandleAsync(
        CreateManualTimeEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var contextResult = TimeEntryReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<TimeEntryDto>(contextResult.Error!, contextResult.ErrorCode);

        var employeeId = _tenantContext.EmployeeId!.Value;

        if (!await _projectLookup.ExistsAsync(request.ProjectId, cancellationToken))
            return Result.Failure<TimeEntryDto>("Project not found.", "NOT_FOUND");

        if (request.TaskId.HasValue
            && !await _taskLookup.ExistsAsync(request.TaskId.Value, cancellationToken))
        {
            return Result.Failure<TimeEntryDto>("Task not found.", "NOT_FOUND");
        }

        var workedMinutes = (int)Math.Round(request.Hours * 60);
        var startTime = request.Date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        var endTime = startTime.AddMinutes(workedMinutes);

        if (request.Date > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<TimeEntryDto>("Date cannot be in the future.", "VALIDATION_ERROR");

        var existingMinutes = await _repository.GetTotalMinutesForDateAsync(
            employeeId, request.Date, cancellationToken: cancellationToken);

        if (existingMinutes + workedMinutes > 1440)
            return Result.Failure<TimeEntryDto>("Daily time limit of 24 hours exceeded.", "VALIDATION_ERROR");

        if (await _repository.HasOverlappingEntryAsync(
                employeeId, startTime, endTime, cancellationToken: cancellationToken))
        {
            return Result.Failure<TimeEntryDto>("Time entry overlaps an existing entry.", "CONFLICT");
        }

        try
        {
            var entry = TimeEntry.Create(
                _tenantContext.TenantId,
                employeeId,
                request.ProjectId,
                startTime,
                endTime,
                request.TaskId,
                request.Description,
                request.Billable,
                _tenantContext.UserId);

            await _repository.AddAsync(entry, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "time_entry.manual_created",
                nameof(TimeEntry),
                entry.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manual time entry {TimeEntryId} created", entry.Id);
            return Result.Success(TimeEntryMapping.ToDto(entry));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
