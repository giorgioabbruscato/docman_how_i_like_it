using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrPortal.IntegrationTests.Infrastructure;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string RoleHeaderName = "X-Test-Role";
    public const string UserIdHeaderName = "X-Test-User-Id";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RoleHeaderName, out var roleValues) ||
            string.IsNullOrWhiteSpace(roleValues.FirstOrDefault()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = roleValues.First()!.Trim().ToLowerInvariant();
        var normalizedRole = role switch
        {
            "admin" => "Admin",
            "hr" => "HR",
            "manager" => "Manager",
            "employee" => "Employee",
            _ => role
        };

        var userId = Guid.NewGuid();
        if (Request.Headers.TryGetValue(UserIdHeaderName, out var userIdValues) &&
            Guid.TryParse(userIdValues.FirstOrDefault(), out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "test@demo.local"),
            new(ClaimTypes.Role, normalizedRole)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
