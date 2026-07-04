using HrPortal.Tenancy.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    private readonly string _tenantHeaderName;

    public TenantHeaderOperationFilter(IOptions<TenantResolverOptions> options) =>
        _tenantHeaderName = options.Value.TenantHeaderName;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsExcludedPath(context.ApiDescription.RelativePath))
            return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _tenantHeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Tenant slug (e.g. demo). Required on all business endpoints.",
            Schema = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("demo") }
        });
    }

    private static bool IsExcludedPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var path = "/" + relativePath.TrimEnd('/');
        return path.StartsWith("/api/v1/tenants", StringComparison.OrdinalIgnoreCase);
    }
}
