using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Departments.Domain;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class DepartmentResourceLoader : IEndpointResourceLoader
{
    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public DepartmentResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesResourcePath(httpContext, "/api/v1/departments");

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (!ResourceLoaderHelpers.TryGetRouteId(httpContext, out var id))
            return null;

        var department = await _dbContext.Set<Department>()
            .AsNoTracking()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return department is null
            ? null
            : new ResourceContext(null, department.Id, department.TenantId);
    }
}
