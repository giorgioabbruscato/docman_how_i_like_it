using System.Text.Json;
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

    internal static bool MatchesPostCollectionPath(HttpContext context, string collectionPath) =>
        HttpMethods.IsPost(context.Request.Method) &&
        context.Request.Path.Equals(collectionPath, StringComparison.OrdinalIgnoreCase);

    internal static bool MatchesPostPath(HttpContext context, string path) =>
        HttpMethods.IsPost(context.Request.Method) &&
        context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase);

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

    internal static async Task<Guid?> TryGetEmployeeIdFromJsonBodyAsync(
        HttpContext context,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        if (context.Request.ContentLength is 0 or null)
            return null;

        context.Request.EnableBuffering();

        try
        {
            context.Request.Body.Position = 0;
            using var document = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty(propertyName, out var property))
                return null;

            return property.TryGetGuid(out var employeeId) ? employeeId : null;
        }
        catch (JsonException)
        {
            return null;
        }
        finally
        {
            context.Request.Body.Position = 0;
        }
    }

    internal static async Task<Guid?> TryGetEmployeeIdFromFormAsync(
        HttpContext context,
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        if (!context.Request.HasFormContentType)
            return null;

        var form = await context.Request.ReadFormAsync(cancellationToken);
        if (!form.TryGetValue(fieldName, out var value))
            return null;

        return Guid.TryParse(value.ToString(), out var employeeId) ? employeeId : null;
    }
}
