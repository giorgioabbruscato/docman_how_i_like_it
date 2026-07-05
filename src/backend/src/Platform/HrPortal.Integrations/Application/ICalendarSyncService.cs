using HrPortal.Integrations.Application.Dtos;
using HrPortal.Integrations.Domain;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Integrations.Application;

public interface ICalendarSyncService
{
    Task<Result> SyncLeaveRequestAsync(Guid leaveRequestId, CancellationToken cancellationToken = default);

    Task<Result> DeleteLeaveEventAsync(Guid leaveRequestId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CalendarSyncLogDto>>> GetSyncLogAsync(
        int? limit = null,
        CancellationToken cancellationToken = default);
}

public interface ICalendarConnectionService
{
    Task<Result<IReadOnlyList<CalendarProviderDto>>> GetProvidersAsync(CancellationToken cancellationToken = default);

    Task<Result<CalendarConnectResponse>> GetConnectUrlAsync(
        CalendarProvider provider,
        string redirectUri,
        CancellationToken cancellationToken = default);

    Task<Result<CalendarCallbackResult>> HandleCallbackAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CalendarConnectionDto>>> GetConnectionsAsync(
        CancellationToken cancellationToken = default);

    Task<Result> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default);
}
