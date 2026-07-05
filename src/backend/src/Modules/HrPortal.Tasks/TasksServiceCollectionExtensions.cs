using FluentValidation;
using HrPortal.Tasks.Application;
using HrPortal.Tasks.Application.Commands;
using HrPortal.Tasks.Application.Queries;
using HrPortal.Tasks.Application.Validators;
using HrPortal.Tasks.Infrastructure;
using HrPortal.Tasks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Tasks;

public static class TasksServiceCollectionExtensions
{
    public static IServiceCollection AddTasksModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        services.AddScoped<ITaskLookup, TaskLookup>();

        services.AddScoped<CreateProjectTaskCommandHandler>();
        services.AddScoped<UpdateProjectTaskCommandHandler>();
        services.AddScoped<DeleteProjectTaskCommandHandler>();
        services.AddScoped<GetProjectTaskByIdQueryHandler>();
        services.AddScoped<GetProjectTasksQueryHandler>();
        services.AddScoped<GetTaskBoardQueryHandler>();
        services.AddScoped<UpdateTaskStatusCommandHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateProjectTaskRequestValidator>();
        return services;
    }
}
