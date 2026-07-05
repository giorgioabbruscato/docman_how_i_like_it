using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Notifications;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.TimeTracking.Application;

internal sealed class TimesheetApprovalService : ITimesheetApprovalService
{
    private readonly ITimesheetRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<TimesheetApprovalService> _logger;

    public TimesheetApprovalService(
        ITimesheetRepository repository,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver,
        ILogger<TimesheetApprovalService> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task<Result<TimesheetSubmissionDto>> ApproveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdWithEntriesAsync(id, cancellationToken);
        if (submission is null)
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");

        try
        {
            submission.Approve();

            var approval = TimesheetApproval.Create(
                _tenantContext.TenantId,
                submission.Id,
                _tenantContext.UserId ?? Guid.Empty,
                ApprovalDecision.Approved);

            await _repository.AddApprovalAsync(approval, cancellationToken);
            await _repository.UpdateAsync(submission, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "timesheet.approved",
                nameof(TimesheetSubmission),
                submission.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifyEmployeeAsync(submission, approved: true, cancellationToken);

            _logger.LogInformation("Timesheet {TimesheetId} approved", submission.Id);
            return Result.Success(TimesheetMapping.ToDto(submission, approval));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimesheetSubmissionDto>(ex.Message, ex.ErrorCode ?? "CONFLICT");
        }
    }

    public async Task<Result<TimesheetSubmissionDto>> RejectAsync(
        Guid id,
        RejectTimesheetRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdWithEntriesAsync(id, cancellationToken);
        if (submission is null)
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");

        try
        {
            submission.Reject();

            var approval = TimesheetApproval.Create(
                _tenantContext.TenantId,
                submission.Id,
                _tenantContext.UserId ?? Guid.Empty,
                ApprovalDecision.Rejected,
                request.Comment);

            await _repository.AddApprovalAsync(approval, cancellationToken);
            await _repository.UpdateAsync(submission, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "timesheet.rejected",
                nameof(TimesheetSubmission),
                submission.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifyEmployeeAsync(submission, approved: false, cancellationToken);

            _logger.LogInformation("Timesheet {TimesheetId} rejected", submission.Id);
            return Result.Success(TimesheetMapping.ToDto(submission, approval));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimesheetSubmissionDto>(ex.Message, ex.ErrorCode ?? "CONFLICT");
        }
    }

    private async Task NotifyEmployeeAsync(
        TimesheetSubmission submission,
        bool approved,
        CancellationToken cancellationToken)
    {
        var email = await _employeeLookup.GetEmailAsync(submission.EmployeeId, cancellationToken)
            ?? submission.EmployeeId.ToString();
        var recipient = await _recipientResolver.ResolveForEmployeeAsync(
            submission.EmployeeId, email, cancellationToken);

        if (!recipient.UserId.HasValue)
            return;

        await NotificationHelper.TryNotifyAsync(
            _logger,
            async ct =>
            {
                if (approved)
                {
                    await _notificationService.NotifyTimesheetApprovedAsync(
                        recipient.UserId.Value,
                        submission.PeriodStart,
                        submission.PeriodEnd,
                        ct);
                }
                else
                {
                    await _notificationService.NotifyTimesheetRejectedAsync(
                        recipient.UserId.Value,
                        submission.PeriodStart,
                        submission.PeriodEnd,
                        ct);
                }
            },
            cancellationToken);
    }
}
