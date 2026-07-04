using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HrPortal.Identity.Infrastructure;

public static class UserContextFactory
{
    public static UserContext FromHttpContext(HttpContext? httpContext)
    {
        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return UserContext.Anonymous;

        var userId = ResolveUserId(httpContext.User);
        var email = httpContext.User.FindFirstValue(ClaimTypes.Email)
            ?? httpContext.User.FindFirstValue("email")
            ?? string.Empty;
        var displayName = httpContext.User.FindFirstValue("name")
            ?? httpContext.User.FindFirstValue(ClaimTypes.Name)
            ?? email;

        var roles = httpContext.User.FindAll(ClaimTypes.Role)
            .Concat(httpContext.User.FindAll("roles"))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new UserContext
        {
            UserId = userId,
            Email = email,
            DisplayName = displayName,
            Roles = roles,
            IsAuthenticated = true
        };
    }

    private static Guid ResolveUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
