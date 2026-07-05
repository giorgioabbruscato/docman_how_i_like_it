using FluentValidation;
using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Commands;
using HrPortal.Projects.Application.Queries;
using HrPortal.Projects.Application.Validators;
using HrPortal.Projects.Infrastructure;
using HrPortal.Projects.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Projects;

public static class ProjectsServiceCollectionExtensions
{
    public static IServiceCollection AddProjectsModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IProjectLookup, ProjectLookup>();

        services.AddScoped<CreateProjectCommandHandler>();
        services.AddScoped<UpdateProjectCommandHandler>();
        services.AddScoped<DeleteProjectCommandHandler>();
        services.AddScoped<AddProjectMemberCommandHandler>();
        services.AddScoped<RemoveProjectMemberCommandHandler>();
        services.AddScoped<GetProjectByIdQueryHandler>();
        services.AddScoped<GetProjectsQueryHandler>();
        services.AddScoped<GetProjectMembersQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateProjectRequestValidator>();
        return services;
    }
}
