using HrPortal.Tenancy.Application;
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

        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<TenantContext>(sp =>
        {
            var httpContext = sp.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()?.HttpContext;
            if (httpContext?.Items.TryGetValue(nameof(TenantContext), out var ctx) == true && ctx is TenantContext tenantContext)
                return tenantContext;
            return TenantContext.Empty;
        });

        return services;
    }
}
