using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.TimeTracking.Domain;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class TimeEntryResourceLoader : IEndpointResourceLoader
{
    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public TimeEntryResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesResourcePath(httpContext, "/api/v1/time-entries");

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (!ResourceLoaderHelpers.TryGetRouteId(httpContext, out var id))
            return null;

        var entry = await _dbContext.Set<TimeEntry>()
            .AsNoTracking()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entry is null)
            return null;

        return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
            _dbContext,
            _accessor,
            entry.EmployeeId,
            cancellationToken);
    }
}
