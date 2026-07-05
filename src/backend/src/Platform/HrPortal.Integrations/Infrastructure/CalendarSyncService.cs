using HrPortal.Employees.Application;
using HrPortal.Integrations.Application;
using HrPortal.Integrations.Application.Dtos;
using HrPortal.Integrations.Domain;
using HrPortal.Integrations.Infrastructure.Persistence;
using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class CalendarSyncService : ICalendarSyncService
{
    private readonly ILeaveRequestRepository _leaveRepository;
    private readonly ICalendarConnectionRepository _connectionRepository;
    private readonly IExternalCalendarEventRepository _eventRepository;
    private readonly ICalendarSyncLogRepository _syncLogRepository;
    private readonly CalendarSyncProviderResolver _providerResolver;
    private readonly CalendarTokenService _tokenService;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<CalendarSyncService> _logger;

    public CalendarSyncService(
        ILeaveRequestRepository leaveRepository,
        ICalendarConnectionRepository connectionRepository,
        IExternalCalendarEventRepository eventRepository,
        ICalendarSyncLogRepository syncLogRepository,
        CalendarSyncProviderResolver providerResolver,
        CalendarTokenService tokenService,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        ILogger<CalendarSyncService> logger)
    {
        _leaveRepository = leaveRepository;
        _connectionRepository = connectionRepository;
        _eventRepository = eventRepository;
        _syncLogRepository = syncLogRepository;
        _providerResolver = providerResolver;
        _tokenService = tokenService;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> SyncLeaveRequestAsync(Guid leaveRequestId, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure("Leave request not found.", "NOT_FOUND");

        if (leaveRequest.Status != LeaveStatus.Approved)
            return Result.Failure("Only approved leave requests can be synced.", "VALIDATION_ERROR");

        var connections = await _connectionRepository.GetActiveByEmployeeAsync(
            leaveRequest.EmployeeId,
            cancellationToken);

        if (connections.Count == 0)
            return Result.Success();

        var employeeName = await _employeeLookup.GetFullNameAsync(leaveRequest.EmployeeId, cancellationToken)
            ?? leaveRequest.EmployeeId.ToString();

        var leaveContext = new LeaveEventContext
        {
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employeeName,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            LeaveType = leaveRequest.Type.ToString(),
            Reason = leaveRequest.Reason
        };

        foreach (var connection in connections)
        {
            try
            {
                await SyncConnectionAsync(connection, leaveContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Calendar sync failed for leave {LeaveRequestId} provider {Provider}",
                    leaveRequestId,
                    connection.Provider);

                var log = CalendarSyncLog.CreateFailure(
                    _tenantContext.TenantId,
                    leaveRequestId,
                    leaveRequest.EmployeeId,
                    connection.Provider,
                    ex.Message,
                    retryCount: 1,
                    nextRetryAt: DateTime.UtcNow.AddMinutes(5));

                await _syncLogRepository.AddAsync(log, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteLeaveEventAsync(Guid leaveRequestId, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetByLeaveRequestAsync(leaveRequestId, cancellationToken);
        if (events.Count == 0)
            return Result.Success();

        foreach (var calendarEvent in events)
        {
            var connection = await _connectionRepository.GetByEmployeeAndProviderAsync(
                await ResolveEmployeeIdAsync(leaveRequestId, cancellationToken),
                calendarEvent.Provider,
                cancellationToken);

            if (connection is null || !connection.IsActive)
            {
                await _eventRepository.DeleteAsync(calendarEvent, cancellationToken);
                continue;
            }

            try
            {
                var context = await _tokenService.BuildConnectionContextAsync(connection, cancellationToken);
                var provider = _providerResolver.GetProvider(calendarEvent.Provider);
                await provider.DeleteEventAsync(context, calendarEvent.ExternalEventId, cancellationToken);
                await _eventRepository.DeleteAsync(calendarEvent, cancellationToken);

                var log = CalendarSyncLog.CreateSuccess(
                    _tenantContext.TenantId,
                    leaveRequestId,
                    connection.EmployeeId,
                    calendarEvent.Provider,
                    "External event deleted.");
                await _syncLogRepository.AddAsync(log, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to delete external calendar event {EventId} for leave {LeaveRequestId}",
                    calendarEvent.ExternalEventId,
                    leaveRequestId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<CalendarSyncLogDto>>> GetSyncLogAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _syncLogRepository.GetRecentAsync(limit ?? 100, cancellationToken);
        var dtos = logs.Select(MapLog).ToList();
        return Result.Success<IReadOnlyList<CalendarSyncLogDto>>(dtos);
    }

    private async Task SyncConnectionAsync(
        CalendarConnection connection,
        LeaveEventContext leave,
        CancellationToken cancellationToken)
    {
        var provider = _providerResolver.GetProvider(connection.Provider);
        var context = await _tokenService.BuildConnectionContextAsync(connection, cancellationToken);
        var existing = await _eventRepository.GetByLeaveAndProviderAsync(
            leave.LeaveRequestId,
            connection.Provider,
            cancellationToken);

        var externalEventId = await provider.CreateOrUpdateEventAsync(
            context,
            leave,
            existing?.ExternalEventId,
            cancellationToken);

        if (existing is null)
        {
            var calendarEvent = ExternalCalendarEvent.Create(
                _tenantContext.TenantId,
                leave.LeaveRequestId,
                connection.Provider,
                externalEventId);
            await _eventRepository.AddAsync(calendarEvent, cancellationToken);
        }
        else
        {
            existing.UpdateSync(externalEventId);
            await _eventRepository.UpdateAsync(existing, cancellationToken);
        }

        var log = CalendarSyncLog.CreateSuccess(
            _tenantContext.TenantId,
            leave.LeaveRequestId,
            connection.EmployeeId,
            connection.Provider,
            "Leave synced to external calendar.");
        await _syncLogRepository.AddAsync(log, cancellationToken);
    }

    private async Task<Guid> ResolveEmployeeIdAsync(Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var leave = await _leaveRepository.GetByIdAsync(leaveRequestId, cancellationToken);
        return leave?.EmployeeId ?? Guid.Empty;
    }

    private static CalendarSyncLogDto MapLog(CalendarSyncLog log) =>
        new(
            log.Id,
            log.LeaveRequestId,
            log.EmployeeId,
            log.Provider?.ToString(),
            log.Status.ToString(),
            log.Message,
            log.RetryCount,
            log.NextRetryAt,
            log.CreatedAt);
}
