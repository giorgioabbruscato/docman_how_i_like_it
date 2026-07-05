using HrPortal.Integrations.Application;
using Microsoft.AspNetCore.DataProtection;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class OAuthTokenStore : IOAuthTokenStore
{
    public const string TokenPurpose = "CalendarOAuthTokens";

    private readonly IDataProtector _protector;

    public OAuthTokenStore(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(TokenPurpose);
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedData) => _protector.Unprotect(protectedData);
}
