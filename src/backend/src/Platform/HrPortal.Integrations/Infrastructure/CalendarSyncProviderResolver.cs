using HrPortal.Integrations.Application;
using HrPortal.Integrations.Domain;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class CalendarSyncProviderResolver
{
    private readonly IReadOnlyDictionary<CalendarProvider, ICalendarSyncProvider> _providers;

    public CalendarSyncProviderResolver(IEnumerable<ICalendarSyncProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Provider);
    }

    public ICalendarSyncProvider GetProvider(CalendarProvider provider)
    {
        if (!_providers.TryGetValue(provider, out var syncProvider))
            throw new InvalidOperationException($"No calendar sync provider registered for {provider}.");

        return syncProvider;
    }
}
