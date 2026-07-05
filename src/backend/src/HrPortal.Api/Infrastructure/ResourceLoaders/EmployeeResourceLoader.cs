using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class EmployeeResourceLoader : IEndpointResourceLoader
{
    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public EmployeeResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesResourcePath(httpContext, "/api/v1/employees");

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (!ResourceLoaderHelpers.TryGetRouteId(httpContext, out var id))
            return null;

        return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
            _dbContext,
            _accessor,
            id,
            cancellationToken);
    }
}
