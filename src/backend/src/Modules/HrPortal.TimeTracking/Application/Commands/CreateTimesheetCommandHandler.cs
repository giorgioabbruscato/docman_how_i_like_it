using HrPortal.Audit.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.TimeTracking.Application.Commands;

public sealed class CreateTimesheetCommandHandler
{
    private readonly ITimesheetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateTimesheetCommandHandler> _logger;

    public CreateTimesheetCommandHandler(
        ITimesheetRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CreateTimesheetCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<TimesheetSubmissionDto>> HandleAsync(
        CreateTimesheetRequest request,
        CancellationToken cancellationToken = default)
    {
        var contextResult = TimesheetReadScope.EnsureEmployeeContext(_tenantContext);
        if (!contextResult.IsSuccess)
            return Result.Failure<TimesheetSubmissionDto>(contextResult.Error!, contextResult.ErrorCode);

        var employeeId = _tenantContext.EmployeeId!.Value;

        if (await _repository.HasOverlappingPeriodAsync(
                employeeId, request.PeriodStart, request.PeriodEnd, cancellationToken: cancellationToken))
        {
            return Result.Failure<TimesheetSubmissionDto>(
                "A timesheet already exists for this period.", "CONFLICT");
        }

        var entries = await _repository.GetCompletedEntriesForPeriodAsync(
            employeeId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        var totalMinutes = entries.Sum(e => e.WorkedMinutes);

        try
        {
            var submission = TimesheetSubmission.Create(
                _tenantContext.TenantId,
                employeeId,
                request.PeriodStart,
                request.PeriodEnd,
                totalMinutes,
                entries.Select(e => e.Id).ToList(),
                request.Notes,
                _tenantContext.UserId);

            await _repository.AddAsync(submission, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "timesheet.created",
                nameof(TimesheetSubmission),
                submission.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Timesheet {TimesheetId} created for employee {EmployeeId}", submission.Id, employeeId);
            return Result.Success(TimesheetMapping.ToDto(submission));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimesheetSubmissionDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
