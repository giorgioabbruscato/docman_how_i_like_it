using HrPortal.AccessControl.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HrPortal.Leave.Infrastructure.Workflows;

internal sealed class LeaveWorkflowCompletionHandler : IWorkflowCompletionHandler
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LeaveWorkflowCompletionHandler> _logger;

    public LeaveWorkflowCompletionHandler(
        ILeaveRequestRepository repository,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver,
        IServiceScopeFactory scopeFactory,
        ILogger<LeaveWorkflowCompletionHandler> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public WorkflowRequestType RequestType => WorkflowRequestType.Leave;

    public async Task<Result> HandleCompletionAsync(
        WorkflowInstance instance,
        WorkflowStatus finalStatus,
        Guid actorEmployeeId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _repository.GetByIdAsync(instance.RequestId, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure("Leave request not found.", "NOT_FOUND");

        try
        {
            switch (finalStatus)
            {
                case WorkflowStatus.Approved:
                    if (await _repository.HasOverlappingApprovedAsync(
                            leaveRequest.EmployeeId,
                            leaveRequest.StartDate,
                            leaveRequest.EndDate,
                            leaveRequest.Id,
                            cancellationToken))
                        return Result.Failure(
                            "Leave request overlaps with an existing approved request.",
                            "CONFLICT");

                    if (leaveRequest.Type == LeaveType.Annual)
                    {
                        var approvedDays = await _repository.GetApprovedAnnualDaysInYearAsync(
                            leaveRequest.EmployeeId,
                            leaveRequest.StartDate.Year,
                            leaveRequest.Id,
                            cancellationToken);

                        if (approvedDays + leaveRequest.DayCount > LeaveRequest.MaxAnnualLeaveDays)
                            return Result.Failure(
                                $"Annual leave exceeds the maximum of {LeaveRequest.MaxAnnualLeaveDays} days per year.",
                                "CONFLICT");
                    }

                    leaveRequest.Approve(_tenantContext.UserId ?? actorEmployeeId);
                    await _repository.UpdateAsync(leaveRequest, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await RunCalendarSyncAsync(
                        (sync, ct) => sync.SyncApprovedAsync(leaveRequest.Id, ct),
                        cancellationToken);
                    await NotifyApprovedAsync(leaveRequest, cancellationToken);
                    break;

                case WorkflowStatus.Rejected:
                    leaveRequest.Reject(_tenantContext.UserId ?? actorEmployeeId, comment);
                    await _repository.UpdateAsync(leaveRequest, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await RunCalendarSyncAsync(
                        (sync, ct) => sync.DeleteEventsAsync(leaveRequest.Id, ct),
                        cancellationToken);
                    await _notificationService.SendAsync(new NotificationMessage(
                        leaveRequest.EmployeeId.ToString(),
                        "Leave request rejected",
                        $"Your leave request from {leaveRequest.StartDate} to {leaveRequest.EndDate} has been rejected."),
                        cancellationToken);
                    break;

                case WorkflowStatus.Cancelled:
                    leaveRequest.Cancel(_tenantContext.UserId);
                    await _repository.UpdateAsync(leaveRequest, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await RunCalendarSyncAsync(
                        (sync, ct) => sync.DeleteEventsAsync(leaveRequest.Id, ct),
                        cancellationToken);
                    break;
            }
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message, "VALIDATION_ERROR");
        }

        _logger.LogInformation(
            "Leave request {LeaveRequestId} workflow completed with status {Status}",
            leaveRequest.Id,
            finalStatus);

        return Result.Success();
    }

    private async Task RunCalendarSyncAsync(
        Func<ILeaveCalendarSyncService, CancellationToken, Task> syncAction,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var tenantAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        tenantAccessor.Set(TenantScopingContext.ForSeeding(_tenantContext.TenantId));

        var syncService = scope.ServiceProvider.GetRequiredService<ILeaveCalendarSyncService>();
        await syncAction(syncService, cancellationToken);
    }

    private async Task NotifyApprovedAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        var email = await _employeeLookup.GetEmailAsync(leaveRequest.EmployeeId, cancellationToken)
            ?? leaveRequest.EmployeeId.ToString();
        var recipient = await _recipientResolver.ResolveForEmployeeAsync(
            leaveRequest.EmployeeId,
            email,
            cancellationToken);

        if (recipient.UserId.HasValue)
        {
            await NotificationHelper.TryNotifyAsync(
                _logger,
                ct => _notificationService.NotifyLeaveApprovedAsync(
                    recipient.UserId.Value,
                    leaveRequest.StartDate,
                    leaveRequest.EndDate,
                    ct),
                cancellationToken);
        }
    }
}
