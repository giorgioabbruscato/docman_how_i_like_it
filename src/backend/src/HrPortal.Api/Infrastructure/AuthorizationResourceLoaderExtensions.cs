using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.ResourceLoaders;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Api.Infrastructure;

public static class AuthorizationResourceLoaderExtensions
{
    public static IServiceCollection AddAuthorizationResourceLoaders(this IServiceCollection services)
    {
        services.AddScoped<IEndpointResourceLoader, EmployeeResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, DepartmentResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, LeaveRequestResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, DocumentResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, ProjectResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, TaskResourceLoader>();
        services.AddScoped<IEndpointResourceLoader, TimeEntryResourceLoader>();
        services.AddScoped<IResourceLoader, CompositeResourceLoader>();

        return services;
    }
}
