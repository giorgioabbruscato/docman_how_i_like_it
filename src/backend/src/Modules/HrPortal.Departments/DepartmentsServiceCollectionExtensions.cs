using HrPortal.Departments.Application;
using HrPortal.Departments.Application.Validators;
using HrPortal.Departments.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Departments;

public static class DepartmentsServiceCollectionExtensions
{
    public static IServiceCollection AddDepartmentsModule(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDepartmentLookup>(sp => sp.GetRequiredService<IDepartmentService>());
        services.AddValidatorsFromAssemblyContaining<CreateDepartmentRequestValidator>();
        return services;
    }
}
