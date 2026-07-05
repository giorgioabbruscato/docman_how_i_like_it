using HrPortal.Integrations.Application;
using HrPortal.Integrations.Application.Dtos;
using HrPortal.Integrations.Domain;
using HrPortal.Integrations.Infrastructure.Persistence;
using HrPortal.Leave.Application;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class CalendarTokenService
{
    private readonly IOAuthTokenStore _tokenStore;
    private readonly CalendarSyncProviderResolver _providerResolver;

    public CalendarTokenService(IOAuthTokenStore tokenStore, CalendarSyncProviderResolver providerResolver)
    {
        _tokenStore = tokenStore;
        _providerResolver = providerResolver;
    }

    public void StoreTokens(CalendarConnection connection, OAuthTokens tokens)
    {
        connection.Reconnect(
            _tokenStore.Protect(tokens.AccessToken),
            tokens.RefreshToken is not null ? _tokenStore.Protect(tokens.RefreshToken) : null,
            tokens.ExpiresAt);
    }

    public async Task<CalendarConnectionContext> BuildConnectionContextAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken)
    {
        var accessToken = _tokenStore.Unprotect(connection.AccessTokenEncrypted);
        var refreshToken = connection.RefreshTokenEncrypted is not null
            ? _tokenStore.Unprotect(connection.RefreshTokenEncrypted)
            : null;

        if (connection.TokenExpiresAt.HasValue
            && connection.TokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5)
            && !string.IsNullOrWhiteSpace(refreshToken))
        {
            var provider = _providerResolver.GetProvider(connection.Provider);
            var refreshed = await provider.RefreshTokenAsync(refreshToken, cancellationToken);
            StoreTokens(connection, refreshed);
            accessToken = refreshed.AccessToken;
            refreshToken = refreshed.RefreshToken ?? refreshToken;
        }

        return new CalendarConnectionContext
        {
            ConnectionId = connection.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = connection.TokenExpiresAt,
            ExternalCalendarId = connection.ExternalCalendarId
        };
    }
}

internal sealed class CalendarConnectionService : ICalendarConnectionService
{
    private readonly ICalendarConnectionRepository _connectionRepository;
    private readonly CalendarSyncProviderResolver _providerResolver;
    private readonly CalendarOAuthStateProtector _stateProtector;
    private readonly CalendarTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IntegrationsOptions _options;
    private readonly ILogger<CalendarConnectionService> _logger;

    public CalendarConnectionService(
        ICalendarConnectionRepository connectionRepository,
        CalendarSyncProviderResolver providerResolver,
        CalendarOAuthStateProtector stateProtector,
        CalendarTokenService tokenService,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IOptions<IntegrationsOptions> options,
        ILogger<CalendarConnectionService> logger)
    {
        _connectionRepository = connectionRepository;
        _providerResolver = providerResolver;
        _stateProtector = stateProtector;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _options = options.Value;
        _logger = logger;
    }

    public Task<Result<IReadOnlyList<CalendarProviderDto>>> GetProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CalendarProviderDto> providers =
        [
            new("Google", "Google Calendar"),
            new("Microsoft365", "Microsoft 365")
        ];

        return Task.FromResult(Result.Success(providers));
    }

    public Task<Result<CalendarConnectResponse>> GetConnectUrlAsync(
        CalendarProvider provider,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.EmployeeId is null)
            return Task.FromResult(Result.Failure<CalendarConnectResponse>(
                "Employee context is required to connect a calendar.",
                "FORBIDDEN"));

        var state = _stateProtector.Protect(new CalendarOAuthState
        {
            TenantId = _tenantContext.TenantId,
            EmployeeId = _tenantContext.EmployeeId.Value,
            Provider = provider,
            RedirectUri = redirectUri,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });

        var apiCallbackUri = $"{_options.ApiBaseUrl.TrimEnd('/')}/api/v1/integrations/calendar/callback";
        var syncProvider = _providerResolver.GetProvider(provider);
        var authorizationUrl = syncProvider.GetAuthorizationUrl(state, apiCallbackUri);

        return Task.FromResult(Result.Success(new CalendarConnectResponse(authorizationUrl)));
    }

    public async Task<Result<CalendarCallbackResult>> HandleCallbackAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default)
    {
        var oauthState = _stateProtector.Unprotect(state);
        if (oauthState is null || oauthState.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Success(new CalendarCallbackResult(
                $"{_options.FrontendBaseUrl}/settings/calendar/callback",
                false,
                "Invalid or expired OAuth state."));
        }

        try
        {
            var apiCallbackUri = $"{_options.ApiBaseUrl.TrimEnd('/')}/api/v1/integrations/calendar/callback";
            var provider = _providerResolver.GetProvider(oauthState.Provider);
            var tokens = await provider.ExchangeCodeAsync(code, apiCallbackUri, cancellationToken);

            var existing = await _connectionRepository.GetByEmployeeAndProviderAsync(
                oauthState.EmployeeId,
                oauthState.Provider,
                cancellationToken);

            if (existing is null)
            {
                var connection = CalendarConnection.Create(
                    oauthState.TenantId,
                    oauthState.EmployeeId,
                    oauthState.Provider,
                    string.Empty,
                    null,
                    null);
                _tokenService.StoreTokens(connection, tokens);
                await _connectionRepository.AddAsync(connection, cancellationToken);
            }
            else
            {
                _tokenService.StoreTokens(existing, tokens);
                await _connectionRepository.UpdateAsync(existing, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CalendarCallbackResult(oauthState.RedirectUri, true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback failed for provider {Provider}", oauthState.Provider);
            return Result.Success(new CalendarCallbackResult(
                oauthState.RedirectUri,
                false,
                "Failed to complete calendar connection."));
        }
    }

    public async Task<Result<IReadOnlyList<CalendarConnectionDto>>> GetConnectionsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.EmployeeId is null)
            return Result.Failure<IReadOnlyList<CalendarConnectionDto>>(
                "Employee context is required.",
                "FORBIDDEN");

        var connections = await _connectionRepository.GetActiveByEmployeeAsync(
            _tenantContext.EmployeeId.Value,
            cancellationToken);

        var dtos = connections.Select(c => new CalendarConnectionDto(
            c.Id,
            c.Provider.ToString(),
            c.ConnectedAt,
            c.IsActive)).ToList();

        return Result.Success<IReadOnlyList<CalendarConnectionDto>>(dtos);
    }

    public async Task<Result> DisconnectAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null)
            return Result.Failure("Calendar connection not found.", "NOT_FOUND");

        if (_tenantContext.EmployeeId is null || connection.EmployeeId != _tenantContext.EmployeeId)
            return Result.Failure("You can only disconnect your own calendar connections.", "FORBIDDEN");

        connection.Deactivate();
        await _connectionRepository.UpdateAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class LeaveCalendarSyncService : ILeaveCalendarSyncService
{
    private readonly ICalendarSyncService _calendarSyncService;

    public LeaveCalendarSyncService(ICalendarSyncService calendarSyncService) =>
        _calendarSyncService = calendarSyncService;

    public async Task SyncApprovedAsync(Guid leaveRequestId, CancellationToken cancellationToken = default)
    {
        await _calendarSyncService.SyncLeaveRequestAsync(leaveRequestId, cancellationToken);
    }

    public async Task DeleteEventsAsync(Guid leaveRequestId, CancellationToken cancellationToken = default)
    {
        await _calendarSyncService.DeleteLeaveEventAsync(leaveRequestId, cancellationToken);
    }
}
