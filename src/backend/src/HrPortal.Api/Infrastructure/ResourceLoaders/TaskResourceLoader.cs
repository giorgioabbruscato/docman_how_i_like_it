using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Tasks.Domain;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class TaskResourceLoader : IEndpointResourceLoader
{
    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public TaskResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesResourcePath(httpContext, "/api/v1/tasks");

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (!ResourceLoaderHelpers.TryGetRouteId(httpContext, out var id))
            return null;

        var task = await _dbContext.Set<ProjectTask>()
            .AsNoTracking()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task is null)
            return null;

        if (task.AssignedEmployeeId is null)
            return new ResourceContext(null, task.Id, task.TenantId);

        return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
            _dbContext,
            _accessor,
            task.AssignedEmployeeId.Value,
            cancellationToken);
    }
}
