using HrPortal.Identity;
using HrPortal.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.HrOrAdmin, policy =>
                policy.RequireRole(Roles.Admin, Roles.Hr))
            .AddPolicy(Policies.ManagerOrAbove, policy =>
                policy.RequireRole(Roles.Admin, Roles.Hr, Roles.Manager))
            .AddPolicy(Policies.Authenticated, policy =>
                policy.RequireAuthenticatedUser());

        return services;
    }
}
