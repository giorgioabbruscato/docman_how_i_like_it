using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
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

namespace HrPortal.TimeTracking.Application.Commands;

public sealed class SubmitTimesheetCommandHandler
{
    private readonly ITimesheetRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly ITenantMembershipRepository _membershipRepository;
    private readonly ITenantRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SubmitTimesheetCommandHandler> _logger;

    public SubmitTimesheetCommandHandler(
        ITimesheetRepository repository,
        IEmployeeLookup employeeLookup,
        ITenantMembershipRepository membershipRepository,
        ITenantRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        INotificationService notificationService,
        ILogger<SubmitTimesheetCommandHandler> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _membershipRepository = membershipRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<TimesheetSubmissionDto>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdWithEntriesAsync(id, cancellationToken);
        if (submission is null)
            return Result.Failure<TimesheetSubmissionDto>("Timesheet not found.", "NOT_FOUND");

        var ownResult = TimesheetReadScope.EnsureOwnSubmission(_tenantContext, submission.EmployeeId);
        if (!ownResult.IsSuccess)
            return Result.Failure<TimesheetSubmissionDto>(ownResult.Error!, ownResult.ErrorCode);

        try
        {
            submission.Submit();
            await _repository.UpdateAsync(submission, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "timesheet.submitted",
                nameof(TimesheetSubmission),
                submission.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifySupervisorsAsync(submission, cancellationToken);

            _logger.LogInformation("Timesheet {TimesheetId} submitted", submission.Id);
            return Result.Success(TimesheetMapping.ToDto(submission));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TimesheetSubmissionDto>(ex.Message, ex.ErrorCode ?? "CONFLICT");
        }
    }

    private async Task NotifySupervisorsAsync(TimesheetSubmission submission, CancellationToken cancellationToken)
    {
        var departmentMap = await _employeeLookup.GetDepartmentIdsAsync(
            [submission.EmployeeId], cancellationToken);
        if (!departmentMap.TryGetValue(submission.EmployeeId, out var departmentId) || !departmentId.HasValue)
            return;

        var managerRole = await _roleRepository.GetBySlugAsync(SystemRoleTemplates.ManagerSlug, cancellationToken);
        if (managerRole is null)
            return;

        var departmentKey = departmentId.Value.ToString();
        var memberships = await _membershipRepository.GetAllAsync(cancellationToken);

        foreach (var membership in memberships.Where(m => m.IsActive))
        {
            var attrs = membership.GetAttributes();
            if (!attrs.TryGetValue("departmentId", out var attrDept) || attrDept != departmentKey)
                continue;

            if (!membership.GetRoleIds().Contains(managerRole.Id))
                continue;

            await NotificationHelper.TryNotifyAsync(
                _logger,
                ct => _notificationService.NotifyTimesheetSubmittedAsync(
                    membership.UserId,
                    submission.PeriodStart,
                    submission.PeriodEnd,
                    ct),
                cancellationToken);
        }
    }
}
