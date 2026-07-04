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
        services.AddValidatorsFromAssemblyContaining<CreateTenantRequestValidator>();
        services.AddScoped<TenantContext>(sp => sp.GetRequiredService<ITenantContextAccessor>().Current);

        return services;
    }
}
