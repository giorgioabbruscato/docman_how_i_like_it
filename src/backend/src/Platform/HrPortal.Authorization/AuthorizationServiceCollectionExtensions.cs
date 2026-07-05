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
        services.AddScoped<IAuthorizationHandler, PermissionAnyAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Authenticated, policy =>
                policy.RequireAuthenticatedUser());

        return services;
    }
}
