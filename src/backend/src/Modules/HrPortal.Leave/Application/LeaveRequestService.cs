using HrPortal.AccessControl.Application;
using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application.Dtos;
using HrPortal.Leave.Domain;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Domain;
using Microsoft.Extensions.Logging;

namespace HrPortal.Leave.Application;

public interface ILeaveRequestService
{
    Task<Result<IReadOnlyList<LeaveRequestDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestDto>> CreateAsync(CreateLeaveRequest request, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequestDto>> RejectAsync(Guid id, RejectLeaveRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        ILeaveRequestRepository repository,
        IEmployeeLookup employeeLookup,
        IWorkflowEngine workflowEngine,
        IWorkflowInstanceRepository workflowInstanceRepository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver,
        ILogger<LeaveRequestService> logger)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _workflowEngine = workflowEngine;
        _workflowInstanceRepository = workflowInstanceRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(requests.Select(MapToDto).ToList() as IReadOnlyList<LeaveRequestDto>);
    }

    public async Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure<LeaveRequestDto>("Leave request not found.", "NOT_FOUND");

        return Result.Success(MapToDto(leaveRequest));
    }

    public async Task<Result<LeaveRequestDto>> CreateAsync(
        CreateLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<LeaveType>(request.Type, true, out var leaveType))
            return Result.Failure<LeaveRequestDto>("Invalid leave type.", "VALIDATION_ERROR");

        if (!await _employeeLookup.ExistsAndIsActiveAsync(request.EmployeeId, cancellationToken))
            return Result.Failure<LeaveRequestDto>("Employee not found or inactive.", "NOT_FOUND");

        if (await _repository.HasOverlappingApprovedAsync(
                request.EmployeeId, request.StartDate, request.EndDate, cancellationToken: cancellationToken))
            return Result.Failure<LeaveRequestDto>("Leave request overlaps with an existing approved request.", "CONFLICT");

        if (leaveType == LeaveType.Annual)
        {
            var leaveRequest = LeaveRequest.Create(
                _tenantContext.TenantId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate,
                leaveType,
                request.Reason,
                _tenantContext.UserId);

            var approvedDays = await _repository.GetApprovedAnnualDaysInYearAsync(
                request.EmployeeId,
                request.StartDate.Year,
                cancellationToken: cancellationToken);

            if (approvedDays + leaveRequest.DayCount > LeaveRequest.MaxAnnualLeaveDays)
                return Result.Failure<LeaveRequestDto>(
                    $"Annual leave exceeds the maximum of {LeaveRequest.MaxAnnualLeaveDays} days per year.",
                    "CONFLICT");

            await _repository.AddAsync(leaveRequest, cancellationToken);
            await LogAndSaveAsync("leave_request.created", leaveRequest, cancellationToken);

            var workflowResult = await _workflowEngine.StartWorkflowAsync(
                WorkflowRequestType.Leave,
                leaveRequest.Id,
                request.EmployeeId,
                cancellationToken);

            if (!workflowResult.IsSuccess)
                return Result.Failure<LeaveRequestDto>(workflowResult.Error!, workflowResult.ErrorCode);

            _logger.LogInformation("Leave request {LeaveRequestId} created for employee {EmployeeId}", leaveRequest.Id, request.EmployeeId);
            return Result.Success(MapToDto(leaveRequest));
        }

        var request2 = LeaveRequest.Create(
            _tenantContext.TenantId,
            request.EmployeeId,
            request.StartDate,
            request.EndDate,
            leaveType,
            request.Reason,
            _tenantContext.UserId);

        await _repository.AddAsync(request2, cancellationToken);
        await LogAndSaveAsync("leave_request.created", request2, cancellationToken);

        var workflowResult2 = await _workflowEngine.StartWorkflowAsync(
            WorkflowRequestType.Leave,
            request2.Id,
            request.EmployeeId,
            cancellationToken);

        if (!workflowResult2.IsSuccess)
            return Result.Failure<LeaveRequestDto>(workflowResult2.Error!, workflowResult2.ErrorCode);

        _logger.LogInformation("Leave request {LeaveRequestId} created for employee {EmployeeId}", request2.Id, request.EmployeeId);
        return Result.Success(MapToDto(request2));
    }

    public async Task<Result<LeaveRequestDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure<LeaveRequestDto>("Leave request not found.", "NOT_FOUND");

        var actorUserId = _tenantContext.UserId;
        if (!actorUserId.HasValue)
            return Result.Failure<LeaveRequestDto>("User context is required.", "FORBIDDEN");

        var instance = await _workflowInstanceRepository.GetActiveByRequestAsync(
            WorkflowRequestType.Leave,
            id,
            cancellationToken);

        if (instance is null)
            return Result.Failure<LeaveRequestDto>("No active workflow found for this leave request.", "NOT_FOUND");

        var result = await _workflowEngine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Approve,
            actorUserId.Value,
            _tenantContext.EmployeeId,
            comment: null,
            cancellationToken);

        if (!result.IsSuccess)
            return Result.Failure<LeaveRequestDto>(result.Error!, result.ErrorCode);

        leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        _logger.LogInformation("Leave request {LeaveRequestId} approved via workflow", id);
        return Result.Success(MapToDto(leaveRequest!));
    }

    public async Task<Result<LeaveRequestDto>> RejectAsync(
        Guid id,
        RejectLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure<LeaveRequestDto>("Leave request not found.", "NOT_FOUND");

        var actorUserId = _tenantContext.UserId;
        if (!actorUserId.HasValue)
            return Result.Failure<LeaveRequestDto>("User context is required.", "FORBIDDEN");

        var instance = await _workflowInstanceRepository.GetActiveByRequestAsync(
            WorkflowRequestType.Leave,
            id,
            cancellationToken);

        if (instance is null)
            return Result.Failure<LeaveRequestDto>("No active workflow found for this leave request.", "NOT_FOUND");

        var result = await _workflowEngine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Reject,
            actorUserId.Value,
            _tenantContext.EmployeeId,
            request.Reason,
            cancellationToken);

        if (!result.IsSuccess)
            return Result.Failure<LeaveRequestDto>(result.Error!, result.ErrorCode);

        leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        _logger.LogInformation("Leave request {LeaveRequestId} rejected via workflow", id);
        return Result.Success(MapToDto(leaveRequest!));
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _repository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure("Leave request not found.", "NOT_FOUND");

        var actorUserId = _tenantContext.UserId ?? Guid.Empty;
        var actorEmployeeId = _tenantContext.EmployeeId ?? leaveRequest.EmployeeId;

        var instance = await _workflowInstanceRepository.GetActiveByRequestAsync(
            WorkflowRequestType.Leave,
            id,
            cancellationToken);

        if (instance is not null)
        {
            var workflowResult = await _workflowEngine.ProcessActionAsync(
                instance.Id,
                WorkflowActionType.Cancel,
                actorUserId,
                actorEmployeeId,
                comment: null,
                cancellationToken);

            if (!workflowResult.IsSuccess)
                return Result.Failure(workflowResult.Error!, workflowResult.ErrorCode);

            _logger.LogInformation("Leave request {LeaveRequestId} cancelled via workflow", id);
            return Result.Success();
        }

        try
        {
            leaveRequest.Cancel(_tenantContext.UserId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message, "VALIDATION_ERROR");
        }

        await _repository.UpdateAsync(leaveRequest, cancellationToken);
        await LogAndSaveAsync("leave_request.cancelled", leaveRequest, cancellationToken);
        _logger.LogInformation("Leave request {LeaveRequestId} cancelled", id);
        return Result.Success();
    }

    private async Task LogAndSaveAsync(string action, LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        await _auditService.LogAsync(new AuditEntry(action, nameof(LeaveRequest), leaveRequest.Id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leaveRequest) =>
        new(
            leaveRequest.Id,
            leaveRequest.EmployeeId,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.Type.ToString(),
            leaveRequest.Status.ToString(),
            leaveRequest.Reason,
            leaveRequest.ApprovedBy,
            leaveRequest.ApprovedAt);
}
