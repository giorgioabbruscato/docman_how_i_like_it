using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Application.Validators;
using HrPortal.AccessControl.Infrastructure;
using HrPortal.AccessControl.Infrastructure.Persistence;
using HrPortal.AccessControl.Infrastructure.Seeding;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.AccessControl;

public static class AccessControlServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalAccessControl(this IServiceCollection services)
    {
        services.AddScoped<ITenantRoleRepository, TenantRoleRepository>();
        services.AddScoped<ITenantMembershipRepository, TenantMembershipRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        services.AddScoped<IPermissionResolver, PermissionResolver>();
        services.AddScoped<IScopeResolver, ScopeResolver>();
        services.AddScoped<IPolicyEngine, PolicyEngine>();
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
        services.AddScoped<ITenantContextFactory, TenantContextFactory>();
        services.AddScoped<IMeService, MeService>();
        services.AddScoped<ITenantRoleService, TenantRoleService>();
        services.AddScoped<ITenantMembershipService, TenantMembershipService>();
        services.AddScoped<ISystemRoleSeeder, SystemRoleSeeder>();

        services.AddValidatorsFromAssemblyContaining<CreateTenantRoleRequestValidator>();

        return services;
    }
}
