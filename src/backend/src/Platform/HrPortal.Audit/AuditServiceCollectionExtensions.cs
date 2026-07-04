using HrPortal.Audit.Application;
using HrPortal.Audit.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Audit;

public static class AuditServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalAudit(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
