using System.Text.Json;
using HrPortal.Integrations.Domain;
using Microsoft.AspNetCore.DataProtection;

namespace HrPortal.Integrations.Infrastructure;

public sealed class CalendarOAuthStateProtector
{
    public const string StatePurpose = "CalendarOAuthState";

    private readonly IDataProtector _protector;

    public CalendarOAuthStateProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(StatePurpose);
    }

    public string Protect(CalendarOAuthState state)
    {
        var json = JsonSerializer.Serialize(state);
        return _protector.Protect(json);
    }

    public CalendarOAuthState? Unprotect(string protectedState)
    {
        try
        {
            var json = _protector.Unprotect(protectedState);
            return JsonSerializer.Deserialize<CalendarOAuthState>(json);
        }
        catch
        {
            return null;
        }
    }
}

public sealed class CalendarOAuthState
{
    public Guid TenantId { get; init; }
    public Guid EmployeeId { get; init; }
    public CalendarProvider Provider { get; init; }
    public string RedirectUri { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
