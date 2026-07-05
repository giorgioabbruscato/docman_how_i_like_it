using HrPortal.AccessControl.Application;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.ResourceLoaders;

internal static class ResourceLoaderHelpers
{
    internal static bool MatchesResourcePath(HttpContext context, string resourcePath) =>
        context.Request.Path.StartsWithSegments(resourcePath, StringComparison.OrdinalIgnoreCase) &&
        TryGetRouteId(context, out _);

    internal static bool TryGetRouteId(HttpContext context, out Guid id)
    {
        id = default;

        if (context.Request.RouteValues.TryGetValue("id", out var routeValue) &&
            Guid.TryParse(routeValue?.ToString(), out id))
        {
            return true;
        }

        return false;
    }

    internal static async Task<ResourceContext?> LoadEmployeeContextAsync(
        HrPortalDbContext db,
        ITenantContextAccessor accessor,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var employee = await db.Set<HrPortal.Employees.Domain.Employee>()
            .AsNoTracking()
            .ApplyTenantScope(accessor.Current)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        return employee is null
            ? null
            : new ResourceContext(employee.Id, employee.DepartmentId, employee.TenantId);
    }
}
