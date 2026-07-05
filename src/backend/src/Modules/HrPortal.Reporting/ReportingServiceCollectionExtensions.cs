using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using HrPortal.Reporting.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Reporting;

public static class ReportingServiceCollectionExtensions
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddScoped<IReportGenerator, AttendanceReportGenerator>();
        services.AddScoped<IReportGenerator, ProjectsReportGenerator>();
        services.AddScoped<IReportGenerator, WorkedHoursReportGenerator>();
        services.AddScoped<IReportGenerator, EmployeesReportGenerator>();
        services.AddScoped<IReportGenerator, DepartmentsReportGenerator>();

        services.AddScoped<ReportGeneratorFactory>();
        services.AddScoped<GenerateReportQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<ReportQueryParamsValidator>();

        return services;
    }
}
