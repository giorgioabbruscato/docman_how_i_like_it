using HrPortal.Employees.Application;
using HrPortal.Employees.Application.Validators;
using HrPortal.Employees.Domain;
using HrPortal.Employees.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Employees;

public static class EmployeesServiceCollectionExtensions
{
    public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeLookup>(sp => sp.GetRequiredService<IEmployeeService>());
        services.AddValidatorsFromAssemblyContaining<CreateEmployeeRequestValidator>();
        return services;
    }
}
