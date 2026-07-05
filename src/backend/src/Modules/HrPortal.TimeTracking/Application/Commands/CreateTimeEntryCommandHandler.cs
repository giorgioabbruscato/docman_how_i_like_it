using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
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

public sealed class CreateTimeEntryCommandHandler
{
    private readonly ITimeEntryRepository _repository;
    private readonly IProjectLookup _projectLookup;
    private readonly ITaskLookup _taskLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateTimeEntryCommandHandler> _logger;

    public CreateTimeEntryCommandHandler(
        ITimeEntryRepository repository,
        IProjectLookup projectLookup,
        ITaskLookup taskLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CreateTimeEntryCommandHandler> logger)
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
        CreateTimeEntryRequest request,
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

        if (await _repository.HasOverlappingEntryAsync(
                employeeId, request.StartTime, request.EndTime, cancellationToken: cancellationToken))
        {
            return Result.Failure<TimeEntryDto>("Time entry overlaps an existing entry.", "CONFLICT");
        }

        try
        {
            var entry = TimeEntry.Create(
                _tenantContext.TenantId,
                employeeId,
                request.ProjectId,
                request.StartTime,
                request.EndTime,
                request.TaskId,
                request.Description,
                request.Billable,
                _tenantContext.UserId);

            await _repository.AddAsync(entry, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "time_entry.created",
                nameof(TimeEntry),
                entry.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Time entry {TimeEntryId} created", entry.Id);
            return Result.Success(TimeEntryMapping.ToDto(entry));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimeEntryDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
