using FluentValidation;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Application.Validators;
using HrPortal.Workflows.Infrastructure;
using HrPortal.Workflows.Infrastructure.Persistence;
using HrPortal.Workflows.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Workflows;

public static class WorkflowsServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalWorkflows(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
        services.AddScoped<IWorkflowActionRepository, WorkflowActionRepository>();
        services.AddScoped<IWorkflowApproverResolver, WorkflowApproverResolver>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
        services.AddScoped<IWorkflowQueryService, WorkflowQueryService>();
        services.AddScoped<IWorkflowSeeder, WorkflowSeeder>();
        services.AddValidatorsFromAssemblyContaining<CreateWorkflowDefinitionRequestValidator>();
        return services;
    }
}
