using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Documents.Domain;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class DocumentResourceLoader : IEndpointResourceLoader
{
    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public DocumentResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesResourcePath(httpContext, "/api/v1/documents") ||
        ResourceLoaderHelpers.MatchesPostCollectionPath(httpContext, "/api/v1/documents");

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        if (ResourceLoaderHelpers.MatchesPostCollectionPath(httpContext, "/api/v1/documents"))
        {
            var employeeId = await ResourceLoaderHelpers.TryGetEmployeeIdFromFormAsync(
                httpContext, "employeeId", cancellationToken);

            if (!employeeId.HasValue)
                return null;

            return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
                _dbContext,
                _accessor,
                employeeId.Value,
                cancellationToken);
        }

        if (!ResourceLoaderHelpers.TryGetRouteId(httpContext, out var id))
            return null;

        var document = await _dbContext.Set<Document>()
            .AsNoTracking()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document is null)
            return null;

        return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
            _dbContext,
            _accessor,
            document.EmployeeId,
            cancellationToken);
    }
}
