using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal sealed class AttendanceResourceLoader : IEndpointResourceLoader
{
    private const string CheckInPath = "/api/v1/attendance/check-in";
    private const string CheckOutPath = "/api/v1/attendance/check-out";

    private readonly HrPortalDbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public AttendanceResourceLoader(HrPortalDbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public bool CanLoad(HttpContext httpContext) =>
        ResourceLoaderHelpers.MatchesPostPath(httpContext, CheckInPath) ||
        ResourceLoaderHelpers.MatchesPostPath(httpContext, CheckOutPath);

    public async Task<ResourceContext?> LoadAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await ResourceLoaderHelpers.TryGetEmployeeIdFromJsonBodyAsync(
            httpContext, "employeeId", cancellationToken);

        if (!employeeId.HasValue)
            return null;

        return await ResourceLoaderHelpers.LoadEmployeeContextAsync(
            _dbContext,
            _accessor,
            employeeId.Value,
            cancellationToken);
    }
}
