using FluentValidation;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Application.Validators;
using HrPortal.Tenancy.Infrastructure;
using HrPortal.Tenancy.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Tenancy;

public static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TenantResolverOptions>(
            configuration.GetSection(TenantResolverOptions.SectionName));

        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeatureGateService, FeatureGateService>();
        services.AddScoped<IPlatformMetricsService, PlatformMetricsService>();
        services.AddValidatorsFromAssemblyContaining<CreateTenantRequestValidator>();

        // Application services inject TenantContext (resolved from ITenantContextAccessor.Current).
        // RequestContextMiddleware must run before controller/service resolution to enrich it.
        // Never inject IHttpContextAccessor or UserContext into application services.
        services.AddScoped<TenantContext>(sp => sp.GetRequiredService<ITenantContextAccessor>().Current);

        return services;
    }
}
